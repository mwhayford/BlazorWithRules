# 🔒 Security Implementation Guide

## 🚨 Critical Security Notice

**WARNING**: The current implementation lacks authentication and authorization mechanisms. This guide provides immediate fixes and long-term security hardening strategies.

## Immediate Security Fixes Required

### 1. 🛡️ Fix CORS Configuration

**Current Issue**: Overly permissive CORS policy allows all origins
```csharp
// ❌ CURRENT - INSECURE
policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
```

**Fix**: Restrict to specific origins
```csharp
// ✅ SECURE - Update in Program.cs
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development - localhost only
            policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
                  .WithMethods("GET", "POST", "PUT", "DELETE")
                  .WithHeaders("Content-Type", "Authorization")
                  .AllowCredentials();
        }
        else
        {
            // Production - specific domains only
            policy.WithOrigins("https://yourdomain.com", "https://api.yourdomain.com")
                  .WithMethods("GET", "POST", "PUT", "DELETE")
                  .WithHeaders("Content-Type", "Authorization")
                  .AllowCredentials();
        }
    });
});
```

### 2. 🔐 Implement Authentication

**Add JWT Authentication**:
```csharp
// Add to BlazorApp.Web.csproj
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />

// Add to Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Administrator"));
    options.AddPolicy("UserOnly", policy => 
        policy.RequireRole("User", "Administrator"));
});

// Add to pipeline BEFORE app.MapRazorComponents
app.UseAuthentication();
app.UseAuthorization();
```

### 3. 🛡️ Add Request Limits

```csharp
// Add to Program.cs
builder.Services.Configure<KestrelServerOptions>(options =>
{
    // Limit request body size to 10MB
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024;
    
    // Set request timeout
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
    
    // Limit concurrent connections
    options.Limits.MaxConcurrentConnections = 100;
    options.Limits.MaxConcurrentUpgradedConnections = 100;
});

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Enable rate limiting
app.UseRateLimiter();
```

## Enhanced Security Headers

### Update Security Headers Middleware
```csharp
// Replace existing security headers in Program.cs
app.Use(async (context, next) =>
{
    // Prevent clickjacking
    context.Response.Headers["X-Frame-Options"] = "DENY";
    
    // Prevent MIME type sniffing
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    
    // Enable XSS protection
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    
    // Referrer policy
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    
    // Enhanced Content Security Policy
    var csp = new StringBuilder();
    csp.Append("default-src 'self'; ");
    csp.Append("script-src 'self' 'unsafe-inline' 'unsafe-eval'; ");
    csp.Append("style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; ");
    csp.Append("img-src 'self' data: https:; ");
    csp.Append("font-src 'self' data: https://fonts.gstatic.com; ");
    csp.Append("connect-src 'self'; ");
    csp.Append("frame-ancestors 'none'; ");
    csp.Append("base-uri 'self'; ");
    csp.Append("form-action 'self';");
    
    context.Response.Headers["Content-Security-Policy"] = csp.ToString();
    
    // Additional security headers
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
    context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
    context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
    
    // Remove server header
    context.Response.Headers.Remove("Server");
    
    await next();
});
```

## Secrets Management

### 1. Add Azure Key Vault Integration

```csharp
// Add to BlazorApp.Web.csproj
<PackageReference Include="Azure.Extensions.AspNetCore.Configuration.Secrets" Version="1.3.2" />

// Add to Program.cs
if (!builder.Environment.IsDevelopment())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    var keyVaultUrl = $"https://{keyVaultName}.vault.azure.net/";
    
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}
```

### 2. Secure Configuration
```json
// appsettings.json - Remove sensitive values
{
  "ConnectionStrings": {
    "DefaultConnection": "{{WILL_BE_REPLACED_BY_KEYVAULT}}"
  },
  "Jwt": {
    "Issuer": "{{FROM_KEYVAULT}}",
    "Audience": "{{FROM_KEYVAULT}}",
    "Key": "{{FROM_KEYVAULT}}"
  },
  "KeyVaultName": "your-keyvault-name"
}
```

### 3. Environment Variables for Development
```bash
# Set in development environment
export ConnectionStrings__DefaultConnection="Server=(localdb)\\mssqllocaldb;Database=BlazorAppDb_Dev;Trusted_Connection=true"
export Jwt__Key="your-super-secret-key-at-least-32-chars"
export Jwt__Issuer="BlazorApp"
export Jwt__Audience="BlazorApp"
```

## Input Validation & Sanitization

