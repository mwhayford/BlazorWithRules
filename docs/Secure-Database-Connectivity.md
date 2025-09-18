# 🔒 Secure Database Connectivity Guide

## 🚨 **Security Issue: Password Exposure**

The current documentation exposes SQL Server passwords in command lines and scripts. This is a security risk that needs to be addressed.

## ✅ **Solution 1: Environment Variables (Recommended)**

### **Create Environment File**

```bash
# Create .env file (add to .gitignore)
echo "SQLSERVER_PASSWORD=[REDACTED_PASSWORD]" > .env
echo "SQLSERVER_USERNAME=sa" >> .env
echo "SQLSERVER_DATABASE=BlazorWithRules" >> .env
```

### **Update Docker Compose**

```yaml
# docker-compose.yml
services:
    sqlserver:
        environment:
            - ACCEPT_EULA=Y
            - SA_PASSWORD=${SQLSERVER_PASSWORD}
            - MSSQL_PID=Developer
```

### **Secure Commands**

```bash
# Load environment variables
source .env

# Check database connectivity (secure)
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U $SQLSERVER_USERNAME -P $SQLSERVER_PASSWORD -C -Q "SELECT 1"

# Or use environment variable directly
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SQLSERVER_PASSWORD" -C -Q "SELECT 1"
```

---

## ✅ **Solution 2: Interactive Password Prompt**

### **Create Secure Script**

```bash
#!/bin/bash
# scripts/secure-db-connect.sh

read -s -p "Enter SQL Server password: " SQL_PASSWORD
echo

docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SQL_PASSWORD" -C -Q "SELECT 1"
```

### **Usage**

```bash
chmod +x scripts/secure-db-connect.sh
./scripts/secure-db-connect.sh
```

---

## ✅ **Solution 3: Docker Secrets (Production)**

### **Create Secret**

```bash
# Create Docker secret
echo "[REDACTED_PASSWORD]" | docker secret create sqlserver_password -
```

### **Update Docker Compose**

```yaml
# docker-compose.prod.yml
services:
    sqlserver:
        environment:
            - ACCEPT_EULA=Y
            - SA_PASSWORD_FILE=/run/secrets/sqlserver_password
            - MSSQL_PID=Developer
        secrets:
            - sqlserver_password

secrets:
    sqlserver_password:
        external: true
```

---

## ✅ **Solution 4: PowerShell Secure String**

### **Create Secure PowerShell Script**

```powershell
# scripts/secure-db-connect.ps1

param(
    [string]$Query = "SELECT 1"
)

# Read password securely
$SecurePassword = Read-Host "Enter SQL Server password" -AsSecureString
$Password = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword))

# Execute command
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd `
  -S localhost -U sa -P $Password -C -Q $Query
```

### **Usage**

```powershell
.\scripts\secure-db-connect.ps1
.\scripts\secure-db-connect.ps1 -Query "SELECT name FROM sys.databases"
```

---

## ✅ **Solution 5: Configuration File**

### **Create Config File**

```json
// config/database.json (add to .gitignore)
{
    "SqlServer": {
        "Username": "sa",
        "Password": "[REDACTED_PASSWORD]",
        "Database": "BlazorWithRules",
        "Server": "localhost"
    }
}
```

### **Create Helper Script**

```bash
#!/bin/bash
# scripts/db-helper.sh

CONFIG_FILE="config/database.json"
USERNAME=$(jq -r '.SqlServer.Username' $CONFIG_FILE)
PASSWORD=$(jq -r '.SqlServer.Password' $CONFIG_FILE)
DATABASE=$(jq -r '.SqlServer.Database' $CONFIG_FILE)

docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U $USERNAME -P $PASSWORD -C -d $DATABASE -Q "$1"
```

---

## 🛠️ **Implementation Steps**

### **Step 1: Create Environment File**

```bash
# Create .env file
cat > .env << EOF
SQLSERVER_PASSWORD=[REDACTED_PASSWORD]
SQLSERVER_USERNAME=sa
SQLSERVER_DATABASE=BlazorWithRules
EOF

# Add to .gitignore
echo ".env" >> .gitignore
```

### **Step 2: Update Documentation**

Replace all hardcoded passwords with environment variable references:

```bash
# OLD (insecure)
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "[REDACTED_PASSWORD]" -C -Q "SELECT 1"

# NEW (secure)
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SQLSERVER_PASSWORD" -C -Q "SELECT 1"
```

### **Step 3: Create Helper Scripts**

```bash
# Create scripts directory
mkdir -p scripts

# Create secure database helper
cat > scripts/db-connect.sh << 'EOF'
#!/bin/bash
source .env
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U $SQLSERVER_USERNAME -P $SQLSERVER_PASSWORD -C -Q "$1"
EOF

chmod +x scripts/db-connect.sh
```

### **Step 4: Update Docker Compose**

```yaml
# docker-compose.yml
services:
    sqlserver:
        environment:
            - ACCEPT_EULA=Y
            - SA_PASSWORD=${SQLSERVER_PASSWORD:-[REDACTED_PASSWORD]}
            - MSSQL_PID=Developer
```

---

## 🔐 **Best Practices**

### **Development Environment**

1. **Use .env files** for local development
2. **Add .env to .gitignore** to prevent accidental commits
3. **Use environment variables** in all scripts and documentation
4. **Create helper scripts** to avoid typing passwords repeatedly

### **Production Environment**

1. **Use Docker Secrets** or Kubernetes Secrets
2. **Use Azure Key Vault** or AWS Secrets Manager
3. **Never hardcode passwords** in any configuration files
4. **Rotate passwords regularly**

### **Documentation Security**

1. **Use placeholders** in documentation: `$SQLSERVER_PASSWORD`
2. **Provide setup instructions** for environment variables
3. **Include security warnings** about password management
4. **Use examples** that don't expose real credentials

---

## 📋 **Migration Checklist**

- [ ] Create `.env` file with database credentials
- [ ] Add `.env` to `.gitignore`
- [ ] Update all documentation to use environment variables
- [ ] Create helper scripts for common database operations
- [ ] Update Docker Compose to use environment variables
- [ ] Test all database connectivity commands
- [ ] Update CI/CD pipelines to use secure credential management
- [ ] Document the new secure approach for team members

---

## 🚨 **Security Warnings**

1. **Never commit passwords** to version control
2. **Use strong, unique passwords** for each environment
3. **Rotate passwords regularly** (every 90 days)
4. **Monitor access logs** for suspicious activity
5. **Use least privilege principle** for database users
6. **Enable SQL Server audit logging** in production

---

_Last Updated: January 18, 2025_  
_Version: 1.0_
