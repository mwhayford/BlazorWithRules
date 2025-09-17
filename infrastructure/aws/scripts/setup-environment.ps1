# BlazorWithRules AWS Environment Setup Script
# This script sets up the AWS environment for deployment

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment,
    
    [Parameter(Mandatory = $false)]
    [string]$AwsProfile = "default",
    
    [Parameter(Mandatory = $false)]
    [string]$Region = "us-east-1",
    
    [Parameter(Mandatory = $false)]
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$CdkDir = Join-Path $ScriptDir "..\cdk"

# Colors for output
$Colors = @{
    Success = "Green"
    Error = "Red"
    Warning = "Yellow"
    Info = "Cyan"
    Header = "Magenta"
}

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Colors[$Color]
}

function Test-AwsCli {
    Write-ColorOutput "🔍 Checking AWS CLI..." "Info"
    
    try {
        $version = aws --version
        Write-ColorOutput "  ✅ AWS CLI found: $version" "Success"
        return $true
    }
    catch {
        Write-ColorOutput "  ❌ AWS CLI not found" "Error"
        return $false
    }
}

function Test-AwsCredentials {
    Write-ColorOutput "🔐 Checking AWS credentials..." "Info"
    
    try {
        $identity = aws sts get-caller-identity --profile $AwsProfile --output json | ConvertFrom-Json
        Write-ColorOutput "  ✅ AWS credentials valid" "Success"
        Write-ColorOutput "  Account: $($identity.Account)" "Info"
        Write-ColorOutput "  User: $($identity.Arn)" "Info"
        return $true
    }
    catch {
        Write-ColorOutput "  ❌ AWS credentials invalid or not configured" "Error"
        return $false
    }
}

function Test-Docker {
    Write-ColorOutput "🐳 Checking Docker..." "Info"
    
    try {
        $version = docker --version
        Write-ColorOutput "  ✅ Docker found: $version" "Success"
        return $true
    }
    catch {
        Write-ColorOutput "  ❌ Docker not found" "Error"
        return $false
    }
}

function Test-NodeJs {
    Write-ColorOutput "📦 Checking Node.js..." "Info"
    
    try {
        $version = node --version
        Write-ColorOutput "  ✅ Node.js found: $version" "Success"
        return $true
    }
    catch {
        Write-ColorOutput "  ❌ Node.js not found" "Error"
        return $false
    }
}

function Test-Cdk {
    Write-ColorOutput "🚀 Checking AWS CDK..." "Info"
    
    try {
        $version = cdk --version
        Write-ColorOutput "  ✅ AWS CDK found: $version" "Success"
        return $true
    }
    catch {
        Write-ColorOutput "  ❌ AWS CDK not found" "Error"
        return $false
    }
}

function Install-CdkDependencies {
    Write-ColorOutput "📦 Installing CDK dependencies..." "Info"
    
    Push-Location $CdkDir
    
    try {
        if (Test-Path "package.json") {
            npm install
            Write-ColorOutput "  ✅ CDK dependencies installed" "Success"
        }
        else {
            Write-ColorOutput "  ❌ package.json not found in CDK directory" "Error"
            throw "CDK package.json not found"
        }
    }
    catch {
        Write-ColorOutput "  ❌ Failed to install CDK dependencies" "Error"
        throw "CDK dependency installation failed"
    }
    finally {
        Pop-Location
    }
}

function Invoke-CdkBootstrap {
    Write-ColorOutput "🚀 Bootstrapping CDK..." "Info"
    
    Push-Location $CdkDir
    
    try {
        $bootstrapArgs = @(
            "bootstrap",
            "--context", "environment=$Environment"
        )
        
        if ($Verbose) {
            $bootstrapArgs += "--verbose"
        }
        
        & cdk @bootstrapArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "CDK bootstrap failed"
        }
        
        Write-ColorOutput "  ✅ CDK bootstrapped successfully" "Success"
    }
    catch {
        Write-ColorOutput "  ❌ CDK bootstrap failed" "Error"
        throw "CDK bootstrap failed"
    }
    finally {
        Pop-Location
    }
}

