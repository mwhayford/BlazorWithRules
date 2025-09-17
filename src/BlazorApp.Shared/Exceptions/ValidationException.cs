namespace BlazorApp.Shared.Exceptions;

public class ValidationException : Exception
{
    public IDictionary<string, IEnumerable<string>> Errors { get; }

    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, IEnumerable<string>>();
    }

    public ValidationException(IDictionary<string, IEnumerable<string>> errors)
        : this()
    {
        Errors = errors;
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, IEnumerable<string>>();
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = new Dictionary<string, IEnumerable<string>>();
    }
}
