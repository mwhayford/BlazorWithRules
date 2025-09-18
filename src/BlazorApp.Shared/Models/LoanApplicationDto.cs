using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Shared.Models;

/// <summary>
/// Data transfer object for loan application responses
/// </summary>
public record LoanApplicationDto
{
    /// <summary>
    /// Application unique identifier
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Application reference number
    /// </summary>
    public string ApplicationNumber { get; init; } = string.Empty;

    /// <summary>
    /// Current status of the application
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Requested loan amount
    /// </summary>
    public decimal RequestedAmount { get; init; }

    /// <summary>
    /// Requested loan term in months
    /// </summary>
    public int TermInMonths { get; init; }

    /// <summary>
    /// Purpose of the loan
    /// </summary>
    public string LoanPurpose { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's full name
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's email address
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's phone number
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's full address
    /// </summary>
    public string FullAddress { get; init; } = string.Empty;

    /// <summary>
    /// Total monthly income
    /// </summary>
    public decimal TotalMonthlyIncome { get; init; }

    /// <summary>
    /// Debt-to-income ratio
    /// </summary>
    public decimal DebtToIncomeRatio { get; init; }

    /// <summary>
    /// Annual Percentage Rate
    /// </summary>
    public decimal AnnualPercentageRate { get; init; }

    /// <summary>
    /// Monthly payment amount
    /// </summary>
    public decimal MonthlyPaymentAmount { get; init; }

    /// <summary>
    /// Date when application was created
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Date when application was last updated
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Data transfer object for creating a new loan application
/// </summary>
public record CreateLoanApplicationDto
{
    /// <summary>
    /// Requested loan amount
    /// </summary>
    [Required(ErrorMessage = "Loan amount is required")]
    [Range(1000, 1000000, ErrorMessage = "Loan amount must be between $1,000 and $1,000,000")]
    public decimal RequestedAmount { get; init; }

    /// <summary>
    /// Requested loan term in months
    /// </summary>
    [Required(ErrorMessage = "Loan term is required")]
    [Range(12, 360, ErrorMessage = "Loan term must be between 12 and 360 months")]
    public int TermInMonths { get; init; }

    /// <summary>
    /// Purpose of the loan
    /// </summary>
    [Required(ErrorMessage = "Loan purpose is required")]
    [StringLength(100, ErrorMessage = "Loan purpose cannot exceed 100 characters")]
    public string LoanPurpose { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's first name
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's last name
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's middle name or initial
    /// </summary>
    [StringLength(50, ErrorMessage = "Middle name cannot exceed 50 characters")]
    public string? MiddleName { get; init; }

    /// <summary>
    /// Applicant's date of birth
    /// </summary>
    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; init; }

    /// <summary>
    /// Applicant's Social Security Number
    /// </summary>
    [Required(ErrorMessage = "Social Security Number is required")]
    [StringLength(20, ErrorMessage = "Social Security Number cannot exceed 20 characters")]
    public string SocialSecurityNumber { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's email address
    /// </summary>
    [Required(ErrorMessage = "Email address is required")]
    [StringLength(256, ErrorMessage = "Email address cannot exceed 256 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's phone number
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's street address
    /// </summary>
    [Required(ErrorMessage = "Street address is required")]
    [StringLength(200, ErrorMessage = "Street address cannot exceed 200 characters")]
    public string StreetAddress { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's city
    /// </summary>
    [Required(ErrorMessage = "City is required")]
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's state
    /// </summary>
    [Required(ErrorMessage = "State is required")]
    [StringLength(50, ErrorMessage = "State cannot exceed 50 characters")]
    public string State { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's ZIP code
    /// </summary>
    [Required(ErrorMessage = "ZIP code is required")]
    [StringLength(10, ErrorMessage = "ZIP code cannot exceed 10 characters")]
    public string ZipCode { get; init; } = string.Empty;

    /// <summary>
    /// How long applicant has lived at current address (in months)
    /// </summary>
    [Required(ErrorMessage = "Residence duration is required")]
    [Range(0, 1200, ErrorMessage = "Residence duration must be between 0 and 100 years")]
    public int ResidenceDurationMonths { get; init; }

    /// <summary>
    /// Applicant's housing status
    /// </summary>
    [Required(ErrorMessage = "Housing status is required")]
    [StringLength(20, ErrorMessage = "Housing status cannot exceed 20 characters")]
    public string HousingStatus { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's employment status
    /// </summary>
    [Required(ErrorMessage = "Employment status is required")]
    [StringLength(50, ErrorMessage = "Employment status cannot exceed 50 characters")]
    public string EmploymentStatus { get; init; } = string.Empty;

    /// <summary>
    /// Applicant's employer name
    /// </summary>
    [StringLength(200, ErrorMessage = "Employer name cannot exceed 200 characters")]
    public string? EmployerName { get; init; }

    /// <summary>
    /// Applicant's job title
    /// </summary>
    [StringLength(100, ErrorMessage = "Job title cannot exceed 100 characters")]
    public string? JobTitle { get; init; }

    /// <summary>
    /// How long applicant has been employed (in months)
    /// </summary>
    [Range(0, 1200, ErrorMessage = "Employment duration must be between 0 and 100 years")]
    public int? EmploymentDurationMonths { get; init; }

    /// <summary>
    /// Applicant's monthly gross income
    /// </summary>
    [Required(ErrorMessage = "Monthly gross income is required")]
    [Range(0, 100000, ErrorMessage = "Monthly income must be between $0 and $100,000")]
    public decimal MonthlyGrossIncome { get; init; }

    /// <summary>
    /// Additional monthly income sources
    /// </summary>
    [Range(0, 50000, ErrorMessage = "Additional income must be between $0 and $50,000")]
    public decimal AdditionalMonthlyIncome { get; init; }

    /// <summary>
    /// Description of additional income sources
    /// </summary>
    [StringLength(500, ErrorMessage = "Additional income description cannot exceed 500 characters")]
    public string? AdditionalIncomeDescription { get; init; }

    /// <summary>
    /// Monthly housing payment
    /// </summary>
    [Required(ErrorMessage = "Monthly housing payment is required")]
    [Range(0, 10000, ErrorMessage = "Housing payment must be between $0 and $10,000")]
    public decimal MonthlyHousingPayment { get; init; }

    /// <summary>
    /// Other monthly debt payments
    /// </summary>
    [Range(0, 50000, ErrorMessage = "Other debt payments must be between $0 and $50,000")]
    public decimal OtherMonthlyDebtPayments { get; init; }

    /// <summary>
    /// Whether applicant has acknowledged TILA disclosure
    /// </summary>
    [Required(ErrorMessage = "TILA acknowledgment is required")]
    public bool TilaAcknowledged { get; init; }
}

/// <summary>
/// Data transfer object for updating loan application status
/// </summary>
public record UpdateLoanApplicationStatusDto
{
    /// <summary>
    /// New status for the application
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; init; }
}

/// <summary>
/// Data transfer object for loan application step data
/// </summary>
public record LoanApplicationStepDto
{
    /// <summary>
    /// Current step number (1, 2, or 3)
    /// </summary>
    [Required]
    [Range(1, 3, ErrorMessage = "Step must be between 1 and 3")]
    public int Step { get; init; }

    /// <summary>
    /// Step-specific data as JSON string
    /// </summary>
    [Required]
    public string StepData { get; init; } = string.Empty;

    /// <summary>
    /// Whether this step is complete
    /// </summary>
    public bool IsComplete { get; init; }
}
