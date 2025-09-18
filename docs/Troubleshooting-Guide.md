# 🔧 Troubleshooting Guide - Developer Support

This guide provides solutions to common issues developers encounter when working with the BlazorWithRules application.

## 📋 Table of Contents

- [Quick Diagnostics](#quick-diagnostics)
- [Docker Issues](#docker-issues)
- [Database Issues](#database-issues)
- [Authentication Issues](#authentication-issues)
- [Build and Compilation Issues](#build-and-compilation-issues)
- [Runtime Issues](#runtime-issues)
- [Performance Issues](#performance-issues)
- [Development Environment Issues](#development-environment-issues)
- [Getting Help](#getting-help)

---

## 🚀 Quick Diagnostics

### **Health Check Commands**

```bash
# Check if all services are running
docker ps

# Check application health
curl http://localhost:5000/health

# Check database connectivity (secure)
./scripts/db-connect.sh "SELECT 1"

# Check Redis connectivity
docker exec blazorwithrules-redis redis-cli ping

# View application logs
docker logs blazorwithrules-app --tail 50
```

### **Common Status Codes**

| Status | Meaning         | Common Causes                   |
| ------ | --------------- | ------------------------------- |
| `200`  | ✅ OK           | Everything working              |
| `302`  | 🔄 Redirect     | Authentication redirect, normal |
| `400`  | ❌ Bad Request  | Invalid data, validation errors |
| `401`  | 🔒 Unauthorized | Authentication required         |
| `403`  | 🚫 Forbidden    | Insufficient permissions        |
| `404`  | 🔍 Not Found    | Missing page/endpoint           |
| `500`  | 💥 Server Error | Application error, check logs   |

---

## 🐳 Docker Issues

### **Problem: Containers Won't Start**

**Symptoms:**

- `docker-compose up` fails
- Containers exit immediately
- Port conflicts

**Solutions:**

```bash
# 1. Check for port conflicts
netstat -an | findstr :5000
netstat -an | findstr :1433
netstat -an | findstr :6379

# 2. Stop conflicting processes
taskkill /F /IM dotnet.exe
taskkill /F /IM sqlservr.exe

# 3. Clean up Docker resources
docker system prune -f
docker volume prune -f

# 4. Rebuild containers
docker-compose down
docker-compose build --no-cache
docker-compose up -d
```

### **Problem: Database Connection Issues**

**Symptoms:**

- `SqlException: A network-related or instance-specific error`
- `Login failed for user 'sa'`
- Database not accessible

**Solutions:**

```bash
# 1. Check SQL Server container status
docker logs blazorwithrules-sqlserver

# 2. Restart SQL Server container
docker restart blazorwithrules-sqlserver

# 3. Wait for SQL Server to be ready (can take 30-60 seconds)
./scripts/db-connect.sh "SELECT 1"

# 4. Check database exists
./scripts/db-query.sh "SELECT name FROM sys.databases"
```

### **Problem: Application Container Crashes**

**Symptoms:**

- Application starts then immediately exits
- `Exit code: 139` or similar
- No response on port 5000

**Solutions:**

```bash
# 1. Check application logs
docker logs blazorwithrules-app

# 2. Check for missing environment variables
docker exec blazorwithrules-app env | grep -E "(GOOGLE|ConnectionStrings)"

# 3. Verify database connectivity from container
docker exec blazorwithrules-app dotnet ef database update --context ApplicationDbContext

# 4. Check for missing migrations
docker exec blazorwithrules-app dotnet ef migrations list --context ApplicationDbContext
```

---

## 🗄️ Database Issues

### **Problem: Migration Errors**

**Symptoms:**

- `Invalid object name 'TableName'`
- `Migration already applied`
- `Database does not exist`

**Solutions:**

```bash
# 1. Check migration history
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -d BlazorWithRules -Q "SELECT * FROM __EFMigrationsHistory"

# 2. Reset migrations (DANGER: Data loss)
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -Q "DROP DATABASE BlazorWithRules"
docker exec blazorwithrules-app dotnet ef database update --context ApplicationDbContext

# 3. Create missing tables manually
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -d BlazorWithRules -Q "CREATE TABLE IF NOT EXISTS LoanApplications (...)"
```

### **Problem: Identity Tables Missing**

**Symptoms:**

- `Invalid object name 'AspNetUsers'`
- Authentication fails
- User management errors

**Solutions:**

```bash
# 1. Check if Identity tables exist
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -d BlazorWithRules -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'AspNet%'"

# 2. Create Identity tables manually (if missing)
# Use the create-identity-tables.sql script from the project
```

### **Problem: Data Seeding Issues**

**Symptoms:**

- `Violation of PRIMARY KEY constraint`
- Seeding fails on startup
- Duplicate data errors

**Solutions:**

```bash
# 1. Check existing data
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -d BlazorWithRules -Q "SELECT COUNT(*) FROM Users"

# 2. Clear existing data (if needed)
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -d BlazorWithRules -Q "DELETE FROM Users"

# 3. Disable seeding temporarily
# Comment out the seeding line in Program.cs
```

---

## 🔐 Authentication Issues

### **Problem: Google OAuth 500 Error**

**Symptoms:**

- `/signin-google` returns 500
- Google authentication fails
- Redirect URI errors

**Solutions:**

```bash
# 1. Check Google OAuth configuration
docker exec blazorwithrules-app env | grep GOOGLE

# 2. Verify redirect URI in Google Cloud Console
# Should be: http://localhost:5000/signin-google

# 3. Check application logs for specific errors
docker logs blazorwithrules-app | grep -i "google\|auth\|signin"

# 4. Test OAuth endpoint
curl -v http://localhost:5000/signin-google
```

### **Problem: Authentication Not Working**

**Symptoms:**

- Login button doesn't work
- User not authenticated after login
- Role-based access denied

**Solutions:**

```bash
# 1. Check if user exists in database
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -d BlazorWithRules -Q "SELECT Id, UserName, Email FROM AspNetUsers"

# 2. Check user roles
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -d BlazorWithRules -Q "SELECT u.UserName, r.Name FROM AspNetUsers u JOIN AspNetUserRoles ur ON u.Id = ur.UserId JOIN AspNetRoles r ON ur.RoleId = r.Id"

# 3. Verify authentication middleware order in Program.cs
# UseAuthentication() must come before UseAuthorization()
```

---

## 🔨 Build and Compilation Issues

### **Problem: StyleCop Errors**

**Symptoms:**

- `SA1028: Code should not contain trailing whitespace`
- `SA1514: Element documentation header should be preceded by blank line`
- Build fails due to StyleCop

**Solutions:**

```bash
# 1. Fix StyleCop errors automatically
dotnet format

# 2. Disable StyleCop during build (temporary)
dotnet build --property:RunAnalyzersDuringBuild=false

# 3. Fix specific StyleCop rules
# Add blank lines before XML documentation
# Remove trailing whitespace
# Fix parameter formatting
```

### **Problem: Missing Package References**

**Symptoms:**

- `CS0246: The type or namespace name 'X' could not be found`
- `CS0012: The type 'X' is defined in an assembly that is not referenced`

**Solutions:**

```bash
# 1. Restore packages
dotnet restore

# 2. Add missing package references
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.Google

# 3. Check project references
dotnet list reference
```

### **Problem: Entity Framework Errors**

**Symptoms:**

- `CS0246: The type or namespace name 'DbContext' could not be found`
- `CS0115: 'X.BuildRenderTree(RenderTreeBuilder)': no suitable method found to override`

**Solutions:**

```bash
# 1. Add EF Core packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools

# 2. Update using statements
# Add: using Microsoft.EntityFrameworkCore;

# 3. Fix Blazor component inheritance
# Use: LayoutComponentBase instead of ComponentBase
```

---

## ⚡ Runtime Issues

### **Problem: Hot Reload Not Working**

**Symptoms:**

- Changes not reflected in browser
- `HotReloadException: Attempted to invoke a deleted lambda`
- Hot reload fails

**Solutions:**

```bash
# 1. Stop all dotnet processes
taskkill /F /IM dotnet.exe

# 2. Clean and rebuild
dotnet clean
dotnet build

# 3. Restart with hot reload
dotnet watch run

# 4. Check CSP configuration for WebSocket support
# Ensure connect-src includes ws://localhost:*
```

### **Problem: JavaScript Interop Errors**

**Symptoms:**

- `JavaScript interop calls cannot be issued at this time`
- `This is because the component is being statically rendered`
- JSRuntime errors

**Solutions:**

```csharp
// 1. Move JSRuntime calls to OnAfterRenderAsync
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await JSRuntime.InvokeVoidAsync("console.log", "Component rendered");
    }
}

// 2. Check if component is prerendering
if (OperatingSystem.IsBrowser())
{
    await JSRuntime.InvokeVoidAsync("console.log", "Browser environment");
}
```

### **Problem: Validation Errors**

**Symptoms:**

- `ValidationException` thrown
- Form validation not working
- Client-side validation errors

**Solutions:**

```bash
# 1. Check validation rules
# Review FluentValidation rules in LoanApplicationValidator.cs

# 2. Check data annotations
# Ensure [Required], [Range], [StringLength] attributes are correct

# 3. Check validation error handling
# Ensure ValidationException is caught and handled properly
```

---

## 🚀 Performance Issues

### **Problem: Slow Database Queries**

**Symptoms:**

- Long response times
- Database timeout errors
- High CPU usage

**Solutions:**

```bash
# 1. Check database performance
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -Q "SELECT * FROM sys.dm_exec_query_stats ORDER BY total_elapsed_time DESC"

# 2. Add database indexes
# Review slow queries and add appropriate indexes

# 3. Enable query logging
# Set LogLevel for Microsoft.EntityFrameworkCore.Database.Command to Information
```

### **Problem: Memory Issues**

**Symptoms:**

- High memory usage
- OutOfMemoryException
- Slow performance

**Solutions:**

```bash
# 1. Check memory usage
docker stats blazorwithrules-app

# 2. Increase container memory limits
# Update docker-compose.yml with memory limits

# 3. Review caching strategy
# Check IMemoryCache usage and cache expiration
```

---

## 🛠️ Development Environment Issues

### **Problem: Port Conflicts**

**Symptoms:**

- `Address already in use`
- Services can't start
- Port binding errors

**Solutions:**

```bash
# 1. Find processes using ports
netstat -ano | findstr :5000
netstat -ano | findstr :1433

# 2. Kill conflicting processes
taskkill /PID <process_id> /F

# 3. Change ports in configuration
# Update docker-compose.yml with different ports
```

### **Problem: File Permission Issues**

**Symptoms:**

- `Access denied` errors
- Can't write to volumes
- Docker volume mounting fails

**Solutions:**

```bash
# 1. Check Docker Desktop permissions
# Ensure Docker Desktop has proper permissions

# 2. Run as Administrator (Windows)
# Right-click PowerShell and "Run as Administrator"

# 3. Check volume permissions
docker volume inspect blazorwithrules_sqlserver_data
```

### **Problem: Environment Variables Not Loading**

**Symptoms:**

- Configuration values are null
- Environment variables not found
- Docker secrets not working

**Solutions:**

```bash
# 1. Check environment variables
docker exec blazorwithrules-app env

# 2. Verify docker-compose.yml configuration
# Check environment section

# 3. Test configuration loading
docker exec blazorwithrules-app dotnet run --no-build -- --help
```

---

## 🆘 Getting Help

### **Before Asking for Help**

1. **Check this troubleshooting guide** for your specific issue
2. **Review application logs** using `docker logs blazorwithrules-app`
3. **Verify all services are running** using `docker ps`
4. **Test basic connectivity** using the health check commands

### **Information to Include**

When reporting issues, please include:

```bash
# System Information
docker --version
docker-compose --version
dotnet --version

# Service Status
docker ps
docker logs blazorwithrules-app --tail 20

# Error Details
# Full error message
# Steps to reproduce
# Expected vs actual behavior
```

### **Useful Commands for Debugging**

```bash
# View all logs
docker-compose logs

# Follow logs in real-time
docker-compose logs -f app

# Execute commands in container
docker exec -it blazorwithrules-app bash

# Check container resource usage
docker stats

# Inspect container configuration
docker inspect blazorwithrules-app
```

### **Common Solutions Summary**

| Issue                     | Quick Fix                                        |
| ------------------------- | ------------------------------------------------ |
| Containers won't start    | `docker-compose down && docker-compose up -d`    |
| Database connection fails | `docker restart blazorwithrules-sqlserver`       |
| Authentication errors     | Check Google OAuth redirect URI                  |
| Build errors              | `dotnet clean && dotnet build`                   |
| Hot reload issues         | `taskkill /F /IM dotnet.exe && dotnet watch run` |
| Port conflicts            | `netstat -ano \| findstr :5000`                  |

---

## 📚 Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Google OAuth Documentation](https://developers.google.com/identity/protocols/oauth2)

---

_Last Updated: January 18, 2025_  
_Version: 1.0_
