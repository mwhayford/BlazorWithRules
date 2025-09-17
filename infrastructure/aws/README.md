# AWS Infrastructure as Code

This directory contains Infrastructure as Code (IaC) definitions for deploying BlazorWithRules to AWS.

## 🛠️ Tools Used

### **Primary: AWS CDK (Cloud Development Kit)**

- **Language**: TypeScript
- **Pros**:
    - Type-safe infrastructure definitions
    - High-level constructs
    - Built-in best practices
    - Excellent AWS integration
    - Easy to test and maintain
- **Cons**: AWS-specific, learning curve

### **Supporting: AWS CLI + Shell Scripts**

- **Purpose**: Deployment automation, environment setup
- **Language**: PowerShell (Windows) / Bash (Linux/Mac)
- **Use cases**: CI/CD pipelines, one-off deployments

## 📁 Directory Structure

```
infrastructure/aws/
├── cdk/                          # AWS CDK TypeScript definitions
│   ├── lib/                      # CDK stack definitions
│   │   ├── blazorwithrules-stack.ts
│   │   ├── networking-stack.ts
│   │   ├── database-stack.ts
│   │   └── application-stack.ts
│   ├── bin/                      # CDK app entry points
│   │   └── blazorwithrules.ts
│   ├── test/                     # CDK unit tests
│   ├── cdk.json                  # CDK configuration
│   ├── package.json              # Node.js dependencies
│   └── tsconfig.json             # TypeScript configuration
├── scripts/                      # Deployment automation scripts
│   ├── deploy.ps1               # Main deployment script
│   ├── build-and-push.ps1       # Container build and push
│   ├── setup-environment.ps1    # Environment setup
│   └── cleanup.ps1              # Resource cleanup
├── config/                       # Environment configurations
│   ├── dev.json                 # Development environment
│   ├── staging.json             # Staging environment
│   └── prod.json                # Production environment
└── templates/                    # CloudFormation templates (if needed)
    └── custom-resources.json
```

## 🚀 Quick Start

### Prerequisites

```bash
# Install AWS CDK
npm install -g aws-cdk

# Install Node.js dependencies
cd infrastructure/aws/cdk
npm install

# Bootstrap CDK (one-time setup)
cdk bootstrap
```

### Deploy to Development

```bash
# Deploy infrastructure
cdk deploy --all --context environment=dev

# Deploy application
./scripts/deploy.ps1 -Environment dev
```

### Deploy to Production

```bash
# Deploy infrastructure
cdk deploy --all --context environment=prod

# Deploy application
./scripts/deploy.ps1 -Environment prod
```

## 🔧 Configuration

### Environment Variables

```bash
# Required
AWS_REGION=us-east-1
AWS_ACCOUNT_ID=123456789012
ENVIRONMENT=dev|staging|prod

# Optional
DOMAIN_NAME=yourdomain.com
SSL_CERTIFICATE_ARN=arn:aws:acm:...
```

### CDK Context

```json
{
    "environments": {
        "dev": {
            "vpcCidr": "10.0.0.0/16",
            "instanceType": "t3.micro",
            "minCapacity": 1,
            "maxCapacity": 3
        },
        "prod": {
            "vpcCidr": "10.1.0.0/16",
            "instanceType": "t3.small",
            "minCapacity": 2,
            "maxCapacity": 10
        }
    }
}
```

## 📊 Architecture

The CDK stacks are organized by concern:

1. **NetworkingStack** - VPC, subnets, security groups
2. **DatabaseStack** - RDS SQL Server, ElastiCache Redis
3. **ApplicationStack** - ECS, ALB, CloudFront
4. **BlazorWithRulesStack** - Main stack that orchestrates others

## 🧪 Testing

```bash
# Run CDK unit tests
npm test

# Synthesize CloudFormation templates
cdk synth

# Diff against deployed stack
cdk diff
```

## 🔒 Security

- **IAM roles** with least privilege
- **Secrets Manager** for sensitive data
- **VPC** with private subnets
- **Security groups** with minimal access
- **Encryption** at rest and in transit

## 📈 Monitoring

- **CloudWatch** logs and metrics
- **X-Ray** tracing (optional)
- **Custom dashboards** for application metrics
- **Alarms** for critical thresholds

## 🚨 Troubleshooting

### Common Issues

1. **CDK Bootstrap**: Run `cdk bootstrap` if you get bootstrap errors
2. **Permissions**: Ensure AWS credentials have sufficient permissions
3. **Resource Limits**: Check AWS service limits in your account
4. **Dependencies**: Ensure all CDK dependencies are installed

### Debug Commands

```bash
# Verbose deployment
cdk deploy --all --verbose

# Check stack status
aws cloudformation describe-stacks --stack-name BlazorWithRules-*

# View stack events
aws cloudformation describe-stack-events --stack-name BlazorWithRules-*
```

## 📚 Resources

- [AWS CDK Documentation](https://docs.aws.amazon.com/cdk/)
- [CDK TypeScript API Reference](https://docs.aws.amazon.com/cdk/api/v2/docs/aws-cdk-lib.aws_ecs-readme.html)
- [CDK Best Practices](https://docs.aws.amazon.com/cdk/v2/guide/best-practices.html)
