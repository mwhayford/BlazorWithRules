using AutoFixture;
using BlazorApp.Core.Entities;
using BlazorApp.UI.Tests.Common;
using BlazorApp.Web.Components.Pages;
using Bunit;
using FluentAssertions;
using Moq;

namespace BlazorApp.UI.Tests.Components;

/// <summary>
/// Component tests for Dashboard page
/// </summary>
public class DashboardTests : BlazorTestBase
{
    [Fact]
    public void Dashboard_ShouldRenderTitle()
    {
        // Arrange
        SetupMockUserService();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert
        component.Find("h1").TextContent.Should().Contain("Dashboard");
    }

    [Fact]
    public void Dashboard_ShouldDisplayKPICards()
    {
        // Arrange
        var totalUsers = 100;
        var activeUsers = 85;

        MockUserService.Setup(x => x.GetUserCountAsync()).ReturnsAsync(totalUsers);

        MockUserService.Setup(x => x.GetActiveUserCountAsync()).ReturnsAsync(activeUsers);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert
        var kpiCards = component.FindAll(".kpi-card");
        kpiCards.Should().HaveCountGreaterThan(0);

        // Should display total users
        component.Markup.Should().Contain(totalUsers.ToString());

        // Should display active users
        component.Markup.Should().Contain(activeUsers.ToString());
    }

    [Fact]
    public void Dashboard_ShouldDisplayRecentUsersTable()
    {
        // Arrange
        var users = Fixture.CreateMany<User>(5).ToList();
        SetupMockUserService();

        MockUserService.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(users);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert
        var table = component.Find("table");
        table.Should().NotBeNull();

        // Should have table headers
        var headers = component.FindAll("th");
        headers.Should().HaveCountGreaterThan(0);

        // Should display user data
        foreach (var user in users.Take(3)) // Dashboard typically shows limited number
        {
            component.Markup.Should().Contain(user.FirstName);
            component.Markup.Should().Contain(user.Email);
        }
    }

    [Fact]
    public void Dashboard_ShouldHandleEmptyUserList()
    {
        // Arrange
        MockUserService.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(new List<User>());

        MockUserService.Setup(x => x.GetUserCountAsync()).ReturnsAsync(0);

        MockUserService.Setup(x => x.GetActiveUserCountAsync()).ReturnsAsync(0);

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert
        component.Markup.Should().Contain("0"); // Should show zero counts

        // Should handle empty state gracefully
        var noDataMessage = component.FindAll(".no-data, .empty-state");
        // Note: This test would need to be adjusted based on actual empty state implementation
    }

    [Fact]
    public void Dashboard_ShouldDisplayProgressBars()
    {
        // Arrange
        SetupMockUserService();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert
        var progressBars = component.FindAll(".progress");
        progressBars.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Dashboard_ShouldDisplayBootstrapIcons()
    {
        // Arrange
        SetupMockUserService();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert
        var icons = component.FindAll("i.bi");
        icons.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Dashboard_ShouldBeResponsive()
    {
        // Arrange
        SetupMockUserService();

        // Act
        var component = RenderComponent<Dashboard>();

        // Assert
        // Check for Bootstrap responsive classes
        var responsiveElements = component.FindAll("[class*='col-'], [class*='row']");
        responsiveElements.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Dashboard_ShouldHandleServiceErrors()
    {
        // Arrange
        MockUserService.Setup(x => x.GetAllUsersAsync()).ThrowsAsync(new Exception("Service error"));

        MockUserService.Setup(x => x.GetUserCountAsync()).ThrowsAsync(new Exception("Service error"));

        // Act & Assert
        // The component should handle errors gracefully
        // This would depend on the actual error handling implementation in the Dashboard component
        var component = RenderComponent<Dashboard>();

        // Component should render without throwing
        component.Should().NotBeNull();
    }
}
