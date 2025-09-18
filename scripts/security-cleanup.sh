#!/bin/bash
# Security Cleanup Script - Remove sensitive data from documentation

echo "🔐 Starting security cleanup of documentation files..."

# List of files to clean
FILES=(
    "docs/Pre-commit-Security-Correction.md"
    "docs/Git-Secrets-Integration.md"
    "docs/Troubleshooting-Guide.md"
    "docs/Secure-Database-Connectivity.md"
    "docs/Performance-Profiling-Tools.md"
    "docs/Getting-Started.md"
    "docs/Docker-Development-Setup.md"
)

# Sensitive patterns to replace
declare -A REPLACEMENTS=(
    ["YourStrong@Passw0rd"]="[REDACTED_PASSWORD]"
    ["[REDACTED_CLIENT_ID]"]="[REDACTED_CLIENT_ID]"
    ["[REDACTED_CLIENT_SECRET]"]="[REDACTED_CLIENT_SECRET]"
    ["[REDACTED_CLIENT_ID_PREFIX]"]="[REDACTED_CLIENT_ID_PREFIX]"
    ["[REDACTED_SECRET_PREFIX]"]="[REDACTED_SECRET_PREFIX]"
)

# Process each file
for file in "${FILES[@]}"; do
    if [ -f "$file" ]; then
        echo "Processing: $file"
        
        # Create backup
        cp "$file" "$file.backup"
        
        # Apply replacements
        for pattern in "${!REPLACEMENTS[@]}"; do
            replacement="${REPLACEMENTS[$pattern]}"
            sed -i "s|$pattern|$replacement|g" "$file"
        done
        
        echo "✅ Cleaned: $file"
    else
        echo "⚠️  File not found: $file"
    fi
done

echo ""
echo "🔐 Security cleanup completed!"
echo "📁 Backup files created with .backup extension"
echo "🚨 Please review changes before committing"
