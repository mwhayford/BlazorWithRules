using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using BlazorApp.Core.Interfaces;
using BlazorApp.Core.Services;
using BlazorApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AutoFixture;

namespace BlazorApp.UI.Tests.Common;

/// <summary>
/// Base class for Blazor component tests using bUnit
/// </summary>
public abstract class BlazorTestBase : TestContext, IDisposable
{
    protected readonly IFixture Fixture;
    protected readonly Mock<IUserService> MockUserService;
    protected readonly Mock<ICacheService> MockCacheService;
    protected readonly Mock<ILogger> MockLogger;

    protected BlazorTestBase()
    {
        Fixture = new Fixture();
        MockUserService = new Mock<IUserService>();
        MockCacheService = new Mock<ICacheService>();
        MockLogger = new Mock<ILogger>();
        
        // Configure AutoFixture
        ConfigureAutoFixture();
        
        // Configure services for testing
        ConfigureTestServices();
    }

    private void ConfigureAutoFixture()
    {
        // Handle circular references
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    private void ConfigureTestServices()
    {
        // Add in-memory database for testing
        Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add logging
        Services.AddLogging();

        // Add mock services
        Services.AddSingleton(MockUserService.Object);
        Services.AddSingleton(MockCacheService.Object);

        // Add other required services
        Services.AddScoped<ILogger>(provider => MockLogger.Object);
    }

    /// <summary>
    /// Create a mock logger for the specified type
    /// </summary>
    protected Mock<ILogger<T>> CreateMockLogger<T>()
    {
        var mockLogger = new Mock<ILogger<T>>();
        Services.AddSingleton(mockLogger.Object);
        return mockLogger;
    }

    /// <summary>
    /// Add a service to the test service collection
    /// </summary>
    protected void AddService<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface
    {
        Services.AddScoped<TInterface, TImplementation>();
    }

    /// <summary>
    /// Add a singleton service to the test service collection
    /// </summary>
    protected void AddSingletonService<TInterface>(TInterface implementation)
        where TInterface : class
    {
        Services.AddSingleton(implementation);
    }

    /// <summary>
    /// Setup a mock user service with default behavior
    /// </summary>
    protected void SetupMockUserService()
    {
        var users = Fixture.CreateMany<BlazorApp.Core.Entities.User>(3).ToList();
        
        MockUserService.Setup(x => x.GetAllUsersAsync())
            .ReturnsAsync(users);
        
        MockUserService.Setup(x => x.GetUserCountAsync())
            .ReturnsAsync(users.Count);
        
        MockUserService.Setup(x => x.GetActiveUserCountAsync())
            .ReturnsAsync(users.Count(u => u.IsActive));
        
        foreach (var user in users)
        {
            MockUserService.Setup(x => x.GetUserByIdAsync(user.Id))
                .ReturnsAsync(user);
        }
    }

    /// <summary>
    /// Verify that an element exists in the rendered component
    /// </summary>
    protected void VerifyElementExists(IRenderedFragment component, string selector)
    {
        var element = component.Find(selector);
        Assert.NotNull(element);
    }

    /// <summary>
    /// Verify that an element contains the expected text
    /// </summary>
    protected void VerifyElementText(IRenderedFragment component, string selector, string expectedText)
    {
        var element = component.Find(selector);
        Assert.Contains(expectedText, element.TextContent);
    }

    /// <summary>
    /// Verify that an element has the expected attribute value
    /// </summary>
    protected void VerifyElementAttribute(IRenderedFragment component, string selector, string attributeName, string expectedValue)
    {
        var element = component.Find(selector);
        var actualValue = element.GetAttribute(attributeName);
        Assert.Equal(expectedValue, actualValue);
    }

    /// <summary>
    /// Click an element and wait for the component to re-render
    /// </summary>
    protected void ClickAndWait(IRenderedFragment component, string selector)
    {
        var element = component.Find(selector);
        element.Click();
        component.WaitForAssertion(() => { }, TimeSpan.FromSeconds(1));
    }

    new public void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
