using BlazorApp.Core.Entities;
using FluentValidation;

namespace BlazorApp.Core.Validators;

/// <summary>
/// Validator for LoanApplication entity
/// </summary>
public class LoanApplicationValidator : AbstractValidator<LoanApplication>
{
    public LoanApplicationValidator()
    {
        // Basic loan information validation
        RuleFor(x => x.ApplicationNumber)
            .NotEmpty()
            .WithMessage("Application number is required")
            .MaximumLength(20)
            .WithMessage("Application number must not exceed 20 characters");

        RuleFor(x => x.RequestedAmount)
            .GreaterThan(0)
            .WithMessage("Requested amount must be greater than 0")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Requested amount cannot exceed $1,000,000");

        RuleFor(x => x.TermInMonths)
            .GreaterThanOrEqualTo(12)
            .WithMessage("Loan term must be at least 12 months")
            .LessThanOrEqualTo(360)
            .WithMessage("Loan term cannot exceed 360 months");

        RuleFor(x => x.LoanPurpose)
            .NotEmpty()
            .WithMessage("Loan purpose is required")
            .MaximumLength(100)
            .WithMessage("Loan purpose must not exceed 100 characters");

        // Demographics validation
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(50)
            .WithMessage("First name must not exceed 50 characters")
            .Matches("^[a-zA-Z\\s]*$")
            .WithMessage("First name can only contain letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(50)
            .WithMessage("Last name must not exceed 50 characters")
            .Matches("^[a-zA-Z\\s]*$")
            .WithMessage("Last name can only contain letters and spaces");

        RuleFor(x => x.MiddleName)
            .MaximumLength(50)
            .WithMessage("Middle name must not exceed 50 characters")
            .Matches("^[a-zA-Z\\s]*$")
            .WithMessage("Middle name can only contain letters and spaces")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required")
            .LessThan(DateTime.Now.AddYears(-18))
            .WithMessage("Applicant must be at least 18 years old")
            .GreaterThan(DateTime.Now.AddYears(-120))
            .WithMessage("Invalid date of birth");

        RuleFor(x => x.SocialSecurityNumber)
            .NotEmpty()
            .WithMessage("Social Security Number is required")
            .Matches(@"^\d{3}-?\d{2}-?\d{4}$")
            .WithMessage("Social Security Number must be in format XXX-XX-XXXX");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email address is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(256)
            .WithMessage("Email address must not exceed 256 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone number format");

        // Address validation
        RuleFor(x => x.StreetAddress)
            .NotEmpty()
            .WithMessage("Street address is required")
            .MaximumLength(200)
            .WithMessage("Street address must not exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(100)
            .WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.State)
            .NotEmpty()
            .WithMessage("State is required")
            .MaximumLength(50)
            .WithMessage("State must not exceed 50 characters");

        RuleFor(x => x.ZipCode)
            .NotEmpty()
            .WithMessage("ZIP code is required")
            .Matches(@"^\d{5}(-\d{4})?$")
            .WithMessage("ZIP code must be in format XXXXX or XXXXX-XXXX");

        RuleFor(x => x.ResidenceDurationMonths)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Residence duration cannot be negative")
            .LessThanOrEqualTo(1200)
            .WithMessage("Residence duration cannot exceed 100 years");

        RuleFor(x => x.HousingStatus)
            .NotEmpty()
            .WithMessage("Housing status is required")
            .Must(status => new[] { "Own", "Rent", "Other" }.Contains(status))
            .WithMessage("Housing status must be Own, Rent, or Other");

        // Income validation
        RuleFor(x => x.EmploymentStatus)
            .NotEmpty()
            .WithMessage("Employment status is required")
            .MaximumLength(50)
            .WithMessage("Employment status must not exceed 50 characters");

        RuleFor(x => x.EmployerName)
            .MaximumLength(200)
            .WithMessage("Employer name must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.EmployerName));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100)
            .WithMessage("Job title must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.JobTitle));

        RuleFor(x => x.EmploymentDurationMonths)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Employment duration cannot be negative")
            .LessThanOrEqualTo(1200)
            .WithMessage("Employment duration cannot exceed 100 years")
            .When(x => x.EmploymentDurationMonths.HasValue);

        RuleFor(x => x.MonthlyGrossIncome)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Monthly gross income cannot be negative")
            .LessThanOrEqualTo(100000)
            .WithMessage("Monthly gross income cannot exceed $100,000");

        RuleFor(x => x.AdditionalMonthlyIncome)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Additional monthly income cannot be negative")
            .LessThanOrEqualTo(50000)
            .WithMessage("Additional monthly income cannot exceed $50,000");

        RuleFor(x => x.AdditionalIncomeDescription)
            .MaximumLength(500)
            .WithMessage("Additional income description must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.AdditionalIncomeDescription));

        RuleFor(x => x.MonthlyHousingPayment)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Monthly housing payment cannot be negative")
            .LessThanOrEqualTo(10000)
            .WithMessage("Monthly housing payment cannot exceed $10,000");

        RuleFor(x => x.OtherMonthlyDebtPayments)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Other monthly debt payments cannot be negative")
            .LessThanOrEqualTo(50000)
            .WithMessage("Other monthly debt payments cannot exceed $50,000");

        // TILA validation
        RuleFor(x => x.AnnualPercentageRate)
            .GreaterThanOrEqualTo(0)
            .WithMessage("APR cannot be negative")
            .LessThanOrEqualTo(50)
            .WithMessage("APR cannot exceed 50%");

        RuleFor(x => x.FinanceCharge)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Finance charge cannot be negative")
            .LessThanOrEqualTo(1000000)
            .WithMessage("Finance charge cannot exceed $1,000,000");

        RuleFor(x => x.TotalAmountToBePaid)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Total amount to be paid cannot be negative")
            .LessThanOrEqualTo(2000000)
            .WithMessage("Total amount to be paid cannot exceed $2,000,000");

        RuleFor(x => x.MonthlyPaymentAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Monthly payment amount cannot be negative")
            .LessThanOrEqualTo(10000)
            .WithMessage("Monthly payment amount cannot exceed $10,000");

        RuleFor(x => x.TilaAcknowledged).Equal(true).WithMessage("TILA acknowledgment is required");

        RuleFor(x => x.TilaAcknowledgedDate)
            .NotNull()
            .WithMessage("TILA acknowledgment date is required")
            .When(x => x.TilaAcknowledged);

        // Business logic validation
        RuleFor(x => x)
            .Must(HaveReasonableDebtToIncomeRatio)
            .WithMessage("Debt-to-income ratio appears too high for loan approval");

        RuleFor(x => x)
            .Must(HaveMinimumIncome)
            .WithMessage("Monthly income appears insufficient for the requested loan amount");
    }

    private static bool HaveReasonableDebtToIncomeRatio(LoanApplication application)
    {
        var totalIncome = application.MonthlyGrossIncome + application.AdditionalMonthlyIncome;
        var totalDebt = application.MonthlyHousingPayment + application.OtherMonthlyDebtPayments;

        if (totalIncome <= 0)
        {
            return false;
        }

        var debtToIncomeRatio = (totalDebt / totalIncome) * 100;
        return debtToIncomeRatio <= 50; // Maximum 50% debt-to-income ratio
    }

    private static bool HaveMinimumIncome(LoanApplication application)
    {
        var totalIncome = application.MonthlyGrossIncome + application.AdditionalMonthlyIncome;
        var monthlyPayment = application.MonthlyPaymentAmount;

        // Income should be at least 2.5 times the monthly payment
        return totalIncome >= monthlyPayment * 2.5m;
    }
}

/// <summary>
/// Validator for creating a new loan application
/// </summary>
public class CreateLoanApplicationValidator : AbstractValidator<LoanApplication>
{
    public CreateLoanApplicationValidator()
    {
        Include(new LoanApplicationValidator());

        // Additional rules for creating applications
        RuleFor(x => x.Id).Empty().WithMessage("ID should not be provided when creating a loan application");
        RuleFor(x => x.Status).Equal("Draft").WithMessage("New applications must start with Draft status");
        // Note: CreatedAt and UpdatedAt are automatically set by BaseEntity, so we don't validate them
    }
}

/// <summary>
/// Validator for updating loan application status
/// </summary>
public class UpdateLoanApplicationStatusValidator : AbstractValidator<LoanApplication>
{
    public UpdateLoanApplicationStatusValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(status =>
                new[] { "Draft", "Submitted", "Under Review", "Approved", "Rejected", "Cancelled" }.Contains(status)
            )
            .WithMessage("Status must be one of: Draft, Submitted, Under Review, Approved, Rejected, Cancelled");

        RuleFor(x => x.Notes)
            .MaximumLength(2000)
            .WithMessage("Notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
