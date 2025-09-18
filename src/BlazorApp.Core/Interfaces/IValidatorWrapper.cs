using FluentValidation.Results;

namespace BlazorApp.Core.Interfaces;

/// <summary>
/// Wrapper interface for FluentValidation validators to enable mocking
/// </summary>
/// <typeparam name="T">Type to validate</typeparam>
public interface IValidatorWrapper<T>
{
    /// <summary>
    /// Validates the specified instance asynchronously
    /// </summary>
    /// <param name="instance">Instance to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}
