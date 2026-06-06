using System.Security.Claims;
using JobBoard.API.Data;
using JobBoard.API.DTOs;
using JobBoard.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // এই কন্ট্রোলারের সব এন্ডপয়েন্টের জন্যই লগইন থাকা বাধ্যতামূলক
public class ApplicationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApplicationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ১. APPLY FOR A JOB (শুধুমাত্র JobSeeker-দের জন্য)
    [HttpPost("apply")]
    [Authorize(Roles = "JobSeeker")]
    public async Task<IActionResult> ApplyJob([FromBody] ApplyJobDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();
        var seekerId = Guid.Parse(userIdClaim);

        // জবটি আসলেই এক্সিস্ট করে কিনা চেক করা
        var jobExists = await _context.Jobs.AnyAsync(j => j.Id == dto.JobId);
        if (!jobExists) return NotFound(new { message = "Job not found." });

        // ইউজার অলরেডি এই জবে অ্যাপ্লাই করেছে কিনা চেক করা (ডুপ্লিকেট অ্যাপ্লিকেশন বন্ধ)
        var alreadyApplied = await _context.Applications
            .AnyAsync(a => a.JobId == dto.JobId && a.JobSeekerId == seekerId);
            
        if (alreadyApplied)
        {
            return BadRequest(new { message = "You have already applied for this job." });
        }

        var application = new JobApplication
        {
            JobId = dto.JobId,
            JobSeekerId = seekerId,
            ResumeUrl = dto.ResumeUrl,
            CoverLetter = dto.CoverLetter,
            Status = "Pending"
        };

        _context.Applications.Add(application);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Application submitted successfully!" });
    }

    // ২. GET CANDIDATES FOR A JOB (শুধুমাত্র জবের মালিক/Employer দেখতে পারবে)
    [HttpGet("job/{jobId}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetJobApplications(Guid jobId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();
        var employerId = Guid.Parse(userIdClaim);

        // নিশ্চিত করা যে এই জবটি এই এমপ্লয়ারেরই তৈরি করা
        var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerId == employerId);
        if (job == null) return Forbid(); // অন্য এমপ্লয়ারের অ্যাপ্লিকেন্ট লিস্ট দেখা নিষেধ

        // অ্যাপ্লিকেন্টদের প্রোফাইলসহ লিস্ট নিয়ে আসা
        var applications = await _context.Applications
            .Where(a => a.JobId == jobId)
            .Include(a => a.JobSeeker)
            .Select(a => new
            {
                a.Id,
                a.ResumeUrl,
                a.CoverLetter,
                a.Status,
                a.AppliedAt,
                Applicant = new { a.JobSeeker!.FullName, a.JobSeeker.Email }
            })
            .ToListAsync();

        return Ok(applications);
    }

    // ৩. UPDATE APPLICATION STATUS (শুধুমাত্র Employer করতে পারবে)
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateApplicationStatusDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();
        var employerId = Guid.Parse(userIdClaim);

        // অ্যাপ্লিকেশনটি খুঁজে বের করা এবং সাথে জব ডাটা লোড করা
        var application = await _context.Applications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null) return NotFound(new { message = "Application not found." });

        // চেক করা যে এই জবের মালিক আসলেই এই এমপ্লয়ার কিনা
        if (application.Job?.EmployerId != employerId)
        {
            return Forbid();
        }

        // স্ট্যাটাস আপডেট
        application.Status = dto.Status;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Application status updated to {dto.Status}." });
    }

    // ৪. GET JOBSEEKER APPLICATIONS (লগইন থাকা ক্যান্ডিডেট নিজের সব আবেদন দেখতে পারবে)
    [HttpGet("my-applications")]
    [Authorize(Roles = "JobSeeker")]
    public async Task<IActionResult> GetMyApplications()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();
        var seekerId = Guid.Parse(userIdClaim);

        var applications = await _context.Applications
            .Where(a => a.JobSeekerId == seekerId)
            .Include(a => a.Job) // জবের ডিটেইলসসহ লোড করবে
            .Select(a => new
            {
                a.Id,
                a.Status,
                a.AppliedAt,
                a.ResumeUrl,
                Job = a.Job != null ? new
                {
                    a.Job.Id,
                    a.Job.Title,
                    a.Job.Category,
                    a.Job.SalaryRange
                } : null
            })
            .ToListAsync();

        return Ok(applications);
    }
}