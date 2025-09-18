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
    echo "  1. Create .env file with: SQLSERVER_PASSWORD=YourStrong@Passw0rd"
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
