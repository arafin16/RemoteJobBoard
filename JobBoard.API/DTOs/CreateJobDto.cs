using System.ComponentModel.DataAnnotations;

namespace JobBoard.API.DTOs;

public class CreateJobDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty; // e.g., Remote, Hybrid, Full-time

    [Required]
    public string SalaryRange { get; set; } = string.Empty;

    public string Requirements { get; set; } = string.Empty;
}