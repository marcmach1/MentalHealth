namespace MentalHealthSupport.Models;

public class UserProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? City { get; set; }
    public int Age { get; set; }
    public string? MainConcern { get; set; }
}
