using BlazorApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext
    /// </summary>
    /// <param name="options">Database context options</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    /// <summary>
    /// Users DbSet
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Orders DbSet
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// Order Items DbSet
    /// </summary>
    public DbSet<OrderItem> OrderItems { get; set; }

    /// <summary>
    /// Configure the model and relationships
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Role).HasDefaultValue("User");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Configure relationship with Orders
            entity
                .HasMany(e => e.Orders)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Configure relationship with User
            entity
                .HasOne(e => e.User)
                .WithMany(e => e.Orders)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship with OrderItems
            entity
                .HasMany(e => e.OrderItems)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Configure relationship with Order
            entity
                .HasOne(e => e.Order)
                .WithMany(e => e.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Apply configurations from assemblies
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    /// <summary>
    /// Override SaveChangesAsync to implement audit fields
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected records</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddAuditInformation();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Override SaveChanges to implement audit fields
    /// </summary>
    /// <returns>Number of affected records</returns>
    public override int SaveChanges()
    {
        AddAuditInformation();
        return base.SaveChanges();
    }

    /// <summary>
    /// Add audit information to entities
    /// </summary>
    private void AddAuditInformation()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var currentTime = DateTime.UtcNow;
        var currentUser = "System"; // TODO: Get from current user context

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = currentTime;
                    entry.Entity.CreatedBy = currentUser;
                    entry.Entity.UpdatedAt = currentTime;
                    entry.Entity.UpdatedBy = currentUser;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = currentTime;
                    entry.Entity.UpdatedBy = currentUser;

                    // If this is a soft delete
                    if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                    {
                        entry.Entity.DeletedAt = currentTime;
                        entry.Entity.DeletedBy = currentUser;
                    }
                    break;
            }
        }
    }
}
