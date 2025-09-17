using BlazorApp.Core.Entities;
using BlazorApp.Core.Interfaces;
using BlazorApp.Core.Services;
using BlazorApp.Infrastructure.Data;
using BlazorApp.Infrastructure.Repositories;
using BlazorApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp.Infrastructure;

/// <summary>
/// Dependency injection configuration for Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add Infrastructure services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            // Enable sensitive data logging in development
            if (configuration.GetSection("Logging")["EnableSensitiveDataLogging"] == "true")
            {
                options.EnableSensitiveDataLogging();
            }

            // Enable detailed errors in development
            if (configuration.GetSection("Logging")["EnableDetailedErrors"] == "true")
            {
                options.EnableDetailedErrors();
            }
        });

        // Add repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();

        // Register cache service
        services.AddScoped<ICacheService, CacheService>();

        // Register services
        services.AddScoped<IUserService, UserService>();

        return services;
    }

    /// <summary>
    /// Ensure database is created and migrations are applied
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>Task</returns>
    public static async Task EnsureDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        try
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            
            // Apply any pending migrations
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            // Log error - in a real application, use proper logging
            Console.WriteLine($"An error occurred while ensuring database: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Seed initial data if database is empty
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <returns>Task</returns>
    public static async Task SeedDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Check if we already have data
        if (await context.Users.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Seed initial users
        var users = new[]
        {
            new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Role = "Administrator",
                IsActive = true,
                PhoneNumber = "+1-555-0101",
                DateOfBirth = new DateTime(1985, 5, 15)
            },
            new User
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Role = "Manager",
                IsActive = true,
                PhoneNumber = "+1-555-0102",
                DateOfBirth = new DateTime(1990, 8, 22)
            },
            new User
            {
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@example.com",
                Role = "User",
                IsActive = true,
                PhoneNumber = "+1-555-0103",
                DateOfBirth = new DateTime(1992, 12, 3)
            },
            new User
            {
                FirstName = "Alice",
                LastName = "Williams",
                Email = "alice.williams@example.com",
                Role = "User",
                IsActive = false,
                PhoneNumber = "+1-555-0104",
                DateOfBirth = new DateTime(1988, 3, 17)
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Seed some sample orders
        var johnUser = users[0];
        var janeUser = users[1];

        var orders = new[]
        {
            new Order
            {
                OrderNumber = "ORD-001",
                User = johnUser,
                OrderDate = DateTime.UtcNow.AddDays(-30),
                TotalAmount = 199.99m,
                Status = "Delivered",
                ShippingAddress = "123 Main St, Anytown, ST 12345",
                BillingAddress = "123 Main St, Anytown, ST 12345",
                ShippedAt = DateTime.UtcNow.AddDays(-25),
                DeliveredAt = DateTime.UtcNow.AddDays(-22)
            },
            new Order
            {
                OrderNumber = "ORD-002",
                User = janeUser,
                OrderDate = DateTime.UtcNow.AddDays(-15),
                TotalAmount = 89.99m,
                Status = "Shipped",
                ShippingAddress = "456 Oak Ave, Another City, ST 67890",
                BillingAddress = "456 Oak Ave, Another City, ST 67890",
                ShippedAt = DateTime.UtcNow.AddDays(-10)
            }
        };

        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        // Seed order items
        var orderItems = new[]
        {
            new OrderItem
            {
                Order = orders[0],
                ProductName = "Wireless Headphones",
                ProductSku = "WH-001",
                Quantity = 1,
                UnitPrice = 149.99m,
                TotalPrice = 149.99m,
                Category = "Electronics",
                ProductDescription = "High-quality wireless headphones with noise cancellation"
            },
            new OrderItem
            {
                Order = orders[0],
                ProductName = "Phone Case",
                ProductSku = "PC-001",
                Quantity = 2,
                UnitPrice = 25.00m,
                TotalPrice = 50.00m,
                Category = "Accessories",
                ProductDescription = "Protective phone case with screen protector"
            },
            new OrderItem
            {
                Order = orders[1],
                ProductName = "Bluetooth Speaker",
                ProductSku = "BS-001",
                Quantity = 1,
                UnitPrice = 89.99m,
                TotalPrice = 89.99m,
                Category = "Electronics",
                ProductDescription = "Portable Bluetooth speaker with excellent sound quality"
            }
        };

        context.OrderItems.AddRange(orderItems);
        await context.SaveChangesAsync();
    }
}
