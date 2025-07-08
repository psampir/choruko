#!/bin/sh
set -e

echo "Starting Manager Container..."

# Run EF bundle for migrations
echo "Applying database migrations..."
./efbundle --connection "Host=${DB_HOST:-database};Database=${DB_DATABASE_NAME};Username=${DB_USERNAME};Password=${DB_PASSWORD}"

# Create health check signal file
touch /tmp/migrations-complete
echo "Migrations complete."

# Create wrapper scripts for Python tools
echo "Creating wrappers for Python admin tools..."
mkdir -p /usr/local/bin
for tool in /app/admin-tools/*.py; do
    if [ -f "$tool" ]; then
        toolname=$(basename "$tool" .py)
        cat > "/usr/local/bin/$toolname" << EOF
#!/bin/sh
/app/venv/bin/python "$tool" "\$@"
EOF
        chmod +x "/usr/local/bin/$toolname"
        echo "Created wrapper for $toolname"
    fi
done

echo "Manager ready for admin tasks."

# Keep running for admin access and health checks
exec tail -f /dev/null