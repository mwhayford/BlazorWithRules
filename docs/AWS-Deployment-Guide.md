# AWS Deployment Guide for BlazorWithRules

## 📋 Overview

This guide outlines the steps to deploy your BlazorWithRules application to AWS. The application will be deployed as a containerized service using AWS ECS (Elastic Container Service) with RDS for the database and ElastiCache for Redis.

## 🏗️ AWS Architecture

### **Recommended Architecture**

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   CloudFront    │    │   Route 53      │    │   ACM (SSL)     │
│   (CDN)         │    │   (DNS)         │    │   (Certificates)│
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
┌─────────────────────────────────────────────────────────────────┐
│                        Application Load Balancer                │
│                         (ALB with SSL)                         │
└─────────────────────────────────────────────────────────────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         │                       │                       │
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   ECS Cluster   │    │   RDS SQL       │    │   ElastiCache   │
│   (Fargate)     │    │   Server        │    │   (Redis)       │
│                 │    │                 │    │                 │
│ ┌─────────────┐ │    │ ┌─────────────┐ │    │ ┌─────────────┐ │
│ │ Blazor App  │ │    │ │ SQL Server  │ │    │ │ Redis       │ │
│ │ Container   │ │    │ │ Database    │ │    │ │ Cache       │ │
│ └─────────────┘ │    │ └─────────────┘ │    │ └─────────────┘ │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🚀 Deployment Options

### **Option 1: AWS ECS with Fargate (Recommended)**

- **Pros**: Serverless, no server management, auto-scaling, cost-effective
- **Cons**: Cold starts, less control over underlying infrastructure
- **Best for**: Production workloads, cost optimization

### **Option 2: AWS ECS with EC2**

- **Pros**: More control, better performance, no cold starts
- **Cons**: Server management, higher costs, more complex
- **Best for**: High-performance requirements, existing EC2 infrastructure

### **Option 3: AWS App Runner**

- **Pros**: Simplest deployment, automatic scaling, built-in CI/CD
- **Cons**: Less control, limited customization
- **Best for**: Simple applications, rapid deployment

## 📋 Prerequisites

### **AWS Account Setup**

1. **AWS Account** with appropriate permissions
2. **AWS CLI** installed and configured
3. **Docker** installed locally
4. **Terraform** or **AWS CDK** for infrastructure as code (optional)

### **Required AWS Services**

- **ECS (Elastic Container Service)**
- **ECR (Elastic Container Registry)**
- **RDS (Relational Database Service)**
- **ElastiCache (Redis)**
- **Application Load Balancer (ALB)**
- **Route 53 (DNS)**
- **ACM (SSL Certificates)**
- **CloudFront (CDN)**
- **VPC (Virtual Private Cloud)**
- **IAM (Identity and Access Management)**

## 🛠️ Step-by-Step Deployment

### **Phase 1: Infrastructure Setup**

#### **1.1 Create VPC and Networking**

```bash
# Create VPC
aws ec2 create-vpc --cidr-block 10.0.0.0/16 --tag-specifications 'ResourceType=vpc,Tags=[{Key=Name,Value=blazorwithrules-vpc}]'

# Create subnets (public and private)
aws ec2 create-subnet --vpc-id vpc-xxxxx --cidr-block 10.0.1.0/24 --availability-zone us-east-1a
aws ec2 create-subnet --vpc-id vpc-xxxxx --cidr-block 10.0.2.0/24 --availability-zone us-east-1b
aws ec2 create-subnet --vpc-id vpc-xxxxx --cidr-block 10.0.3.0/24 --availability-zone us-east-1a --tag-specifications 'ResourceType=subnet,Tags=[{Key=Name,Value=blazorwithrules-private-1a}]'
aws ec2 create-subnet --vpc-id vpc-xxxxx --cidr-block 10.0.4.0/24 --availability-zone us-east-1b --tag-specifications 'ResourceType=subnet,Tags=[{Key=Name,Value=blazorwithrules-private-1b}]'

# Create Internet Gateway
aws ec2 create-internet-gateway --tag-specifications 'ResourceType=internet-gateway,Tags=[{Key=Name,Value=blazorwithrules-igw}]'

# Create NAT Gateway (for private subnets)
aws ec2 create-nat-gateway --subnet-id subnet-xxxxx --allocation-id eipalloc-xxxxx
```

#### **1.2 Create Security Groups**

