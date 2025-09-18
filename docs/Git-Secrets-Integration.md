# 🔐 Git Secrets Integration Guide

## 📋 **Overview**

There are two main approaches for managing secrets with Git:

1. **GitHub Secrets** - For CI/CD workflows (already implemented)
2. **Git Secrets Tool** - For preventing sensitive data in commits
3. **Local Development** - Still needs environment variables or secure config

## ✅ **Approach 1: GitHub Secrets (Already Implemented)**

### **Current Implementation**

```yaml
# .github/workflows/deploy-aws.yml
- name: Deploy to AWS
  env:
      GOOGLE_CLIENT_ID: ${{ secrets.GOOGLE_CLIENT_ID }}
      GOOGLE_CLIENT_SECRET: ${{ secrets.GOOGLE_CLIENT_SECRET }}
```

### **Add Database Secrets**

```bash
# Add database secrets to GitHub
gh secret set SQLSERVER_PASSWORD --repo mwhayford/BlazorWithRules --body "[REDACTED_PASSWORD]"
gh secret set SQLSERVER_USERNAME --repo mwhayford/BlazorWithRules --body "sa"
gh secret set SQLSERVER_DATABASE --repo mwhayford/BlazorWithRules --body "BlazorWithRules"
```

### **Update GitHub Actions**

```yaml
# .github/workflows/deploy-aws.yml
- name: Deploy to AWS
  env:
      GOOGLE_CLIENT_ID: ${{ secrets.GOOGLE_CLIENT_ID }}
      GOOGLE_CLIENT_SECRET: ${{ secrets.GOOGLE_CLIENT_SECRET }}
      SQLSERVER_PASSWORD: ${{ secrets.SQLSERVER_PASSWORD }}
      SQLSERVER_USERNAME: ${{ secrets.SQLSERVER_USERNAME }}
      SQLSERVER_DATABASE: ${{ secrets.SQLSERVER_DATABASE }}
```

---

## ✅ **Approach 2: Git Secrets Tool (Recommended)**

### **Installation**

#### **Windows (PowerShell)**

```powershell
# Install Git Secrets via Chocolatey
choco install git-secrets

# Or via Scoop
scoop install git-secrets
```

#### **Linux/macOS**

```bash
# Install Git Secrets
git clone https://github.com/awslabs/git-secrets.git
cd git-secrets
make install

# Or via package manager
# Ubuntu/Debian: apt-get install git-secrets
# macOS: brew install git-secrets
```

### **Setup Git Secrets**

```bash
# Initialize Git Secrets in your repository
git secrets --install

# Add patterns to prevent commits
git secrets --add 'password.*=.*'
git secrets --add 'PASSWORD.*=.*'
git secrets --add '[REDACTED_PASSWORD]'
git secrets --add 'sa.*password'
git secrets --add 'connection.*string.*password'

# Add to .gitignore
echo ".gitsecrets" >> .gitignore
```

### **Test Git Secrets**

```bash
# Test with a file containing secrets
echo "password=[REDACTED_PASSWORD]" > test-secret.txt
git add test-secret.txt
# This should fail and prevent the commit
```

---

## ✅ **Approach 3: Hybrid Solution (Best Practice)**

### **Local Development**

```bash
# Create .env file (gitignored)
cat > .env << EOF
SQLSERVER_PASSWORD=[REDACTED_PASSWORD]
SQLSERVER_USERNAME=sa
SQLSERVER_DATABASE=BlazorWithRules
EOF
```

### **Production/CI/CD**

```yaml
# Use GitHub Secrets in workflows
env:
    SQLSERVER_PASSWORD: ${{ secrets.SQLSERVER_PASSWORD }}
    SQLSERVER_USERNAME: ${{ secrets.SQLSERVER_USERNAME }}
    SQLSERVER_DATABASE: ${{ secrets.SQLSERVER_DATABASE }}
```

### **Docker Compose**

```yaml
# docker-compose.yml
services:
    app:
        environment:
            - SQLSERVER_PASSWORD=${SQLSERVER_PASSWORD}
            - SQLSERVER_USERNAME=${SQLSERVER_USERNAME}
            - SQLSERVER_DATABASE=${SQLSERVER_DATABASE}
```

---

## 🛠️ **Implementation Steps**

### **Step 1: Install Git Secrets**

```bash
# Windows
choco install git-secrets

# Linux/macOS
git clone https://github.com/awslabs/git-secrets.git
cd git-secrets && make install
```

### **Step 2: Configure Git Secrets**

