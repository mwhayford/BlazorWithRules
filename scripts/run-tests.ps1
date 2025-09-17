# BlazorApp Test Runner Script
# This script runs different types of tests in the BlazorApp solution

param(
    [string]$TestType = "all",  # Options: unit, integration, ui, all
    [switch]$SkipBuild,
    [switch]$Coverage,
    [switch]$Verbose
)

Write-Host "🧪 BlazorApp Test Runner" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green

# Set working directory to solution root
$SolutionRoot = Split-Path -Parent $PSScriptRoot
Set-Location $SolutionRoot

# Function to run tests with common parameters
function Run-Tests {
    param(
        [string]$Project,
        [string]$Name
    )
    
    Write-Host "🔄 Running $Name tests..." -ForegroundColor Blue
    
    $args = @("test", $Project)
    
    if ($SkipBuild) {
        $args += "--no-build"
    }
    
    if ($Coverage) {
        $args += "--collect:XPlat Code Coverage"
    }
    
    if ($Verbose) {
        $args += "--verbosity", "detailed"
    } else {
        $args += "--verbosity", "normal"
    }
    
    $args += "--logger", "console;verbosity=normal"
    
    try {
        & dotnet @args
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ $Name tests passed" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ $Name tests failed" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ Error running $Name tests: $_" -ForegroundColor Red
        return $false
    }
}

# Build solution if not skipping
if (-not $SkipBuild) {
    Write-Host "🔨 Building solution..." -ForegroundColor Blue
    & dotnet build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed. Exiting." -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Build successful" -ForegroundColor Green
}

$results = @{}

# Run specific test type or all tests
switch ($TestType.ToLower()) {
    "unit" {
        $results["Unit"] = Run-Tests "tests/BlazorApp.UnitTests" "Unit"
    }
    "integration" {
        Write-Host "⚠️  Integration tests require Docker to be running for Testcontainers" -ForegroundColor Yellow
        $results["Integration"] = Run-Tests "tests/BlazorApp.IntegrationTests" "Integration"
    }
    "ui" {
        $results["UI"] = Run-Tests "tests/BlazorApp.UI.Tests" "UI Component"
    }
    "all" {
        $results["Unit"] = Run-Tests "tests/BlazorApp.UnitTests" "Unit"
        
        Write-Host "⚠️  Integration tests require Docker to be running for Testcontainers" -ForegroundColor Yellow
        Write-Host "   If Docker is not available, integration tests will be skipped." -ForegroundColor Yellow
        $results["Integration"] = Run-Tests "tests/BlazorApp.IntegrationTests" "Integration"
        
        $results["UI"] = Run-Tests "tests/BlazorApp.UI.Tests" "UI Component"
    }
    default {
        Write-Host "❌ Invalid test type: $TestType. Valid options: unit, integration, ui, all" -ForegroundColor Red
        exit 1
    }
}

# Summary
Write-Host "`n📊 Test Results Summary" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan

$totalTests = 0
$passedTests = 0

foreach ($testType in $results.Keys) {
    $totalTests++
    if ($results[$testType]) {
        $passedTests++
        Write-Host "✅ $testType Tests: PASSED" -ForegroundColor Green
    } else {
        Write-Host "❌ $testType Tests: FAILED" -ForegroundColor Red
    }
}

Write-Host "`nOverall: $passedTests/$totalTests test suites passed" -ForegroundColor $(if ($passedTests -eq $totalTests) { "Green" } else { "Yellow" })

if ($Coverage -and (Test-Path "TestResults")) {
    Write-Host "`n📈 Code coverage reports generated in TestResults folder" -ForegroundColor Blue
}

# Exit with appropriate code
exit $(if ($passedTests -eq $totalTests) { 0 } else { 1 })
