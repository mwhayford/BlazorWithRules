# BlazorWithRules AWS Cleanup Script
# This script removes AWS resources created by the deployment

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("dev", "staging", "prod")]
    [string]$Environment,
    
    [Parameter(Mandatory = $false)]
    [switch]$Force,
    
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

function Confirm-Cleanup {
    if (-not $Force) {
        Write-ColorOutput "⚠️  WARNING: This will delete ALL AWS resources for the $Environment environment!" "Warning"
        Write-ColorOutput "This action cannot be undone." "Warning"
        Write-ColorOutput ""
        
        $confirmation = Read-Host "Are you sure you want to continue? (yes/no)"
        
        if ($confirmation -ne "yes") {
            Write-ColorOutput "Cleanup cancelled." "Info"
            exit 0
        }
    }
}

function Invoke-CdkDestroy {
    Write-ColorOutput "🗑️  Destroying CDK stack..." "Info"
    
    Push-Location $CdkDir
    
    try {
        $destroyArgs = @(
            "destroy",
            "--all",
            "--context", "environment=$Environment",
            "--force"
        )
        
        if ($Verbose) {
            $destroyArgs += "--verbose"
        }
        
        & cdk @destroyArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "CDK destroy failed"
        }
        
        Write-ColorOutput "  ✅ CDK stack destroyed successfully" "Success"
    }
    catch {
        Write-ColorOutput "  ❌ CDK destroy failed" "Error"
        throw "CDK destroy failed"
    }
    finally {
        Pop-Location
    }
}

function Remove-EcrRepository {
    param(
        [string]$RepositoryName
    )
    
    Write-ColorOutput "🗑️  Removing ECR repository..." "Info"
    
    try {
        # Delete all images first
        $images = aws ecr list-images --repository-name $RepositoryName --query "imageIds[*]" --output json | ConvertFrom-Json
        
        if ($images.Count -gt 0) {
            Write-ColorOutput "  🗑️  Deleting $($images.Count) images from repository..." "Info"
            
            $imageIds = $images | ForEach-Object { @{ imageDigest = $_.imageDigest } }
            $imageIdsJson = $imageIds | ConvertTo-Json -Depth 3
            
            aws ecr batch-delete-image --repository-name $RepositoryName --image-ids $imageIdsJson
        }
        
        # Delete repository
        aws ecr delete-repository --repository-name $RepositoryName --force
        
        Write-ColorOutput "  ✅ ECR repository removed: $RepositoryName" "Success"
    }
    catch {
        Write-ColorOutput "  ❌ Failed to remove ECR repository" "Error"
        throw "ECR repository removal failed"
    }
}

function Remove-CloudWatchLogs {
    param(
        [string]$LogGroupName
    )
    
    Write-ColorOutput "🗑️  Removing CloudWatch log group..." "Info"
    
    try {
        aws logs delete-log-group --log-group-name $LogGroupName
        
        Write-ColorOutput "  ✅ CloudWatch log group removed: $LogGroupName" "Success"
    }
    catch {
        Write-ColorOutput "  ⚠️  CloudWatch log group may not exist or already deleted" "Warning"
    }
}

function Remove-SecretsManagerSecrets {
    param(
        [string]$Environment
    )
    
    Write-ColorOutput "🗑️  Removing Secrets Manager secrets..." "Info"
    
    $secrets = @(
        "blazorwithrules-$Environment/database",
        "blazorwithrules-$Environment/redis"
    )
    
    foreach ($secret in $secrets) {
        try {
            aws secretsmanager delete-secret --secret-id $secret --force-delete-without-recovery
            Write-ColorOutput "  ✅ Secret removed: $secret" "Success"
        }
        catch {
            Write-ColorOutput "  ⚠️  Secret may not exist or already deleted: $secret" "Warning"
        }
    }
}

function Show-CleanupSummary {
    param(
        [string]$Environment
    )
    
    Write-ColorOutput "📊 Cleanup Summary" "Header"
    Write-ColorOutput "=================" "Header"
    Write-ColorOutput "Environment: $Environment" "Info"
    Write-ColorOutput "Status: Completed" "Success"
    Write-ColorOutput ""
    Write-ColorOutput "Resources removed:" "Info"
    Write-ColorOutput "- ECS Cluster and Service" "Info"
    Write-ColorOutput "- Application Load Balancer" "Info"
    Write-ColorOutput "- RDS Database" "Info"
    Write-ColorOutput "- ElastiCache Redis" "Info"
    Write-ColorOutput "- VPC and Networking" "Info"
    Write-ColorOutput "- ECR Repository" "Info"
    Write-ColorOutput "- CloudWatch Logs" "Info"
    Write-ColorOutput "- Secrets Manager Secrets" "Info"
}

function Main {
    Write-ColorOutput "🗑️  BlazorWithRules AWS Cleanup" "Header"
    Write-ColorOutput "==============================" "Header"
    Write-ColorOutput "Environment: $Environment" "Info"
    Write-ColorOutput ""
    
    try {
        # Confirm cleanup
        Confirm-Cleanup
        
        # Destroy CDK stack
        Invoke-CdkDestroy
        
        # Remove ECR repository
        $repositoryName = "blazorwithrules-$Environment"
        Remove-EcrRepository -RepositoryName $repositoryName
        
        # Remove CloudWatch log group
        $logGroupName = "/ecs/blazorwithrules-$Environment"
        Remove-CloudWatchLogs -LogGroupName $logGroupName
        
        # Remove Secrets Manager secrets
        Remove-SecretsManagerSecrets -Environment $Environment
        
        # Show cleanup summary
        Show-CleanupSummary -Environment $Environment
        
        Write-ColorOutput ""
        Write-ColorOutput "🎉 Cleanup completed successfully!" "Success"
    }
    catch {
        Write-ColorOutput ""
        Write-ColorOutput "❌ Cleanup failed: $($_.Exception.Message)" "Error"
        exit 1
    }
}

# Run main function
Main
