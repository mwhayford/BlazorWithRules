using Microsoft.AspNetCore.Identity;

namespace BlazorApp.Core.Entities;

/// <summary>
/// Application user entity for authentication and authorization
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's full name (computed property)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// User's profile picture URL
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Date when the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Google OAuth provider key
    /// </summary>
    public string? GoogleProviderKey { get; set; }
}
