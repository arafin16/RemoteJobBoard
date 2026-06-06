using System.ComponentModel.DataAnnotations;

namespace JobBoard.API.DTOs;

public class ApplyJobDto
{
    [Required]
    public Guid JobId { get; set; }

    [Required]
    [Url(ErrorMessage = "Please provide a valid URL for your resume.")]
    public string ResumeUrl { get; set; } = string.Empty; // যেমন: গুগল ড্রাইভ বা ড্রপবক্সের সিভির লিংক

    public string CoverLetter { get; set; } = string.Empty;
}