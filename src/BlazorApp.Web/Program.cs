using BlazorApp.Core.Entities;
using BlazorApp.Core.Interfaces;
using BlazorApp.Core.Services;
using BlazorApp.Core.Validators;
using BlazorApp.Infrastructure;
using BlazorApp.Infrastructure.Data;
using BlazorApp.Infrastructure.Services;
using BlazorApp.Shared.Constants;
using BlazorApp.Web.Components;
using BlazorApp.Web.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
    )
    .WriteTo.File(
        path: "logs/blazorapp-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Add Memory Caching
builder.Services.AddMemoryCache();

// Add Caching Service
builder.Services.AddScoped<ICacheService, CacheService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<BlazorApp.Core.Validators.UserValidator>();

// Add Health Checks
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

// Add Health Checks UI
builder.Services.AddHealthChecksUI().AddInMemoryStorage();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Add Identity services
builder
    .Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        // Sign-in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add Google Authentication
builder
    .Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId =
            builder.Configuration["Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException("Google ClientId not configured");
        options.ClientSecret =
            builder.Configuration["Authentication:Google:ClientSecret"]
            ?? throw new InvalidOperationException("Google ClientSecret not configured");
        options.CallbackPath = "/signin-google";
    });

// Add Authentication service
builder.Services.AddScoped<
    BlazorApp.Core.Services.IAuthenticationService,
    BlazorApp.Infrastructure.Services.AuthenticationService
>();

// Add HTTP Context Accessor for authentication
builder.Services.AddHttpContextAccessor();

// Add Infrastructure services (Entity Framework, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Ensure database is created and seeded
await app.Services.EnsureDatabaseAsync();

// await app.Services.SeedDataAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Use custom global exception handler
    app.UseMiddleware<GlobalExceptionMiddleware>();

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    // Use custom global exception handler in development too for consistency
    app.UseMiddleware<GlobalExceptionMiddleware>();
}

// Add request/response logging
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.UseHttpsRedirection();

// Add security headers
app.Use(
    async (context, next) =>
    {
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        // Configure CSP based on environment
        if (app.Environment.IsDevelopment())
        {
            // More permissive CSP for development to allow hot-reload
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; img-src 'self' data: https:; font-src 'self' data: https://cdn.jsdelivr.net; connect-src 'self' https://cdn.jsdelivr.net ws://localhost:* wss://localhost:*;";
        }
        else
        {
            // Strict CSP for production
            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; img-src 'self' data: https:; font-src 'self' data: https://cdn.jsdelivr.net; connect-src 'self' https://cdn.jsdelivr.net;";
        }

        await next();
    }
);

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Map Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

app.UseStaticFiles();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Ensure proper cleanup of Serilog
try
{
    Log.Information("Starting BlazorApp.Web");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
public partial class Program { }
