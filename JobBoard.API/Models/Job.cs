using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobBoard.API.Models;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Category { get; set; } = string.Empty; // e.g., Remote, Hybrid
    
    [Required]
    public string SalaryRange { get; set; } = string.Empty;
    
    public string Requirements { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Key for Employer
    [Required]
    public Guid EmployerId { get; set; }
    
    [ForeignKey("EmployerId")]
    public User? Employer { get; set; }

    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}