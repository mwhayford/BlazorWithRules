using BlazorApp.Core.Entities;
using BlazorApp.Core.Interfaces;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for LoanApplication entity
/// </summary>
public class LoanApplicationRepository : Repository<LoanApplication>, ILoanApplicationRepository
{
    public LoanApplicationRepository(ApplicationDbContext context)
        : base(context) { }

    public async Task<LoanApplication?> GetByApplicationNumberAsync(string applicationNumber)
    {
        return await _context.Set<LoanApplication>().FirstOrDefaultAsync(x => x.ApplicationNumber == applicationNumber);
    }

    public async Task<IEnumerable<LoanApplication>> GetByStatusAsync(string status)
    {
        return await _context
            .Set<LoanApplication>()
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanApplication>> GetByEmailAsync(string email)
    {
        return await _context
            .Set<LoanApplication>()
            .Where(x => x.Email.ToLower() == email.ToLower())
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanApplication>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context
            .Set<LoanApplication>()
            .Where(x => x.CreatedAt >= startDate && x.CreatedAt <= endDate)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<LoanApplication> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _context.Set<LoanApplication>().OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<LoanApplication>> SearchAsync(
        string? searchTerm = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null
    )
    {
        var query = _context.Set<LoanApplication>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(x =>
                x.FirstName.ToLower().Contains(lowerSearchTerm)
                || x.LastName.ToLower().Contains(lowerSearchTerm)
                || x.Email.ToLower().Contains(lowerSearchTerm)
                || x.ApplicationNumber.ToLower().Contains(lowerSearchTerm)
                || x.PhoneNumber.Contains(searchTerm)
            );
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= endDate.Value);
        }

        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
    }

    public async Task<Dictionary<string, object>> GetStatisticsAsync()
    {
        var applications = await _context.Set<LoanApplication>().ToListAsync();

        var stats = new Dictionary<string, object>
        {
            ["TotalApplications"] = applications.Count,
            ["DraftApplications"] = applications.Count(x => x.Status == "Draft"),
            ["SubmittedApplications"] = applications.Count(x => x.Status == "Submitted"),
            ["UnderReviewApplications"] = applications.Count(x => x.Status == "Under Review"),
            ["ApprovedApplications"] = applications.Count(x => x.Status == "Approved"),
            ["RejectedApplications"] = applications.Count(x => x.Status == "Rejected"),
            ["CancelledApplications"] = applications.Count(x => x.Status == "Cancelled"),
            ["TotalRequestedAmount"] = applications.Sum(x => x.RequestedAmount),
            ["AverageRequestedAmount"] = applications.Any() ? applications.Average(x => x.RequestedAmount) : 0,
            ["ApplicationsThisMonth"] = applications.Count(x => x.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
            ["ApplicationsThisWeek"] = applications.Count(x => x.CreatedAt >= DateTime.UtcNow.AddDays(-7)),
            ["ApplicationsToday"] = applications.Count(x => x.CreatedAt >= DateTime.UtcNow.Date),
        };

        return stats;
    }

    public async Task<string> GenerateApplicationNumberAsync()
    {
        var prefix = "LA";
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month.ToString("D2");
        // Get the last application number for this month
        var lastApplication = await _context
            .Set<LoanApplication>()
            .Where(x => x.ApplicationNumber.StartsWith($"{prefix}{year}{month}"))
            .OrderByDescending(x => x.ApplicationNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastApplication != null)
        {
            var lastNumberPart = lastApplication.ApplicationNumber.Substring(8); // After LA202412
            if (int.TryParse(lastNumberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{prefix}{year}{month}{nextNumber:D4}";
    }
}