```bash
# ALB Security Group
aws ec2 create-security-group --group-name blazorwithrules-alb-sg --description "Security group for ALB" --vpc-id vpc-xxxxx

# ECS Security Group
aws ec2 create-security-group --group-name blazorwithrules-ecs-sg --description "Security group for ECS tasks" --vpc-id vpc-xxxxx

# RDS Security Group
aws ec2 create-security-group --group-name blazorwithrules-rds-sg --description "Security group for RDS" --vpc-id vpc-xxxxx

# ElastiCache Security Group
aws ec2 create-security-group --group-name blazorwithrules-redis-sg --description "Security group for Redis" --vpc-id vpc-xxxxx
```

#### **1.3 Create RDS SQL Server Instance**

```bash
# Create DB Subnet Group
aws rds create-db-subnet-group \
    --db-subnet-group-name blazorwithrules-db-subnet-group \
    --db-subnet-group-description "Subnet group for BlazorWithRules database" \
    --subnet-ids subnet-xxxxx subnet-yyyyy

# Create RDS SQL Server instance
aws rds create-db-instance \
    --db-instance-identifier blazorwithrules-db \
    --db-instance-class db.t3.micro \
    --engine sqlserver-ex \
    --master-username admin \
    --master-user-password YourStrongPassword123! \
    --allocated-storage 20 \
    --vpc-security-group-ids sg-xxxxx \
    --db-subnet-group-name blazorwithrules-db-subnet-group \
    --backup-retention-period 7 \
    --multi-az \
    --storage-encrypted
```

#### **1.4 Create ElastiCache Redis Cluster**

```bash
# Create Redis Subnet Group
aws elasticache create-cache-subnet-group \
    --cache-subnet-group-name blazorwithrules-redis-subnet-group \
    --cache-subnet-group-description "Subnet group for Redis" \
    --subnet-ids subnet-xxxxx subnet-yyyyy

# Create Redis Cluster
aws elasticache create-cache-cluster \
    --cache-cluster-id blazorwithrules-redis \
    --cache-node-type cache.t3.micro \
    --engine redis \
    --num-cache-nodes 1 \
    --cache-subnet-group-name blazorwithrules-redis-subnet-group \
    --security-group-ids sg-xxxxx
```

### **Phase 2: Container Setup**

#### **2.1 Create ECR Repository**

```bash
# Create ECR repository
aws ecr create-repository --repository-name blazorwithrules --region us-east-1

# Get login token
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com
```

#### **2.2 Create Production Dockerfile**

Create `Dockerfile.prod`:

```dockerfile
# Multi-stage build for production
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY BlazorWithRules.sln .
COPY src/BlazorApp.Web/BlazorApp.Web.csproj src/BlazorApp.Web/
COPY src/BlazorApp.Core/BlazorApp.Core.csproj src/BlazorApp.Core/
COPY src/BlazorApp.Infrastructure/BlazorApp.Infrastructure.csproj src/BlazorApp.Infrastructure/
COPY src/BlazorApp.Shared/BlazorApp.Shared.csproj src/BlazorApp.Shared/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish
WORKDIR /src/src/BlazorApp.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install necessary packages
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "BlazorApp.Web.dll"]
```

#### **2.3 Build and Push Container**

```bash
# Build the container
docker build -f Dockerfile.prod -t blazorwithrules:latest .

# Tag for ECR
docker tag blazorwithrules:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/blazorwithrules:latest

# Push to ECR
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/blazorwithrules:latest
```

### **Phase 3: ECS Setup**

#### **3.1 Create ECS Cluster**

```bash
# Create ECS cluster
aws ecs create-cluster --cluster-name blazorwithrules-cluster --capacity-providers FARGATE
```

#### **3.2 Create Task Definition**

Create `task-definition.json`:

