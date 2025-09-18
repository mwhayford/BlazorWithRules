#!/bin/bash
# Database Query Helper Script
# Executes SQL queries against the database securely

# Load environment variables
if [ -f .env ]; then
    source .env
else
    echo "Error: .env file not found. Please create it with database credentials."
    exit 1
fi

# Check if query is provided
if [ -z "$1" ]; then
    echo "Usage: $0 \"SQL_QUERY\""
    echo "Example: $0 \"SELECT COUNT(*) FROM AspNetUsers\""
    exit 1
fi

# Execute query
docker exec blazorwithrules-sqlserver /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U ${SQLSERVER_USERNAME:-sa} -P "$SQLSERVER_PASSWORD" -C \
    -d ${SQLSERVER_DATABASE:-BlazorWithRules} -Q "$1"
