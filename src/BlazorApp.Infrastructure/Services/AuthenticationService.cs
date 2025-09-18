using System.Security.Claims;
using BlazorApp.Core.Entities;
using BlazorApp.Core.Services;
using BlazorApp.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace BlazorApp.Infrastructure.Services;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthenticationService : BlazorApp.Core.Services.IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the AuthenticationService
    /// </summary>
    /// <param name="userManager">User manager</param>
    /// <param name="httpContextAccessor">HTTP context accessor</param>
    public AuthenticationService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return null;
        }

        return await _userManager.FindByIdAsync(userId);
    }

    /// <inheritdoc />
    public async Task<bool> IsInRoleAsync(string role)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            return false;
        }

        return await _userManager.IsInRoleAsync(user, role);
    }

    /// <inheritdoc />
    public async Task<bool> IsAdminAsync()
    {
        return await IsInRoleAsync(ApplicationRoles.Admin);
    }

    /// <inheritdoc />
    public async Task<bool> IsMemberAsync()
    {
        return await IsInRoleAsync(ApplicationRoles.Member);
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthenticatedAsync()
    {
        var user = await GetCurrentUserAsync();
        return user != null;
    }

    /// <inheritdoc />
    public async Task<string?> GetCurrentUserIdAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.Id;
    }

    /// <inheritdoc />
    public async Task SignOutAsync()
    {
        // For now, we'll just clear the authentication cookie
        // In a full implementation, you'd use SignInManager.SignOutAsync()
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            context.Response.Cookies.Delete(".AspNetCore.Identity.Application");
        }
        await Task.CompletedTask;
    }
}
