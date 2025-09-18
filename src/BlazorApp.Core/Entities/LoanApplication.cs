using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Core.Entities;

/// <summary>
/// Loan application entity representing a complete loan application
/// </summary>
public class LoanApplication : BaseEntity
{
    /// <summary>
    /// Application reference number (unique identifier for customer-facing reference)
    /// </summary>
    [Required]
    [StringLength(20)]
    public required string ApplicationNumber { get; set; }

    /// <summary>
    /// Current status of the loan application
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// Requested loan amount
    /// </summary>
    [Required]
    [Range(1000, 1000000, ErrorMessage = "Loan amount must be between $1,000 and $1,000,000")]
    public decimal RequestedAmount { get; set; }

    /// <summary>
    /// Requested loan term in months
    /// </summary>
    [Required]
    [Range(12, 360, ErrorMessage = "Loan term must be between 12 and 360 months")]
    public int TermInMonths { get; set; }

    /// <summary>
    /// Purpose of the loan
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string LoanPurpose { get; set; }

    // Demographics Information (Step 1)

    /// <summary>
    /// Applicant's first name
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }

    /// <summary>
    /// Applicant's last name
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string LastName { get; set; }

    /// <summary>
    /// Applicant's middle name or initial
    /// </summary>
    [StringLength(50)]
    public string? MiddleName { get; set; }

    /// <summary>
    /// Applicant's date of birth
    /// </summary>
    [Required]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Applicant's Social Security Number (encrypted)
    /// </summary>
    [Required]
    [StringLength(20)]
    public required string SocialSecurityNumber { get; set; }

    /// <summary>
    /// Applicant's email address
    /// </summary>
    [Required]
    [StringLength(256)]
    [EmailAddress]
    public required string Email { get; set; }

    /// <summary>
    /// Applicant's phone number
    /// </summary>
    [Required]
    [StringLength(20)]
    public required string PhoneNumber { get; set; }

    /// <summary>
    /// Applicant's street address
    /// </summary>
    [Required]
    [StringLength(200)]
    public required string StreetAddress { get; set; }

    /// <summary>
    /// Applicant's city
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string City { get; set; }

    /// <summary>
    /// Applicant's state
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string State { get; set; }

    /// <summary>
    /// Applicant's ZIP code
    /// </summary>
    [Required]
    [StringLength(10)]
    public required string ZipCode { get; set; }

    /// <summary>
    /// How long applicant has lived at current address (in months)
    /// </summary>
    [Required]
    [Range(0, 1200, ErrorMessage = "Residence duration must be between 0 and 100 years")]
    public int ResidenceDurationMonths { get; set; }

    /// <summary>
    /// Applicant's housing status (Own, Rent, Other)
    /// </summary>
    [Required]
    [StringLength(20)]
    public required string HousingStatus { get; set; }

    // Income Information (Step 2)

    /// <summary>
    /// Applicant's employment status
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string EmploymentStatus { get; set; }

    /// <summary>
    /// Applicant's employer name
    /// </summary>
    [StringLength(200)]
    public string? EmployerName { get; set; }

    /// <summary>
    /// Applicant's job title
    /// </summary>
    [StringLength(100)]
    public string? JobTitle { get; set; }

    /// <summary>
    /// How long applicant has been employed (in months)
    /// </summary>
    [Range(0, 1200, ErrorMessage = "Employment duration must be between 0 and 100 years")]
    public int? EmploymentDurationMonths { get; set; }

    /// <summary>
    /// Applicant's monthly gross income
    /// </summary>
    [Required]
    [Range(0, 100000, ErrorMessage = "Monthly income must be between $0 and $100,000")]
    public decimal MonthlyGrossIncome { get; set; }

    /// <summary>
    /// Additional monthly income sources
    /// </summary>
    [Range(0, 50000, ErrorMessage = "Additional income must be between $0 and $50,000")]
    public decimal AdditionalMonthlyIncome { get; set; }

    /// <summary>
    /// Description of additional income sources
    /// </summary>
    [StringLength(500)]
    public string? AdditionalIncomeDescription { get; set; }

    /// <summary>
    /// Monthly housing payment (rent/mortgage)
    /// </summary>
    [Required]
    [Range(0, 10000, ErrorMessage = "Housing payment must be between $0 and $10,000")]
    public decimal MonthlyHousingPayment { get; set; }

    /// <summary>
    /// Other monthly debt payments
    /// </summary>
    [Range(0, 50000, ErrorMessage = "Other debt payments must be between $0 and $50,000")]
    public decimal OtherMonthlyDebtPayments { get; set; }

    // TILA Information (Step 3)

    /// <summary>
    /// Annual Percentage Rate (APR) disclosed to applicant
    /// </summary>
    [Required]
    [Range(0, 50, ErrorMessage = "APR must be between 0% and 50%")]
    public decimal AnnualPercentageRate { get; set; }

    /// <summary>
    /// Finance charge disclosed to applicant
    /// </summary>
    [Required]
    [Range(0, 1000000, ErrorMessage = "Finance charge must be between $0 and $1,000,000")]
    public decimal FinanceCharge { get; set; }

    /// <summary>
    /// Total amount to be paid disclosed to applicant
    /// </summary>
    [Required]
    [Range(0, 2000000, ErrorMessage = "Total amount must be between $0 and $2,000,000")]
    public decimal TotalAmountToBePaid { get; set; }

    /// <summary>
    /// Monthly payment amount disclosed to applicant
    /// </summary>
    [Required]
    [Range(0, 10000, ErrorMessage = "Monthly payment must be between $0 and $10,000")]
    public decimal MonthlyPaymentAmount { get; set; }

    /// <summary>
    /// Whether applicant has acknowledged TILA disclosure
    /// </summary>
    [Required]
    public bool TilaAcknowledged { get; set; }

    /// <summary>
    /// Date when TILA was acknowledged
    /// </summary>
    public DateTime? TilaAcknowledgedDate { get; set; }

    /// <summary>
    /// IP address of the applicant when submitting
    /// </summary>
    [StringLength(45)]
    public string? SubmissionIpAddress { get; set; }

    /// <summary>
    /// User agent string from applicant's browser
    /// </summary>
    [StringLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    [StringLength(2000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Applicant's full name (computed property)
    /// </summary>
    public string FullName =>
        string.IsNullOrWhiteSpace(MiddleName) ? $"{FirstName} {LastName}" : $"{FirstName} {MiddleName} {LastName}";

    /// <summary>
    /// Applicant's full address (computed property)
    /// </summary>
    public string FullAddress => $"{StreetAddress}, {City}, {State} {ZipCode}";

    /// <summary>
    /// Total monthly income including additional sources (computed property)
    /// </summary>
    public decimal TotalMonthlyIncome => MonthlyGrossIncome + AdditionalMonthlyIncome;

    /// <summary>
    /// Total monthly obligations (computed property)
    /// </summary>
    public decimal TotalMonthlyObligations => MonthlyHousingPayment + OtherMonthlyDebtPayments;

    /// <summary>
    /// Debt-to-income ratio (computed property)
    /// </summary>
    public decimal DebtToIncomeRatio =>
        TotalMonthlyIncome > 0 ? (TotalMonthlyObligations / TotalMonthlyIncome) * 100 : 0;
}
