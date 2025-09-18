using Microsoft.AspNetCore.Identity;

namespace BlazorApp.Core.Entities;

/// <summary>
/// Application role entity for role-based authorization
/// </summary>
public class ApplicationRole : IdentityRole
{
    /// <summary>
    /// Role description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Date when the role was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the role is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