### 1. Enhanced Validation Rules
```csharp
// Update UserValidator.cs
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters")
            .Matches("^[a-zA-Z\\s'-]*$").WithMessage("First name contains invalid characters")
            .Must(NotContainSqlKeywords).WithMessage("Invalid input detected");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(254).WithMessage("Email must not exceed 254 characters")
            .Must(BeValidEmailDomain).WithMessage("Email domain not allowed");
    }

    private bool NotContainSqlKeywords(string input)
    {
        if (string.IsNullOrEmpty(input)) return true;
        
        var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "DROP", "EXEC", "SCRIPT" };
        return !sqlKeywords.Any(keyword => 
            input.ToUpperInvariant().Contains(keyword));
    }

    private bool BeValidEmailDomain(string email)
    {
        if (string.IsNullOrEmpty(email)) return true;
        
        var domain = email.Split('@').LastOrDefault();
        // Add your allowed domains or domain validation logic
        return !string.IsNullOrEmpty(domain);
    }
}
```

### 2. Add Input Sanitization Service
```csharp
// Create BlazorApp.Core/Services/IInputSanitizer.cs
public interface IInputSanitizer
{
    string SanitizeHtml(string input);
    string SanitizeForLog(string input);
    bool ContainsMaliciousContent(string input);
}

// Create BlazorApp.Infrastructure/Services/InputSanitizer.cs
public class InputSanitizer : IInputSanitizer
{
    public string SanitizeHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        // Remove potentially dangerous HTML
        return Regex.Replace(input, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", "", 
            RegexOptions.IgnoreCase);
    }

    public string SanitizeForLog(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        // Remove line breaks and potential log injection
        return input.Replace("\r", "").Replace("\n", "").Replace("\t", " ");
    }

    public bool ContainsMaliciousContent(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        
        var maliciousPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"vbscript:",
            @"onload\s*=",
            @"onerror\s*=",
            @"document\.cookie",
            @"document\.write"
        };

        return maliciousPatterns.Any(pattern => 
            Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
    }
}
```

## Logging Security

### 1. Secure Logging Configuration
```csharp
// Update Program.cs Serilog configuration
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // Don't log Debug in production
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Filter.ByExcluding(Matching.WithProperty<string>("RequestPath", path => 
        path.StartsWith("/health") || path.StartsWith("/_framework")))
    .WriteTo.Console(
        restrictedToMinimumLevel: LogEventLevel.Information,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/blazorapp-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30, // Keep 30 days
        fileSizeLimitBytes: 50 * 1024 * 1024, // 50MB per file
        restrictedToMinimumLevel: LogEventLevel.Information,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
```

### 2. Secure Request Logging
```csharp
// Update RequestResponseLoggingMiddleware.cs
private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
{
    if (!request.Body.CanRead || request.ContentLength == 0)
        return string.Empty;

    // Limit body size for logging
    const int maxBodySize = 4096; // 4KB max
    if (request.ContentLength > maxBodySize)
        return $"[Body too large: {request.ContentLength} bytes]";

    request.EnableBuffering();
    var buffer = new byte[Convert.ToInt32(Math.Min(request.ContentLength ?? 0, maxBodySize))];
    await request.Body.ReadAsync(buffer, 0, buffer.Length);
    var requestBody = Encoding.UTF8.GetString(buffer);
    request.Body.Position = 0;

    // Sanitize sensitive data
    return SanitizeLogData(requestBody);
}

private static string SanitizeLogData(string data)
{
    if (string.IsNullOrEmpty(data)) return data;

    // Remove potential passwords, tokens, etc.
    var patterns = new Dictionary<string, string>
    {
        { @"""password""\s*:\s*""[^""]*""", @"""password"":""[REDACTED]""" },
        { @"""token""\s*:\s*""[^""]*""", @"""token"":""[REDACTED]""" },
        { @"""secret""\s*:\s*""[^""]*""", @"""secret"":""[REDACTED]""" },
        { @"""key""\s*:\s*""[^""]*""", @"""key"":""[REDACTED]""" }
    };

    var sanitized = data;
    foreach (var pattern in patterns)
    {
        sanitized = Regex.Replace(sanitized, pattern.Key, pattern.Value, RegexOptions.IgnoreCase);
    }

    return sanitized;
}
```

## Database Security

