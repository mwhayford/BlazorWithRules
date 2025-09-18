# 🔐 Git Secrets Alternative: Pre-commit Hooks

## 🚨 **SECURITY NOTICE**

**This documentation contains NO sensitive information** - all actual secret values have been redacted with `[REDACTED_PASSWORD]` placeholders for security.

**Actual secret values are stored securely in:**

- GitHub Secrets (for CI/CD)
- Environment variables (for local development)
- Never in documentation or version control

---

Since Git Secrets isn't available via Chocolatey, here's a comprehensive alternative solution using pre-commit hooks and GitHub Secrets.

## ✅ **Solution 1: Pre-commit Hooks (Recommended)**

### **Create Pre-commit Hook**

```bash
# Create .git/hooks/pre-commit
cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
# Pre-commit hook to prevent sensitive data commits

# Patterns to check for
PATTERNS=(
    "password.*=.*"
    "PASSWORD.*=.*"
    "[REDACTED_PASSWORD]"
    "sa.*password"
    "connection.*string.*password"
    "SA_PASSWORD.*=.*"
    "ClientSecret.*=.*"
    "ClientId.*=.*"
)

# Files to check
FILES=$(git diff --cached --name-only)

for file in $FILES; do
    for pattern in "${PATTERNS[@]}"; do
        if grep -i "$pattern" "$file" > /dev/null 2>&1; then
            echo "❌ SECURITY VIOLATION: Sensitive data detected in $file"
            echo "Pattern: $pattern"
            echo "Please remove sensitive data before committing."
            exit 1
        fi
    done
done

echo "✅ Pre-commit check passed - no sensitive data detected"
exit 0
EOF

# Make executable
chmod +x .git/hooks/pre-commit
```

### **Test Pre-commit Hook**

```bash
# Test with a file containing secrets
echo "password=[REDACTED_PASSWORD]" > test-secret.txt
git add test-secret.txt
git commit -m "Test commit"
# This should fail and prevent the commit
```

---

## ✅ **Solution 2: GitHub Secrets Integration**

### **Add Database Secrets to GitHub**

```bash
# Add database secrets to GitHub
gh secret set SQLSERVER_PASSWORD --repo mwhayford/BlazorWithRules --body "[REDACTED_PASSWORD]"
gh secret set SQLSERVER_USERNAME --repo mwhayford/BlazorWithRules --body "sa"
gh secret set SQLSERVER_DATABASE --repo mwhayford/BlazorWithRules --body "BlazorWithRules"
```

### **Update GitHub Actions Workflow**

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

## ✅ **Solution 3: Enhanced Scripts**

### **Create Universal Database Script**

```bash
#!/bin/bash
# Universal database connectivity script
# Works with both .env files and GitHub Secrets

# Load from .env file (local development)
if [ -f .env ]; then
    source .env
fi

# Use environment variables (from GitHub Secrets in CI/CD)
USERNAME=${SQLSERVER_USERNAME:-sa}
PASSWORD=${SQLSERVER_PASSWORD}
DATABASE=${SQLSERVER_DATABASE:-BlazorWithRules}
SERVER=${SQLSERVER_SERVER:-localhost}

if [ -z "$PASSWORD" ]; then
    echo "❌ Error: SQLSERVER_PASSWORD not set"
    echo ""
    echo "For local development:"
    echo "  1. Create .env file with: SQLSERVER_PASSWORD=[REDACTED_PASSWORD]"
    echo "  2. Run: ./scripts/db-connect.sh \"SELECT 1\""
    echo ""
    echo "For CI/CD:"
    echo "  1. Set GitHub Secrets: SQLSERVER_PASSWORD"
    echo "  2. Environment variables will be automatically loaded"
    exit 1
fi

# Execute SQL command
echo "🔍 Executing query: $1"
echo "📊 Database: $DATABASE"
echo "👤 User: $USERNAME"

docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S $SERVER -U $USERNAME -P $PASSWORD -C -d $DATABASE -Q "$1"
```

### **Create PowerShell Version**

