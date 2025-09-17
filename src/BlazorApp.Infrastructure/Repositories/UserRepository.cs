using BlazorApp.Core.Entities;
using BlazorApp.Core.Interfaces;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Repositories;

/// <summary>
/// User-specific repository implementation
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the UserRepository
    /// </summary>
    /// <param name="context">Database context</param>
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get user by email address
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User or null if not found</returns>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    /// <summary>
    /// Get active users only
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active users</returns>
    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.IsActive)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get users by role
    /// </summary>
    /// <param name="role">User role</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of users with the specified role</returns>
    public async Task<IEnumerable<User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(role))
            return new List<User>();

        return await _dbSet
            .Where(u => u.Role.ToLower() == role.ToLower())
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Search users by name or email
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of users matching the search term</returns>
    public async Task<IEnumerable<User>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync(cancellationToken);

        var term = searchTerm.ToLower();
        return await _dbSet
            .Where(u => u.FirstName.ToLower().Contains(term) ||
                       u.LastName.ToLower().Contains(term) ||
                       u.Email.ToLower().Contains(term))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get users with their orders
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of users with orders included</returns>
    public async Task<IEnumerable<User>> GetUsersWithOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Orders)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get user with orders by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User with orders or null if not found</returns>
    public async Task<User?> GetUserWithOrdersByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Update user's last login time
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated successfully</returns>
    public async Task<bool> UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return false;

        user.LastLoginAt = DateTime.UtcNow;
        await SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>
/// Interface for User repository with specific methods
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetUsersWithOrdersAsync(CancellationToken cancellationToken = default);
    Task<User?> GetUserWithOrdersByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default);
}
