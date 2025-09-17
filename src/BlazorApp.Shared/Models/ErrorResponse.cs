namespace BlazorApp.Shared.Models;

/// <summary>
/// Represents a standardized error response for API operations.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional error details.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the trace identifier for request tracking.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets validation errors grouped by field name.
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
