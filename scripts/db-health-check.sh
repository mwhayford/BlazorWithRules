#!/bin/bash
# Database Health Check Script
# Checks database connectivity and basic health metrics

# Load environment variables
if [ -f .env ]; then
    source .env
else
    echo "Error: .env file not found. Please create it with database credentials."
    exit 1
fi

echo "🔍 Checking database connectivity..."

# Test basic connectivity
if docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U ${SQLSERVER_USERNAME:-sa} -P "$SQLSERVER_PASSWORD" -C -Q "SELECT 1" > /dev/null 2>&1; then
    echo "✅ Database connection successful"
else
    echo "❌ Database connection failed"
    exit 1
fi

# Check database exists
echo "🔍 Checking if database exists..."
DB_EXISTS=$(docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U ${SQLSERVER_USERNAME:-sa} -P "$SQLSERVER_PASSWORD" -C \
    -Q "SELECT COUNT(*) FROM sys.databases WHERE name = '${SQLSERVER_DATABASE:-BlazorWithRules}'" \
    -h -1 -W)

if [ "$DB_EXISTS" = "1" ]; then
    echo "✅ Database '${SQLSERVER_DATABASE:-BlazorWithRules}' exists"
else
    echo "❌ Database '${SQLSERVER_DATABASE:-BlazorWithRules}' does not exist"
fi

# Check key tables
echo "🔍 Checking key tables..."
TABLES=$(docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U ${SQLSERVER_USERNAME:-sa} -P "$SQLSERVER_PASSWORD" -C \
    -d ${SQLSERVER_DATABASE:-BlazorWithRules} \
    -Q "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('AspNetUsers', 'LoanApplications', 'Users')" \
    -h -1 -W)

echo "📊 Found $TABLES key tables in database"

echo "✅ Database health check completed"
