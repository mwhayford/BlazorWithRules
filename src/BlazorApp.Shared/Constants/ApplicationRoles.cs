namespace BlazorApp.Shared.Constants;

/// <summary>
/// Application role constants
/// </summary>
public static class ApplicationRoles
{
    /// <summary>
    /// Administrator role - full access to all features
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Member role - access to member features and loan applications
    /// </summary>
    public const string Member = "Member";

    /// <summary>
    /// Get all available roles
    /// </summary>
    public static readonly string[] AllRoles = { Admin, Member };
}
