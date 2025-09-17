namespace BlazorApp.Shared.Constants;

/// <summary>
/// Application-wide constants
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// Application name
    /// </summary>
    public const string ApplicationName = "BlazorApp";

    /// <summary>
    /// Default page size for pagination
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Maximum page size for pagination
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Connection string names
    /// </summary>
    public static class ConnectionStrings
    {
        public const string DefaultConnection = "DefaultConnection";
        public const string AzureSqlConnection = "AzureSqlConnection";
    }

    /// <summary>
    /// Cache keys
    /// </summary>
    public static class CacheKeys
    {
        public const string Users = "users";
        public const string UserById = "user_{0}";
    }

    /// <summary>
    /// Configuration section names
    /// </summary>
    public static class ConfigurationSections
    {
        public const string ApplicationInsights = "ApplicationInsights";
        public const string AzureKeyVault = "AzureKeyVault";
        public const string Logging = "Logging";
    }

    /// <summary>
    /// User roles
    /// </summary>
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string User = "User";
        public const string Manager = "Manager";
    }

    /// <summary>
    /// Claims
    /// </summary>
    public static class Claims
    {
        public const string UserId = "userid";
        public const string Email = "email";
        public const string Role = "role";
    }
}
