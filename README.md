# 🏦 BlazorWithRules - Loan Application System

A modern, secure loan application system built with **Blazor Server**, **ASP.NET Core**, and **Entity Framework Core**. Features Google OAuth authentication, multi-step loan application workflow, TILA compliance, and comprehensive business logic validation.

[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download)
[![Blazor](https://img.shields.io/badge/Blazor-Server-purple.svg)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED.svg)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 🚀 Quick Start

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Recommended)
- [.NET 9 SDK](https://dotnet.microsoft.com/download) (Alternative)
- [Git](https://git-scm.com/)

### 🐳 Docker Setup (Recommended)

```bash
# Clone the repository
git clone https://github.com/mwhayford/BlazorWithRules.git
cd BlazorWithRules

# Start all services
docker-compose up -d

# Access the application
# 🌐 Main App: http://localhost:5000
# 🗄️ Database Admin: http://localhost:8080
# 🔴 Redis Admin: http://localhost:8081
```

### 💻 Local Development Setup

```bash
# Clone and navigate
git clone https://github.com/mwhayford/BlazorWithRules.git
cd BlazorWithRules

# Restore packages
dotnet restore

# Run database migrations
dotnet ef database update --project src/BlazorApp.Infrastructure --startup-project src/BlazorApp.Web

# Start the application
dotnet run --project src/BlazorApp.Web
```

## 🏗️ Architecture

### **Clean Architecture Pattern**

```
src/
├── BlazorApp.Core/           # Business logic, entities, interfaces
├── BlazorApp.Infrastructure/ # Data access, external services
├── BlazorApp.Shared/         # DTOs, models, exceptions
└── BlazorApp.Web/           # Blazor UI, controllers, middleware
```

### **Key Components**

- **🏦 Loan Application System** - Multi-step wizard with TILA compliance
- **🔐 Google OAuth Authentication** - ASP.NET Core Identity integration
- **👥 Role-Based Authorization** - Admin and Member roles
- **📊 Business Logic Validation** - 200+ FluentValidation rules
- **🗄️ Entity Framework Core** - SQL Server with migrations
- **⚡ Redis Caching** - Performance optimization
- **🐳 Docker Infrastructure** - Complete containerized environment

## 🎯 Features

### **Core Business Features**

- ✅ **Multi-Step Loan Application** (Demographics → Income → TILA)
- ✅ **TILA Compliance** - Truth in Lending Act disclosures
- ✅ **Real-Time Loan Calculations** - APR, finance charges, monthly payments
- ✅ **Application Status Management** - Draft, Submitted, Under Review, Approved, Rejected
- ✅ **Credit Score Integration** - Dynamic interest rate calculation
- ✅ **Debt-to-Income Ratio Validation** - Business rule enforcement

### **Authentication & Security**

- ✅ **Google OAuth Integration** - Secure social login
- ✅ **ASP.NET Core Identity** - User management and sessions
- ✅ **Role-Based Authorization** - Admin and Member access levels
- ✅ **GitHub Secrets Management** - Secure credential handling
- ✅ **Security Headers** - XSS, CSRF, and content security policies
- ✅ **Input Validation** - Multi-layer validation with FluentValidation

### **Development & Infrastructure**

- ✅ **Docker Development Environment** - SQL Server, Redis, web interfaces
- ✅ **Health Checks** - Service monitoring and diagnostics
- ✅ **Comprehensive Testing** - Unit, integration, and UI tests
- ✅ **Hot Reload Support** - Fast development iteration
- ✅ **Database Management Tools** - Adminer and Redis Commander
- ✅ **AWS Deployment Ready** - Infrastructure as Code with CDK

## 🛠️ Technology Stack

### **Backend**

- **.NET 9** - Latest framework with performance improvements
- **ASP.NET Core** - Web framework and API
- **Blazor Server** - Interactive web UI
- **Entity Framework Core** - ORM with SQL Server
- **FluentValidation** - Business rule validation
- **Serilog** - Structured logging

### **Frontend**

- **Blazor Components** - Server-side rendering
- **Bootstrap 5** - Responsive UI framework
- **Bootstrap Icons** - Icon library
- **JavaScript Interop** - Client-side functionality

### **Infrastructure**

- **Docker & Docker Compose** - Containerization
- **SQL Server 2022** - Primary database
- **Redis 7** - Caching and session storage
- **AWS CDK** - Infrastructure as Code
- **GitHub Actions** - CI/CD pipeline

### **Development Tools**

- **Visual Studio Enterprise** - Primary IDE
- **GitHub CLI** - Repository management
- **PowerShell** - Automation scripts
- **Adminer** - Database administration
- **Redis Commander** - Redis management

## 📁 Project Structure

```
BlazorWithRules/
├── src/
│   ├── BlazorApp.Core/              # Business logic layer
│   │   ├── Entities/               # Domain entities
│   │   ├── Interfaces/              # Service contracts
│   │   ├── Services/                # Business services
│   │   └── Validators/              # FluentValidation rules
│   ├── BlazorApp.Infrastructure/    # Data access layer
│   │   ├── Data/                   # DbContext and configurations
│   │   ├── Repositories/           # Data access implementations
│   │   └── Services/                # External service implementations
│   ├── BlazorApp.Shared/           # Shared models and DTOs
│   │   ├── Models/                  # Data transfer objects
│   │   ├── Exceptions/              # Custom exceptions
│   │   └── Constants/               # Application constants
│   └── BlazorApp.Web/              # Presentation layer
│       ├── Components/             # Blazor components
│       ├── Middleware/             # Custom middleware
│       └── Program.cs              # Application startup
├── tests/                          # Test projects
│   ├── BlazorApp.UnitTests/        # Unit tests
│   ├── BlazorApp.IntegrationTests/ # Integration tests
│   └── BlazorApp.UI.Tests/        # UI component tests
├── docs/                           # Documentation
├── infrastructure/                  # Infrastructure as Code
└── docker-compose.yml              # Docker services
```

## 🔧 Development Commands

### **Docker Commands**

```bash
# Start all services
docker-compose up -d

# Start with specific profiles
docker-compose --profile app --profile tools up -d

# View logs
docker-compose logs -f app

# Stop all services
docker-compose down

# Rebuild containers
docker-compose build --no-cache
```

### **Database Commands**

```bash
# Run migrations
docker exec blazorwithrules-app dotnet ef database update --context ApplicationDbContext

# Create new migration
dotnet ef migrations add MigrationName --project src/BlazorApp.Infrastructure --startup-project src/BlazorApp.Web

# Check database status
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "SELECT name FROM sys.databases"
```

### **Testing Commands**

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/BlazorApp.UnitTests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"
```

## 🔐 Security Features

### **Authentication**

- **Google OAuth 2.0** - Secure social login
- **ASP.NET Core Identity** - User management
- **Role-Based Access Control** - Admin and Member roles
- **Session Management** - Secure session handling

### **Authorization**

- **Policy-Based Authorization** - Flexible access control
- **Resource-Level Security** - Per-endpoint protection
- **Admin Dashboard** - Role-restricted functionality
- **Member Dashboard** - User-specific features

### **Data Protection**

- **Input Validation** - Multi-layer validation
- **SQL Injection Prevention** - Parameterized queries
- **XSS Protection** - Content Security Policy
- **CSRF Protection** - Anti-forgery tokens
- **Secrets Management** - GitHub Secrets integration

## 📊 Business Logic

### **Loan Application Workflow**

1. **Step 1: Demographics** - Personal information collection
2. **Step 2: Income** - Financial information and debt obligations
3. **Step 3: TILA Disclosure** - Truth in Lending Act compliance

### **Loan Calculations**

- **Annual Percentage Rate (APR)** - Based on credit score
- **Monthly Payment** - Amortization calculation
- **Finance Charge** - Total interest over loan term
- **Total Amount** - Principal + interest

### **Validation Rules**

- **200+ FluentValidation Rules** - Comprehensive business validation
- **Debt-to-Income Ratio** - Maximum 43% for approval
- **Minimum Income** - Based on loan amount and term
- **Credit Score Integration** - Dynamic interest rates

## 🚀 Deployment

### **Docker Deployment**

```bash
# Build production image
docker build -f Dockerfile.prod -t blazorwithrules:latest .

# Run production container
docker run -d -p 5000:80 --name blazorwithrules-prod blazorwithrules:latest
```

### **AWS Deployment**

```bash
# Deploy to AWS using CDK
cd infrastructure/aws/cdk
npm install
cdk deploy BlazorWithRulesStack --profile your-aws-profile
```

### **Environment Configuration**

```bash
# Set environment variables
export GOOGLE_CLIENT_ID="your-client-id"
export GOOGLE_CLIENT_SECRET="your-client-secret"
export ConnectionStrings__DefaultConnection="your-connection-string"
```

## 📚 Documentation

- **[Getting Started Guide](docs/Getting-Started.md)** - Quick setup instructions
- **[Docker Development Setup](docs/Docker-Development-Setup.md)** - Containerized development
- **[Security Guide](docs/Security-Guide.md)** - Security best practices
- **[Troubleshooting Guide](docs/Troubleshooting-Guide.md)** - Common issues and solutions
- **[AWS Deployment Guide](docs/AWS-Deployment-Guide.md)** - Cloud deployment
- **[Solution Analysis](docs/Solution-Analysis-Updated.md)** - Architecture and quality analysis

## 🧪 Testing

### **Test Coverage**

- **Unit Tests** - Business logic and services
- **Integration Tests** - Database and external services
- **UI Tests** - Blazor component testing
- **End-to-End Tests** - Complete user workflows

### **Running Tests**

```bash
# All tests
dotnet test

# Specific test categories
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=UI
```

## 🤝 Contributing

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

### **Development Guidelines**

- Follow **Clean Architecture** principles
- Write **comprehensive tests** for new features
- Use **FluentValidation** for business rules
- Follow **C# coding conventions**
- Update **documentation** for new features

## 📈 Performance

### **Optimization Features**

- **Redis Caching** - Frequently accessed data
- **Entity Framework Query Optimization** - Efficient database queries
- **Blazor Server Optimization** - Minimal re-rendering
- **Static File Caching** - CDN integration
- **Database Indexing** - Optimized query performance

### **Monitoring**

- **Health Checks** - Service status monitoring
- **Structured Logging** - Comprehensive application logs
- **Performance Counters** - Resource usage tracking
- **Error Tracking** - Exception monitoring

## 🐛 Troubleshooting

### **Common Issues**

| Issue                     | Solution                                         |
| ------------------------- | ------------------------------------------------ |
| Containers won't start    | `docker-compose down && docker-compose up -d`    |
| Database connection fails | `docker restart blazorwithrules-sqlserver`       |
| Authentication errors     | Check Google OAuth redirect URI                  |
| Build errors              | `dotnet clean && dotnet build`                   |
| Hot reload issues         | `taskkill /F /IM dotnet.exe && dotnet watch run` |

### **Getting Help**

1. **Check the [Troubleshooting Guide](docs/Troubleshooting-Guide.md)**
2. **Review application logs** using `docker logs blazorwithrules-app`
3. **Verify all services are running** using `docker ps`
4. **Open an issue** with detailed error information

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Microsoft** - .NET and Blazor frameworks
- **Google** - OAuth authentication
- **Bootstrap** - UI framework
- **Entity Framework** - Data access
- **Docker** - Containerization platform

## 📞 Support

- **Documentation**: [docs/](docs/)
- **Issues**: [GitHub Issues](https://github.com/mwhayford/BlazorWithRules/issues)
- **Discussions**: [GitHub Discussions](https://github.com/mwhayford/BlazorWithRules/discussions)

---

**Built with ❤️ using Blazor, .NET 9, and modern web technologies**

_Last Updated: January 18, 2025_  
_Version: 3.0_
