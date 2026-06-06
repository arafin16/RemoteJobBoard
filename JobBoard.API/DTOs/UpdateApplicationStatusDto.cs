using System.ComponentModel.DataAnnotations;

namespace JobBoard.API.DTOs;

public class UpdateApplicationStatusDto
{
    [Required]
    public string Status { get; set; } = "Pending"; // Pending, Interviewing, Hired, Rejected
}