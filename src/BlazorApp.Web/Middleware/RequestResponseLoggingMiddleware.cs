using System.Diagnostics;
using System.Text;

namespace BlazorApp.Web.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for static files and health checks
        if (ShouldSkipLogging(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var requestBody = await ReadRequestBodyAsync(context.Request);

        _logger.LogInformation("HTTP {Method} {Path} started. TraceId: {TraceId}",
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);

        if (!string.IsNullOrEmpty(requestBody))
        {
            _logger.LogDebug("Request Body: {RequestBody}", requestBody);
        }

        // Capture the original response body stream
        var originalResponseBodyStream = context.Response.Body;

        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request failed. HTTP {Method} {Path} - TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Read response body
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();

            // Copy response body back to original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalResponseBodyStream);

            _logger.LogInformation("HTTP {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode,
                context.TraceIdentifier);

            if (!string.IsNullOrEmpty(responseBody) && context.Response.StatusCode >= 400)
            {
                _logger.LogDebug("Error Response Body: {ResponseBody}", responseBody);
            }
        }
    }

    private static bool ShouldSkipLogging(PathString path)
    {
        return path.StartsWithSegments("/_framework") ||
               path.StartsWithSegments("/css") ||
               path.StartsWithSegments("/js") ||
               path.StartsWithSegments("/lib") ||
               path.StartsWithSegments("/images") ||
               path.StartsWithSegments("/favicon") ||
               path.StartsWithSegments("/health") ||
               path.StartsWithSegments("/_blazor");
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!request.Body.CanRead || request.ContentLength == 0)
        {
            return string.Empty;
        }

        request.EnableBuffering();
        var buffer = new byte[Convert.ToInt32(request.ContentLength ?? 0)];
        await request.Body.ReadAsync(buffer, 0, buffer.Length);
        var requestBody = Encoding.UTF8.GetString(buffer);
        request.Body.Position = 0;

        return requestBody;
    }
}
