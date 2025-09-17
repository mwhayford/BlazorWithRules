# PowerShell script to start the development environment
# Usage: .\scripts\start-dev.ps1

Write-Host "🚀 Starting BlazorWithRules Development Environment..." -ForegroundColor Green

# Check if Docker is running
try {
    docker version | Out-Null
    Write-Host "✅ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Check if docker-compose is available
try {
    docker-compose version | Out-Null
    Write-Host "✅ Docker Compose is available" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker Compose is not available. Please install Docker Compose." -ForegroundColor Red
    exit 1
}

# Start the services
Write-Host "🐳 Starting Docker services..." -ForegroundColor Yellow
docker-compose --profile app --profile tools up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Services started successfully!" -ForegroundColor Green
    
    # Wait for services to be ready
    Write-Host "⏳ Waiting for services to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    # Check service health
    Write-Host "🔍 Checking service health..." -ForegroundColor Yellow
    
    # Check SQL Server
    try {
        docker-compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT 1" | Out-Null
        Write-Host "✅ SQL Server is ready" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ SQL Server may still be starting up..." -ForegroundColor Yellow
    }
    
    # Check Redis
    try {
        docker-compose exec -T redis redis-cli ping | Out-Null
        Write-Host "✅ Redis is ready" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Redis may still be starting up..." -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "🎉 Development Environment is ready!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Access URLs:" -ForegroundColor Cyan
    Write-Host "  • Blazor App (HTTP):  http://localhost:5000" -ForegroundColor White
    Write-Host "  • Blazor App (HTTPS): https://localhost:5001" -ForegroundColor White
    Write-Host "  • Database Admin:     http://localhost:8080" -ForegroundColor White
    Write-Host "  • Redis Admin:        http://localhost:8081" -ForegroundColor White
    Write-Host ""
    Write-Host "🔧 Database Connection:" -ForegroundColor Cyan
    Write-Host "  • Server: localhost,1433" -ForegroundColor White
    Write-Host "  • Username: sa" -ForegroundColor White
    Write-Host "  • Password: YourStrong@Passw0rd" -ForegroundColor White
    Write-Host "  • Database: BlazorWithRules" -ForegroundColor White
    Write-Host ""
    Write-Host "💡 Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Run database migrations: docker-compose exec app dotnet ef database update" -ForegroundColor White
    Write-Host "  2. View logs: docker-compose logs -f app" -ForegroundColor White
    Write-Host "  3. Stop services: docker-compose down" -ForegroundColor White
    
} else {
    Write-Host "❌ Failed to start services. Check the logs with: docker-compose logs" -ForegroundColor Red
    exit 1
}
