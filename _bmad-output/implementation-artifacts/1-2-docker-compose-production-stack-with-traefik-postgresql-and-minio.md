# Story 1.2: Docker Compose Production Stack with Traefik, PostgreSQL, and MinIO

Status: review

## Story

As a developer,
I want a Docker Compose stack that runs the full Blinder infrastructure locally and in production,
so that all services are consistently deployable and network-isolated.

## Acceptance Criteria

1. Given `docker-compose.yml` exists at the project root, when `docker compose up -d` is run, then containers start for `traefik`, `identityserver`, `api`, `adminpanel`, `postgres`, and `minio`.
2. Only Traefik exposes public ports `80` and `443`; all other services communicate on an internal Docker network.
3. Host-based routing separates `api.<domain>`, `auth.<domain>`, and `admin.<domain>`.
4. PostgreSQL uses version `18` and both PostgreSQL and MinIO persist data through named volumes.
5. ASP.NET Core Data Protection keys are persisted outside ephemeral container filesystems and mounted into the app containers that need them.
6. Identity signing and encryption keys are persisted and mounted for runtime use so container restarts do not invalidate key material.
7. `identityserver`, `api`, and `adminpanel` run as stateless containers; all mutable state lives in infrastructure volumes or mounted key directories.
8. A `deploy/hetzner/` directory contains environment-specific Compose override/config examples for VPS deployment.
9. `docker compose down` followed by `docker compose up -d` restores services without PostgreSQL or MinIO data loss.
10. All app services expose working health checks and the Compose stack uses them to avoid startup races.

## Tasks / Subtasks

- [x] Create container build assets for the three ASP.NET hosts (AC: 1, 7, 10)
  - [x] Add one production-ready Dockerfile per app host under `backend/src/Blinder.IdentityServer/`, `backend/src/Blinder.Api/`, and `backend/src/Blinder.AdminPanel/`.
  - [x] Add a root `.dockerignore` that excludes `bin/`, `obj/`, test results, Git metadata, local secrets, and PostgreSQL data directories from build context.
  - [x] Add `appsettings.Production.json` files or equivalent environment-driven production settings needed for container HTTP binding and reverse-proxy deployment.
  - [x] Ensure each container listens on internal HTTP only (for example `8080`) behind Traefik and keeps `/health` reachable from the internal network.

- [x] Author the root Docker Compose stack (AC: 1, 2, 3, 4, 5, 6, 7, 9, 10)
  - [x] Create root `docker-compose.yml` with service definitions for `traefik`, `identityserver`, `api`, `adminpanel`, `postgres`, and `minio`.
  - [x] Define a dedicated internal network for app-to-app communication; do not publish ports for any service except Traefik.
  - [x] Configure PostgreSQL 18 and MinIO with named volumes.
  - [x] Define separate persistent volumes for Data Protection keys and IdentityServer key material.
  - [x] Add service health checks and `depends_on` conditions so app services wait for PostgreSQL and MinIO readiness before starting.

- [x] Configure Traefik routing and reverse-proxy correctness (AC: 2, 3, 10)
  - [x] Add Traefik labels/routing rules for `api.<domain>`, `auth.<domain>`, and `admin.<domain>`.
  - [x] Keep Traefik as the sole public ingress on `80/443`.
  - [x] Do not expose the MinIO console publicly.
  - [x] Update the ASP.NET app hosts as needed so TLS termination at Traefik does not trigger redirect loops from `UseHttpsRedirection()`.
  - [x] Configure forwarded headers handling if required so apps correctly understand the external HTTPS scheme.

- [x] Add Hetzner deployment examples and operator documentation (AC: 8)
  - [x] Create `deploy/hetzner/README.md` documenting the intended VPS layout, deployment flow, and secrets handling.
  - [x] Add `deploy/hetzner/.env.example` with non-secret placeholders for domain, registry, database, MinIO, and certificate settings.
  - [x] Add `deploy/hetzner/docker-compose.override.example.yml` showing registry-based images and production-specific overrides.
  - [x] Add `deploy/hetzner/traefik/traefik.yml` or equivalent static Traefik example config with ACME/TLS wiring.

- [x] Validate persistence, routing, and restart behavior (AC: 1, 3, 4, 8, 9, 10)
  - [x] Run `docker compose config` to validate the final Compose model.
  - [x] Run `docker compose up -d` from the repo root and verify all six services become healthy.
  - [x] Verify routing through Traefik reaches the app health endpoints using the chosen local domain strategy.
  - [x] Restart the stack with `docker compose down` then `docker compose up -d` and confirm PostgreSQL and MinIO state persists.
  - [x] Capture any required local hostname/DNS setup in docs rather than relying on tribal knowledge.

## Dev Notes

### Story Intent

This story establishes the first deployable infrastructure skeleton for the whole system. It is not the story that completes IdentityServer, EF Core, or photo workflows; it creates the container topology, persistence model, proxy routing, and deployment examples those later stories depend on.

### Previous Story Intelligence

