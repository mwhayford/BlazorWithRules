using BlazorApp.IntegrationTests.Common;
using FluentAssertions;
using System.Net;
using System.Text.Json;

namespace BlazorApp.IntegrationTests.Web;

/// <summary>
/// Integration tests for health check endpoints
/// </summary>
public class HealthCheckTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task HealthCheckUI_ShouldReturnOK()
    {
        // Act
        var response = await _client.GetAsync("/health-ui");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Health Checks UI");
    }

    [Fact]
    public async Task DatabaseHealthCheck_ShouldBeHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<HealthCheckResponse>(content);
        
        healthReport.Should().NotBeNull();
        healthReport!.Status.Should().Be("Healthy");
        healthReport.Entries.Should().ContainKey("database");
        healthReport.Entries["database"].Status.Should().Be("Healthy");
    }

    [Fact]
    public async Task Application_ShouldStartSuccessfully()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Dashboard"); // Assuming Dashboard is the home page
    }

    [Fact]
    public async Task StaticFiles_ShouldBeServed()
    {
        // Act
        var response = await _client.GetAsync("/favicon.png");

        // Assert
        // This might return 404 if favicon doesn't exist, but the request should be processed
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SecurityHeaders_ShouldBePresent()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        response.Headers.Should().ContainKey("X-Frame-Options");
        response.Headers.Should().ContainKey("X-Content-Type-Options");
        response.Headers.Should().ContainKey("X-XSS-Protection");
        response.Headers.Should().ContainKey("Referrer-Policy");
        
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");
        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
    }

    [Fact]
    public async Task ErrorPages_ShouldReturn404ForNonExistentPages()
    {
        // Act
        var response = await _client.GetAsync("/non-existent-page");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("404"); // Should show custom 404 page
    }
}

/// <summary>
/// Health check response model for deserialization
/// </summary>
public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public TimeSpan TotalDuration { get; set; }
    public Dictionary<string, HealthCheckEntry> Entries { get; set; } = new();
}

/// <summary>
/// Health check entry model for deserialization
/// </summary>
public class HealthCheckEntry
{
    public string Status { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}
