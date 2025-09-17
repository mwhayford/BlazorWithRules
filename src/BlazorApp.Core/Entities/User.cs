using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Core.Entities;

/// <summary>
/// User entity representing application users
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// User's first name
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string LastName { get; set; }

    /// <summary>
    /// User's email address (unique)
    /// </summary>
    [Required]
    [StringLength(256)]
    [EmailAddress]
    public required string Email { get; set; }

    /// <summary>
    /// User's phone number
    /// </summary>
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// User's date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// User's role in the system
    /// </summary>
    [StringLength(50)]
    public string Role { get; set; } = "User";

    /// <summary>
    /// Date when user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// User's profile picture URL
    /// </summary>
    [StringLength(500)]
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// User's full name (computed property)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Navigation property for user's orders (example relationship)
    /// </summary>
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
