# Build from repository root:
#   docker build -f frontend/Dockerfile \
#     --build-arg VITE_API_BASE_URL=http://localhost:5000 \
#     --build-arg VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback \
#     -t mrffilipe/kyvo-frontend:<tag> .

# SPA build (monolith image leaves API/redirect URL empty — same origin as nginx at runtime)
FROM node:22-alpine AS build
WORKDIR /app

ARG VITE_API_BASE_URL=http://localhost:5000
ARG VITE_API_VERSION=1.0
ARG VITE_API_TIMEOUT_MS=30000
ARG VITE_OAUTH_CLIENT_ID=platform-admin-web
ARG VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback

ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_API_VERSION=$VITE_API_VERSION
ENV VITE_API_TIMEOUT_MS=$VITE_API_TIMEOUT_MS
ENV VITE_OAUTH_CLIENT_ID=$VITE_OAUTH_CLIENT_ID
ENV VITE_OAUTH_REDIRECT_URI=$VITE_OAUTH_REDIRECT_URI

COPY frontend/package.json frontend/package-lock.json* ./
RUN npm ci

COPY frontend/ .
RUN npm run build

# Serve static SPA via nginx
FROM nginx:alpine AS runtime
COPY docker/nginx/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
