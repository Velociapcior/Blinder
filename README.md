# Blinder

Blinder is a container-first backend stack for a privacy-focused matching app. The repository contains the ASP.NET Core 10 API, PostgreSQL 16 + PostGIS, and Nginx reverse proxy orchestrated with Docker Compose.

## Table of Contents

- [Repository Layout](#repository-layout)
- [Architecture Overview](#architecture-overview)
- [Prerequisites](#prerequisites)
- [Initial Setup](#initial-setup)
- [Run the Project](#run-the-project)
- [Run the Mobile App](mobile/README.md)
- [Database and Migrations](docs/database.md)
- [Run Tests](#run-tests)
- [Configuration](#configuration)
- [API Endpoints](#api-endpoints)
- [Operational Notes](#operational-notes)
- [Troubleshooting](#troubleshooting)

## Repository Layout

```
.
├── backend/
│   ├── Blinder.Api/          # ASP.NET Core 10 Web API
│   └── Blinder.Tests/        # xUnit test project
├── mobile/                   # Expo SDK 55 React Native app
│   ├── app/                  # Expo Router screens
│   ├── components/           # Shared UI components
│   ├── contexts/             # React contexts (AccessibilityContext)
│   ├── hooks/                # Custom hooks (useAccessibility, useResponsiveLayout)
│   ├── services/             # apiClient, signalrService, storageService
│   └── constants/            # Design tokens (theme.ts, errors.ts)
├── docs/                     # Project context and tech preferences
├── migrations/
│   └── latest.sql            # Idempotent SQL for production deploys
├── nginx/                    # Reverse proxy config
├── postgres/
│   └── initdb/               # SQL scripts run once on fresh DB container
├── docker-compose.yml        # Production-oriented base stack
├── docker-compose.override.yml  # Dev overrides (SDK image + source mount)
└── .env.example              # Required environment variables template
```

## Architecture Overview

```
                    ┌─────────────────────────────────────┐
  HTTP :80          │  Docker Compose network              │
  ─────────────►  nginx ──► api:8080 ──► db:5432          │
                    │  (nginx:1.27-alpine)  (postgis:16-3.4)│
                    └─────────────────────────────────────┘

Dev:   SDK build stage — source mounted from ./backend, dotnet run
Prod:  Runtime-only stage — pre-built binary, migrations/latest.sql applied manually
```

## Prerequisites

| Requirement | Purpose |
|---|---|
| Docker Desktop (or Engine + Compose v2) | Run the full stack |
| Git | Clone and version control |
| .NET 10 SDK | Host-side test runs and EF tooling |
| VS Code + C# Dev Kit (recommended) | IDE support |

## Initial Setup

1. Clone the repository.
2. Create your local environment file from the template.

   PowerShell:
   ```powershell
   Copy-Item .env.example .env
   ```

   Bash:
   ```bash
   cp .env.example .env
   ```

3. Edit `.env` and replace placeholder values. At minimum set:
   - `POSTGRES_PASSWORD` — strong random password
   - `JWT_SECRET` — at least 32 random characters

## Run the Project

### Development mode (recommended)

Applies `docker-compose.yml` + `docker-compose.override.yml` automatically. The API container builds from the SDK image with a live source mount — no image rebuild needed for code changes, just restart the container.

```bash
docker compose up -d --build
```

Smoke test (the only currently active endpoint):

```bash
curl -i http://localhost/health
```

Check container status:

```bash
docker compose ps
```

Stream API logs:

```bash
docker compose logs -f api
```

Stop the stack (data is preserved in `./postgres/data/pgdata`):

```bash
docker compose down
```

### Production-like mode

Runs only the base compose file (no dev overrides, runtime stage only):

```bash
docker compose -f docker-compose.yml up -d --build
```

Apply the migration script before starting the API in production:

```bash
docker compose exec -T db psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" < migrations/latest.sql
```

## Database and Migrations

- **Development:** DB is created and migrations are applied automatically on startup.
- **All other environments:** apply `migrations/latest.sql` manually before deploying a new API version.

For full details — adding migrations, regenerating the SQL artifact, connecting directly to the DB — see [docs/database.md](docs/database.md).

## Run the Mobile App

See [mobile/README.md](mobile/README.md) for full setup instructions, project structure, scripts, and accessibility conventions.

---

## Run Tests

From the repository root:

```bash
dotnet test backend/Blinder.Tests/Blinder.Tests.csproj
```

With coverage collection:

```bash
dotnet test backend/Blinder.Tests/Blinder.Tests.csproj --collect:"XPlat Code Coverage"
```

## Configuration

| Variable | Where | Purpose |
|---|---|---|
| `POSTGRES_DB` | `.env` | Database name |
| `POSTGRES_USER` | `.env` | Database user |
| `POSTGRES_PASSWORD` | `.env` | Database password |
| `JWT_SECRET` | `.env` | HS256 signing key (min 32 chars) |
| `JWT_ISSUER` | `.env` | Token issuer claim |
| `JWT_AUDIENCE` | `.env` | Token audience claim |
| `S3_ENDPOINT` | `.env` | MinIO / S3-compatible endpoint |
| `S3_ACCESS_KEY` | `.env` | Object storage access key |
| `S3_SECRET_KEY` | `.env` | Object storage secret key |
| `S3_BUCKET_NAME` | `.env` | Default storage bucket |
| `FIREBASE_CREDENTIALS_JSON` | `.env` | Firebase service account JSON |
| `APNS_*` | `.env` | APNs push notification credentials |
| `ConnectionStrings__DefaultConnection` | `appsettings.json` / env | Full Npgsql connection string |
| `ASPNETCORE_ENVIRONMENT` | container env | `Development` enables DB auto-migrate |

See `.env.example` for the full list with descriptions. Primary app config files:

- `backend/Blinder.Api/appsettings.json`
- `backend/Blinder.Api/appsettings.Development.json`

## API Endpoints

Base URL (through Nginx in dev): `http://localhost`

| Method | Path | Description |
|---|---|---|
| `GET` | `/health` | Liveness check — returns `{ status, utc }` |

```bash
curl -i http://localhost/health
# HTTP/1.1 200 OK
# {"status":"ok","utc":"2026-..."}
```

`backend/Blinder.Api/Blinder.Api.http` contains request templates for VS Code REST Client.

Endpoint surface will expand as feature stories are implemented.

## Operational Notes

1. Never commit `.env` to source control.
2. `Development` environment only: DB is created and migrations are applied automatically on startup.
3. All other environments: apply `migrations/latest.sql` before deploying a new API version.
4. Do **not** run `docker compose down -v` — it would destroy the bind-mounted DB data at `./postgres/data/pgdata`.
5. To fully wipe the dev database and start fresh: `Remove-Item -Recurse ./postgres/data/pgdata` then `docker compose up -d --build`.
6. Nginx already includes WebSocket upgrade headers for `/hubs/` (SignalR).
7. API logs write to console and rolling files in `logs/` inside the container.
8. EF design-time tooling (`dotnet ef`) is only available inside the dev container (SDK stage). It is not available in the production runtime image.

## Troubleshooting

**API container exits immediately (`exited with code 0`)**
- Check `docker compose logs api`. Ensure `docker-compose.override.yml` is present and sets a `command:` override — the SDK build stage default is `/bin/bash` which exits immediately without it.

**API fails to start — missing environment variable errors**
- Validate `.env` exists and all required keys are populated. Variables with `:?` in the override file are required and will fail fast if absent.

**Stack starts but `http://localhost` is unreachable**
- Check `docker compose ps` and `docker compose logs -f nginx api`.

**`relation "..." already exists` on startup (SqlState 42P07)**
- Tables already exist but `__EFMigrationsHistory` is empty (e.g. after applying `latest.sql` manually then restarting in `Development`). The startup code catches this and logs a warning — the API will continue booting. This is expected in the dev recovery scenario.

**`No password has been provided` (SASL/SCRAM-SHA-256)**
- Connection string must include the password. Verify `ConnectionStrings__DefaultConnection` in `appsettings.Development.json` or as an environment variable override matches the `POSTGRES_PASSWORD` in `.env`.

**EF tooling command fails in container**
- Ensure you are using dev mode so the SDK build stage is active. The `--rm` run container must use the same image that has `dotnet ef` available.