- Story 1.1 already scaffolded all three ASP.NET app hosts and exposed `/health` on each host. Reuse those endpoints for Compose health checks; do not invent alternate probe routes.
- The backend solution already targets .NET 10 and builds cleanly. Keep any containerization changes consistent with `backend/Directory.Build.props` and the existing solution structure.
- The current app hosts all call `UseHttpsRedirection()`. Behind Traefik TLS termination, that can create redirect loops unless forwarded headers and container HTTP binding are configured deliberately.

### Current Repo Snapshot

- No existing `docker-compose.yml`, Dockerfiles, `.dockerignore`, or `deploy/hetzner/` directory are present in the repository today.
- The working tree is currently clean, so this story should create the first deployment assets rather than adapting existing ones.
- Backend app entry points live at:
  - `backend/src/Blinder.IdentityServer/Program.cs`
  - `backend/src/Blinder.Api/Program.cs`
  - `backend/src/Blinder.AdminPanel/Program.cs`

### Technical Requirements

- Compose topology must include exactly these services in this story: `traefik`, `identityserver`, `api`, `adminpanel`, `postgres`, `minio`.
- PostgreSQL version is `18`. Do not copy older examples that use PostgreSQL 16.
- Use named volumes for:
  - PostgreSQL data
  - MinIO data
  - ASP.NET Core Data Protection keys
  - IdentityServer signing/encryption key material
  - Traefik ACME certificate storage if TLS automation is included in examples
- Keep `identityserver`, `api`, and `adminpanel` stateless. Do not write application state into container filesystems.
- The production deployment path is `deploy/hetzner/`. Root Compose defines the baseline stack; Hetzner-specific files provide production examples and overrides.
- Images should be designed for CI-built deployment. If the root Compose file uses local `build:` for developer smoke tests, the Hetzner override must switch to registry-backed `image:` references.
- Internal container traffic can stay HTTP inside the Docker network; external TLS terminates at Traefik.

### Reverse Proxy Guardrails

- All three ASP.NET app hosts currently redirect HTTP to HTTPS. In a reverse-proxy deployment, the app must respect forwarded headers or conditionally avoid internal redirect loops.
- Do not leave the containers depending on an internal HTTPS certificate setup just to satisfy `UseHttpsRedirection()`.
- Prefer explicit container HTTP endpoints plus reverse-proxy-aware ASP.NET configuration.
- Keep `/health` working without browser-only assumptions so Compose health checks remain reliable.

### File Structure Requirements

Expected new assets for this story:

```text
docker-compose.yml
.dockerignore
deploy/
  hetzner/
    README.md
    .env.example
    docker-compose.override.example.yml
    traefik/
      traefik.yml
backend/
  src/
    Blinder.IdentityServer/
      Dockerfile
      appsettings.Production.json
    Blinder.Api/
      Dockerfile
      appsettings.Production.json
    Blinder.AdminPanel/
      Dockerfile
      appsettings.Production.json
```

Alternative file placement is acceptable only if it preserves clear per-app ownership and keeps production deployment assets under `deploy/hetzner/`.

### Architecture Compliance Guardrails

- Only Traefik may publish ports to the host.
- Host-based routing must separate `auth`, `api`, and `admin` subdomains.
- Do not expose the MinIO console publicly; if object storage routing is added later, expose only the required API endpoint, not the console UI.
- Do not embed secrets in tracked Compose files or `appsettings*.json`; use example placeholders only.
- Persist Data Protection and identity keys now, even if the full IdentityServer key-loading logic is completed in a later story.
- Keep this story focused on deployment scaffolding; do not add Redis, Kubernetes, or non-required infrastructure.

### Skill Guidance and Conflict Resolution

- Use the `blinder-docker-compose-hetzner` skill for stack topology, Traefik patterns, MinIO persistence, and Hetzner operator documentation.
- That skill contains an older example stack that does not fully match current planning artifacts. Planning artifacts override the skill where they conflict.
- Explicit overrides to follow in this story:
  - use PostgreSQL `18`, not `16`
  - include `identityserver` and `adminpanel`, not just `api`
  - keep the MinIO console unexposed
  - preserve `deploy/hetzner/` as the deployment asset root
  
### Git Intelligence Summary

- Recent commits show active work around auth and IdentityServer setup (`prepare 2-0`, `2-0 Identity server`, `fix OIDC Adjustments`). Keep this story infrastructure-only and avoid coupling Compose scaffolding to unfinished auth implementation details.
- Because `1.1` is in `review`, assume the backend project structure is stable enough to containerize, but keep changes minimal and boundary-preserving.

### Testing Requirements

- Validate `docker compose config` with no schema or interpolation errors.
- Validate `docker compose up -d` brings up all services and health checks pass.
- Validate Traefik routes requests to all three app services.
- Validate data persistence by restarting the stack without losing PostgreSQL or MinIO state.
- Validate no secrets were introduced into tracked files.
- Validate app containers remain reachable through Traefik after any reverse-proxy fixes to ASP.NET startup.

### Out of Scope

