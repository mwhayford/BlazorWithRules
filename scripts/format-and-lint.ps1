#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs formatting and linting tools on the codebase
.DESCRIPTION
    This script runs various formatting and linting tools to ensure code quality:
    - CSharpier for C# formatting
    - dotnet format for additional C# formatting
    - ESLint for JavaScript/TypeScript linting
    - Prettier for frontend file formatting
.PARAMETER Fix
    Whether to automatically fix issues where possible
.PARAMETER CheckOnly
    Only check for issues without fixing them
.EXAMPLE
    .\scripts\format-and-lint.ps1 -Fix
    .\scripts\format-and-lint.ps1 -CheckOnly
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$Fix,
    
    [Parameter(Mandatory = $false)]
    [switch]$CheckOnly
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Colors for output (using escape sequences compatible with PowerShell 5.1+)
$Red = if ($PSVersionTable.PSVersion.Major -ge 6) { "`e[31m" } else { "" }
$Green = if ($PSVersionTable.PSVersion.Major -ge 6) { "`e[32m" } else { "" }
$Yellow = if ($PSVersionTable.PSVersion.Major -ge 6) { "`e[33m" } else { "" }
$Blue = if ($PSVersionTable.PSVersion.Major -ge 6) { "`e[34m" } else { "" }
$Reset = if ($PSVersionTable.PSVersion.Major -ge 6) { "`e[0m" } else { "" }

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = $Reset
    )
    Write-Host "${Color}${Message}${Reset}"
}

function Write-Section {
    param([string]$Title)
    Write-Host ""
    Write-ColorOutput "============================================" $Blue
    Write-ColorOutput $Title $Blue
    Write-ColorOutput "============================================" $Blue
}

function Test-CommandExists {
    param([string]$Command)
    return $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

# Main execution
try {
    Write-ColorOutput "Code Formatting and Linting Tool" $Green
    Write-ColorOutput "Fix: $Fix | CheckOnly: $CheckOnly" $Yellow
    
    # Determine mode
    $mode = if ($CheckOnly) { "check" } elseif ($Fix) { "fix" } else { "check" }
    Write-ColorOutput "Mode: $mode" $Yellow

    # C# Formatting with CSharpier
    Write-Section "C# Code Formatting (CSharpier)"
    if (Test-CommandExists "csharpier") {
        if ($mode -eq "fix") {
            Write-ColorOutput "Running CSharpier (format mode)..." $Yellow
            csharpier format .
        } else {
            Write-ColorOutput "Running CSharpier (check mode)..." $Yellow
            csharpier check .
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "[OK] CSharpier completed successfully" $Green
        } else {
            Write-ColorOutput "[ERROR] CSharpier found formatting issues" $Red
        }
    } else {
        Write-ColorOutput "[WARNING] CSharpier not found. Install with: dotnet tool install -g csharpier" $Yellow
    }

    # .NET Format
    Write-Section ".NET Format"
    if (Test-CommandExists "dotnet") {
        if ($mode -eq "fix") {
            Write-ColorOutput "Running dotnet format..." $Yellow
            dotnet format --verbosity minimal
        } else {
            Write-ColorOutput "Running dotnet format (verify-no-changes)..." $Yellow
            dotnet format --verify-no-changes --verbosity minimal
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "[OK] dotnet format completed successfully" $Green
        } else {
            Write-ColorOutput "[ERROR] dotnet format found issues" $Red
        }
    } else {
        Write-ColorOutput "[ERROR] dotnet CLI not found" $Red
        throw "dotnet CLI is required"
    }

    # Frontend Linting (ESLint)
    Write-Section "JavaScript/TypeScript Linting (ESLint)"
    if (Test-Path "node_modules/.bin/eslint" -PathType Leaf) {
        if ($mode -eq "fix") {
            Write-ColorOutput "Running ESLint with --fix..." $Yellow
            & "node_modules/.bin/eslint" "." "--ext" "js,mjs,ts" "--fix"
        } else {
            Write-ColorOutput "Running ESLint..." $Yellow
            & "node_modules/.bin/eslint" "." "--ext" "js,mjs,ts"
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "[OK] ESLint completed successfully" $Green
        } else {
            Write-ColorOutput "[ERROR] ESLint found issues" $Red
        }
    } else {
        Write-ColorOutput "[WARNING] ESLint not found. Run: npm install" $Yellow
    }

    # Frontend Formatting (Prettier)
    Write-Section "Frontend Formatting (Prettier)"
    if (Test-Path "node_modules/.bin/prettier" -PathType Leaf) {
        if ($mode -eq "fix") {
            Write-ColorOutput "Running Prettier with --write..." $Yellow
            & "node_modules/.bin/prettier" "." "--ignore-path" ".prettierignore" "--write"
        } else {
            Write-ColorOutput "Running Prettier with --check..." $Yellow
            & "node_modules/.bin/prettier" "." "--ignore-path" ".prettierignore" "--check"
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "[OK] Prettier completed successfully" $Green
        } else {
            Write-ColorOutput "[ERROR] Prettier found formatting issues" $Red
        }
    } else {
        Write-ColorOutput "[WARNING] Prettier not found. Run: npm install" $Yellow
    }

    # Build check
    Write-Section "Build Verification"
    Write-ColorOutput "Running build to verify code compiles..." $Yellow
    dotnet build --configuration Release --verbosity minimal --no-restore

    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "[OK] Build completed successfully" $Green
    } else {
        Write-ColorOutput "[ERROR] Build failed" $Red
        throw "Build verification failed"
    }

    Write-Section "Summary"
    Write-ColorOutput "All formatting and linting checks completed!" $Green
    Write-ColorOutput "Mode: $mode" $Yellow
    
    if ($mode -eq "check") {
        Write-ColorOutput "TIP: To automatically fix issues, run with -Fix parameter" $Blue
    }

} catch {
    Write-ColorOutput "[ERROR] Script failed: $($_.Exception.Message)" $Red
    exit 1
}
