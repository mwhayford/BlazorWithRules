namespace BlazorApp.Shared.Models;

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? TraceId { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
