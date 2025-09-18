using BlazorApp.Core.Entities;

namespace BlazorApp.Core.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Get the current user
    /// </summary>
    /// <returns>Current user or null if not authenticated</returns>
    Task<ApplicationUser?> GetCurrentUserAsync();

    /// <summary>
    /// Check if the current user has a specific role
    /// </summary>
    /// <param name="role">Role to check</param>
    /// <returns>True if user has the role</returns>
    Task<bool> IsInRoleAsync(string role);

    /// <summary>
    /// Check if the current user is an admin
    /// </summary>
    /// <returns>True if user is admin</returns>
    Task<bool> IsAdminAsync();

    /// <summary>
    /// Check if the current user is a member
    /// </summary>
    /// <returns>True if user is member</returns>
    Task<bool> IsMemberAsync();

    /// <summary>
    /// Check if the current user is authenticated
    /// </summary>
    /// <returns>True if user is authenticated</returns>
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Get the current user's ID
    /// </summary>
    /// <returns>User ID or null if not authenticated</returns>
    Task<string?> GetCurrentUserIdAsync();

    /// <summary>
    /// Sign out the current user
    /// </summary>
    /// <returns>Task</returns>
    Task SignOutAsync();
}
