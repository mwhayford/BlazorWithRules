using BlazorApp.Core.Entities;

namespace BlazorApp.Core.Interfaces;

/// <summary>
/// Repository interface for LoanApplication entity
/// </summary>
public interface ILoanApplicationRepository : IRepository<LoanApplication>
{
    /// <summary>
    /// Get loan application by application number
    /// </summary>
    /// <param name="applicationNumber">Application number</param>
    /// <returns>Loan application or null if not found</returns>
    Task<LoanApplication?> GetByApplicationNumberAsync(string applicationNumber);

    /// <summary>
    /// Get loan applications by status
    /// </summary>
    /// <param name="status">Application status</param>
    /// <returns>List of loan applications</returns>
    Task<IEnumerable<LoanApplication>> GetByStatusAsync(string status);

    /// <summary>
    /// Get loan applications by email
    /// </summary>
    /// <param name="email">Applicant's email</param>
    /// <returns>List of loan applications</returns>
    Task<IEnumerable<LoanApplication>> GetByEmailAsync(string email);

    /// <summary>
    /// Get loan applications created within a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of loan applications</returns>
    Task<IEnumerable<LoanApplication>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get loan applications with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of loan applications</returns>
    Task<(IEnumerable<LoanApplication> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Search loan applications by various criteria
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of matching loan applications</returns>
    Task<IEnumerable<LoanApplication>> SearchAsync(
        string? searchTerm = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null
    );

    /// <summary>
    /// Get loan application statistics
    /// </summary>
    /// <returns>Dictionary containing various statistics</returns>
    Task<Dictionary<string, object>> GetStatisticsAsync();

    /// <summary>
    /// Generate a unique application number
    /// </summary>
    /// <returns>Unique application number</returns>
    Task<string> GenerateApplicationNumberAsync();
}
