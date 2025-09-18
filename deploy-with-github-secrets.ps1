# Deploy with GitHub secrets using environment variables
# This script sets environment variables and starts Docker

Write-Host "Setting up environment variables for Docker deployment..." -ForegroundColor Cyan

# Set environment variables (these will be used by docker-compose.yml)
# You can set these manually or use a .env file
$env:GOOGLE_CLIENT_ID = "[CONFIGURED]"
$env:GOOGLE_CLIENT_SECRET = "[CONFIGURED]"

Write-Host "Environment variables set!" -ForegroundColor Green
Write-Host "GOOGLE_CLIENT_ID: $($env:GOOGLE_CLIENT_ID.Substring(0,20))..." -ForegroundColor Yellow
Write-Host "GOOGLE_CLIENT_SECRET: $($env:GOOGLE_CLIENT_SECRET.Substring(0,10))..." -ForegroundColor Yellow

Write-Host "Starting Docker with GitHub secrets..." -ForegroundColor Cyan
docker-compose --profile app down
docker-compose build --no-cache app
docker-compose --profile app up -d

if ($LASTEXITCODE -eq 0) {
    Write-Host "Docker started with GitHub secrets!" -ForegroundColor Green
    Write-Host "Application should now be running at http://localhost:5000" -ForegroundColor Green
} else {
    Write-Host "Failed to start Docker with GitHub secrets." -ForegroundColor Red
}
