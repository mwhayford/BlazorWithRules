# Universal PowerShell database connectivity script
# Works with both .env files and GitHub Secrets
param(
    [Parameter(Mandatory=$true)]
    [string]$Query,
    
    [string]$Database = "BlazorWithRules",
    [string]$Server = "localhost"
)

# Load from .env file (local development)
if (Test-Path ".env") {
    Get-Content ".env" | ForEach-Object {
        if ($_ -match "^([^#][^=]+)=(.*)$") {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            [Environment]::SetEnvironmentVariable($name, $value, "Process")
        }
    }
}

# Use environment variables (from GitHub Secrets in CI/CD)
$username = [Environment]::GetEnvironmentVariable("SQLSERVER_USERNAME", "Process")
$password = [Environment]::GetEnvironmentVariable("SQLSERVER_PASSWORD", "Process")
$database = [Environment]::GetEnvironmentVariable("SQLSERVER_DATABASE", "Process")
$server = [Environment]::GetEnvironmentVariable("SQLSERVER_SERVER", "Process")

if ([string]::IsNullOrEmpty($username)) { $username = "sa" }
if ([string]::IsNullOrEmpty($database)) { $database = $Database }
if ([string]::IsNullOrEmpty($server)) { $server = $Server }

if ([string]::IsNullOrEmpty($password)) {
    Write-Error "❌ SQLSERVER_PASSWORD not set"
    Write-Host ""
    Write-Host "For local development:"
    Write-Host "  1. Create .env file with: SQLSERVER_PASSWORD=YourStrong@Passw0rd"
    Write-Host "  2. Run: .\scripts\db-connect.ps1 -Query 'SELECT 1'"
    Write-Host ""
    Write-Host "For CI/CD:"
    Write-Host "  1. Set GitHub Secrets: SQLSERVER_PASSWORD"
    Write-Host "  2. Environment variables will be automatically loaded"
    exit 1
}

# Execute SQL command
Write-Host "🔍 Executing query: $Query" -ForegroundColor Cyan
Write-Host "📊 Database: $database" -ForegroundColor Cyan
Write-Host "👤 User: $username" -ForegroundColor Cyan

docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd `
    -S $server -U $username -P $password -C -d $database -Q $Query
