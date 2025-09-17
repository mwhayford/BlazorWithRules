# 🚀 Getting Started Guide

## Prerequisites

Before you begin, ensure you have the following installed on your development machine:

### Required Software

- **.NET 9 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Visual Studio 2022** (recommended) or **VS Code**
- **SQL Server** (LocalDB is sufficient for development)
- **Git** for version control

### Optional but Recommended

- **Docker Desktop** (for integration tests with Testcontainers)
- **Azure CLI** (for deployment)
- **SQL Server Management Studio** (for database management)

## Quick Start (5 Minutes)

### 1. Clone and Setup

```bash
# Clone the repository
git clone <repository-url>
cd BlazorWithRules

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build
```

### 2. Database Setup

```bash
# Navigate to the web project
cd src/BlazorApp.Web

# Run the application (database will be created automatically)
dotnet run
```

### 3. Access the Application

- **Main Application**: https://localhost:5001
- **Health Checks**: https://localhost:5001/health-ui
- **Logs**: Check `src/BlazorApp.Web/logs/` folder

### 4. Run Tests

```bash
# From solution root
dotnet test

# Or use the custom test runner
powershell -ExecutionPolicy Bypass -File scripts/run-tests.ps1 -TestType unit
```

## Development Environment Setup

### 1. Visual Studio Configuration

1. Open `BlazorWithRules.sln` in Visual Studio 2022
2. Set `BlazorApp.Web` as the startup project
3. Press F5 to run with debugging

### 2. VS Code Setup

1. Install the C# extension
2. Open the project folder in VS Code
3. Use `Ctrl+Shift+P` → ".NET: Generate Assets for Build and Debug"
4. Press F5 to start debugging

### 3. Database Configuration

#### Using LocalDB (Default)

The application is configured to use LocalDB by default. No additional setup required.

#### Using SQL Server

Update the connection string in `appsettings.Development.json`:

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=localhost;Database=BlazorAppDb_Dev;Integrated Security=true;TrustServerCertificate=true;"
    }
}
```

#### Using SQL Server in Docker

```bash
# Run SQL Server in Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest

# Update connection string
"DefaultConnection": "Server=localhost,1433;Database=BlazorAppDb_Dev;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
```

## Project Structure Overview

```
BlazorWithRules/
├── 🎯 src/
│   ├── BlazorApp.Web/         # UI Layer (Blazor Server)
│   ├── BlazorApp.Core/        # Business Logic & Domain
│   ├── BlazorApp.Infrastructure/ # Data Access & Services
│   └── BlazorApp.Shared/      # Shared Models & DTOs
├── 🧪 tests/
│   ├── BlazorApp.UnitTests/   # Fast, isolated tests
│   ├── BlazorApp.IntegrationTests/ # Database & API tests
│   └── BlazorApp.UI.Tests/    # Component tests
├── 📜 scripts/                # Automation scripts
├── 📚 docs/                   # Documentation
└── 🏗️ infrastructure/         # Deployment configs
```

## Common Development Tasks

### Adding a New Feature

1. **Create the domain entity** in `BlazorApp.Core/Entities/`
2. **Add the repository interface** in `BlazorApp.Core/Interfaces/`
3. **Implement the repository** in `BlazorApp.Infrastructure/Repositories/`
4. **Create the service** in `BlazorApp.Infrastructure/Services/`
5. **Add validation** in `BlazorApp.Core/Validators/`
6. **Create the Blazor page** in `BlazorApp.Web/Components/Pages/`
7. **Write tests** for each layer

### Running Specific Tests

```bash
# Unit tests only
dotnet test tests/BlazorApp.UnitTests

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Using custom script
powershell scripts/run-tests.ps1 -TestType unit -Coverage
```

### Database Migrations (Future)

```bash
# Add migration
dotnet ef migrations add MigrationName --project src/BlazorApp.Infrastructure

# Update database
dotnet ef database update --project src/BlazorApp.Web
```

### Viewing Logs

- **Console**: Logs appear in the terminal when running `dotnet run`
- **File**: Check `src/BlazorApp.Web/logs/blazorapp-YYYYMMDD.log`
- **Structured**: Logs are in JSON format for easy parsing

## Troubleshooting

### Common Issues

#### "Database connection failed"

1. Ensure SQL Server/LocalDB is running
2. Check connection string in `appsettings.Development.json`
3. Verify database permissions

#### "Package restore failed"

1. Clear NuGet cache: `dotnet nuget locals all --clear`
2. Restore packages: `dotnet restore`
3. Check internet connection and NuGet sources

#### "Tests failing"

1. Ensure Docker is running (for integration tests)
2. Check test output for specific errors
3. Run tests individually to isolate issues

#### "Port already in use"

1. Change the port in `Properties/launchSettings.json`
2. Or kill the process using the port:
    ```bash
    netstat -ano | findstr :5001
    taskkill /PID <PID> /F
    ```

### Getting Help

1. Check the logs in `src/BlazorApp.Web/logs/`
2. Review the health checks at `/health-ui`
3. Search existing issues in the repository
4. Create a new issue with detailed error information

## Next Steps

Once you have the application running:

1. **Explore the Dashboard** - Navigate to the main page
2. **Check Health Status** - Visit `/health-ui`
3. **Review the Code** - Start with `Program.cs` to understand the setup
4. **Run the Tests** - Get familiar with the testing patterns
5. **Read the Architecture** - Review `docs/Solution-Analysis.md`

## Development Guidelines

### Code Style

- Follow C# naming conventions
- Use meaningful variable and method names
- Keep methods small and focused
- Write self-documenting code

### Git Workflow

1. Create feature branches from `main`
2. Write tests before implementing features
3. Ensure all tests pass before committing
4. Write clear commit messages
5. Submit pull requests for review

### Testing Strategy

- **Unit Tests**: Test business logic in isolation
- **Integration Tests**: Test database and external dependencies
- **Component Tests**: Test UI components
- **Write tests first** when adding new features

---

**Happy Coding! 🎉**

_For more detailed information, see the complete documentation in the `docs/` folder._
