# BlazorWithRules AWS Deployment Script
# This script handles the complete deployment pipeline for AWS

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipInfrastructure,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipApplication,
    
    [Parameter(Mandatory = $false)]
    [switch]$Force,
    
    [Parameter(Mandatory = $false)]
    [string]$ImageTag = "latest",
    
    [Parameter(Mandatory = $false)]
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Configuration
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$CdkDir = Join-Path $ScriptDir "..\cdk"
$ConfigFile = Join-Path $ScriptDir "..\config\$Environment.json"

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

function Test-Prerequisites {
    Write-ColorOutput "🔍 Checking prerequisites..." "Info"
    
    $prerequisites = @(
        @{ Name = "AWS CLI"; Command = "aws --version" },
        @{ Name = "Docker"; Command = "docker --version" },
        @{ Name = "Node.js"; Command = "node --version" },
        @{ Name = "CDK"; Command = "cdk --version" }
    )
    
    foreach ($prereq in $prerequisites) {
        try {
            $null = Invoke-Expression $prereq.Command 2>$null
            Write-ColorOutput "  ✅ $($prereq.Name)" "Success"
        }
        catch {
            Write-ColorOutput "  ❌ $($prereq.Name) - Not found or not working" "Error"
            throw "Prerequisite check failed: $($prereq.Name)"
        }
    }
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
        Write-ColorOutput "  User: $($accountInfo.Arn)" "Info"
        
        return @{
            AccountId = $accountInfo.Account
            Region = $region
            UserArn = $accountInfo.Arn
        }
    }
    catch {
        Write-ColorOutput "  ❌ Failed to get AWS account information" "Error"
        throw "AWS CLI not configured or credentials invalid"
    }
}

function Test-EnvironmentConfig {
    Write-ColorOutput "📋 Validating environment configuration..." "Info"
    
    if (-not (Test-Path $ConfigFile)) {
        Write-ColorOutput "  ❌ Configuration file not found: $ConfigFile" "Error"
        throw "Environment configuration file missing"
    }
    
    try {
        $config = Get-Content $ConfigFile -Raw | ConvertFrom-Json
        Write-ColorOutput "  ✅ Configuration loaded for $Environment" "Success"
        return $config
    }
    catch {
        Write-ColorOutput "  ❌ Invalid JSON in configuration file" "Error"
        throw "Configuration file is not valid JSON"
    }
}

function Invoke-InfrastructureDeployment {
    param(
        [hashtable]$AwsInfo,
        [object]$Config
    )
    
    Write-ColorOutput "🏗️  Deploying infrastructure..." "Header"
    
    Push-Location $CdkDir
    
    try {
        # Install dependencies if needed
        if (-not (Test-Path "node_modules")) {
            Write-ColorOutput "  📦 Installing CDK dependencies..." "Info"
            npm install
        }
        
        # Bootstrap CDK if needed
        Write-ColorOutput "  🚀 Bootstrapping CDK..." "Info"
        cdk bootstrap --context environment=$Environment
        
        # Deploy infrastructure
        Write-ColorOutput "  🚀 Deploying CDK stack..." "Info"
        $deployArgs = @(
            "deploy",
            "--all",
            "--context", "environment=$Environment",
            "--require-approval", "never"
        )
        
        if ($Verbose) {
            $deployArgs += "--verbose"
        }
        
        & cdk @deployArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "CDK deployment failed"
        }
        
        Write-ColorOutput "  ✅ Infrastructure deployed successfully" "Success"
    }
    finally {
        Pop-Location
    }
}

function Invoke-ApplicationDeployment {
    param(
        [hashtable]$AwsInfo,
        [object]$Config
    )
    
    Write-ColorOutput "🚀 Deploying application..." "Header"
    
    # Build and push container
    Write-ColorOutput "  🐳 Building and pushing container..." "Info"
    & "$ScriptDir\build-and-push.ps1" -Environment $Environment -ImageTag $ImageTag -Verbose:$Verbose
    
    if ($LASTEXITCODE -ne 0) {
        throw "Container build and push failed"
    }
    
    # Update ECS service
    Write-ColorOutput "  🔄 Updating ECS service..." "Info"
    $clusterName = "blazorwithrules-$Environment"
    $serviceName = "BlazorWithRules-$Environment-Service"
    
    try {
        aws ecs update-service `
            --cluster $clusterName `
            --service $serviceName `
            --force-new-deployment `
            --region $AwsInfo.Region
        
        Write-ColorOutput "  ✅ ECS service updated successfully" "Success"
    }
    catch {
        Write-ColorOutput "  ❌ Failed to update ECS service" "Error"
        throw "ECS service update failed"
    }
}

function Show-DeploymentSummary {
    param(
        [hashtable]$AwsInfo
    )
    
    Write-ColorOutput "📊 Deployment Summary" "Header"
    Write-ColorOutput "===================" "Header"
    Write-ColorOutput "Environment: $Environment" "Info"
    Write-ColorOutput "Account: $($AwsInfo.AccountId)" "Info"
    Write-ColorOutput "Region: $($AwsInfo.Region)" "Info"
    Write-ColorOutput "Image Tag: $ImageTag" "Info"
    
    # Get load balancer DNS
    try {
        $stackName = "BlazorWithRules-$Environment"
        $outputs = aws cloudformation describe-stacks --stack-name $stackName --query "Stacks[0].Outputs" --output json | ConvertFrom-Json
        
        foreach ($output in $outputs) {
            switch ($output.OutputKey) {
                "LoadBalancerDNS" {
                    Write-ColorOutput "Load Balancer: http://$($output.OutputValue)" "Info"
                }
                "ECRRepositoryURI" {
                    Write-ColorOutput "ECR Repository: $($output.OutputValue)" "Info"
                }
            }
        }
    }
    catch {
        Write-ColorOutput "  ⚠️  Could not retrieve deployment outputs" "Warning"
    }
}

function Main {
    Write-ColorOutput "🚀 BlazorWithRules AWS Deployment" "Header"
    Write-ColorOutput "=================================" "Header"
    Write-ColorOutput "Environment: $Environment" "Info"
    Write-ColorOutput "Image Tag: $ImageTag" "Info"
    Write-ColorOutput ""
    
    try {
        # Check prerequisites
        Test-Prerequisites
        
        # Get AWS account information
        $awsInfo = Get-AwsAccountInfo
        
        # Validate environment configuration
        $config = Test-EnvironmentConfig
        
        # Deploy infrastructure
        if (-not $SkipInfrastructure) {
            Invoke-InfrastructureDeployment -AwsInfo $awsInfo -Config $config
        }
        else {
            Write-ColorOutput "⏭️  Skipping infrastructure deployment" "Warning"
        }
        
        # Deploy application
        if (-not $SkipApplication) {
            Invoke-ApplicationDeployment -AwsInfo $awsInfo -Config $config
        }
        else {
            Write-ColorOutput "⏭️  Skipping application deployment" "Warning"
        }
        
        # Show deployment summary
        Show-DeploymentSummary -AwsInfo $awsInfo
        
        Write-ColorOutput ""
        Write-ColorOutput "🎉 Deployment completed successfully!" "Success"
    }
    catch {
        Write-ColorOutput ""
        Write-ColorOutput "❌ Deployment failed: $($_.Exception.Message)" "Error"
        exit 1
    }
}

# Run main function
Main
