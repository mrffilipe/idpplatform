#!/bin/sh
set -eu

if [ "${Database__ApplyMigrationsOnStartup:-true}" = "true" ]; then
  echo "Applying database migrations..."
  ./efbundle
fi

exec dotnet IdPPlatform.API.dll