```json
{
    "family": "blazorwithrules-task",
    "networkMode": "awsvpc",
    "requiresCompatibilities": ["FARGATE"],
    "cpu": "512",
    "memory": "1024",
    "executionRoleArn": "arn:aws:iam::<account-id>:role/ecsTaskExecutionRole",
    "taskRoleArn": "arn:aws:iam::<account-id>:role/ecsTaskRole",
    "containerDefinitions": [
        {
            "name": "blazorwithrules",
            "image": "<account-id>.dkr.ecr.us-east-1.amazonaws.com/blazorwithrules:latest",
            "portMappings": [
                {
                    "containerPort": 8080,
                    "protocol": "tcp"
                }
            ],
            "essential": true,
            "environment": [
                {
                    "name": "ASPNETCORE_ENVIRONMENT",
                    "value": "Production"
                },
                {
                    "name": "ASPNETCORE_URLS",
                    "value": "http://+:8080"
                }
            ],
            "secrets": [
                {
                    "name": "ConnectionStrings__DefaultConnection",
                    "valueFrom": "arn:aws:secretsmanager:us-east-1:<account-id>:secret:blazorwithrules/db-connection"
                },
                {
                    "name": "REDIS_CONNECTION_STRING",
                    "valueFrom": "arn:aws:secretsmanager:us-east-1:<account-id>:secret:blazorwithrules/redis-connection"
                }
            ],
            "logConfiguration": {
                "logDriver": "awslogs",
                "options": {
                    "awslogs-group": "/ecs/blazorwithrules",
                    "awslogs-region": "us-east-1",
                    "awslogs-stream-prefix": "ecs"
                }
            },
            "healthCheck": {
                "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
                "interval": 30,
                "timeout": 5,
                "retries": 3,
                "startPeriod": 60
            }
        }
    ]
}
```

#### **3.3 Register Task Definition**

```bash
# Register task definition
aws ecs register-task-definition --cli-input-json file://task-definition.json
```

#### **3.4 Create ECS Service**

```bash
# Create ECS service
aws ecs create-service \
    --cluster blazorwithrules-cluster \
    --service-name blazorwithrules-service \
    --task-definition blazorwithrules-task:1 \
    --desired-count 2 \
    --launch-type FARGATE \
    --network-configuration "awsvpcConfiguration={subnets=[subnet-xxxxx,subnet-yyyyy],securityGroups=[sg-xxxxx],assignPublicIp=ENABLED}" \
    --load-balancers "targetGroupArn=arn:aws:elasticloadbalancing:us-east-1:<account-id>:targetgroup/blazorwithrules-tg/xxxxx,containerName=blazorwithrules,containerPort=8080" \
    --health-check-grace-period-seconds 300
```

### **Phase 4: Load Balancer Setup**

#### **4.1 Create Application Load Balancer**

```bash
# Create ALB
aws elbv2 create-load-balancer \
    --name blazorwithrules-alb \
    --subnets subnet-xxxxx subnet-yyyyy \
    --security-groups sg-xxxxx \
    --scheme internet-facing \
    --type application \
    --ip-address-type ipv4
```

#### **4.2 Create Target Group**

```bash
# Create target group
aws elbv2 create-target-group \
    --name blazorwithrules-tg \
    --protocol HTTP \
    --port 8080 \
    --vpc-id vpc-xxxxx \
    --target-type ip \
    --health-check-path /health \
    --health-check-interval-seconds 30 \
    --health-check-timeout-seconds 5 \
    --healthy-threshold-count 2 \
    --unhealthy-threshold-count 3
```

#### **4.3 Create Listener**

```bash
# Create listener
aws elbv2 create-listener \
    --load-balancer-arn arn:aws:elasticloadbalancing:us-east-1:<account-id>:loadbalancer/app/blazorwithrules-alb/xxxxx \
    --protocol HTTPS \
    --port 443 \
    --certificates CertificateArn=arn:aws:acm:us-east-1:<account-id>:certificate/xxxxx \
    --default-actions Type=forward,TargetGroupArn=arn:aws:elasticloadbalancing:us-east-1:<account-id>:targetgroup/blazorwithrules-tg/xxxxx
```

### **Phase 5: DNS and SSL**

#### **5.1 Create Route 53 Hosted Zone**

```bash
# Create hosted zone
aws route53 create-hosted-zone \
    --name yourdomain.com \
    --caller-reference $(date +%s)
```

#### **5.2 Request SSL Certificate**

```bash
# Request certificate
aws acm request-certificate \
    --domain-name yourdomain.com \
    --subject-alternative-names www.yourdomain.com \
    --validation-method DNS \
    --region us-east-1
```

#### **5.3 Create CloudFront Distribution**

```bash
# Create CloudFront distribution
aws cloudfront create-distribution \
    --distribution-config file://cloudfront-config.json
```

## 🔧 Configuration Changes Required

