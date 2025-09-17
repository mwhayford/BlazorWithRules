using AutoFixture;
using BlazorApp.Core.Entities;
using BlazorApp.Core.Interfaces;
using BlazorApp.Infrastructure.Repositories;
using BlazorApp.Infrastructure.Services;
using BlazorApp.UnitTests.Common;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using ValidationException = BlazorApp.Shared.Exceptions.ValidationException;

namespace BlazorApp.UnitTests.Services;

/// <summary>
/// Unit tests for UserService
/// </summary>
public class UserServiceTestsFixed : TestBase
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<IValidator<User>> _mockValidator;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTestsFixed()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockValidator = new Mock<IValidator<User>>();
        _mockLogger = CreateMockLogger<UserService>();

        _userService = new UserService(
            _mockUserRepository.Object,
            _mockCacheService.Object,
            _mockValidator.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnCachedUsers_WhenCacheHit()
    {
        // Arrange
        var expectedUsers = Fixture.CreateMany<User>(3).ToList();
        _mockCacheService
            .Setup(x => x.GetOrSetAsync("all_users", It.IsAny<Func<Task<List<User>>>>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedUsers);
        _mockUserRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldFetchFromRepository_WhenCacheMiss()
    {
        // Arrange
        var expectedUsers = Fixture.CreateMany<User>(3).ToList();
        _mockCacheService
            .Setup(x => x.GetOrSetAsync("all_users", It.IsAny<Func<Task<List<User>>>>(), It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<List<User>>>, TimeSpan>(async (key, factory, expiration) => await factory());

        _mockUserRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.GetAllUsersAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedUsers);
        _mockUserRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        VerifyLogCalled(_mockLogger, LogLevel.Information, "Getting all users", Times.Once());
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var expectedUser = Fixture.Create<User>();
        var userId = expectedUser.Id;

        _mockCacheService
            .Setup(x => x.GetOrSetAsync($"user_{userId}", It.IsAny<Func<Task<User?>>>(), It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<User?>>, TimeSpan>(async (key, factory, expiration) => await factory());

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Fixture.Create<int>();

        _mockCacheService
            .Setup(x => x.GetOrSetAsync($"user_{userId}", It.IsAny<Func<Task<User?>>>(), It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<User?>>, TimeSpan>(async (key, factory, expiration) => await factory());

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
        VerifyLogCalled(_mockLogger, LogLevel.Warning, $"User with ID {userId} not found", Times.Once());
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateUser_WhenValidationPasses()
    {
        // Arrange
        var user = Fixture.Create<User>();
        var validationResult = new FluentValidation.Results.ValidationResult();

        _mockValidator.Setup(x => x.ValidateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserRepository.Setup(x => x.AddAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _userService.CreateUserAsync(user);

        // Assert
        result.Should().BeEquivalentTo(user);
        _mockUserRepository.Verify(x => x.AddAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync("all_users"), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var user = Fixture.Create<User>();
        var validationResult = new FluentValidation.Results.ValidationResult();
        validationResult.Errors.Add(new FluentValidation.Results.ValidationFailure("Email", "Email is required"));

        _mockValidator.Setup(x => x.ValidateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _userService.CreateUserAsync(user));
        exception.Errors.Should().ContainKey("Email");
        exception.Errors["Email"].Should().Contain("Email is required");

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldThrowInvalidOperationException_WhenEmailAlreadyExists()
    {
        // Arrange
        var user = Fixture.Create<User>();
        var existingUser = Fixture.Create<User>();
        existingUser.Email = user.Email;

        var validationResult = new FluentValidation.Results.ValidationResult();

        _mockValidator.Setup(x => x.ValidateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(validationResult);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(user));
        exception.Message.Should().Contain($"User with email {user.Email} already exists");

        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [CustomAutoData]
    public async Task ToggleUserStatusAsync_ShouldToggleStatus_WhenUserExists(User user)
    {
        // Arrange
        var originalStatus = user.IsActive;
        _mockUserRepository.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _mockUserRepository.Setup(x => x.UpdateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await _userService.ToggleUserStatusAsync(user.Id);

        // Assert
        result.Should().BeTrue();
        user.IsActive.Should().Be(!originalStatus);
        _mockUserRepository.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync("all_users"), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync($"user_{user.Id}"), Times.Once);
    }

    [Fact]
    public async Task ToggleUserStatusAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Fixture.Create<int>();
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.ToggleUserStatusAsync(userId);

        // Assert
        result.Should().BeFalse();
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        VerifyLogCalled(_mockLogger, LogLevel.Warning, $"User with ID {userId} not found", Times.Once());
    }

    [Fact]
    public async Task GetUserCountAsync_ShouldReturnCachedCount_WhenCacheHit()
    {
        // Arrange
        var expectedCount = 5;
        _mockCacheService
            .Setup(x => x.GetOrSetValueAsync("user_count", It.IsAny<Func<Task<int>>>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _userService.GetUserCountAsync();

        // Assert
        result.Should().Be(expectedCount);
        _mockUserRepository.Verify(x => x.CountAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserCountAsync_ShouldFetchFromRepository_WhenCacheMiss()
    {
        // Arrange
        var expectedCount = 5;
        _mockCacheService
            .Setup(x => x.GetOrSetValueAsync("user_count", It.IsAny<Func<Task<int>>>(), It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<int>>, TimeSpan>(async (key, factory, expiration) => await factory());

        _mockUserRepository.Setup(x => x.CountAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expectedCount);

        // Act
        var result = await _userService.GetUserCountAsync();

        // Assert
        result.Should().Be(expectedCount);
        _mockUserRepository.Verify(x => x.CountAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
