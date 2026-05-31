#!/bin/sh
# Apply EF migrations when enabled, then start Kestrel.
set -eu

if [ "${Database__ApplyMigrationsOnStartup:-true}" = "true" ]; then
  echo "Applying database migrations..."
  ./efbundle
fi

exec dotnet Kyvo.API.dll
