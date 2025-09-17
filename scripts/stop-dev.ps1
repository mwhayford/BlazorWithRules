# PowerShell script to stop the development environment
# Usage: .\scripts\stop-dev.ps1

Write-Host "🛑 Stopping BlazorWithRules Development Environment..." -ForegroundColor Yellow

# Stop all services
docker-compose down

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Services stopped successfully!" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to stop services. Check the logs with: docker-compose logs" -ForegroundColor Red
    exit 1
}

# Ask if user wants to remove volumes
$response = Read-Host "Do you want to remove volumes (this will delete all data)? (y/N)"
if ($response -eq "y" -or $response -eq "Y") {
    Write-Host "🗑️ Removing volumes..." -ForegroundColor Yellow
    docker-compose down -v
    Write-Host "✅ Volumes removed!" -ForegroundColor Green
}

Write-Host "👋 Development environment stopped!" -ForegroundColor Green
