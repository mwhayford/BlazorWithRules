# BlazorWithRules

A modern Blazor Web Application built with .NET 9, featuring clean architecture, comprehensive testing, and Azure deployment capabilities.

## 🏗️ Architecture

This solution follows Clean Architecture principles with the following structure:

```
BlazorWithRules/
├── src/
│   ├── BlazorApp.Web/           # Blazor Server UI Layer
│   ├── BlazorApp.Core/          # Business Logic & Domain Models
│   ├── BlazorApp.Infrastructure/ # Data Access & External Services
│   └── BlazorApp.Shared/        # Shared Models & DTOs
├── tests/
│   ├── BlazorApp.UnitTests/     # Unit Tests
│   ├── BlazorApp.IntegrationTests/ # Integration Tests
│   └── BlazorApp.UI.Tests/      # UI Component Tests
└── infrastructure/
    └── azure/                   # Azure Deployment Files
```

## 🚀 Technology Stack

- **Frontend**: Blazor Server with .NET 9
- **UI Framework**: MudBlazor (Material Design)
- **Database**: Azure SQL Database with Entity Framework Core
- **Logging**: Serilog with Azure Application Insights
- **Testing**: xUnit, bUnit, Moq, FluentAssertions
- **Hosting**: Azure App Service
- **CI/CD**: GitHub Actions / Azure DevOps

## 🛠️ Development Setup

### Prerequisites

- .NET 9 SDK
- Visual Studio 2022 or VS Code
- SQL Server (LocalDB or full instance)
- Azure CLI (for deployment)

### Getting Started

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd BlazorWithRules
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   cd src/BlazorApp.Web
   dotnet run
   ```

5. **Run tests**
   ```bash
   dotnet test
   ```

## 🧪 Testing Strategy

The project implements Test-Driven Development (TDD) with three levels of testing:

- **Unit Tests**: Fast, isolated tests for business logic
- **Integration Tests**: Database and API integration testing
- **UI Tests**: Blazor component testing with bUnit

## 📊 Features

- **Dashboard**: Real-time analytics and KPI visualization
- **Data Grids**: Advanced filtering, sorting, and pagination
- **Charts**: Interactive charts and graphs
- **User Management**: Complete CRUD operations
- **Authentication**: Azure AD integration
- **Responsive Design**: Mobile-first approach

## 🔧 Configuration

The application uses the standard ASP.NET Core configuration system:

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- Environment variables and Azure Key Vault for secrets

## 🚀 Deployment

### Azure Deployment

1. **Setup Azure Resources**
   ```bash
   az group create --name rg-blazorapp --location eastus
   ```

2. **Deploy Infrastructure**
   ```bash
   az deployment group create --resource-group rg-blazorapp --template-file infrastructure/azure/main.json
   ```

3. **Deploy Application**
   ```bash
   dotnet publish -c Release
   az webapp deployment source config-zip --resource-group rg-blazorapp --name blazorapp --src publish.zip
   ```

## 📈 Monitoring

- **Application Insights**: Performance monitoring and diagnostics
- **Serilog**: Structured logging with multiple sinks
- **Health Checks**: Endpoint monitoring

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Implement the feature
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions, please open an issue in the GitHub repository.
