using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;

namespace BlazorApp.IntegrationTests.Common;

/// <summary>
/// Test fixture for database integration tests using Testcontainers
/// </summary>
public class DatabaseTestFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer;
    public string ConnectionString { get; private set; } = string.Empty;

    public DatabaseTestFixture()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        ConnectionString = _msSqlContainer.GetConnectionString();

        // Create and migrate the database
        await CreateDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _msSqlContainer.StopAsync();
        await _msSqlContainer.DisposeAsync();
    }

    private async Task CreateDatabaseAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .EnableServiceProviderCaching(false)
            .EnableSensitiveDataLogging()
            .Options;

        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Create a new database context for testing
    /// </summary>
    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .EnableServiceProviderCaching(false)
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Create a service collection configured for testing
    /// </summary>
    public IServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        // Add Entity Framework with test database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(ConnectionString).EnableServiceProviderCaching(false).EnableSensitiveDataLogging()
        );

        return services;
    }
}

/// <summary>
/// Base class for integration tests with database
/// </summary>
public abstract class DatabaseIntegrationTestBase : IClassFixture<DatabaseTestFixture>
{
    protected readonly DatabaseTestFixture DatabaseFixture;
    protected readonly IServiceProvider ServiceProvider;

    protected DatabaseIntegrationTestBase(DatabaseTestFixture databaseFixture)
    {
        DatabaseFixture = databaseFixture;

        var services = DatabaseFixture.CreateServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Override this method to configure additional services for the test
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Override in derived classes to add additional services
    }

    /// <summary>
    /// Create a new database context for the test
    /// </summary>
    protected ApplicationDbContext CreateDbContext()
    {
        return DatabaseFixture.CreateDbContext();
    }

    /// <summary>
    /// Get a service from the test service provider
    /// </summary>
    protected T GetService<T>()
        where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Clean up the database after each test
    /// </summary>
    protected async Task CleanupDatabaseAsync()
    {
        await using var context = CreateDbContext();

        // Remove all test data
        context.Users.RemoveRange(context.Users);
        context.Orders.RemoveRange(context.Orders);
        context.OrderItems.RemoveRange(context.OrderItems);

        await context.SaveChangesAsync();
    }
}
