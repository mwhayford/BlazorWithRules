using BlazorApp.Core.Entities;

namespace BlazorApp.Core.Services;

/// <summary>
/// User service interface for business logic operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get all users asynchronously
    /// </summary>
    /// <returns>Collection of users</returns>
    Task<IEnumerable<User>> GetAllUsersAsync();

    /// <summary>
    /// Get user by ID asynchronously
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User or null if not found</returns>
    Task<User?> GetUserByIdAsync(int id);

    /// <summary>
    /// Get user by email asynchronously
    /// </summary>
    /// <param name="email">User email</param>
    /// <returns>User or null if not found</returns>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Create new user asynchronously
    /// </summary>
    /// <param name="user">User entity</param>
    /// <returns>Created user</returns>
    Task<User> CreateUserAsync(User user);

    /// <summary>
    /// Update existing user asynchronously
    /// </summary>
    /// <param name="user">User entity</param>
    /// <returns>Updated user</returns>
    Task<User> UpdateUserAsync(User user);

    /// <summary>
    /// Delete user asynchronously
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteUserAsync(int id);

    /// <summary>
    /// Toggle user active status
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if status was toggled successfully</returns>
    Task<bool> ToggleUserStatusAsync(int id);

    /// <summary>
    /// Get total user count
    /// </summary>
    /// <returns>Total number of users</returns>
    Task<int> GetUserCountAsync();

    /// <summary>
    /// Get active user count
    /// </summary>
    /// <returns>Number of active users</returns>
    Task<int> GetActiveUserCountAsync();
}
