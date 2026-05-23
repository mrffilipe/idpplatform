# Build from repository root:
#   docker build -f backend/Dockerfile -t <username>/idpplatform-api:<tag> .

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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
