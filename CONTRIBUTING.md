# 🤝 Contributing to BlazorWithRules

Thank you for your interest in contributing to BlazorWithRules! This document provides guidelines for contributing to this project.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Process](#development-process)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Issue Reporting](#issue-reporting)

## 🌟 Code of Conduct

This project adheres to a code of conduct to ensure a welcoming environment for all contributors. By participating, you agree to:

- Be respectful and inclusive
- Focus on constructive feedback
- Accept responsibility for mistakes
- Show empathy towards other contributors

## 🚀 Getting Started

### Prerequisites

Before contributing, ensure you have:

- **.NET 9 SDK** installed
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** for version control
- **Docker Desktop** (optional, for integration tests)

### Development Setup

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/yourusername/BlazorWithRules.git
   cd BlazorWithRules
   ```
3. **Add upstream remote**:
   ```bash
   git remote add upstream https://github.com/original/BlazorWithRules.git
   ```
4. **Install dependencies**:
   ```bash
   dotnet restore
   ```
5. **Build the solution**:
   ```bash
   dotnet build
   ```
6. **Run tests**:
   ```bash
   dotnet test
   ```

## 🔄 Development Process

### Branching Strategy

We follow a **Git Flow** approach:

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/feature-name` - Feature branches
- `hotfix/issue-description` - Urgent fixes
- `release/version-number` - Release preparation

### Workflow

1. **Create a feature branch**:
   ```bash
   git checkout develop
   git pull upstream develop
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following our coding standards

3. **Write tests** for new functionality

4. **Run all tests** to ensure nothing breaks:
   ```bash
   dotnet test
   ```

5. **Commit your changes** with clear messages:
   ```bash
   git add .
   git commit -m "feat: add user authentication feature"
   ```

6. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a Pull Request** on GitHub

## 📝 Coding Standards

### Code Style

We use **EditorConfig** for consistent formatting. Key guidelines:

#### C# Conventions
- **PascalCase** for classes, methods, properties, and public fields
- **camelCase** for private fields (with underscore prefix: `_fieldName`)
- **PascalCase** for constants and static readonly fields
- **Interfaces** prefixed with `I` (e.g., `IUserService`)

#### Example:
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private const int MaxRetries = 3;

    public async Task<User> GetUserByIdAsync(int userId)
    {
        _logger.LogInformation("Getting user {UserId}", userId);
        return await _userRepository.GetByIdAsync(userId);
    }
}
```

### File Organization

```
src/
├── BlazorApp.Core/           # Domain logic, entities, interfaces
│   ├── Entities/            # Domain models
│   ├── Interfaces/          # Service contracts
│   ├── Services/            # Business logic interfaces
│   └── Validators/          # Input validation
├── BlazorApp.Infrastructure/ # Data access, external services
│   ├── Data/               # DbContext, configurations
│   ├── Repositories/       # Data access implementations
│   └── Services/           # External service implementations
├── BlazorApp.Shared/        # Shared models, DTOs
│   ├── Models/             # Data transfer objects
│   └── Constants/          # Application constants
└── BlazorApp.Web/           # UI layer
    ├── Components/         # Razor components
    ├── Pages/              # Blazor pages
    └── Middleware/         # Custom middleware
```

### Naming Conventions

#### Files and Folders
- **PascalCase** for file names: `UserService.cs`
- **PascalCase** for folder names: `Components/`
- **kebab-case** for URLs: `/user-management`

#### Database
- **PascalCase** for table names: `Users`
- **PascalCase** for column names: `FirstName`
- **Plural** for table names: `Users`, `Orders`

#### Tests
- **Descriptive test names**: `GetUserById_ShouldReturnUser_WhenUserExists`
- **Arrange-Act-Assert** pattern
- **One assertion per test** (when possible)

## 🧪 Testing Guidelines

### Test Structure

We follow a **three-layer testing approach**:

1. **Unit Tests** - Fast, isolated tests for business logic
2. **Integration Tests** - Database and external service tests
3. **Component Tests** - Blazor component testing with bUnit

### Writing Tests

#### Unit Test Example
```csharp
[Fact]
public async Task CreateUserAsync_ShouldCreateUser_WhenValidDataProvided()
{
    // Arrange
    var user = new User { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
    var mockRepository = new Mock<IUserRepository>();
    var mockValidator = new Mock<IValidator<User>>();
    
    mockValidator.Setup(x => x.ValidateAsync(user, default))
        .ReturnsAsync(new ValidationResult());
    mockRepository.Setup(x => x.AddAsync(user, default))
        .ReturnsAsync(user);

    var service = new UserService(mockRepository.Object, mockValidator.Object);

    // Act
    var result = await service.CreateUserAsync(user);

    // Assert
    result.Should().NotBeNull();
    result.Email.Should().Be("john@example.com");
    mockRepository.Verify(x => x.AddAsync(user, default), Times.Once);
}
```

#### Test Categories
- **Unit Tests**: `[Fact]` for single test cases
- **Parameterized Tests**: `[Theory]` with `[InlineData]` or `[AutoData]`
- **Integration Tests**: `[Fact]` with database setup
- **Component Tests**: bUnit with Blazor components

### Test Coverage

- **Minimum 80% code coverage** for new features
- **100% coverage** for critical business logic
- **Edge cases** should be tested
- **Error scenarios** must be covered

### Running Tests

```bash
# All tests
dotnet test

# Specific project
dotnet test tests/BlazorApp.UnitTests

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Using custom script
powershell scripts/run-tests.ps1 -TestType unit -Coverage
```

## 🔀 Pull Request Process

### Before Submitting

- [ ] Code follows style guidelines
- [ ] All tests pass locally
- [ ] New features have tests
- [ ] Documentation updated (if needed)
- [ ] No sensitive data in commits
- [ ] Branch is up to date with `develop`

### PR Template

```markdown
## Description
Brief description of changes made.

## Type of Change
- [ ] Bug fix (non-breaking change)
- [ ] New feature (non-breaking change)
- [ ] Breaking change (fix or feature causing existing functionality to break)
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] All tests pass locally

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No merge conflicts
```

### Review Process

1. **Automated checks** must pass (build, tests, linting)
2. **At least one reviewer** approval required
3. **Security review** for sensitive changes
4. **Architecture review** for significant changes
5. **Squash and merge** for clean history

## 🐛 Issue Reporting

### Bug Reports

Use the bug report template and include:

- **Environment details** (OS, .NET version, browser)
- **Steps to reproduce** the issue
- **Expected vs actual behavior**
- **Screenshots or logs** (if applicable)
- **Minimal reproduction** example

### Feature Requests

For new features, provide:

- **Clear description** of the feature
- **Use case** and business value
- **Proposed implementation** (if you have ideas)
- **Acceptance criteria**

### Security Issues

**DO NOT** create public issues for security vulnerabilities.
Instead, email: security@yourdomain.com

## 🏷️ Commit Message Guidelines

We follow **Conventional Commits** specification:

### Format
```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Types
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples
```bash
feat(auth): add JWT authentication
fix(user): resolve email validation issue
docs: update API documentation
test(user): add user service unit tests
refactor(cache): improve cache service performance
```

## 🚀 Release Process

### Version Numbers

We use **Semantic Versioning** (SemVer):
- `MAJOR.MINOR.PATCH` (e.g., `1.2.3`)
- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Release Workflow

1. **Create release branch** from `develop`
2. **Update version numbers** and changelog
3. **Final testing** and bug fixes
4. **Merge to main** and tag release
5. **Deploy to production**
6. **Merge back to develop**

## 📚 Additional Resources

- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [xUnit Testing](https://xunit.net/)

## 🙋‍♀️ Getting Help

- **Documentation**: Check the `docs/` folder
- **Issues**: Search existing issues first
- **Discussions**: Use GitHub Discussions for questions
- **Chat**: Join our development chat (link TBD)

---

## 🎉 Recognition

Contributors will be recognized in:
- **CONTRIBUTORS.md** file
- **Release notes** for significant contributions
- **Annual contributor appreciation** post

Thank you for contributing to BlazorWithRules! 🚀

---

*Last Updated: September 17, 2025*  
*Next Review: October 17, 2025*
