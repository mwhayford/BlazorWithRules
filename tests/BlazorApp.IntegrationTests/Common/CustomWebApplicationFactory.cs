using BlazorApp.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlazorApp.IntegrationTests.Common;

/// <summary>
/// Custom web application factory for integration testing
/// </summary>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class
{
    private readonly DatabaseTestFixture _databaseFixture;

    public CustomWebApplicationFactory()
    {
        _databaseFixture = new DatabaseTestFixture();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbContextServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextServiceDescriptor != null)
            {
                services.Remove(dbContextServiceDescriptor);
            }

            // Add a database context using the test container
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(_databaseFixture.ConnectionString);
                options.EnableServiceProviderCaching(false);
                options.EnableSensitiveDataLogging();
            });

            // Ensure the database is created
            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
        
        // Configure logging for tests
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
    }

    public new async Task DisposeAsync()
    {
        await _databaseFixture.DisposeAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Create a database context for testing
    /// </summary>
    public ApplicationDbContext CreateDbContext()
    {
        return _databaseFixture.CreateDbContext();
    }

    /// <summary>
    /// Seed test data into the database
    /// </summary>
    public async Task SeedTestDataAsync<T>(IEnumerable<T> entities) where T : class
    {
        await using var context = CreateDbContext();
        await context.Set<T>().AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Clean up all test data from the database
    /// </summary>
    public async Task CleanupDatabaseAsync()
    {
        await using var context = CreateDbContext();
        
        // Remove all test data in the correct order to avoid FK constraints
        context.OrderItems.RemoveRange(context.OrderItems);
        context.Orders.RemoveRange(context.Orders);
        context.Users.RemoveRange(context.Users);
        
        await context.SaveChangesAsync();
    }
}
