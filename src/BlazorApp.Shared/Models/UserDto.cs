using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Shared.Models;

/// <summary>
/// User data transfer object for API responses
/// </summary>
public record UserDto
{
    /// <summary>
    /// User unique identifier
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// User first name
    /// </summary>
    required public string FirstName { get; init; }

    /// <summary>
    /// User last name
    /// </summary>
    required public string LastName { get; init; }

    /// <summary>
    /// User email address
    /// </summary>
    required public string Email { get; init; }

    /// <summary>
    /// User full name (computed property)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Date when user was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Date when user was last updated
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Whether the user is active
    /// </summary>
    public bool IsActive { get; init; } = true;
}

/// <summary>
/// Data transfer object for creating a new user
/// </summary>
public record CreateUserDto
{
    /// <summary>
    /// User first name
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public required string FirstName { get; init; }

    /// <summary>
    /// User last name
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public required string LastName { get; init; }

    /// <summary>
    /// User email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; init; }
}

/// <summary>
/// Data transfer object for updating an existing user
/// </summary>
public record UpdateUserDto
{
    /// <summary>
    /// User first name
    /// </summary>
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; init; }

    /// <summary>
    /// User last name
    /// </summary>
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; init; }

    /// <summary>
    /// User email address
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; init; }

    /// <summary>
    /// Whether the user is active
    /// </summary>
    public bool? IsActive { get; init; }
}
