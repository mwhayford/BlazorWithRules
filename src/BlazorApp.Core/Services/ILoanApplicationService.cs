using BlazorApp.Core.Entities;
using BlazorApp.Shared.Models;

namespace BlazorApp.Core.Services;

/// <summary>
/// Service interface for loan application business logic
/// </summary>
public interface ILoanApplicationService
{
    /// <summary>
    /// Get loan application by ID
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <returns>Loan application DTO or null if not found</returns>
    Task<LoanApplicationDto?> GetByIdAsync(int id);

    /// <summary>
    /// Get loan application by application number
    /// </summary>
    /// <param name="applicationNumber">Application number</param>
    /// <returns>Loan application DTO or null if not found</returns>
    Task<LoanApplicationDto?> GetByApplicationNumberAsync(string applicationNumber);

    /// <summary>
    /// Get all loan applications
    /// </summary>
    /// <returns>List of loan application DTOs</returns>
    Task<IEnumerable<LoanApplicationDto>> GetAllAsync();

    /// <summary>
    /// Get loan applications by status
    /// </summary>
    /// <param name="status">Application status</param>
    /// <returns>List of loan application DTOs</returns>
    Task<IEnumerable<LoanApplicationDto>> GetByStatusAsync(string status);

    /// <summary>
    /// Get loan applications with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of loan application DTOs</returns>
    Task<(IEnumerable<LoanApplicationDto> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Search loan applications
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <returns>List of matching loan application DTOs</returns>
    Task<IEnumerable<LoanApplicationDto>> SearchAsync(
        string? searchTerm = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null
    );

    /// <summary>
    /// Create a new loan application
    /// </summary>
    /// <param name="createDto">Loan application creation data</param>
    /// <returns>Created loan application DTO</returns>
    Task<LoanApplicationDto> CreateAsync(CreateLoanApplicationDto createDto);

    /// <summary>
    /// Update loan application status
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <param name="updateDto">Status update data</param>
    /// <returns>Updated loan application DTO</returns>
    Task<LoanApplicationDto> UpdateStatusAsync(int id, UpdateLoanApplicationStatusDto updateDto);

    /// <summary>
    /// Save loan application step data
    /// </summary>
    /// <param name="stepDto">Step data</param>
    /// <returns>Updated loan application DTO</returns>
    Task<LoanApplicationDto> SaveStepAsync(LoanApplicationStepDto stepDto);

    /// <summary>
    /// Submit loan application for review
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <returns>Updated loan application DTO</returns>
    Task<LoanApplicationDto> SubmitForReviewAsync(int id);

    /// <summary>
    /// Calculate loan terms and TILA information
    /// </summary>
    /// <param name="requestedAmount">Requested loan amount</param>
    /// <param name="termInMonths">Loan term in months</param>
    /// <param name="creditScore">Applicant's credit score (optional)</param>
    /// <returns>Calculated loan terms</returns>
    Task<LoanTermsCalculation> CalculateLoanTermsAsync(
        decimal requestedAmount,
        int termInMonths,
        int? creditScore = null
    );

    /// <summary>
    /// Get loan application statistics
    /// </summary>
    /// <returns>Dictionary containing various statistics</returns>
    Task<Dictionary<string, object>> GetStatisticsAsync();

    /// <summary>
    /// Delete loan application (soft delete)
    /// </summary>
    /// <param name="id">Application ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// Loan terms calculation result
/// </summary>
public record LoanTermsCalculation
{
    /// <summary>
    /// Annual Percentage Rate
    /// </summary>
    public decimal AnnualPercentageRate { get; init; }

    /// <summary>
    /// Monthly payment amount
    /// </summary>
    public decimal MonthlyPaymentAmount { get; init; }

    /// <summary>
    /// Total finance charge
    /// </summary>
    public decimal FinanceCharge { get; init; }

    /// <summary>
    /// Total amount to be paid
    /// </summary>
    public decimal TotalAmountToBePaid { get; init; }

    /// <summary>
    /// Interest rate used for calculation
    /// </summary>
    public decimal InterestRate { get; init; }

    /// <summary>
    /// Whether the loan terms are approved
    /// </summary>
    public bool IsApproved { get; init; }

    /// <summary>
    /// Approval message or rejection reason
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
