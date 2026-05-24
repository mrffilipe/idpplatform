#!/bin/sh
# IdP Platform monolith: migrations, API (background), nginx (foreground).
set -eu

cd /app

if [ "${Database__ApplyMigrationsOnStartup:-true}" = "true" ]; then
  echo "Applying database migrations..."
  ./efbundle
fi

export ASPNETCORE_URLS="${ASPNETCORE_URLS:-http://127.0.0.1:8080}"

echo "Starting API..."
dotnet IdPPlatform.API.dll &
API_PID=$!

trap 'kill -TERM "$API_PID" 2>/dev/null || true' TERM INT

echo "Starting nginx..."
exec nginx -g 'daemon off;'
