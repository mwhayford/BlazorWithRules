using FluentValidation;
using BlazorApp.Core.Entities;

namespace BlazorApp.Core.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters")
            .Matches("^[a-zA-Z\\s]*$").WithMessage("First name can only contain letters and spaces");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters")
            .Matches("^[a-zA-Z\\s]*$").WithMessage("Last name can only contain letters and spaces");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Now).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("Invalid date of birth")
            .When(x => x.DateOfBirth.HasValue);
    }
}

public class CreateUserValidator : AbstractValidator<User>
{
    public CreateUserValidator()
    {
        Include(new UserValidator());
        
        // Additional rules for creating users
        RuleFor(x => x.Id)
            .Empty().WithMessage("ID should not be provided when creating a user");
    }
}

public class UpdateUserValidator : AbstractValidator<User>
{
    public UpdateUserValidator()
    {
        Include(new UserValidator());
        
        // Additional rules for updating users
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required when updating a user");
    }
}
