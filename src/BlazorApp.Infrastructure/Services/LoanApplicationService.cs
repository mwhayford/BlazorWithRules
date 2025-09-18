using System.Text.Json;
using BlazorApp.Core.Entities;
using BlazorApp.Core.Interfaces;
using BlazorApp.Core.Services;
using BlazorApp.Core.Validators;
using BlazorApp.Shared.Exceptions;
using BlazorApp.Shared.Models;
using Microsoft.Extensions.Logging;

namespace BlazorApp.Infrastructure.Services;

/// <summary>
/// Service implementation for loan application business logic
/// </summary>
public class LoanApplicationService : ILoanApplicationService
{
    private readonly ILoanApplicationRepository _loanApplicationRepository;
    private readonly ICacheService _cacheService;
    private readonly LoanApplicationValidatorWrapper _validator;
    private readonly CreateLoanApplicationValidatorWrapper _createValidator;
    private readonly UpdateLoanApplicationStatusValidatorWrapper _updateValidator;
    private readonly ILogger<LoanApplicationService> _logger;

    private const string LoanApplicationCacheKeyPrefix = "loan_application";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    public LoanApplicationService(
        ILoanApplicationRepository loanApplicationRepository,
        ICacheService cacheService,
        LoanApplicationValidatorWrapper validator,
        CreateLoanApplicationValidatorWrapper createValidator,
        UpdateLoanApplicationStatusValidatorWrapper updateValidator,
        ILogger<LoanApplicationService> logger
    )
    {
        _loanApplicationRepository = loanApplicationRepository;
        _cacheService = cacheService;
        _validator = validator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<LoanApplicationDto?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Getting loan application with ID {ApplicationId}", id);

        var cacheKey = $"{LoanApplicationCacheKeyPrefix}_{id}";

        var application = await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var app = await _loanApplicationRepository.GetByIdAsync(id);
                if (app != null)
                {
                    _logger.LogInformation("Retrieved loan application {ApplicationId} from database", id);
                }
                else
                {
                    _logger.LogWarning("Loan application with ID {ApplicationId} not found", id);
                }
                return app!;
            },
            CacheExpiration
        );

        return application != null ? MapToDto(application) : null;
    }

    public async Task<LoanApplicationDto?> GetByApplicationNumberAsync(string applicationNumber)
    {
        _logger.LogInformation("Getting loan application with number {ApplicationNumber}", applicationNumber);

        var application = await _loanApplicationRepository.GetByApplicationNumberAsync(applicationNumber);
        return application != null ? MapToDto(application) : null;
    }

    public async Task<IEnumerable<LoanApplicationDto>> GetAllAsync()
    {
        _logger.LogInformation("Getting all loan applications");

        try
        {
            var applications = await _loanApplicationRepository.GetAllAsync();
            _logger.LogInformation("Retrieved {Count} applications from repository", applications.Count());
            var dtos = applications.Select(MapToDto);
            _logger.LogInformation("Mapped {Count} applications to DTOs", dtos.Count());
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all loan applications");
            throw;
        }
    }

    public async Task<IEnumerable<LoanApplicationDto>> GetByStatusAsync(string status)
    {
        _logger.LogInformation("Getting loan applications with status {Status}", status);

        var applications = await _loanApplicationRepository.GetByStatusAsync(status);
        return applications.Select(MapToDto);
    }

    public async Task<(IEnumerable<LoanApplicationDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize
    )
    {
        _logger.LogInformation(
            "Getting loan applications page {PageNumber} with size {PageSize}",
            pageNumber,
            pageSize
        );

        var (items, totalCount) = await _loanApplicationRepository.GetPagedAsync(pageNumber, pageSize);
        var dtoItems = items.Select(MapToDto);
        return (dtoItems, totalCount);
    }

    public async Task<IEnumerable<LoanApplicationDto>> SearchAsync(
        string? searchTerm = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null
    )
    {
        _logger.LogInformation(
            "Searching loan applications with term: {SearchTerm}, status: {Status}",
            searchTerm,
            status
        );

        var applications = await _loanApplicationRepository.SearchAsync(searchTerm, status, startDate, endDate);
        return applications.Select(MapToDto);
    }

    public async Task<LoanApplicationDto> CreateAsync(CreateLoanApplicationDto createDto)
    {
        _logger.LogInformation("Creating new loan application for {Email}", createDto.Email);

        // Generate application number
        var applicationNumber = await _loanApplicationRepository.GenerateApplicationNumberAsync();

        // Create entity
        var application = new LoanApplication
        {
            ApplicationNumber = applicationNumber,
            Status = "Draft",
            RequestedAmount = createDto.RequestedAmount,
            TermInMonths = createDto.TermInMonths,
            LoanPurpose = createDto.LoanPurpose,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            MiddleName = createDto.MiddleName,
            DateOfBirth = createDto.DateOfBirth,
            SocialSecurityNumber = createDto.SocialSecurityNumber,
            Email = createDto.Email,
            PhoneNumber = createDto.PhoneNumber,
            StreetAddress = createDto.StreetAddress,
            City = createDto.City,
            State = createDto.State,
            ZipCode = createDto.ZipCode,
            ResidenceDurationMonths = createDto.ResidenceDurationMonths,
            HousingStatus = createDto.HousingStatus,
            EmploymentStatus = createDto.EmploymentStatus,
            EmployerName = createDto.EmployerName,
            JobTitle = createDto.JobTitle,
            EmploymentDurationMonths = createDto.EmploymentDurationMonths,
            MonthlyGrossIncome = createDto.MonthlyGrossIncome,
            AdditionalMonthlyIncome = createDto.AdditionalMonthlyIncome,
            AdditionalIncomeDescription = createDto.AdditionalIncomeDescription,
            MonthlyHousingPayment = createDto.MonthlyHousingPayment,
            OtherMonthlyDebtPayments = createDto.OtherMonthlyDebtPayments,
            TilaAcknowledged = createDto.TilaAcknowledged,
        };

        // Calculate loan terms
        var loanTerms = await CalculateLoanTermsAsync(createDto.RequestedAmount, createDto.TermInMonths);
        application.AnnualPercentageRate = loanTerms.AnnualPercentageRate;
        application.FinanceCharge = loanTerms.FinanceCharge;
        application.TotalAmountToBePaid = loanTerms.TotalAmountToBePaid;
        application.MonthlyPaymentAmount = loanTerms.MonthlyPaymentAmount;

        if (createDto.TilaAcknowledged)
        {
            application.TilaAcknowledgedDate = DateTime.UtcNow;
        }

        // Validate
        _logger.LogInformation(
            "Validating loan application with TilaAcknowledged: {TilaAcknowledged}, TilaAcknowledgedDate: {TilaAcknowledgedDate}",
            application.TilaAcknowledged,
            application.TilaAcknowledgedDate
        );

        var validationResult = await _createValidator.ValidateAsync(application);
        if (!validationResult.IsValid)
        {
            var errors = validationResult
                .Errors.GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray().AsEnumerable());

            _logger.LogWarning(
                "Loan application validation failed for {Email}: {ValidationErrors}",
                createDto.Email,
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
            );

            throw new ValidationException(errors);
        }

        // Create the application
        var createdApplication = await _loanApplicationRepository.AddAsync(application);

        // Save changes to persist to database
        await _loanApplicationRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Successfully created loan application {ApplicationId} with number {ApplicationNumber}",
            createdApplication.Id,
            createdApplication.ApplicationNumber
        );

        // Invalidate cache
        await InvalidateApplicationCacheAsync();

        return MapToDto(createdApplication);
    }

    public async Task<LoanApplicationDto> UpdateStatusAsync(int id, UpdateLoanApplicationStatusDto updateDto)
    {
        _logger.LogInformation("Updating loan application {ApplicationId} status to {Status}", id, updateDto.Status);

        var application = await _loanApplicationRepository.GetByIdAsync(id);
        if (application == null)
        {
            throw new InvalidOperationException($"Loan application with ID {id} not found");
        }

        application.Status = updateDto.Status;
        application.Notes = updateDto.Notes;

        // Validate
        var validationResult = await _updateValidator.ValidateAsync(application);
        if (!validationResult.IsValid)
        {
            var errors = validationResult
                .Errors.GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray().AsEnumerable());

            throw new ValidationException(errors);
        }

        var updatedApplication = await _loanApplicationRepository.UpdateAsync(application);

        // Save changes to persist to database
        await _loanApplicationRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Successfully updated loan application {ApplicationId} status to {Status}",
            updatedApplication.Id,
            updatedApplication.Status
        );

        // Invalidate cache
        await InvalidateApplicationCacheAsync();

        return MapToDto(updatedApplication);
    }

    public Task<LoanApplicationDto> SaveStepAsync(LoanApplicationStepDto stepDto)
    {
        _logger.LogInformation("Saving loan application step {Step}", stepDto.Step);

        // This would typically involve parsing the step data and updating the appropriate fields
        // For now, we'll implement a basic version that handles the step data as JSON
        var stepData = JsonSerializer.Deserialize<Dictionary<string, object>>(stepDto.StepData);
        // Implementation would depend on the specific step data structure
        // This is a placeholder for the actual step handling logic
        throw new NotImplementedException("Step saving logic needs to be implemented based on specific requirements");
    }

    public async Task<LoanApplicationDto> SubmitForReviewAsync(int id)
    {
        _logger.LogInformation("Submitting loan application {ApplicationId} for review", id);

        var application = await _loanApplicationRepository.GetByIdAsync(id);
        if (application == null)
        {
            throw new InvalidOperationException($"Loan application with ID {id} not found");
        }

        if (application.Status != "Draft")
        {
            throw new InvalidOperationException($"Cannot submit application with status {application.Status}");
        }

        // Validate the complete application
        var validationResult = await _validator.ValidateAsync(application);
        if (!validationResult.IsValid)
        {
            var errors = validationResult
                .Errors.GroupBy(x => x.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray().AsEnumerable());

            throw new ValidationException(errors);
        }

        application.Status = "Submitted";
        var updatedApplication = await _loanApplicationRepository.UpdateAsync(application);

        // Save changes to persist to database
        await _loanApplicationRepository.SaveChangesAsync();

        _logger.LogInformation(
            "Successfully submitted loan application {ApplicationId} for review",
            updatedApplication.Id
        );

        // Invalidate cache
        await InvalidateApplicationCacheAsync();

        return MapToDto(updatedApplication);
    }

    public Task<LoanTermsCalculation> CalculateLoanTermsAsync(
        decimal requestedAmount,
        int termInMonths,
        int? creditScore = null
    )
    {
        _logger.LogInformation(
            "Calculating loan terms for amount {Amount} and term {Term} months",
            requestedAmount,
            termInMonths
        );

        // Simple loan calculation logic - in a real application, this would be more sophisticated
        var baseRate = 5.0m; // 5% base rate
        // Adjust rate based on credit score
        if (creditScore.HasValue)
        {
            if (creditScore >= 750)
                baseRate = 3.5m;
            else if (creditScore >= 700)
                baseRate = 4.0m;
            else if (creditScore >= 650)
                baseRate = 4.5m;
            else if (creditScore >= 600)
                baseRate = 5.5m;
            else
                baseRate = 7.0m;
        }

        var monthlyRate = baseRate / 100 / 12;
        var monthlyPayment =
            requestedAmount
            * (monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, termInMonths))
            / ((decimal)Math.Pow(1 + (double)monthlyRate, termInMonths) - 1);

        var totalAmount = (decimal)monthlyPayment * termInMonths;
        var financeCharge = totalAmount - requestedAmount;

        var isApproved = creditScore >= 600 || !creditScore.HasValue;
        var message = isApproved ? "Loan terms approved" : "Credit score too low for approval";

        return Task.FromResult(
            new LoanTermsCalculation
            {
                AnnualPercentageRate = baseRate,
                MonthlyPaymentAmount = (decimal)monthlyPayment,
                FinanceCharge = financeCharge,
                TotalAmountToBePaid = totalAmount,
                InterestRate = baseRate,
                IsApproved = isApproved,
                Message = message,
            }
        );
    }

    public async Task<Dictionary<string, object>> GetStatisticsAsync()
    {
        _logger.LogInformation("Getting loan application statistics");

        return await _loanApplicationRepository.GetStatisticsAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        _logger.LogInformation("Deleting loan application {ApplicationId}", id);

        var application = await _loanApplicationRepository.GetByIdAsync(id);
        if (application == null)
        {
            return false;
        }

        await _loanApplicationRepository.DeleteAsync(id);

        // Save changes to persist to database
        await _loanApplicationRepository.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted loan application {ApplicationId}", id);

        // Invalidate cache
        await InvalidateApplicationCacheAsync();

        return true;
    }

    private static LoanApplicationDto MapToDto(LoanApplication application)
    {
        return new LoanApplicationDto
        {
            Id = application.Id,
            ApplicationNumber = application.ApplicationNumber,
            Status = application.Status,
            RequestedAmount = application.RequestedAmount,
            TermInMonths = application.TermInMonths,
            LoanPurpose = application.LoanPurpose,
            FullName = application.FullName,
            Email = application.Email,
            PhoneNumber = application.PhoneNumber,
            FullAddress = application.FullAddress,
            TotalMonthlyIncome = application.TotalMonthlyIncome,
            DebtToIncomeRatio = application.DebtToIncomeRatio,
            AnnualPercentageRate = application.AnnualPercentageRate,
            MonthlyPaymentAmount = application.MonthlyPaymentAmount,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt,
        };
    }

    private Task InvalidateApplicationCacheAsync()
    {
        // In a real implementation, you would invalidate specific cache keys
        // For now, we'll just log the action
        _logger.LogInformation("Invalidating loan application cache");
        return Task.CompletedTask;
    }
}
