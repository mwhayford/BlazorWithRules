using BlazorApp.Core.Entities;
using BlazorApp.Core.Interfaces;
using BlazorApp.Core.Services;
using BlazorApp.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ValidationException = BlazorApp.Shared.Exceptions.ValidationException;

namespace BlazorApp.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IValidator<User> _validator;
    private readonly ILogger<UserService> _logger;
    private const string UserCacheKeyPrefix = "user";
    private const string AllUsersCacheKey = "all_users";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public UserService(
        IUserRepository userRepository,
        ICacheService cacheService,
        IValidator<User> validator,
        ILogger<UserService> logger
    )
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        _logger.LogInformation("Getting all users");

        return await _cacheService.GetOrSetAsync(
            AllUsersCacheKey,
            async () =>
            {
                var users = await _userRepository.GetAllAsync();
                _logger.LogInformation("Retrieved {UserCount} users from database", users.Count());
                return users.ToList();
            },
            CacheExpiration
        );
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        _logger.LogInformation("Getting user with ID {UserId}", id);

        var cacheKey = $"{UserCacheKeyPrefix}_{id}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user != null)
                {
                    _logger.LogInformation("Retrieved user {UserId} from database", id);
                }
                else
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                }
                return user;
            },
            CacheExpiration
        );
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        _logger.LogInformation("Getting user with email {Email}", email);

        var cacheKey = $"{UserCacheKeyPrefix}_email_{email.ToLowerInvariant()}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user != null)
                {
                    _logger.LogInformation("Retrieved user with email {Email} from database", email);
                }
                else
                {
                    _logger.LogWarning("User with email {Email} not found", email);
                }
                return user;
            },
            CacheExpiration
        );
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _logger.LogInformation("Creating new user with email {Email}", user.Email);

        // Validate the user
        var validationResult = await _validator.ValidateAsync(user);
        if (!validationResult.IsValid)
        {
            var errors = validationResult
                .Errors.GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray().AsEnumerable());

            _logger.LogWarning(
                "User validation failed for email {Email}: {ValidationErrors}",
                user.Email,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
            );

            throw new ValidationException(errors);
        }

        // Check if user with email already exists
        var existingUser = await _userRepository.GetByEmailAsync(user.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("User with email {Email} already exists", user.Email);
            throw new InvalidOperationException($"User with email {user.Email} already exists");
        }

        // Create the user
        var createdUser = await _userRepository.AddAsync(user);

        _logger.LogInformation(
            "Successfully created user {UserId} with email {Email}",
            createdUser.Id,
            createdUser.Email
        );

        // Invalidate cache
        await InvalidateUserCacheAsync();

        return createdUser;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        _logger.LogInformation("Updating user {UserId}", user.Id);

        // Validate the user
        var validationResult = await _validator.ValidateAsync(user);
        if (!validationResult.IsValid)
        {
            var errors = validationResult
                .Errors.GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray().AsEnumerable());

            _logger.LogWarning(
                "User validation failed for user {UserId}: {ValidationErrors}",
                user.Id,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
            );

            throw new ValidationException(errors);
        }

        // Check if user exists
        var existingUser = await _userRepository.GetByIdAsync(user.Id);
        if (existingUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found for update", user.Id);
            throw new KeyNotFoundException($"User with ID {user.Id} not found");
        }

        // Check if email is being changed and if new email already exists
        if (existingUser.Email != user.Email)
        {
            var userWithEmail = await _userRepository.GetByEmailAsync(user.Email);
            if (userWithEmail != null && userWithEmail.Id != user.Id)
            {
                _logger.LogWarning("Cannot update user {UserId}: email {Email} already exists", user.Id, user.Email);
                throw new InvalidOperationException($"User with email {user.Email} already exists");
            }
        }

        // Update the user
        var updatedUser = await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Successfully updated user {UserId}", user.Id);

        // Invalidate cache
        await InvalidateUserCacheAsync(user.Id, existingUser.Email);

        return updatedUser;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        _logger.LogInformation("Deleting user {UserId}", id);

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found for deletion", id);
            return false;
        }

        var deleted = await _userRepository.DeleteAsync(id);

        if (deleted)
        {
            _logger.LogInformation("Successfully deleted user {UserId}", id);

            // Invalidate cache
            await InvalidateUserCacheAsync(id, user.Email);
        }
        else
        {
            _logger.LogWarning("Failed to delete user {UserId}", id);
        }

        return deleted;
    }

    public async Task<bool> ToggleUserStatusAsync(int id)
    {
        _logger.LogInformation("Toggling status for user {UserId}", id);

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found for status toggle", id);
            return false;
        }

        user.IsActive = !user.IsActive;
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation(
            "Successfully toggled status for user {UserId} to {Status}",
            id,
            user.IsActive ? "Active" : "Inactive"
        );

        // Invalidate cache
        await InvalidateUserCacheAsync(id, user.Email);

        return true;
    }

    public async Task<int> GetUserCountAsync()
    {
        _logger.LogInformation("Getting user count");

        var cacheKey = "user_count";

        return await _cacheService.GetOrSetValueAsync(
            cacheKey,
            async () =>
            {
                var count = await _userRepository.CountAsync();
                _logger.LogInformation("User count: {UserCount}", count);
                return count;
            },
            TimeSpan.FromMinutes(5)
        );
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        _logger.LogInformation("Getting active user count");

        var cacheKey = "active_user_count";

        return await _cacheService.GetOrSetValueAsync(
            cacheKey,
            async () =>
            {
                var users = await _userRepository.GetAllAsync();
                var count = users.Count(u => u.IsActive);
                _logger.LogInformation("Active user count: {ActiveUserCount}", count);
                return count;
            },
            TimeSpan.FromMinutes(5)
        );
    }

    private async Task InvalidateUserCacheAsync(int? userId = null, string? email = null)
    {
        // Clear all users cache
        await _cacheService.RemoveAsync(AllUsersCacheKey);
        await _cacheService.RemoveAsync("user_count");
        await _cacheService.RemoveAsync("active_user_count");

        // Clear specific user cache if provided
        if (userId.HasValue)
        {
            await _cacheService.RemoveAsync($"{UserCacheKeyPrefix}_{userId.Value}");
        }

        // Clear email-based cache if provided
        if (!string.IsNullOrEmpty(email))
        {
            await _cacheService.RemoveAsync($"{UserCacheKeyPrefix}_email_{email.ToLowerInvariant()}");
        }

        _logger.LogDebug("Invalidated user cache entries");
    }
}
