using BlazorApp.Shared.Models;

namespace BlazorApp.Core.Services;

/// <summary>
/// User service interface for business logic operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get all users asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user DTOs</returns>
    Task<IEnumerable<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by ID asynchronously
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User DTO or null if not found</returns>
    Task<UserDto?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new user asynchronously
    /// </summary>
    /// <param name="userDto">User data transfer object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user DTO</returns>
    Task<UserDto> CreateUserAsync(CreateUserDto userDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing user asynchronously
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="userDto">User data transfer object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user DTO</returns>
    Task<UserDto> UpdateUserAsync(int id, UpdateUserDto userDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete user asynchronously
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
}
