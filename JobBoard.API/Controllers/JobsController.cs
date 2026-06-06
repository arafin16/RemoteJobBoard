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
public class JobsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public JobsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ১. GET ALL JOBS (সবার জন্য উন্মুক্ত - সার্চ ও ফিল্টারসহ)
    [HttpGet]
    public async Task<IActionResult> GetAllJobs([FromQuery] string? search, [FromQuery] string? category)
    {
        var query = _context.Jobs.AsQueryable();

        // সার্চ ফিল্টার (টাইটেল বা ডেসক্রিপশনে খুঁজবে)
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(j => j.Title.ToLower().Contains(search.ToLower()) || 
                                     j.Description.ToLower().Contains(search.ToLower()));
        }

        // ক্যাটাগরি ফিল্টার (যেমন: Remote)
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(j => j.Category.ToLower() == category.ToLower());
        }

        var jobs = await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
        return Ok(jobs);
    }

    // ২. GET JOB BY ID (নির্দিষ্ট একটি জবের ডিটেইলস - Public)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobById(Guid id)
    {
        var job = await _context.Jobs
            .Include(j => j.Employer) // এমপ্লয়ারের ডিটেইলসসহ লোড করবে
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null)
        {
            return NotFound(new { message = "Job not found." });
        }

        // সিকিউরিটির জন্য এমপ্লয়ারের পাসওয়ার্ড হ্যাশ বাদ দিয়ে রিটার্ন করা
        var result = new
        {
            job.Id,
            job.Title,
            job.Description,
            job.Category,
            job.SalaryRange,
            job.Requirements,
            job.CreatedAt,
            Employer = new { job.Employer?.FullName, job.Employer?.Email }
        };

        return Ok(result);
    }

    // ৩. POST A NEW JOB (শুধুমাত্র Employer করতে পারবে)
    [HttpPost]
    [Authorize(Roles = "Employer")] // টোকেন ভেরিফাই করবে এবং দেখবে রোল 'Employer' কিনা
    public async Task<IActionResult> CreateJob([FromBody] CreateJobDto dto)
    {
        // JWT টোকেন থেকে লগইন থাকা ইউজারের (Employer) ID বের করা
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();

        var employerId = Guid.Parse(userIdClaim);

        var job = new Job
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            SalaryRange = dto.SalaryRange,
            Requirements = dto.Requirements,
            EmployerId = employerId
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, job);
    }

    // ৪. DELETE A JOB (শুধুমাত্র পোস্টকারী Employer ডিলিট করতে পারবে)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> DeleteJob(Guid id)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job == null) return NotFound(new { message = "Job not found." });

        // টোকেনের ইউজার আর জবের এমপ্লয়ার সেম কিনা চেক করা (অন্য কেউ যেন ডিলিট করতে না পারে)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != job.EmployerId.ToString())
        {
            return Forbid(); // ৪০৩ Forbidden - নিজের জব ছাড়া অন্য কারো জব ডিলিট করা নিষেধ
        }

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Job deleted successfully." });
    }
    // ৫. UPDATE A JOB (শুধুমাত্র পোস্টকারী Employer এডিট করতে পারবে)
[HttpPut("{id}")]
[Authorize(Roles = "Employer")]
public async Task<IActionResult> UpdateJob(Guid id, [FromBody] CreateJobDto dto)
{
    var job = await _context.Jobs.FindAsync(id);
    if (job == null) return NotFound(new { message = "Job not found." });

    // টোকেনের ইউজার আর জবের এমপ্লয়ার সেম কিনা চেক করা (অন্য কেউ যেন এডিট করতে না পারে)
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userIdClaim != job.EmployerId.ToString())
    {
        return Forbid(); // ৪MD/৪০৩ Forbidden
    }

    // ডাটা আপডেট করা
    job.Title = dto.Title;
    job.Description = dto.Description;
    job.Category = dto.Category;
    job.SalaryRange = dto.SalaryRange;
    job.Requirements = dto.Requirements;

    await _context.SaveChangesAsync();
    return Ok(new { message = "Job updated successfully.", job });
}
}