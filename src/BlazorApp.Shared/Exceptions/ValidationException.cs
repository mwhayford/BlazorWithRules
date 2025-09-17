namespace BlazorApp.Shared.Exceptions;

/// <summary>
/// Exception thrown when validation failures occur.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, IEnumerable<string>>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public ValidationException(IDictionary<string, IEnumerable<string>> errors)
        : this()
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ValidationException(string message)
        : base(message)
    {
        Errors = new Dictionary<string, IEnumerable<string>>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new Dictionary<string, IEnumerable<string>>();
    }

    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IDictionary<string, IEnumerable<string>> Errors { get; }
}