```powershell
# Universal PowerShell database connectivity script
param(
    [Parameter(Mandatory=$true)]
    [string]$Query,

    [string]$Database = "BlazorWithRules",
    [string]$Server = "localhost"
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
$server = [Environment]::GetEnvironmentVariable("SQLSERVER_SERVER", "Process")

if ([string]::IsNullOrEmpty($username)) { $username = "sa" }
if ([string]::IsNullOrEmpty($database)) { $database = $Database }
if ([string]::IsNullOrEmpty($server)) { $server = $Server }

if ([string]::IsNullOrEmpty($password)) {
    Write-Error "❌ SQLSERVER_PASSWORD not set"
    Write-Host ""
    Write-Host "For local development:"
    Write-Host "  1. Create .env file with: SQLSERVER_PASSWORD=[REDACTED_PASSWORD]"
    Write-Host "  2. Run: .\scripts\db-connect.ps1 -Query 'SELECT 1'"
    Write-Host ""
    Write-Host "For CI/CD:"
    Write-Host "  1. Set GitHub Secrets: SQLSERVER_PASSWORD"
    Write-Host "  2. Environment variables will be automatically loaded"
    exit 1
}

# Execute SQL command
Write-Host "🔍 Executing query: $Query" -ForegroundColor Cyan
Write-Host "📊 Database: $database" -ForegroundColor Cyan
Write-Host "👤 User: $username" -ForegroundColor Cyan

docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd `
    -S $server -U $username -P $password -C -d $database -Q $Query
```

---

## 🛠️ **Implementation Steps**

### **Step 1: Create Pre-commit Hook**

```bash
# Create the pre-commit hook
mkdir -p .git/hooks
cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
# Pre-commit hook to prevent sensitive data commits

PATTERNS=(
    "password.*=.*"
    "PASSWORD.*=.*"
    "[REDACTED_PASSWORD]"
    "sa.*password"
    "connection.*string.*password"
    "SA_PASSWORD.*=.*"
    "ClientSecret.*=.*"
    "ClientId.*=.*"
)

FILES=$(git diff --cached --name-only)

for file in $FILES; do
    for pattern in "${PATTERNS[@]}"; do
        if grep -i "$pattern" "$file" > /dev/null 2>&1; then
            echo "❌ SECURITY VIOLATION: Sensitive data detected in $file"
            echo "Pattern: $pattern"
            echo "Please remove sensitive data before committing."
            exit 1
        fi
    done
done

echo "✅ Pre-commit check passed - no sensitive data detected"
exit 0
EOF

chmod +x .git/hooks/pre-commit
```

### **Step 2: Add GitHub Secrets**

```bash
# Add database secrets to GitHub
gh secret set SQLSERVER_PASSWORD --repo mwhayford/BlazorWithRules --body "[REDACTED_PASSWORD]"
gh secret set SQLSERVER_USERNAME --repo mwhayford/BlazorWithRules --body "sa"
gh secret set SQLSERVER_DATABASE --repo mwhayford/BlazorWithRules --body "BlazorWithRules"
```

### **Step 3: Update Scripts**

```bash
# Update existing scripts to use the universal approach
# They will work with both .env files and GitHub Secrets
```

### **Step 4: Test the Setup**

```bash
# Test pre-commit hook
echo "password=test" > test.txt
git add test.txt
git commit -m "Test"
# Should fail

# Test database connectivity
./scripts/db-connect.sh "SELECT 1"
```

---

## 📊 **Benefits of This Approach**

### **Security**

- ✅ **Prevents accidental commits** of sensitive data
- ✅ **GitHub Secrets** for secure CI/CD
- ✅ **Environment variables** for local development
- ✅ **No hardcoded passwords** in codebase

### **Flexibility**

- ✅ **Works locally** with .env files
- ✅ **Works in CI/CD** with GitHub Secrets
- ✅ **Cross-platform** (Linux, macOS, Windows)
- ✅ **Easy to use** for developers

### **Maintenance**

- ✅ **No additional tools** required
- ✅ **Easy to update** patterns
- ✅ **Clear error messages** for developers
- ✅ **Comprehensive documentation**

---

## 🎯 **Usage Examples**

### **Local Development**

```bash
# Create .env file
echo "SQLSERVER_PASSWORD=[REDACTED_PASSWORD]" > .env

# Use database scripts
./scripts/db-connect.sh "SELECT 1"
./scripts/db-health-check.sh
```

### **CI/CD Pipeline**

```yaml
# GitHub Actions automatically uses secrets
- name: Database Health Check
  env:
      SQLSERVER_PASSWORD: ${{ secrets.SQLSERVER_PASSWORD }}
  run: ./scripts/db-health-check.sh
```

### **Production Deployment**

```bash
# Set environment variables
export SQLSERVER_PASSWORD="production-password"

# Run scripts
./scripts/db-connect.sh "SELECT 1"
```

---

## 🚨 **Security Checklist**

- [ ] Pre-commit hook installed and tested
- [ ] GitHub Secrets configured
- [ ] .env files added to .gitignore
- [ ] Scripts updated to use environment variables
- [ ] Documentation updated with secure commands
- [ ] Team trained on new secure workflow
- [ ] Regular password rotation schedule established

---

_Last Updated: January 18, 2025_  
_Version: 1.0_
