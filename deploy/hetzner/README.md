# Blinder — Hetzner VPS Deployment Guide

## VPS Layout

This deployment targets a single Hetzner VPS running Ubuntu 24 LTS with Docker Engine installed.

```
/opt/blinder/
├── docker-compose.yml             # Symlink or copy from repo root
├── docker-compose.override.yml   # From override.example.yml — gitignored, manage manually
├── .env                           # Real secrets — gitignored, manage manually
├── deploy/
│   └── hetzner/
│       └── traefik/
│           └── traefik.yml       # Static Traefik config
```

## Initial Setup

```bash
# 1. Clone the repo (or copy files) to /opt/blinder
git clone https://github.com/your-org/blinder /opt/blinder
cd /opt/blinder

# 2. Create the .env file from the example
cp deploy/hetzner/.env.example .env
# Edit .env with real values — never commit this file

# 3. Copy the production Compose override
cp deploy/hetzner/docker-compose.override.example.yml docker-compose.override.yml
# Edit docker-compose.override.yml with your registry and image tags

# 4. Pull images and start the stack
docker compose pull
docker compose up -d
```

## Deployment Flow

### Full stack redeploy (downtime ~seconds)

```bash
cd /opt/blinder
docker compose pull
docker compose up -d
```

### Rolling update for a single app service (no-downtime for stateless services)

```bash
docker compose pull api
docker compose up -d --no-deps api
```

### Restart without data loss

```bash
docker compose down && docker compose up -d
# PostgreSQL and MinIO state persists in named Docker volumes
```

## Secrets Handling

- Never commit `.env` or `docker-compose.override.yml` to the repository.
- Manage secrets manually on the VPS or inject via CI/CD (GitHub Actions SSH deployment).
- Use a dedicated PostgreSQL bootstrap admin credential plus separate runtime credentials for `identity.*` and `app.*` schema owners.
- Keep `POSTGRES_HOST_PORT` bound to loopback unless you intentionally need host-level database access on the VPS.
- Database passwords and MinIO credentials must be strong random values.
- Traefik stores ACME state in the `traefik-certs` named Docker volume.

## DNS Setup

Point the following A records to your VPS IP before starting the stack:

| Record | Target |
|--------|--------|
| `api.<your-domain>` | VPS IP |
| `auth.<your-domain>` | VPS IP |
| `admin.<your-domain>` | VPS IP |

Let's Encrypt HTTP challenge requires ports 80 and 443 to be reachable from the internet.

## Local Development / Smoke Testing

For local testing without real DNS, add entries to `/etc/hosts` (or Windows `hosts` file):

```
127.0.0.1  api.blinder.local auth.blinder.local admin.blinder.local
```

Then run the root stack without the Hetzner override:

```bash
cp .env.example .env
docker compose up -d
```

The PostgreSQL init scripts in `postgres/init/` only run when the database volume is created for the first time. Use `docker compose down -v` before re-testing schema and role bootstrap behavior locally.

The baseline stack routes over HTTP on port `80` using the root `traefik/traefik.yml` config. The Hetzner override switches Traefik to ACME-backed TLS on `443`.

## Service Health Checks

All three ASP.NET app services expose `/health` on their internal HTTP port `8080`.
PostgreSQL uses `pg_isready` and MinIO uses the `/minio/health/live` endpoint.
The Compose stack uses `depends_on: condition: service_healthy` to prevent startup races.

## Notes

- MinIO console (port 9001) is intentionally not exposed via Traefik. Access it through
  `docker compose exec minio mc` or via an SSH tunnel if needed.
- ASP.NET Core apps run on HTTP internally (port 8080). TLS terminates at Traefik.
  The root stack keeps local routing on HTTP, while the Hetzner override enables TLS at Traefik.
- Data Protection keys and IdentityServer signing keys are persisted in named Docker volumes
  and loaded from mounted paths so container restarts reuse the same key material.
