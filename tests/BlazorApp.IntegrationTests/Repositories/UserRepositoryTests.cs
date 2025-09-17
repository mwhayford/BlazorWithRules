using AutoFixture;
using BlazorApp.Core.Entities;
using BlazorApp.Infrastructure.Data;
using BlazorApp.Infrastructure.Repositories;
using BlazorApp.IntegrationTests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorApp.IntegrationTests.Repositories;

/// <summary>
/// Integration tests for UserRepository
/// </summary>
public class UserRepositoryTests : DatabaseIntegrationTestBase
{
    private readonly UserRepository _userRepository;
    private readonly ApplicationDbContext _context;
    private readonly IFixture _fixture;

    public UserRepositoryTests(DatabaseTestFixture databaseFixture)
        : base(databaseFixture)
    {
        _context = GetService<ApplicationDbContext>();
        _userRepository = new UserRepository(_context);
        _fixture = new Fixture();

        // Configure AutoFixture
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task AddAsync_ShouldAddUser_WhenUserIsValid()
    {
        // Arrange
        var user = _fixture.Build<User>().Without(x => x.Id).With(x => x.Email, "test@example.com").Create();

        // Act
        var result = await _userRepository.AddAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Email.Should().Be("test@example.com");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));

        // Verify in database
        var savedUser = await _context.Users.FindAsync(result.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Email.Should().Be("test@example.com");

        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = _fixture.Build<User>().Without(x => x.Id).With(x => x.Email, "getbyid@example.com").Create();

        var savedUser = await _userRepository.AddAsync(user);

        // Act
        var result = await _userRepository.GetByIdAsync(savedUser.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(savedUser.Id);
        result.Email.Should().Be("getbyid@example.com");

        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await _userRepository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
    {
        // Arrange
        var email = "getbyemail@example.com";
        var user = _fixture.Build<User>().Without(x => x.Id).With(x => x.Email, email).Create();

        await _userRepository.AddAsync(user);

        // Act
        var result = await _userRepository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);

        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenEmailDoesNotExist()
    {
        // Arrange
        var nonExistentEmail = "nonexistent@example.com";

        // Act
        var result = await _userRepository.GetByEmailAsync(nonExistentEmail);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveUsers()
    {
        // Arrange
        var users = _fixture.Build<User>().Without(x => x.Id).With(x => x.IsDeleted, false).CreateMany(3).ToList();

        // Add a deleted user that should not be returned
        var deletedUser = _fixture.Build<User>().Without(x => x.Id).With(x => x.IsDeleted, true).Create();

        foreach (var user in users)
        {
            await _userRepository.AddAsync(user);
        }
        await _userRepository.AddAsync(deletedUser);

        // Act
        var result = await _userRepository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(u => !u.IsDeleted);

        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenUserExists()
    {
        // Arrange
        var user = _fixture
            .Build<User>()
            .Without(x => x.Id)
            .With(x => x.Email, "update@example.com")
            .With(x => x.FirstName, "Original")
            .Create();

        var savedUser = await _userRepository.AddAsync(user);

        // Modify the user
        savedUser.FirstName = "Updated";
        savedUser.LastName = "NewLastName";

        // Act
        var result = await _userRepository.UpdateAsync(savedUser);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("NewLastName");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));

        // Verify in database
        var updatedUser = await _context.Users.FindAsync(savedUser.Id);
        updatedUser!.FirstName.Should().Be("Updated");
        updatedUser.LastName.Should().Be("NewLastName");

        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteUser_WhenUserExists()
    {
        // Arrange
        var user = _fixture.Build<User>().Without(x => x.Id).With(x => x.Email, "delete@example.com").Create();

        var savedUser = await _userRepository.AddAsync(user);

        // Act
        var result = await _userRepository.DeleteAsync(savedUser.Id);

        // Assert
        result.Should().BeTrue();

        // Verify user is soft deleted
        var deletedUser = await _context.Users.FindAsync(savedUser.Id);
        deletedUser!.IsDeleted.Should().BeTrue();
        deletedUser.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));

        // Verify user is not returned by normal queries
        var userFromRepo = await _userRepository.GetByIdAsync(savedUser.Id);
        userFromRepo.Should().BeNull();

        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await _userRepository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var users = _fixture.Build<User>().Without(x => x.Id).With(x => x.IsDeleted, false).CreateMany(5).ToList();

        // Add a deleted user that should not be counted
        var deletedUser = _fixture.Build<User>().Without(x => x.Id).With(x => x.IsDeleted, true).Create();

        foreach (var user in users)
        {
            await _userRepository.AddAsync(user);
        }
        await _userRepository.AddAsync(deletedUser);

        // Act
        var result = await _userRepository.CountAsync();

        // Assert
        result.Should().Be(5); // Only non-deleted users

        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingUsers()
    {
        // Arrange
        var users = new[]
        {
            _fixture
                .Build<User>()
                .Without(x => x.Id)
                .With(x => x.FirstName, "John")
                .With(x => x.IsActive, true)
                .Create(),
            _fixture
                .Build<User>()
                .Without(x => x.Id)
                .With(x => x.FirstName, "Jane")
                .With(x => x.IsActive, true)
                .Create(),
            _fixture
                .Build<User>()
                .Without(x => x.Id)
                .With(x => x.FirstName, "Bob")
                .With(x => x.IsActive, false)
                .Create(),
        };

        foreach (var user in users)
        {
            await _userRepository.AddAsync(user);
        }

        // Act
        var result = await _userRepository.FindAsync(u => u.IsActive);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.IsActive);
        result.Should().Contain(u => u.FirstName == "John");
        result.Should().Contain(u => u.FirstName == "Jane");

        // Cleanup
        await CleanupDatabaseAsync();
    }
}
