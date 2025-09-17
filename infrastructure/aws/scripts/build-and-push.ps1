# BlazorWithRules Container Build and Push Script
# This script builds the Docker container and pushes it to ECR

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment,
    
    [Parameter(Mandatory = $false)]
    [string]$ImageTag = "latest",
    
    [Parameter(Mandatory = $false)]
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $ScriptDir))
$DockerfilePath = Join-Path $ProjectRoot "Dockerfile.prod"

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

function Get-AwsAccountInfo {
    Write-ColorOutput "🔐 Getting AWS account information..." "Info"
    
    try {
        $accountInfo = aws sts get-caller-identity --output json | ConvertFrom-Json
        $region = aws configure get region
        
        if (-not $region) {
            $region = "us-east-1"
            Write-ColorOutput "  ⚠️  No default region set, using us-east-1" "Warning"
        }
        
        Write-ColorOutput "  Account ID: $($accountInfo.Account)" "Info"
        Write-ColorOutput "  Region: $region" "Info"
        
        return @{
            AccountId = $accountInfo.Account
            Region = $region
        }
    }
    catch {
        Write-ColorOutput "  ❌ Failed to get AWS account information" "Error"
        throw "AWS CLI not configured or credentials invalid"
    }
}

function Test-Dockerfile {
    Write-ColorOutput "📋 Checking Dockerfile..." "Info"
    
    if (-not (Test-Path $DockerfilePath)) {
        Write-ColorOutput "  ❌ Dockerfile not found: $DockerfilePath" "Error"
        throw "Production Dockerfile not found"
    }
    
    Write-ColorOutput "  ✅ Dockerfile found" "Success"
}

function Invoke-DockerBuild {
    param(
        [string]$ImageName,
        [string]$Tag
    )
    
    Write-ColorOutput "🐳 Building Docker image..." "Info"
    Write-ColorOutput "  Image: $ImageName" "Info"
    Write-ColorOutput "  Tag: $Tag" "Info"
    Write-ColorOutput "  Dockerfile: $DockerfilePath" "Info"
    
    $buildArgs = @(
        "build",
        "-f", $DockerfilePath,
        "-t", "$ImageName`:$Tag",
        "-t", "$ImageName`:latest"
    )
    
    if ($Verbose) {
        $buildArgs += "--progress=plain"
    }
    
    $buildArgs += $ProjectRoot
    
    try {
        & docker @buildArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Docker build failed"
        }
        
        Write-ColorOutput "  ✅ Docker image built successfully" "Success"
    }
    catch {
        Write-ColorOutput "  ❌ Docker build failed" "Error"
        throw "Docker build failed with exit code $LASTEXITCODE"
    }
}

function Invoke-ECRLogin {
    param(
        [string]$Region
    )
    
    Write-ColorOutput "🔐 Logging into ECR..." "Info"
    
    try {
        aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin "$($env:AWS_ACCOUNT_ID).dkr.ecr.$Region.amazonaws.com"
        
        if ($LASTEXITCODE -ne 0) {
            throw "ECR login failed"
        }
        
        Write-ColorOutput "  ✅ Successfully logged into ECR" "Success"
    }
    catch {
        Write-ColorOutput "  ❌ ECR login failed" "Error"
        throw "Failed to login to ECR"
    }
}

function Invoke-DockerPush {
    param(
        [string]$ImageName,
        [string]$Tag
    )
    
    Write-ColorOutput "📤 Pushing Docker image to ECR..." "Info"
    Write-ColorOutput "  Image: $ImageName`:$Tag" "Info"
    
    try {
        # Push specific tag
        docker push "$ImageName`:$Tag"
        
        if ($LASTEXITCODE -ne 0) {
            throw "Docker push failed for tag $Tag"
        }
        
        # Push latest tag if it's different
        if ($Tag -ne "latest") {
            docker push "$ImageName`:latest"
            
            if ($LASTEXITCODE -ne 0) {
                throw "Docker push failed for latest tag"
            }
        }
        
        Write-ColorOutput "  ✅ Docker image pushed successfully" "Success"
    }
    catch {
        Write-ColorOutput "  ❌ Docker push failed" "Error"
        throw "Docker push failed with exit code $LASTEXITCODE"
    }
}

function Test-ECRRepository {
    param(
        [string]$RepositoryName,
        [string]$Region
    )
    
    Write-ColorOutput "🔍 Checking ECR repository..." "Info"
    
    try {
        $repo = aws ecr describe-repositories --repository-names $RepositoryName --region $Region --output json | ConvertFrom-Json
        
        if ($repo.repositories.Count -eq 0) {
            Write-ColorOutput "  ❌ ECR repository not found: $RepositoryName" "Error"
            throw "ECR repository does not exist"
        }
        
        Write-ColorOutput "  ✅ ECR repository found" "Success"
        return $repo.repositories[0].repositoryUri
    }
    catch {
        Write-ColorOutput "  ❌ Failed to check ECR repository" "Error"
        throw "ECR repository check failed"
    }
}

function Show-BuildSummary {
    param(
        [string]$ImageName,
        [string]$Tag,
        [string]$RepositoryUri
    )
    
    Write-ColorOutput "📊 Build Summary" "Header"
    Write-ColorOutput "===============" "Header"
    Write-ColorOutput "Environment: $Environment" "Info"
    Write-ColorOutput "Image Name: $ImageName" "Info"
    Write-ColorOutput "Tag: $Tag" "Info"
    Write-ColorOutput "Repository URI: $RepositoryUri" "Info"
    Write-ColorOutput "Full Image: $RepositoryUri`:$Tag" "Info"
}

function Main {
    Write-ColorOutput "🐳 BlazorWithRules Container Build and Push" "Header"
    Write-ColorOutput "===========================================" "Header"
    Write-ColorOutput "Environment: $Environment" "Info"
    Write-ColorOutput "Image Tag: $ImageTag" "Info"
    Write-ColorOutput ""
    
    try {
        # Get AWS account information
        $awsInfo = Get-AwsAccountInfo
        
        # Check Dockerfile
        Test-Dockerfile
        
        # Check ECR repository
        $repositoryName = "blazorwithrules-$Environment"
        $repositoryUri = Test-ECRRepository -RepositoryName $repositoryName -Region $awsInfo.Region
        
        # Login to ECR
        Invoke-ECRLogin -Region $awsInfo.Region
        
        # Build Docker image
        Invoke-DockerBuild -ImageName $repositoryUri -Tag $ImageTag
        
        # Push Docker image
        Invoke-DockerPush -ImageName $repositoryUri -Tag $ImageTag
        
        # Show build summary
        Show-BuildSummary -ImageName $repositoryUri -Tag $ImageTag -RepositoryUri $repositoryUri
        
        Write-ColorOutput ""
        Write-ColorOutput "🎉 Container build and push completed successfully!" "Success"
    }
    catch {
        Write-ColorOutput ""
        Write-ColorOutput "❌ Build and push failed: $($_.Exception.Message)" "Error"
        exit 1
    }
}

# Run main function
Main
