# Docker Development Environment Setup

This guide explains how to set up and use the Docker-based development environment for BlazorWithRules.

## 🐳 Overview

The Docker development environment provides:

- **SQL Server 2022** - Database server
- **Redis** - Caching and session storage
- **Blazor Application** - The main application
- **Adminer** - Database management interface
- **Redis Commander** - Redis management interface

## 📋 Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- [Docker Compose](https://docs.docker.com/compose/install/) (included with Docker Desktop)
- At least 4GB of available RAM
- Ports 1433, 5000, 5001, 6379, 8080, 8081 available

## 🚀 Quick Start

### 1. Clone and Navigate

```bash
git clone https://github.com/mwhayford/BlazorWithRules.git
cd BlazorWithRules
```

### 2. Start the Development Environment

```bash
# Start all services
docker-compose up -d

# Or start with specific profiles
docker-compose --profile app --profile tools up -d
```

### 3. Run Database Migrations

```bash
# Execute migrations
docker-compose exec app dotnet ef database update --project src/BlazorApp.Infrastructure
```

### 4. Access the Application

- **Blazor App**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Database Admin**: http://localhost:8080 (Adminer)
- **Redis Admin**: http://localhost:8081 (Redis Commander)

## 🔧 Service Details

### SQL Server

- **Port**: 1433
- **Username**: `sa`
- **Password**: `YourStrong@Passw0rd`
- **Database**: `BlazorWithRules`
- **Connection String**: `Server=localhost,1433;Database=BlazorWithRules;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true`

### Redis

- **Port**: 6379
- \*\*No authentication required for development
- **Memory limit**: 256MB

### Blazor Application

- **HTTP Port**: 5000
- **HTTPS Port**: 5001
- **Hot Reload**: Enabled
- **Environment**: Development

## 🛠️ Development Commands

### Start Services

```bash
# Start all services
docker-compose up -d

# Start with logs
docker-compose up

# Start specific services
docker-compose up sqlserver redis
```

### Stop Services

```bash
# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

### View Logs

```bash
# View all logs
docker-compose logs

# View specific service logs
docker-compose logs app
docker-compose logs sqlserver
```

### Execute Commands

```bash
# Run EF migrations
docker-compose exec app dotnet ef database update

# Run tests
docker-compose exec app dotnet test

# Access database
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd
```

## 🔍 Troubleshooting

### Common Issues

#### Port Conflicts

If you get port binding errors:

```bash
# Check what's using the port
netstat -ano | findstr :1433
netstat -ano | findstr :5000

# Stop conflicting services or change ports in docker-compose.yml
```

#### Database Connection Issues

```bash
# Check if SQL Server is ready
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1"

# Reset database
docker-compose down -v
docker-compose up -d sqlserver
```

#### Application Build Issues

```bash
# Rebuild the application
docker-compose build --no-cache app

# Check build logs
docker-compose logs app
```

### Reset Everything

```bash
# Stop and remove everything
docker-compose down -v --remove-orphans

# Remove all images
docker system prune -a

# Start fresh
docker-compose up -d
```

## 📁 File Structure

```
├── docker-compose.yml              # Main compose file
├── docker-compose.override.yml     # Development overrides
├── Dockerfile.dev                  # Development Dockerfile
├── scripts/
│   └── init-db.sql                # Database initialization
└── docs/
    └── Docker-Development-Setup.md # This file
```

## 🔐 Security Notes

⚠️ **Important**: The default passwords are for development only!

- **SQL Server SA Password**: `YourStrong@Passw0rd`
- **Application User Password**: `BlazorApp@Passw0rd123`

For production, change these passwords and use proper secrets management.

## 🚀 Production Considerations

This setup is optimized for development. For production:

1. Use proper secrets management
2. Remove development tools (Adminer, Redis Commander)
3. Use production-grade images
4. Configure proper networking and security
5. Set up monitoring and logging
6. Use external managed services (Azure SQL, Redis Cache)

## 📚 Additional Resources

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [SQL Server on Docker](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-docker-container-deployment)
- [Redis on Docker](https://hub.docker.com/_/redis)
- [Blazor Development](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
