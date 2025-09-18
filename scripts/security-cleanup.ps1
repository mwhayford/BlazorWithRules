# Security Cleanup Script - Remove sensitive data from documentation

Write-Host "🔐 Starting security cleanup of documentation files..." -ForegroundColor Cyan

# List of files to clean
$Files = @(
    "docs/Pre-commit-Security-Correction.md",
    "docs/Git-Secrets-Integration.md", 
    "docs/Troubleshooting-Guide.md",
    "docs/Secure-Database-Connectivity.md",
    "docs/Performance-Profiling-Tools.md",
    "docs/Getting-Started.md",
    "docs/Docker-Development-Setup.md"
)

# Sensitive patterns to replace
$Replacements = @{
    "YourStrong@Passw0rd" = "[REDACTED_PASSWORD]"
    "[REDACTED_CLIENT_ID]" = "[REDACTED_CLIENT_ID]"
    "[REDACTED_CLIENT_SECRET]" = "[REDACTED_CLIENT_SECRET]"
    "[REDACTED_CLIENT_ID_PREFIX]" = "[REDACTED_CLIENT_ID_PREFIX]"
    "[REDACTED_SECRET_PREFIX]" = "[REDACTED_SECRET_PREFIX]"
}

# Process each file
foreach ($File in $Files) {
    if (Test-Path $File) {
        Write-Host "Processing: $File" -ForegroundColor Yellow
        
        # Create backup
        Copy-Item $File "$File.backup"
        
        # Apply replacements
        $content = Get-Content $File -Raw
        foreach ($pattern in $Replacements.Keys) {
            $replacement = $Replacements[$pattern]
            $content = $content -replace [regex]::Escape($pattern), $replacement
        }
        
        Set-Content $File $content
        Write-Host "✅ Cleaned: $File" -ForegroundColor Green
    } else {
        Write-Host "⚠️  File not found: $File" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🔐 Security cleanup completed!" -ForegroundColor Green
Write-Host "📁 Backup files created with .backup extension" -ForegroundColor Cyan
Write-Host "🚨 Please review changes before committing" -ForegroundColor Red