```bash
# Initialize in your repository
git secrets --install

# Add secret patterns
git secrets --add 'password.*=.*'
git secrets --add 'PASSWORD.*=.*'
git secrets --add '[REDACTED_PASSWORD]'
git secrets --add 'connection.*string.*password'
git secrets --add 'SA_PASSWORD.*=.*'
```

### **Step 3: Add GitHub Secrets**

```bash
# Add database secrets to GitHub
gh secret set SQLSERVER_PASSWORD --repo mwhayford/BlazorWithRules --body "[REDACTED_PASSWORD]"
gh secret set SQLSERVER_USERNAME --repo mwhayford/BlazorWithRules --body "sa"
gh secret set SQLSERVER_DATABASE --repo mwhayford/BlazorWithRules --body "BlazorWithRules"
```

### **Step 4: Update Scripts**

```bash
# Update scripts to use environment variables
# They will work with both .env files and GitHub Secrets
```

---

## 🔧 **Updated Scripts for Git Secrets**

### **Enhanced db-connect.sh**

```bash
#!/bin/bash
# Enhanced script that works with both .env and GitHub Secrets

# Load from .env file (local development)
if [ -f .env ]; then
    source .env
fi

# Use environment variables (from GitHub Secrets in CI/CD)
USERNAME=${SQLSERVER_USERNAME:-sa}
PASSWORD=${SQLSERVER_PASSWORD}
DATABASE=${SQLSERVER_DATABASE:-BlazorWithRules}

if [ -z "$PASSWORD" ]; then
    echo "Error: SQLSERVER_PASSWORD not set"
    echo "For local development: Create .env file"
    echo "For CI/CD: Set GitHub Secrets"
    exit 1
fi

docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U $USERNAME -P $PASSWORD -C -d $DATABASE -Q "$1"
```

### **Enhanced db-connect.ps1**

```powershell
# Enhanced PowerShell script
param(
    [Parameter(Mandatory=$true)]
    [string]$Query,

    [string]$Database = "BlazorWithRules"
)

# Load from .env file (local development)
if (Test-Path ".env") {
    Get-Content ".env" | ForEach-Object {
        if ($_ -match "^([^#][^=]+)=(.*)$") {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            [Environment]::SetEnvironmentVariable($name, $value, "Process")
        }
    }
}

# Use environment variables (from GitHub Secrets in CI/CD)
$username = [Environment]::GetEnvironmentVariable("SQLSERVER_USERNAME", "Process")
$password = [Environment]::GetEnvironmentVariable("SQLSERVER_PASSWORD", "Process")
$database = [Environment]::GetEnvironmentVariable("SQLSERVER_DATABASE", "Process")

if ([string]::IsNullOrEmpty($username)) { $username = "sa" }
if ([string]::IsNullOrEmpty($database)) { $database = $Database }
if ([string]::IsNullOrEmpty($password)) {
    Write-Error "SQLSERVER_PASSWORD not set. Create .env file or set GitHub Secrets."
    exit 1
}

docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U $username -P $password -C -d $database -Q $Query
```

---

## 📊 **Comparison of Approaches**

| Approach                  | Local Dev | CI/CD | Production | Security   | Ease of Use |
| ------------------------- | --------- | ----- | ---------- | ---------- | ----------- |
| **Environment Variables** | ✅        | ✅    | ✅         | ⭐⭐⭐     | ⭐⭐⭐⭐⭐  |
| **GitHub Secrets**        | ❌        | ✅    | ❌         | ⭐⭐⭐⭐⭐ | ⭐⭐⭐      |
| **Git Secrets Tool**      | ✅        | ✅    | ✅         | ⭐⭐⭐⭐⭐ | ⭐⭐        |
| **Hybrid Solution**       | ✅        | ✅    | ✅         | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐    |

---

## 🎯 **Recommended Implementation**

### **For BlazorWithRules Project**

1. **Install Git Secrets Tool** (prevents accidental commits)
2. **Use GitHub Secrets** (for CI/CD workflows)
3. **Keep .env files** (for local development)
4. **Update scripts** (to work with both approaches)

### **Benefits**

- ✅ **Prevents accidental commits** of sensitive data
- ✅ **Secure CI/CD** with GitHub Secrets
- ✅ **Easy local development** with .env files
- ✅ **Production ready** with environment variables
- ✅ **Cross-platform compatibility**

---

## 🚨 **Security Best Practices**

1. **Never commit secrets** to version control
2. **Use Git Secrets tool** to prevent accidental commits
3. **Rotate secrets regularly** (every 90 days)
4. **Use least privilege** for database users
5. **Monitor access logs** for suspicious activity
6. **Use strong, unique passwords** for each environment

---

_Last Updated: January 18, 2025_  
_Version: 1.0_
