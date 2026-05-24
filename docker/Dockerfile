# Monolith IdP Platform image (API + admin SPA + nginx TLS proxy).
# Build from repository root:
#   docker build -f docker/Dockerfile -t mrffilipe/idpplatform:<tag> .

# --- API build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src

COPY backend/IdPPlatform.Domain/IdPPlatform.Domain.csproj backend/IdPPlatform.Domain/
COPY backend/IdPPlatform.Application/IdPPlatform.Application.csproj backend/IdPPlatform.Application/
COPY backend/IdPPlatform.Infrastructure/IdPPlatform.Infrastructure.csproj backend/IdPPlatform.Infrastructure/
COPY backend/IdPPlatform.API/IdPPlatform.API.csproj backend/IdPPlatform.API/

RUN dotnet restore backend/IdPPlatform.API/IdPPlatform.API.csproj

COPY backend/ .

WORKDIR /src
RUN dotnet publish IdPPlatform.API/IdPPlatform.API.csproj -c Release -o /app/publish

RUN dotnet tool install --global dotnet-ef --version 8.0.26
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations bundle \
    --project IdPPlatform.Infrastructure/IdPPlatform.Infrastructure.csproj \
    --startup-project IdPPlatform.API/IdPPlatform.API.csproj \
    --output /app/efbundle \
    --self-contained \
    --target-runtime linux-x64

# --- SPA build (empty API/redirect URL → browser uses same origin as nginx in production) ---
FROM node:22-alpine AS spa-build
WORKDIR /app

ENV VITE_API_BASE_URL=
ENV VITE_OAUTH_REDIRECT_URI=
ENV VITE_API_VERSION=1.0
ENV VITE_API_TIMEOUT_MS=30000
ENV VITE_OAUTH_CLIENT_ID=platform-admin-web

COPY frontend/package.json frontend/package-lock.json* ./
RUN npm ci

COPY frontend/ .
RUN npm run build

# --- Runtime: Kestrel + nginx ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl nginx \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/keys/data-protection /etc/nginx/certs \
    && chown -R www-data:www-data /var/lib/nginx

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://127.0.0.1:8080

COPY --from=api-build /app/publish .
COPY --from=api-build /app/efbundle ./efbundle
COPY --from=spa-build /app/dist /usr/share/nginx/html

RUN rm -f /etc/nginx/sites-enabled/default /etc/nginx/conf.d/default.conf
COPY docker/nginx/app.conf /etc/nginx/conf.d/idpplatform.conf
COPY docker/nginx/proxy_params.conf /etc/nginx/conf.d/proxy_params.conf

COPY docker/scripts/entrypoint-app.sh /docker/scripts/entrypoint-app.sh

RUN chmod +x /app/efbundle /docker/scripts/entrypoint-app.sh

EXPOSE 80 443

HEALTHCHECK --interval=30s --timeout=5s --retries=3 --start-period=60s \
  CMD curl -f http://127.0.0.1:8080/v1.0/platform/status || exit 1

ENTRYPOINT ["/docker/scripts/entrypoint-app.sh"]