function Test-EcrRepository {
    param(
        [string]$RepositoryName
    )
    
    Write-ColorOutput "🔍 Checking ECR repository..." "Info"
    
    try {
        $repo = aws ecr describe-repositories --repository-names $RepositoryName --region $Region --profile $AwsProfile --output json | ConvertFrom-Json
        
        if ($repo.repositories.Count -gt 0) {
            Write-ColorOutput "  ✅ ECR repository exists: $RepositoryName" "Success"
            return $true
        }
        else {
            Write-ColorOutput "  ❌ ECR repository not found: $RepositoryName" "Error"
            return $false
        }
    }
    catch {
        Write-ColorOutput "  ❌ Failed to check ECR repository" "Error"
        return $false
    }
}

function New-EcrRepository {
    param(
        [string]$RepositoryName
    )
    
    Write-ColorOutput "📦 Creating ECR repository..." "Info"
    
    try {
        aws ecr create-repository `
            --repository-name $RepositoryName `
            --region $Region `
            --profile $AwsProfile `
            --image-scanning-configuration scanOnPush=true `
            --lifecycle-policy-text '{
                "rules": [
                    {
                        "rulePriority": 1,
                        "description": "Keep last 10 images",
                        "selection": {
                            "tagStatus": "any",
                            "countType": "imageCountMoreThan",
                            "countNumber": 10
                        },
                        "action": {
                            "type": "expire"
                        }
                    }
                ]
            }'
        
        Write-ColorOutput "  ✅ ECR repository created: $RepositoryName" "Success"
    }
    catch {
        Write-ColorOutput "  ❌ Failed to create ECR repository" "Error"
        throw "ECR repository creation failed"
    }
}

function Show-EnvironmentSummary {
    param(
        [string]$Environment,
        [string]$Region,
        [string]$AwsProfile
    )
    
    Write-ColorOutput "📊 Environment Setup Summary" "Header"
    Write-ColorOutput "============================" "Header"
    Write-ColorOutput "Environment: $Environment" "Info"
    Write-ColorOutput "Region: $Region" "Info"
    Write-ColorOutput "AWS Profile: $AwsProfile" "Info"
    Write-ColorOutput ""
    Write-ColorOutput "Next steps:" "Info"
    Write-ColorOutput "1. Run: .\deploy.ps1 -Environment $Environment" "Info"
    Write-ColorOutput "2. Monitor deployment in AWS Console" "Info"
    Write-ColorOutput "3. Test the application" "Info"
}

function Main {
    Write-ColorOutput "🔧 BlazorWithRules AWS Environment Setup" "Header"
    Write-ColorOutput "=======================================" "Header"
    Write-ColorOutput "Environment: $Environment" "Info"
    Write-ColorOutput "Region: $Region" "Info"
    Write-ColorOutput "AWS Profile: $AwsProfile" "Info"
    Write-ColorOutput ""
    
    try {
        # Check prerequisites
        $prerequisites = @(
            @{ Name = "AWS CLI"; Test = { Test-AwsCli } },
            @{ Name = "AWS Credentials"; Test = { Test-AwsCredentials } },
            @{ Name = "Docker"; Test = { Test-Docker } },
            @{ Name = "Node.js"; Test = { Test-NodeJs } },
            @{ Name = "AWS CDK"; Test = { Test-Cdk } }
        )
        
        foreach ($prereq in $prerequisites) {
            if (-not (& $prereq.Test)) {
                throw "Prerequisite check failed: $($prereq.Name)"
            }
        }
        
        # Install CDK dependencies
        Install-CdkDependencies
        
        # Bootstrap CDK
        Invoke-CdkBootstrap
        
        # Check/create ECR repository
        $repositoryName = "blazorwithrules-$Environment"
        if (-not (Test-EcrRepository -RepositoryName $repositoryName)) {
            New-EcrRepository -RepositoryName $repositoryName
        }
        
        # Show summary
        Show-EnvironmentSummary -Environment $Environment -Region $Region -AwsProfile $AwsProfile
        
        Write-ColorOutput ""
        Write-ColorOutput "🎉 Environment setup completed successfully!" "Success"
    }
    catch {
        Write-ColorOutput ""
        Write-ColorOutput "❌ Environment setup failed: $($_.Exception.Message)" "Error"
        exit 1
    }
}

# Run main function
Main
