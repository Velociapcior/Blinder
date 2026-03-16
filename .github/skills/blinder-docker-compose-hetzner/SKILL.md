---
name: blinder-docker-compose-hetzner
description: |
  Defines the Docker Compose deployment stack for Blinder on a self-hosted Hetzner VPS.
  Use this skill when: setting up or modifying the Docker Compose configuration, adding new
  services, configuring Traefik reverse proxy, managing MinIO object storage, setting up
  SSL/TLS, configuring environment variables for production, or debugging container networking.
  Triggers: "Docker Compose", "Hetzner", "deploy", "VPS", "production setup", "Traefik",
  "container", "self-hosted", "MinIO config", "docker stack".
---

# Blinder Docker Compose — Hetzner VPS

## Stack Overview

```
Hetzner VPS (Ubuntu 24 LTS)
└── Docker Compose
    ├── traefik          # Reverse proxy + automatic SSL (Let's Encrypt)
    ├── api              # ASP.NET Core .NET 10 (Blinder backend)
    ├── minio            # Object storage (photos)
    └── postgres         # Primary database
```

All services communicate on an internal Docker network. Only Traefik exposes ports 80/443.

## Directory Layout on VPS

```
/opt/blinder/
├── docker-compose.yml
├── docker-compose.override.yml   # Local overrides (gitignored)
├── .env                          # Secrets (gitignored, managed manually)
├── traefik/
│   ├── traefik.yml              # Static Traefik config
│   └── acme.json                # Let's Encrypt certs (chmod 600)
├── data/
│   ├── postgres/                # Postgres volume mount
│   └── minio/                   # MinIO volume mount
└── logs/
```

## docker-compose.yml

```yaml
version: "3.9"

networks:
  blinder-net:
    driver: bridge

volumes:
  postgres-data:
  minio-data:
  traefik-certs:

services:

  traefik:
    image: traefik:v3.0
    container_name: blinder-traefik
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock:ro
      - ./traefik/traefik.yml:/etc/traefik/traefik.yml:ro
      - traefik-certs:/certs
    networks:
      - blinder-net
    labels:
      - "traefik.enable=false"  # Do not expose Traefik dashboard publicly

  postgres:
    image: postgres:16-alpine
    container_name: blinder-postgres
    restart: unless-stopped
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - blinder-net
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
      interval: 10s
      timeout: 5s
      retries: 5

  minio:
    image: minio/minio:latest
    container_name: blinder-minio
    restart: unless-stopped
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: ${MINIO_ACCESS_KEY}
      MINIO_ROOT_PASSWORD: ${MINIO_SECRET_KEY}
    volumes:
      - minio-data:/data
    networks:
      - blinder-net
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:9000/minio/health/live"]
      interval: 30s
      timeout: 10s
      retries: 3
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.minio.rule=Host(`storage.${DOMAIN}`)"
      - "traefik.http.routers.minio.entrypoints=websecure"
      - "traefik.http.routers.minio.tls.certresolver=letsencrypt"
      - "traefik.http.services.minio.loadbalancer.server.port=9000"

  api:
    image: ${REGISTRY}/blinder-api:${IMAGE_TAG:-latest}
    container_name: blinder-api
    restart: unless-stopped
    depends_on:
      postgres:
        condition: service_healthy
      minio:
        condition: service_healthy
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: >-
        Host=postgres;Database=${POSTGRES_DB};
        Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      Minio__Endpoint: "minio:9000"
      Minio__AccessKey: ${MINIO_ACCESS_KEY}
      Minio__SecretKey: ${MINIO_SECRET_KEY}
      Minio__UseSSL: "false"
      Minio__BucketName: "blinder-photos"
      Jwt__Secret: ${JWT_SECRET}
      Jwt__Issuer: "https://api.${DOMAIN}"
      Jwt__Audience: "blinder-app"
    networks:
      - blinder-net
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.api.rule=Host(`api.${DOMAIN}`)"
      - "traefik.http.routers.api.entrypoints=websecure"
      - "traefik.http.routers.api.tls.certresolver=letsencrypt"
      - "traefik.http.services.api.loadbalancer.server.port=8080"
      # WebSocket support for SignalR
      - "traefik.http.middlewares.sslheader.headers.customrequestheaders.X-Forwarded-Proto=https"
      - "traefik.http.routers.api.middlewares=sslheader"
```

## traefik/traefik.yml

```yaml
api:
  dashboard: false

entryPoints:
  web:
    address: ":80"
    http:
      redirections:
        entryPoint:
          to: websecure
          scheme: https
  websecure:
    address: ":443"

certificatesResolvers:
  letsencrypt:
    acme:
      email: ${ACME_EMAIL}
      storage: /certs/acme.json
      httpChallenge:
        entryPoint: web

providers:
  docker:
    exposedByDefault: false
    network: blinder-net

log:
  level: WARN
```

## .env File (Never Commit)

```dotenv
# Domain
DOMAIN=blinder.app
ACME_EMAIL=admin@blinder.app

# Database
POSTGRES_DB=blinder
POSTGRES_USER=blinder_user
POSTGRES_PASSWORD=<strong-random-password>

# MinIO
MINIO_ACCESS_KEY=<minio-access-key>
MINIO_SECRET_KEY=<minio-secret-key-min-16-chars>

# JWT
JWT_SECRET=<256-bit-random-secret>

# Container registry
REGISTRY=ghcr.io/piotr-palej
IMAGE_TAG=latest
```

## Deployment Commands

```bash
# Initial setup on fresh VPS
mkdir -p /opt/blinder/traefik /opt/blinder/data/postgres /opt/blinder/data/minio
touch /opt/blinder/traefik/acme.json
chmod 600 /opt/blinder/traefik/acme.json

# Pull latest images and redeploy API only (zero-downtime for stateless service)
cd /opt/blinder
docker compose pull api
docker compose up -d --no-deps api

# Full stack restart
docker compose down && docker compose up -d

# View API logs
docker compose logs -f api

# MinIO bucket init (run once after first deploy)
docker compose exec minio mc alias set local http://localhost:9000 $MINIO_ACCESS_KEY $MINIO_SECRET_KEY
docker compose exec minio mc mb local/blinder-photos
docker compose exec minio mc mb local/blinder-photos/pending
docker compose exec minio mc mb local/blinder-photos/approved
```

## GitHub Actions Deployment (CI/CD)

```yaml
# .github/workflows/deploy.yml
- name: Deploy to Hetzner
  run: |
    ssh ${{ secrets.VPS_USER }}@${{ secrets.VPS_HOST }} \
      "cd /opt/blinder && \
       docker compose pull api && \
       docker compose up -d --no-deps api"
```

## SignalR WebSocket Notes

Traefik handles WebSocket upgrades automatically for SignalR. The `X-Forwarded-Proto` header
middleware ensures ASP.NET Core sees the correct scheme for JWT validation on WebSocket connections.

In `appsettings.Production.json`:
```json
{
  "AllowedHosts": "api.blinder.app",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://+:8080"
      }
    }
  }
}
```

## Key Rules

- NEVER expose MinIO console (port 9001) via Traefik — internal admin only
- NEVER commit `.env` — manage secrets manually on VPS or via GitHub Secrets + SSH
- ALWAYS use `depends_on` with `condition: service_healthy` to prevent startup races
- MinIO talks to the API over internal Docker network — no SSL needed internally
- Let's Encrypt `acme.json` must be `chmod 600` — Traefik will refuse to start otherwise
- Use `--no-deps api` on redeploy to avoid restarting Postgres/MinIO unnecessarily
- SignalR requires the `X-Forwarded-Proto` middleware or JWT validation fails on WSS connections
