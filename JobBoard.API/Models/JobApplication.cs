using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobBoard.API.Models;

public class JobApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid JobId { get; set; }
    
    [ForeignKey("JobId")]
    public Job? Job { get; set; }

    [Required]
    public Guid JobSeekerId { get; set; }
    
    [ForeignKey("JobSeekerId")]
    public User? JobSeeker { get; set; }

    [Required]
    public string ResumeUrl { get; set; } = string.Empty;
    
    public string CoverLetter { get; set; } = string.Empty;
    
    [Required]
    public string Status { get; set; } = "Pending"; // Pending, Interviewing, Hired, Rejected
    
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
}