### 1. Connection String Security
```csharp
// Add to DependencyInjection.cs
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, 
    IConfiguration configuration)
{
    // Get connection string securely
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Database connection string is not configured");

    services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(30);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });

        // Disable sensitive data logging in production
        if (configuration.GetValue<bool>("Logging:EnableSensitiveDataLogging", false))
        {
            options.EnableSensitiveDataLogging();
        }
    });

    return services;
}
```

### 2. Add Database Encryption
```csharp
// Update BaseEntity.cs for sensitive fields
public abstract class BaseEntity
{
    public int Id { get; set; }
    
    [PersonalData] // Mark for GDPR compliance
    public DateTime CreatedAt { get; set; }
    
    // Add encryption for sensitive fields
    private string _encryptedEmail;
    
    [NotMapped]
    public string Email
    {
        get => DecryptValue(_encryptedEmail);
        set => _encryptedEmail = EncryptValue(value);
    }
    
    [Column("EncryptedEmail")]
    public string EncryptedEmailStorage
    {
        get => _encryptedEmail;
        set => _encryptedEmail = value;
    }
    
    private string EncryptValue(string value)
    {
        // Implement AES encryption
        return value; // Placeholder
    }
    
    private string DecryptValue(string encryptedValue)
    {
        // Implement AES decryption
        return encryptedValue; // Placeholder
    }
}
```

## Security Testing

### 1. Add Security Tests
```csharp
// Create tests/BlazorApp.SecurityTests/SecurityTests.cs
[Fact]
public async Task Application_Should_ReturnSecurityHeaders()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    using var client = factory.CreateClient();

    // Act
    var response = await client.GetAsync("/");

    // Assert
    response.Headers.Should().ContainKey("X-Frame-Options");
    response.Headers.Should().ContainKey("X-Content-Type-Options");
    response.Headers.Should().ContainKey("Content-Security-Policy");
}

[Fact]
public async Task Api_Should_RejectLargePayloads()
{
    // Test request size limits
    using var factory = new WebApplicationFactory<Program>();
    using var client = factory.CreateClient();
    
    var largeContent = new string('A', 15 * 1024 * 1024); // 15MB
    var content = new StringContent(largeContent, Encoding.UTF8, "application/json");
    
    var response = await client.PostAsync("/api/users", content);
    
    response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
}
```

## Security Monitoring

### 1. Add Security Event Logging
```csharp
// Create BlazorApp.Core/Services/ISecurityAuditService.cs
public interface ISecurityAuditService
{
    Task LogSecurityEventAsync(string eventType, string details, string? userId = null);
    Task LogSuspiciousActivityAsync(string activity, HttpContext context);
}

// Implement security monitoring
public class SecurityAuditService : ISecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger;

    public SecurityAuditService(ILogger<SecurityAuditService> logger)
    {
        _logger = logger;
    }

    public async Task LogSecurityEventAsync(string eventType, string details, string? userId = null)
    {
        _logger.LogWarning("SECURITY_EVENT: {EventType} - User: {UserId} - Details: {Details}",
            eventType, userId ?? "Anonymous", details);
        
        // Send to security monitoring system
        await Task.CompletedTask;
    }

    public async Task LogSuspiciousActivityAsync(string activity, HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        
        _logger.LogError("SUSPICIOUS_ACTIVITY: {Activity} - IP: {ClientIp} - UserAgent: {UserAgent}",
            activity, clientIp, userAgent);
        
        await Task.CompletedTask;
    }
}
```

## Deployment Security Checklist

### Pre-Production Checklist
- [ ] Authentication and authorization implemented
- [ ] CORS properly configured for production domains
- [ ] All secrets moved to Azure Key Vault
- [ ] HTTPS enforced with valid certificates
- [ ] Security headers properly configured
- [ ] Request limits and rate limiting enabled
- [ ] Input validation and sanitization implemented
- [ ] Sensitive data logging disabled
- [ ] Security tests passing
- [ ] Dependency vulnerability scan completed

### Production Monitoring
- [ ] Security event logging configured
- [ ] Failed authentication monitoring
- [ ] Suspicious activity alerts
- [ ] Regular security scans scheduled
- [ ] Log monitoring for security events
- [ ] Incident response plan documented

---

## 📞 Security Incident Response

If you discover a security vulnerability:

1. **DO NOT** create a public issue
2. **Email security concerns** to: security@yourdomain.com
3. **Include details**: vulnerability description, steps to reproduce, potential impact
4. **Response time**: We aim to respond within 24 hours

---

*Last Updated: September 17, 2025*  
*Security Review: Required Monthly*  
*Next Audit: October 17, 2025*
