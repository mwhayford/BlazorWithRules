# 🔒 Secure Database Scripts

This directory contains secure database connectivity scripts that eliminate password exposure in command lines and documentation.

## 📋 **Setup Instructions**

### **1. Create Environment File**

```bash
# Copy the template and update with your credentials
cp .env.template .env

# Edit the .env file with your actual database credentials
# DO NOT COMMIT THE .env FILE TO VERSION CONTROL
```

### **2. Environment File Format**

```bash
# .env file example
SQLSERVER_PASSWORD=YourStrong@Passw0rd
SQLSERVER_USERNAME=sa
SQLSERVER_DATABASE=BlazorWithRules
SQLSERVER_SERVER=localhost
```

## 🛠️ **Available Scripts**

### **Linux/macOS (Bash)**

- `db-connect.sh` - Execute SQL queries securely
- `db-health-check.sh` - Check database connectivity and health
- `db-query.sh` - Execute SQL queries with database context

### **Windows (PowerShell)**

- `db-connect.ps1` - Execute SQL queries securely

## 📖 **Usage Examples**

### **Basic Connectivity Check**

```bash
# Linux/macOS
./scripts/db-connect.sh "SELECT 1"

# Windows PowerShell
.\scripts\db-connect.ps1 -Query "SELECT 1"
```

### **Database Health Check**

```bash
# Linux/macOS
./scripts/db-health-check.sh
```

### **Execute Queries**

```bash
# Linux/macOS
./scripts/db-query.sh "SELECT COUNT(*) FROM AspNetUsers"

# Windows PowerShell
.\scripts\db-connect.ps1 -Query "SELECT COUNT(*) FROM AspNetUsers" -Database "BlazorWithRules"
```

## 🔐 **Security Features**

- ✅ **No password exposure** in command lines
- ✅ **Environment variable loading** from .env file
- ✅ **Error handling** for missing credentials
- ✅ **Cross-platform support** (Linux, macOS, Windows)
- ✅ **Git ignore protection** for .env files

## 🚨 **Security Warnings**

1. **Never commit .env files** to version control
2. **Use strong, unique passwords** for each environment
3. **Rotate passwords regularly** (every 90 days)
4. **Monitor access logs** for suspicious activity
5. **Use least privilege principle** for database users

## 🔧 **Troubleshooting**

### **Script Not Found**

```bash
# Make scripts executable (Linux/macOS)
chmod +x scripts/*.sh
```

### **Permission Denied**

```bash
# Check file permissions
ls -la scripts/

# Fix permissions if needed
chmod +x scripts/db-connect.sh
```

### **Environment File Not Found**

```bash
# Create .env file from template
echo "SQLSERVER_PASSWORD=YourStrong@Passw0rd" > .env
echo "SQLSERVER_USERNAME=sa" >> .env
echo "SQLSERVER_DATABASE=BlazorWithRules" >> .env
```

---

_Last Updated: January 18, 2025_  
_Version: 1.0_
