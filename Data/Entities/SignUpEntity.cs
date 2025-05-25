using System.ComponentModel.DataAnnotations;

namespace Data.Entities;

public class SignUpEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string EventId { get; set; } = null!;

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    /* Help from chatgpt to format timezone */
    public DateTime SignUpDate { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(
        DateTime.UtcNow,
        TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")
    );
}