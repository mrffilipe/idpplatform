# Build from repository root:
#   docker build -f backend/Dockerfile -t mrffilipe/kyvo-api:<tag> .

# Restore and publish the API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend/Kyvo.Domain/Kyvo.Domain.csproj backend/Kyvo.Domain/
COPY backend/Kyvo.Application/Kyvo.Application.csproj backend/Kyvo.Application/
COPY backend/Kyvo.Infrastructure/Kyvo.Infrastructure.csproj backend/Kyvo.Infrastructure/
COPY backend/Kyvo.API/Kyvo.API.csproj backend/Kyvo.API/

RUN dotnet restore backend/Kyvo.API/Kyvo.API.csproj

COPY backend/ .

WORKDIR /src
RUN dotnet publish Kyvo.API/Kyvo.API.csproj -c Release -o /app/publish

# Bundle EF migrations for container startup
RUN dotnet tool install --global dotnet-ef --version 8.0.26
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet ef migrations bundle \
    --project Kyvo.Infrastructure/Kyvo.Infrastructure.csproj \
    --startup-project Kyvo.API/Kyvo.API.csproj \
    --output /app/efbundle \
    --self-contained \
    --target-runtime linux-x64

# Runtime: non-root user, curl for healthcheck, entrypoint applies migrations
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && groupadd --gid 1000 appuser \
    && useradd --uid 1000 --gid appuser --shell /bin/bash --create-home appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
COPY --from=build /app/efbundle ./efbundle
COPY docker/scripts/entrypoint-api.sh /app/entrypoint.sh

RUN chmod +x /app/efbundle /app/entrypoint.sh \
    && mkdir -p /app/keys/data-protection \
    && chown -R appuser:appuser /app

USER appuser

EXPOSE 8080

ENTRYPOINT ["/app/entrypoint.sh"]
