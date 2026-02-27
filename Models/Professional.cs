using System.ComponentModel.DataAnnotations;

namespace MentalHealthSupport.Models;

public class Professional
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Specialty { get; set; } = string.Empty;

    public string? Approach { get; set; }
    public string? City { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    public bool OnlineSessions { get; set; }
    public string? CrpOrCrm { get; set; }
}