### **1. Update appsettings.Production.json**

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=blazorwithrules-db.xxxxx.us-east-1.rds.amazonaws.com,1433;Database=BlazorAppDb;User Id=admin;Password=YourStrongPassword123!;TrustServerCertificate=True;"
    },
    "Redis": {
        "ConnectionString": "blazorwithrules-redis.xxxxx.cache.amazonaws.com:6379"
    },
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "yourdomain.com,www.yourdomain.com"
}
```

### **2. Update Program.cs for Production**

```csharp
// Add to Program.cs
if (app.Environment.IsProduction())
{
    // Production-specific configurations
    app.UseHsts();

    // Configure Serilog for CloudWatch
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.AmazonCloudWatch()
        .CreateLogger();
}
```

### **3. Environment Variables**

```bash
# Set in ECS task definition
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=<from-secrets-manager>
REDIS_CONNECTION_STRING=<from-secrets-manager>
```

## 💰 Cost Estimation

### **Monthly Costs (US East 1)**

| Service                       | Configuration              | Monthly Cost    |
| ----------------------------- | -------------------------- | --------------- |
| **ECS Fargate**               | 2 tasks, 0.5 vCPU, 1GB RAM | ~$30            |
| **RDS SQL Server**            | db.t3.micro, 20GB storage  | ~$25            |
| **ElastiCache Redis**         | cache.t3.micro             | ~$15            |
| **Application Load Balancer** | Standard ALB               | ~$20            |
| **Route 53**                  | Hosted zone + queries      | ~$5             |
| **CloudFront**                | 1TB transfer               | ~$10            |
| **ECR**                       | Container storage          | ~$2             |
| **CloudWatch**                | Logs and metrics           | ~$5             |
| **Total**                     |                            | **~$112/month** |

## 🚀 Deployment Scripts

### **deploy.sh**

```bash
#!/bin/bash
set -e

# Configuration
AWS_REGION="us-east-1"
ECR_REPOSITORY="blazorwithrules"
ECS_CLUSTER="blazorwithrules-cluster"
ECS_SERVICE="blazorwithrules-service"

# Get AWS account ID
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
ECR_URI="${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com/${ECR_REPOSITORY}"

echo "Deploying to AWS..."
echo "Account ID: ${AWS_ACCOUNT_ID}"
echo "Region: ${AWS_REGION}"

# Build and push container
echo "Building and pushing container..."
docker build -f Dockerfile.prod -t ${ECR_REPOSITORY}:latest .
docker tag ${ECR_REPOSITORY}:latest ${ECR_URI}:latest
aws ecr get-login-password --region ${AWS_REGION} | docker login --username AWS --password-stdin ${ECR_URI}
docker push ${ECR_URI}:latest

# Update ECS service
echo "Updating ECS service..."
aws ecs update-service --cluster ${ECS_CLUSTER} --service ${ECS_SERVICE} --force-new-deployment

echo "Deployment complete!"
```

## 🔒 Security Considerations

### **1. Secrets Management**

- Use AWS Secrets Manager for database credentials
- Store Redis connection strings in Secrets Manager
- Use IAM roles for ECS tasks

### **2. Network Security**

- Deploy in private subnets where possible
- Use security groups to restrict traffic
- Enable VPC Flow Logs

### **3. Data Security**

- Enable encryption at rest for RDS and ElastiCache
- Use SSL/TLS for all communications
- Implement proper backup strategies

## 📊 Monitoring and Logging

### **1. CloudWatch Integration**

- Application logs via CloudWatch Logs
- Custom metrics for business logic
- Alarms for critical thresholds

### **2. Health Checks**

- Application health endpoint (`/health`)
- Database connectivity checks
- Redis connectivity checks

### **3. Performance Monitoring**

- ECS service metrics
- ALB target group metrics
- Database performance insights

## 🎯 Next Steps

1. **Set up AWS account and configure CLI**
2. **Create infrastructure using Terraform or AWS CDK**
3. **Build and push container to ECR**
4. **Deploy to ECS and configure load balancer**
5. **Set up DNS and SSL certificates**
6. **Configure monitoring and alerting**
7. **Test deployment and performance**
8. **Set up CI/CD pipeline for automated deployments**

## 📚 Additional Resources

- [AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)
- [AWS RDS SQL Server Guide](https://docs.aws.amazon.com/rds/latest/userguide/CHAP_SQLServer.html)
- [AWS ElastiCache Redis Guide](https://docs.aws.amazon.com/elasticache/latest/red-ug/)
- [AWS Application Load Balancer Guide](https://docs.aws.amazon.com/elasticloadbalancing/latest/application/)

---

_Generated on: September 17, 2025_  
_Guide Version: 1.0_  
_Next Review: October 1, 2025_
