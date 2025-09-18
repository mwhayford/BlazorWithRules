using BlazorApp.Core.Interfaces;
using BlazorApp.Core.Validators;
using FluentValidation.Results;

namespace BlazorApp.Infrastructure.Services;

/// <summary>
/// Wrapper implementation for LoanApplicationValidator to enable mocking
/// </summary>
public class LoanApplicationValidatorWrapper : IValidatorWrapper<Core.Entities.LoanApplication>
{
    private readonly LoanApplicationValidator _validator;

    public LoanApplicationValidatorWrapper()
    {
        _validator = new LoanApplicationValidator();
    }

    public LoanApplicationValidatorWrapper(LoanApplicationValidator validator)
    {
        _validator = validator;
    }

    public virtual Task<ValidationResult> ValidateAsync(
        Core.Entities.LoanApplication instance,
        CancellationToken cancellationToken = default
    )
    {
        return _validator.ValidateAsync(instance, cancellationToken);
    }
}

/// <summary>
/// Wrapper implementation for CreateLoanApplicationValidator to enable mocking
/// </summary>
public class CreateLoanApplicationValidatorWrapper : IValidatorWrapper<Core.Entities.LoanApplication>
{
    private readonly CreateLoanApplicationValidator _validator;

    public CreateLoanApplicationValidatorWrapper()
    {
        _validator = new CreateLoanApplicationValidator();
    }

    public CreateLoanApplicationValidatorWrapper(CreateLoanApplicationValidator validator)
    {
        _validator = validator;
    }

    public virtual Task<ValidationResult> ValidateAsync(
        Core.Entities.LoanApplication instance,
        CancellationToken cancellationToken = default
    )
    {
        return _validator.ValidateAsync(instance, cancellationToken);
    }
}

/// <summary>
/// Wrapper implementation for UpdateLoanApplicationStatusValidator to enable mocking
/// </summary>
public class UpdateLoanApplicationStatusValidatorWrapper : IValidatorWrapper<Core.Entities.LoanApplication>
{
    private readonly UpdateLoanApplicationStatusValidator _validator;

    public UpdateLoanApplicationStatusValidatorWrapper()
    {
        _validator = new UpdateLoanApplicationStatusValidator();
    }

    public UpdateLoanApplicationStatusValidatorWrapper(UpdateLoanApplicationStatusValidator validator)
    {
        _validator = validator;
    }

    public virtual Task<ValidationResult> ValidateAsync(
        Core.Entities.LoanApplication instance,
        CancellationToken cancellationToken = default
    )
    {
        return _validator.ValidateAsync(instance, cancellationToken);
    }
}
