using BlazorApp.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext
    /// </summary>
    /// <param name="options">Database context options</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    /// <summary>
    /// Legacy Users DbSet (for backward compatibility)
    /// </summary>
    public DbSet<User> LegacyUsers { get; set; }

    /// <summary>
    /// Orders DbSet
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    /// <summary>
    /// Order Items DbSet
    /// </summary>
    public DbSet<OrderItem> OrderItems { get; set; }

    /// <summary>
    /// Loan Applications DbSet
    /// </summary>
    public DbSet<LoanApplication> LoanApplications { get; set; }

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

        // Configure LoanApplication entity
        modelBuilder.Entity<LoanApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ApplicationNumber).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            // Basic loan information
            entity.Property(e => e.ApplicationNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Draft");
            entity.Property(e => e.RequestedAmount).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.TermInMonths).IsRequired();
            entity.Property(e => e.LoanPurpose).IsRequired().HasMaxLength(100);

            // Demographics
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MiddleName).HasMaxLength(50);
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.SocialSecurityNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.StreetAddress).IsRequired().HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ZipCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ResidenceDurationMonths).IsRequired();
            entity.Property(e => e.HousingStatus).IsRequired().HasMaxLength(20);

            // Income information
            entity.Property(e => e.EmploymentStatus).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EmployerName).HasMaxLength(200);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.MonthlyGrossIncome).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.AdditionalMonthlyIncome).HasPrecision(18, 2).HasDefaultValue(0);
            entity.Property(e => e.AdditionalIncomeDescription).HasMaxLength(500);
            entity.Property(e => e.MonthlyHousingPayment).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.OtherMonthlyDebtPayments).HasPrecision(18, 2).HasDefaultValue(0);

            // TILA information
            entity.Property(e => e.AnnualPercentageRate).IsRequired().HasPrecision(5, 2);
            entity.Property(e => e.FinanceCharge).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.TotalAmountToBePaid).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.MonthlyPaymentAmount).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.TilaAcknowledged).IsRequired();
            entity.Property(e => e.SubmissionIpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            // Global query filter for soft delete
            entity.HasQueryFilter(e => !e.IsDeleted);
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
