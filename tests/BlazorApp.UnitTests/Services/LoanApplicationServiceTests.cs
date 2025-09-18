using AutoFixture;
using BlazorApp.Core.Entities;
using BlazorApp.Core.Interfaces;
using BlazorApp.Core.Services;
using BlazorApp.Infrastructure.Services;
using BlazorApp.Shared.Exceptions;
using BlazorApp.Shared.Models;
using BlazorApp.UnitTests.Common;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlazorApp.UnitTests.Services;

/// <summary>
/// Unit tests for LoanApplicationService
/// </summary>
public class LoanApplicationServiceTests : TestBase
{
    private readonly Mock<ILoanApplicationRepository> _mockRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<LoanApplicationValidatorWrapper> _mockValidator;
    private readonly Mock<CreateLoanApplicationValidatorWrapper> _mockCreateValidator;
    private readonly Mock<UpdateLoanApplicationStatusValidatorWrapper> _mockUpdateValidator;
    private readonly Mock<ILogger<LoanApplicationService>> _mockLogger;
    private readonly LoanApplicationService _service;

    public LoanApplicationServiceTests()
    {
        _mockRepository = new Mock<ILoanApplicationRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockValidator = new Mock<LoanApplicationValidatorWrapper>();
        _mockCreateValidator = new Mock<CreateLoanApplicationValidatorWrapper>();
        _mockUpdateValidator = new Mock<UpdateLoanApplicationStatusValidatorWrapper>();
        _mockLogger = CreateMockLogger<LoanApplicationService>();

        _service = new LoanApplicationService(
            _mockRepository.Object,
            _mockCacheService.Object,
            _mockValidator.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetByIdAsync_WhenApplicationExists_ReturnsApplicationDto()
    {
        // Arrange
        var applicationId = 1;
        var application = Fixture.Create<LoanApplication>();
        application.Id = applicationId;

        _mockCacheService
            .Setup(x =>
                x.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<LoanApplication>>>(), It.IsAny<TimeSpan>())
            )
            .ReturnsAsync(application);

        // Act
        var result = await _service.GetByIdAsync(applicationId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(applicationId);
        result.ApplicationNumber.Should().Be(application.ApplicationNumber);
        result.Status.Should().Be(application.Status);

        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task GetByIdAsync_WhenApplicationDoesNotExist_ReturnsNull()
    {
        // Arrange
        var applicationId = 999;

        _mockCacheService
            .Setup(x =>
                x.GetOrSetAsync(It.IsAny<string>(), It.IsAny<Func<Task<LoanApplication>>>(), It.IsAny<TimeSpan>())
            )
            .ReturnsAsync((LoanApplication)null!);

        // Act
        var result = await _service.GetByIdAsync(applicationId);

        // Assert
        result.Should().BeNull();
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task GetByApplicationNumberAsync_WhenApplicationExists_ReturnsApplicationDto()
    {
        // Arrange
        var applicationNumber = "LA2025090001";
        var application = Fixture.Create<LoanApplication>();
        application.ApplicationNumber = applicationNumber;

        _mockRepository.Setup(x => x.GetByApplicationNumberAsync(applicationNumber)).ReturnsAsync(application);

        // Act
        var result = await _service.GetByApplicationNumberAsync(applicationNumber);

        // Assert
        result.Should().NotBeNull();
        result!.ApplicationNumber.Should().Be(applicationNumber);
        result.Id.Should().Be(application.Id);

        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task GetByApplicationNumberAsync_WhenApplicationDoesNotExist_ReturnsNull()
    {
        // Arrange
        var applicationNumber = "INVALID";

        _mockRepository
            .Setup(x => x.GetByApplicationNumberAsync(applicationNumber))
            .ReturnsAsync((LoanApplication)null!);

        // Act
        var result = await _service.GetByApplicationNumberAsync(applicationNumber);

        // Assert
        result.Should().BeNull();
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllApplications()
    {
        // Arrange
        var applications = Fixture.CreateMany<LoanApplication>(3).ToList();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(applications);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(dto => dto.Should().NotBeNull());

        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task GetAllAsync_WhenNoApplications_ReturnsEmptyCollection()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LoanApplication>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsApplicationsWithSpecifiedStatus()
    {
        // Arrange
        var status = "Submitted";
        var applications = Fixture.CreateMany<LoanApplication>(2).ToList();
        applications.ForEach(a => a.Status = status);

        _mockRepository.Setup(x => x.GetByStatusAsync(status)).ReturnsAsync(applications);

        // Act
        var result = await _service.GetByStatusAsync(status);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.Status.Should().Be(status));

        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPagedResults()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var applications = Fixture.CreateMany<LoanApplication>(5).ToList();
        var totalCount = 25;

        _mockRepository.Setup(x => x.GetPagedAsync(pageNumber, pageSize)).ReturnsAsync((applications, totalCount));

        // Act
        var result = await _service.GetPagedAsync(pageNumber, pageSize);

        // Assert
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(totalCount);

        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task SearchAsync_WithSearchTerm_ReturnsMatchingApplications()
    {
        // Arrange
        var searchTerm = "John";
        var applications = Fixture.CreateMany<LoanApplication>(2).ToList();

        _mockRepository
            .Setup(x => x.SearchAsync(searchTerm, It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(applications);

        // Act
        var result = await _service.SearchAsync(searchTerm);

        // Assert
        result.Should().HaveCount(2);
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task SearchAsync_WithAllParameters_ReturnsFilteredApplications()
    {
        // Arrange
        var searchTerm = "John";
        var status = "Submitted";
        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;
        var applications = Fixture.CreateMany<LoanApplication>(1).ToList();

        _mockRepository
            .Setup(x =>
                x.SearchAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>())
            )
            .ReturnsAsync(applications);

        // Act
        var result = await _service.SearchAsync(searchTerm, status, startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesAndReturnsApplication()
    {
        // Arrange
        var createDto = Fixture.Create<CreateLoanApplicationDto>();
        var applicationNumber = "LA2025090001";
        var createdApplication = Fixture.Create<LoanApplication>();
        createdApplication.ApplicationNumber = applicationNumber;
        createdApplication.Id = 1;

        _mockRepository.Setup(x => x.GenerateApplicationNumberAsync()).ReturnsAsync(applicationNumber);

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdApplication);

        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockCreateValidator
            .Setup(x => x.ValidateAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.ApplicationNumber.Should().Be(applicationNumber);
        result.Id.Should().Be(1);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.AtLeast(2));
    }

    [Fact]
    public async Task CreateAsync_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var createDto = Fixture.Create<CreateLoanApplicationDto>();
        var validationErrors = new List<ValidationFailure> { new("Property", "Error message") };
        var validationResult = new ValidationResult(validationErrors);

        _mockRepository.Setup(x => x.GenerateApplicationNumberAsync()).ReturnsAsync("LA2025090001");

        _mockCreateValidator
            .Setup(x => x.ValidateAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        ValidationException exception = await Assert.ThrowsAsync<BlazorApp.Shared.Exceptions.ValidationException>(() =>
            _service.CreateAsync(createDto)
        );
        exception.Errors.Should().ContainKey("Property");
        exception.Errors["Property"].Should().Contain("Error message");

        VerifyLogCalled(_mockLogger, LogLevel.Warning, Times.Once());
    }

    [Fact]
    public async Task CreateAsync_SetsTilaAcknowledgedDate_WhenTilaAcknowledgedIsTrue()
    {
        // Arrange
        var createDto = Fixture.Create<CreateLoanApplicationDto>();
        createDto = createDto with { TilaAcknowledged = true };

        _mockRepository.Setup(x => x.GenerateApplicationNumberAsync()).ReturnsAsync("LA2025090001");

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoanApplication application, CancellationToken ct) => application);

        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockCreateValidator
            .Setup(x => x.ValidateAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        await _service.CreateAsync(createDto);

        // Assert
        _mockRepository.Verify(
            x =>
                x.AddAsync(
                    It.Is<LoanApplication>(a => a.TilaAcknowledged == true && a.TilaAcknowledgedDate.HasValue),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateStatusAsync_WithValidData_UpdatesAndReturnsApplication()
    {
        // Arrange
        var applicationId = 1;
        var updateDto = Fixture.Create<UpdateLoanApplicationStatusDto>();
        var existingApplication = Fixture.Create<LoanApplication>();
        existingApplication.Id = applicationId;

        _mockRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingApplication);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingApplication);

        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockUpdateValidator
            .Setup(x => x.ValidateAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _service.UpdateStatusAsync(applicationId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(applicationId);

        _mockRepository.Verify(
            x => x.UpdateAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.AtLeast(2));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenApplicationNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var applicationId = 999;
        var updateDto = Fixture.Create<UpdateLoanApplicationStatusDto>();

        _mockRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoanApplication)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateStatusAsync(applicationId, updateDto)
        );

        exception.Message.Should().Contain($"Loan application with ID {applicationId} not found");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var applicationId = 1;
        var updateDto = Fixture.Create<UpdateLoanApplicationStatusDto>();
        var existingApplication = Fixture.Create<LoanApplication>();
        existingApplication.Id = applicationId;

        var validationErrors = new List<ValidationFailure> { new("Status", "Invalid status") };
        var validationResult = new ValidationResult(validationErrors);

        _mockRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingApplication);

        _mockUpdateValidator
            .Setup(x => x.ValidateAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BlazorApp.Shared.Exceptions.ValidationException>(() =>
            _service.UpdateStatusAsync(applicationId, updateDto)
        );

        exception.Errors.Should().ContainKey("Status");
    }

    [Fact]
    public async Task SubmitForReviewAsync_WithDraftApplication_SubmitsSuccessfully()
    {
        // Arrange
        var applicationId = 1;
        var existingApplication = Fixture.Create<LoanApplication>();
        existingApplication.Id = applicationId;
        existingApplication.Status = "Draft";

        _mockRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingApplication);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingApplication);

        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _service.SubmitForReviewAsync(applicationId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(applicationId);

        _mockRepository.Verify(
            x => x.UpdateAsync(It.Is<LoanApplication>(a => a.Status == "Submitted"), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.AtLeast(2));
    }

    [Fact]
    public async Task SubmitForReviewAsync_WhenApplicationNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var applicationId = 999;

        _mockRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoanApplication)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SubmitForReviewAsync(applicationId)
        );

        exception.Message.Should().Contain($"Loan application with ID {applicationId} not found");
    }

    [Fact]
    public async Task SubmitForReviewAsync_WhenNotDraftStatus_ThrowsInvalidOperationException()
    {
        // Arrange
        var applicationId = 1;
        var existingApplication = Fixture.Create<LoanApplication>();
        existingApplication.Id = applicationId;
        existingApplication.Status = "Submitted";

        _mockRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingApplication);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SubmitForReviewAsync(applicationId)
        );

        exception.Message.Should().Contain($"Cannot submit application with status {existingApplication.Status}");
    }

    [Fact]
    public async Task SubmitForReviewAsync_WhenValidationFails_ThrowsValidationException()
    {
        // Arrange
        var applicationId = 1;
        var existingApplication = Fixture.Create<LoanApplication>();
        existingApplication.Id = applicationId;
        existingApplication.Status = "Draft";

        var validationErrors = new List<ValidationFailure> { new("TilaAcknowledged", "TILA must be acknowledged") };
        var validationResult = new ValidationResult(validationErrors);

        _mockRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingApplication);

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<LoanApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BlazorApp.Shared.Exceptions.ValidationException>(() =>
            _service.SubmitForReviewAsync(applicationId)
        );

        exception.Errors.Should().ContainKey("TilaAcknowledged");
    }

    [Theory]
    [InlineData(25000, 60, null, 5.0)]
    [InlineData(25000, 60, 750, 3.5)]
    [InlineData(25000, 60, 700, 4.0)]
    [InlineData(25000, 60, 650, 4.5)]
    [InlineData(25000, 60, 600, 5.5)]
    [InlineData(25000, 60, 500, 7.0)]
    public async Task CalculateLoanTermsAsync_WithDifferentCreditScores_ReturnsCorrectRate(
        decimal requestedAmount,
        int termInMonths,
        int? creditScore,
        decimal expectedRate
    )
    {
        // Act
        var result = await _service.CalculateLoanTermsAsync(requestedAmount, termInMonths, creditScore);

        // Assert
        result.Should().NotBeNull();
        result.AnnualPercentageRate.Should().Be(expectedRate);
        result.MonthlyPaymentAmount.Should().BeGreaterThan(0);
        result.FinanceCharge.Should().BeGreaterThan(0);
        result.TotalAmountToBePaid.Should().BeGreaterThan(requestedAmount);

        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task CalculateLoanTermsAsync_WithHighCreditScore_ApprovesLoan()
    {
        // Arrange
        var requestedAmount = 25000m;
        var termInMonths = 60;
        var creditScore = 750;

        // Act
        var result = await _service.CalculateLoanTermsAsync(requestedAmount, termInMonths, creditScore);

        // Assert
        result.IsApproved.Should().BeTrue();
        result.Message.Should().Be("Loan terms approved");
    }

    [Fact]
    public async Task CalculateLoanTermsAsync_WithLowCreditScore_RejectsLoan()
    {
        // Arrange
        var requestedAmount = 25000m;
        var termInMonths = 60;
        var creditScore = 500;

        // Act
        var result = await _service.CalculateLoanTermsAsync(requestedAmount, termInMonths, creditScore);

        // Assert
        result.IsApproved.Should().BeFalse();
        result.Message.Should().Be("Credit score too low for approval");
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsStatisticsFromRepository()
    {
        // Arrange
        var expectedStats = new Dictionary<string, object>
        {
            ["TotalApplications"] = 10,
            ["ApprovedApplications"] = 7,
            ["PendingApplications"] = 3,
        };

        _mockRepository.Setup(x => x.GetStatisticsAsync()).ReturnsAsync(expectedStats);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedStats);
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task DeleteAsync_WhenApplicationExists_DeletesSuccessfully()
    {
        // Arrange
        var applicationId = 1;
        var existingApplication = Fixture.Create<LoanApplication>();
        existingApplication.Id = applicationId;

        _mockRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingApplication);

        _mockRepository.Setup(x => x.DeleteAsync(applicationId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.DeleteAsync(applicationId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.DeleteAsync(applicationId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.AtLeast(2));
    }

    [Fact]
    public async Task DeleteAsync_WhenApplicationDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var applicationId = 999;

        _mockRepository
            .Setup(x => x.GetByIdAsync(applicationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoanApplication)null!);

        // Act
        var result = await _service.DeleteAsync(applicationId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }

    [Fact]
    public async Task SaveStepAsync_ThrowsNotImplementedException()
    {
        // Arrange
        var stepDto = new LoanApplicationStepDto { Step = 1, StepData = "{\"test\": \"data\"}" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotImplementedException>(() => _service.SaveStepAsync(stepDto));

        exception.Message.Should().Contain("Step saving logic needs to be implemented");
        VerifyLogCalled(_mockLogger, LogLevel.Information, Times.Once());
    }
}