- Implementing OpenIddict, OAuth/OIDC flows, or live identity key loading logic beyond the filesystem/volume scaffolding needed for this stack.
- Implementing EF Core schema separation or application database migrations.
- Wiring mobile clients, Expo environment variables, or frontend API consumption.
- Adding Redis, CI workflows, or production automation beyond example deployment assets.

### References

- Source: `_bmad-output/planning-artifacts/epics.md` (Epic 1, Story 1.2)
- Source: `_bmad-output/planning-artifacts/architecture.md` (Infrastructure & Deployment; Deployment Structure; Decision Impact Analysis; AI Agent Guidelines)
- Source: `_bmad-output/planning-artifacts/prd.md` (Technical Constraints; Integration Requirements; Security & Privacy NFRs)
- Source: `.github/skills/blinder-docker-compose-hetzner/SKILL.md`
- Source: `.claude/settings.local.json` (`react-native-best-practices@callstack-agent-skills` enabled)

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- Story creation context captured on 2026-04-10.
- Repository currently has no deployment assets; this story is expected to introduce them from scratch.
- All three app hosts currently contain `UseHttpsRedirection()`, which must be handled carefully in the Traefik setup.
- .NET 10 deprecates `KnownNetworks` on `ForwardedHeadersOptions` in favour of `KnownIPNetworks` — updated all three Program.cs files accordingly (TreatWarningsAsErrors enforced by Directory.Build.props).

### Completion Notes List

- Story context created from Epic 1, Story 1.2.
- Deployment guardrails include explicit resolution of the PostgreSQL 18 vs. older example mismatch.
- All three ASP.NET app hosts updated: `UseForwardedHeaders()` registered before middleware pipeline, `UseHttpsRedirection()` scoped to `IsDevelopment()` only to prevent redirect loops behind Traefik.
- AdminPanel keeps HSTS in Production while container-local HTTPS redirection remains limited to Development so `/health` stays probeable over internal HTTP.
- Root Compose now provides a self-contained local baseline with HTTP host routing on `*.blinder.local`; the Hetzner override switches Traefik to ACME-backed TLS for production.
- All three ASP.NET app hosts now persist ASP.NET Core Data Protection keys to `/app/dataprotection-keys`.
- IdentityServer now provisions and reloads persisted signing/encryption certificates from `/app/keys` on startup.
- `appsettings.Production.json` added to each app host with Kestrel HTTP-only binding on port 8080.
- Production-ready multi-stage Dockerfiles created for all three hosts using `mcr.microsoft.com/dotnet/sdk:10.0` (build) and `mcr.microsoft.com/dotnet/aspnet:10.0` (runtime); build context is `./backend/`.
- Runtime images install `curl` so Docker and Compose health checks do not depend on base-image internals.
- Root `.dockerignore` excludes `bin/`, `obj/`, `.git`, test results, `.env`, and `postgres/data`.
- `docker-compose.yml` defines all six services with internal `blinder-net` network; only Traefik publishes ports 80/443. MinIO `traefik.enable=false` keeps console unexposed. PostgreSQL image pinned to `18-alpine`.
- Five named volumes: `postgres-data`, `minio-data`, `traefik-certs`, `dataprotection-keys`, `identityserver-keys`.
- Local Traefik baseline added under `traefik/traefik.yml`; `deploy/hetzner/` now contains only production-specific overrides and ACME configuration.
- `deploy/hetzner/` directory created with: `README.md` (VPS layout, deployment flow, DNS setup, secrets handling), `.env.example` (placeholder values only), `docker-compose.override.example.yml` (registry image overrides), `traefik/traefik.yml` (static Traefik config with ACME/Let's Encrypt).
- `docker compose config` validated with exit 0.
- Full backend test suite: 24 tests, 0 failures across Api, IdentityServer, AdminPanel, Integration, Architecture test projects.

### File List

- _bmad-output/implementation-artifacts/1-2-docker-compose-production-stack-with-traefik-postgresql-and-minio.md
- _bmad-output/implementation-artifacts/sprint-status.yaml
- docker-compose.yml
- .dockerignore
- deploy/hetzner/README.md
- deploy/hetzner/.env.example
- deploy/hetzner/docker-compose.override.example.yml
- deploy/hetzner/traefik/traefik.yml
- traefik/traefik.yml
- .env.example
- backend/src/Blinder.Api/Dockerfile
- backend/src/Blinder.Api/appsettings.Production.json
- backend/src/Blinder.Api/Program.cs
- backend/src/Blinder.IdentityServer/Dockerfile
- backend/src/Blinder.IdentityServer/appsettings.Production.json
- backend/src/Blinder.IdentityServer/Program.cs
- backend/src/Blinder.AdminPanel/Dockerfile
- backend/src/Blinder.AdminPanel/appsettings.Production.json
- backend/src/Blinder.AdminPanel/Program.cs

## Change Log

- 2026-04-10: Story 1.2 created and marked ready-for-dev.
- 2026-04-10: Story 1.2 implemented — Docker Compose stack, Dockerfiles, forwarded-headers fix, Hetzner deployment examples. All tasks complete. Status: review.
