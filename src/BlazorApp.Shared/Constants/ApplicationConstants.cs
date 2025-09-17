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
        /// <summary>
        /// Default database connection string name.
        /// </summary>
        public const string DefaultConnection = "DefaultConnection";

        /// <summary>
        /// Azure SQL database connection string name.
        /// </summary>
        public const string AzureSqlConnection = "AzureSqlConnection";
    }

    /// <summary>
    /// Cache keys
    /// </summary>
    public static class CacheKeys
    {
        /// <summary>
        /// Cache key for all users list.
        /// </summary>
        public const string Users = "users";

        /// <summary>
        /// Cache key pattern for individual user by ID.
        /// </summary>
        public const string UserById = "user_{0}";
    }

    /// <summary>
    /// Configuration section names
    /// </summary>
    public static class ConfigurationSections
    {
        /// <summary>
        /// Application Insights configuration section name.
        /// </summary>
        public const string ApplicationInsights = "ApplicationInsights";

        /// <summary>
        /// Azure Key Vault configuration section name.
        /// </summary>
        public const string AzureKeyVault = "AzureKeyVault";

        /// <summary>
        /// Logging configuration section name.
        /// </summary>
        public const string Logging = "Logging";
    }

    /// <summary>
    /// User roles
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// Administrator role with full system access.
        /// </summary>
        public const string Administrator = "Administrator";

        /// <summary>
        /// Standard user role with limited access.
        /// </summary>
        public const string User = "User";

        /// <summary>
        /// Manager role with moderate access.
        /// </summary>
        public const string Manager = "Manager";
    }

    /// <summary>
    /// Claims
    /// </summary>
    public static class Claims
    {
        /// <summary>
        /// User identifier claim name.
        /// </summary>
        public const string UserId = "userid";

        /// <summary>
        /// Email address claim name.
        /// </summary>
        public const string Email = "email";

        /// <summary>
        /// User role claim name.
        /// </summary>
        public const string Role = "role";
    }
}
