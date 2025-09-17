# AWS Deployment Scripts

This directory contains PowerShell scripts for deploying and managing the BlazorWithRules application on AWS.

## 📋 Scripts Overview

### **deploy.ps1** - Main Deployment Script

The primary script for deploying the complete application to AWS.

**Usage:**

```powershell
.\deploy.ps1 -Environment dev
.\deploy.ps1 -Environment prod -ImageTag v1.2.3
.\deploy.ps1 -Environment staging -SkipInfrastructure -Verbose
```

**Parameters:**

- `-Environment` (Required): Environment to deploy to (dev, staging, prod)
- `-SkipInfrastructure`: Skip infrastructure deployment, only deploy application
- `-SkipApplication`: Skip application deployment, only deploy infrastructure
- `-Force`: Force deployment without confirmation
- `-ImageTag`: Docker image tag to deploy (default: latest)
- `-Verbose`: Enable verbose output

### **build-and-push.ps1** - Container Build and Push

Builds the Docker container and pushes it to ECR.

**Usage:**

```powershell
.\build-and-push.ps1 -Environment dev
.\build-and-push.ps1 -Environment prod -ImageTag v1.2.3 -Verbose
```

**Parameters:**

- `-Environment` (Required): Environment to build for
- `-ImageTag`: Docker image tag (default: latest)
- `-Verbose`: Enable verbose output

### **setup-environment.ps1** - Environment Setup

Sets up the AWS environment for deployment (one-time setup).

**Usage:**

```powershell
.\setup-environment.ps1 -Environment dev
.\setup-environment.ps1 -Environment prod -AwsProfile production -Region us-west-2
```

**Parameters:**

- `-Environment` (Required): Environment to set up
- `-AwsProfile`: AWS profile to use (default: default)
- `-Region`: AWS region (default: us-east-1)
- `-Verbose`: Enable verbose output

### **cleanup.ps1** - Resource Cleanup

Removes all AWS resources for an environment.

**Usage:**

```powershell
.\cleanup.ps1 -Environment dev
.\cleanup.ps1 -Environment prod -Force -Verbose
```

**Parameters:**

- `-Environment` (Required): Environment to clean up
- `-Force`: Skip confirmation prompt
- `-Verbose`: Enable verbose output

## 🚀 Quick Start

### 1. First Time Setup

```powershell
# Set up the environment
.\setup-environment.ps1 -Environment dev

# Deploy the application
.\deploy.ps1 -Environment dev
```

### 2. Regular Deployments

```powershell
# Deploy to development
.\deploy.ps1 -Environment dev

# Deploy to production with specific version
.\deploy.ps1 -Environment prod -ImageTag v1.2.3
```

### 3. Cleanup

```powershell
# Remove all resources
.\cleanup.ps1 -Environment dev
```

## 🔧 Prerequisites

### Required Tools

- **AWS CLI** - For AWS operations
- **Docker** - For container builds
- **Node.js** - For CDK operations
- **AWS CDK** - For infrastructure deployment
- **PowerShell** - For script execution

### AWS Configuration

- AWS CLI configured with appropriate credentials
- IAM permissions for ECS, ECR, RDS, ElastiCache, VPC, etc.
- CDK bootstrapped in the target region

### Environment Variables

```bash
# Optional - will be detected automatically
AWS_ACCOUNT_ID=123456789012
AWS_REGION=us-east-1
CDK_DEFAULT_ACCOUNT=123456789012
CDK_DEFAULT_REGION=us-east-1
```

## 📊 Deployment Flow

### Infrastructure Deployment

1. **VPC Setup** - Creates VPC with public/private subnets
2. **Security Groups** - Configures network security
3. **RDS Database** - Creates SQL Server instance
4. **ElastiCache Redis** - Creates Redis cluster
5. **ECS Cluster** - Creates Fargate cluster
6. **Load Balancer** - Creates ALB with target groups
7. **Secrets Manager** - Creates secrets for credentials

### Application Deployment

1. **Container Build** - Builds Docker image
2. **ECR Push** - Pushes image to ECR
3. **ECS Service Update** - Updates service with new image
4. **Health Checks** - Verifies deployment success

## 🔒 Security Features

- **Secrets Manager** for database credentials
- **IAM Roles** with least privilege
- **Security Groups** with minimal access
- **VPC** with private subnets
- **Encryption** at rest and in transit

## 📈 Monitoring

- **CloudWatch Logs** for application logs
- **CloudWatch Metrics** for performance monitoring
- **Health Checks** for service availability
- **Auto Scaling** based on CPU/memory usage

## 🚨 Troubleshooting

### Common Issues

1. **AWS CLI Not Configured**

    ```bash
    aws configure
    ```

2. **CDK Not Bootstrapped**

    ```bash
    cdk bootstrap
    ```

3. **Docker Not Running**

    ```bash
    # Start Docker Desktop
    ```

4. **Insufficient Permissions**
    - Check IAM policies
    - Verify AWS profile

### Debug Commands

```powershell
# Verbose deployment
.\deploy.ps1 -Environment dev -Verbose

# Check AWS credentials
aws sts get-caller-identity

# Check CDK status
cdk list

# Check ECS service status
aws ecs describe-services --cluster blazorwithrules-dev --services BlazorWithRules-dev-Service
```

## 📚 Additional Resources

- [AWS CDK Documentation](https://docs.aws.amazon.com/cdk/)
- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [AWS CLI Documentation](https://docs.aws.amazon.com/cli/)
- [Docker Documentation](https://docs.docker.com/)

## 🤝 Contributing

When adding new scripts or modifying existing ones:

1. Follow PowerShell best practices
2. Add proper error handling
3. Include verbose output options
4. Update this README
5. Test with different environments
