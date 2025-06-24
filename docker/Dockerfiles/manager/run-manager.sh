#!/bin/bash
set -e

echo "Starting Manager Container..."

# Run EF bundle for migrations
echo "Applying database migrations..."
./efbundle --connection "Host=${DB_HOST:-database};Database=${DB_DATABASE_NAME};Username=${DB_USERNAME};Password=${DB_PASSWORD}"

# Create health check signal file
touch /tmp/migrations-complete
echo "Migrations complete."

echo "Manager ready for admin tasks."
# Keep running for admin access and health checks
exec tail -f /dev/null
