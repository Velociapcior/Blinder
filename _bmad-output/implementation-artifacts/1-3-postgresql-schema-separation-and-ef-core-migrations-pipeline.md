# Story 1.3: PostgreSQL Schema Separation and EF Core Migrations Pipeline

Status: in-progress
Review status: patches applied, end-to-end validation pending (requires Linux containers)

## Story

As a developer,
I want a single PostgreSQL database with enforced `identity.*` and `app.*` schema ownership and separate EF Core migration sets,
so that data boundaries are structurally enforced from the first migration.

## Acceptance Criteria

1. Given the PostgreSQL container is running, when EF Core migrations are applied, then `identity.*` schema exists and is owned exclusively by `Blinder.IdentityServer`.
2. `app.*` schema exists and is owned exclusively by `Blinder.Api`.
3. Separate DB roles with schema-scoped permissions are provisioned so IdentityServer cannot write to `app.*` and Api cannot write to `identity.*`.
4. `Blinder.IdentityServer` has its own `Migrations/` folder that only targets `identity.*`.
5. `Blinder.Api` has its own `Migrations/` folder that only targets `app.*`.
6. `dotnet ef migrations add` and `dotnet ef database update` work independently per project.
7. `Blinder.ArchitectureTests` contains a test that fails if any project references a migration outside its schema boundary.

## Tasks / Subtasks

- [x] Add EF Core dependencies and reproducible CLI tooling (AC: 4, 5, 6)
  - [x] Add `Npgsql.EntityFrameworkCore.PostgreSQL` `10.0.1` to `backend/src/Blinder.IdentityServer/Blinder.IdentityServer.csproj` and `backend/src/Blinder.Api/Blinder.Api.csproj` via `dotnet add package`.
  - [x] Add `Microsoft.EntityFrameworkCore.Design` to both owning projects via `dotnet add package`; keep `Blinder.AdminPanel` free of EF Core packages.
  - [x] If the repo still lacks `backend/.config/dotnet-tools.json`, add a local `dotnet-ef` tool manifest so migration commands are reproducible after `dotnet tool restore`.

- [x] Create per-app persistence structure (AC: 1, 2, 4, 5, 6)
  - [x] Add one `DbContext` under each owning app's `Persistence/` folder: one for `Blinder.IdentityServer`, one for `Blinder.Api`.
  - [x] Configure each context with its owning default schema via `modelBuilder.HasDefaultSchema("identity")` or `modelBuilder.HasDefaultSchema("app")`.
  - [x] Put model configuration into `IEntityTypeConfiguration<>` classes so later stories can extend mappings without bloating the contexts.
  - [x] Configure a schema-specific migrations history table for each context, for example `MigrationsHistoryTable("__EFMigrationsHistory", "identity")` and `MigrationsHistoryTable("__EFMigrationsHistory", "app")`, so the two apps do not collide in one database.
  - [x] Add `IDesignTimeDbContextFactory<TContext>` implementations so `dotnet ef` does not depend on the web host booting successfully.

- [x] Provision schema ownership and per-app database roles (AC: 1, 2, 3)
  - [x] Replace the current single shared application database user in Compose/env examples with distinct runtime roles and passwords for IdentityServer and Api.
  - [x] Add a tracked Postgres bootstrap script, mounted through `/docker-entrypoint-initdb.d/`, that creates `identity` and `app` schemas, assigns ownership, and revokes cross-schema write privileges.
  - [x] Update `docker-compose.yml`, `.env.example`, and `deploy/hetzner/.env.example` so IdentityServer and Api receive different role-scoped connection settings.
  - [x] Keep any bootstrap/admin credential path limited to initialization work; do not reuse it as the runtime connection string for either app.

- [x] Create the first isolated migrations (AC: 1, 2, 4, 5, 6)
  - [x] Add an initial IdentityServer migration under its own `Persistence/Migrations/` folder that ensures only `identity.*` artifacts are created.
  - [x] Add an initial Api migration under its own `Persistence/Migrations/` folder that ensures only `app.*` artifacts are created.
  - [x] Keep the bootstrap migrations minimal and boundary-focused; do not invent future auth or business tables beyond what is required to establish the migration pipeline safely.
  - [x] Inspect generated migration code and snapshots to confirm no opposite-schema DDL or references are emitted.

- [x] Wire runtime registration and the developer migration workflow (AC: 5, 6)
  - [x] Register both contexts and their connection strings in the owning app `Program.cs` files using existing repo conventions.
  - [x] Document the exact per-project commands for `dotnet tool restore`, `dotnet ef migrations add`, and `dotnet ef database update` so future stories can extend the pipeline consistently.
  - [x] Keep migration execution explicit in this story; do not auto-run EF Core migrations on app startup.

- [x] Add automated boundary tests and permission verification (AC: 3, 7)
  - [x] Extend `backend/tests/Blinder.ArchitectureTests/BoundaryTests.cs` so Api fails if it depends on IdentityServer migration types or namespaces, and vice versa.
  - [x] Add a focused database permission smoke test that proves the identity-scoped role cannot write to `app.*` and the app-scoped role cannot write to `identity.*`.
  - [x] Keep all existing architecture and health-check tests passing.

- [ ] Validate the migration pipeline end to end (AC: 1-7)
  - [ ] Validate against a fresh PostgreSQL volume so the bootstrap script is exercised.
  - [ ] Run `dotnet ef database update` independently for IdentityServer and Api against the same PostgreSQL instance.
  - [ ] Confirm both schemas exist, each app records migration history only in its own schema, and rerunning the update commands is safe.
  - [ ] Run `dotnet restore`, `dotnet build`, and `dotnet test` for `backend/Blinder.sln`.

## Dev Notes

### Story Intent

This story establishes the first persistence boundary in the repository. The goal is not a finished identity model or a finished business schema; the goal is a reproducible migrations pipeline, strict schema ownership, and database permissions that make later stories harder to implement incorrectly.

### Previous Story Intelligence

- Story 1.1 already established the separate backend applications and `backend/tests/Blinder.ArchitectureTests/BoundaryTests.cs`. Extend that guardrail instead of creating a parallel boundary-testing pattern.
- Story 1.2 added the PostgreSQL 18 Compose stack and deployment env examples, but the current stack still injects the same shared `POSTGRES_USER` credentials into both app connection strings. Story 1.3 must correct that to satisfy the schema-ownership acceptance criteria.
- The current app hosts already expose `/health` and are wired for Traefik-forwarded headers. Keep the persistence work orthogonal to the reverse-proxy changes from story 1.2.

### Current Repo Snapshot

- `backend/src/Blinder.IdentityServer/Blinder.IdentityServer.csproj` and `backend/src/Blinder.Api/Blinder.Api.csproj` currently reference only `Microsoft.AspNetCore.OpenApi`; there are no EF Core/Npgsql packages yet.
- No `Persistence/` folders, `DbContext` types, or migration sets exist yet in either owning backend app.
- No repo-local `dotnet-ef` tool manifest exists under `backend/.config/`.
- `docker-compose.yml`, `.env.example`, and `deploy/hetzner/.env.example` currently model a single shared app user/password for PostgreSQL. That is insufficient for AC3.
- No `project-context.md` file exists in the workspace.

### Technical Requirements

- Use `.NET 10`, `ASP.NET Core 10`, `EF Core 10`, `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1`, and `PostgreSQL 18` as pinned by the planning artifacts.
- Add packages with `dotnet add package`; do not hand-edit `.csproj` files to introduce new package references.
- Keep one `DbContext` per owning app. `Blinder.AdminPanel` must not gain a context, migrations, or EF packages in this story.
- Configure schema ownership explicitly through `HasDefaultSchema(...)` and provider options, not by relying on PostgreSQL defaults or `public`.
- Configure each context to use its own migrations history table in its own schema. Shared `__EFMigrationsHistory` in one schema is a trap in a dual-context single-database setup.
- Prefer `IDesignTimeDbContextFactory<TContext>` for both contexts so `dotnet ef` remains reliable even as startup logic grows in later stories.
- Use descriptive migration names such as `InitialIdentitySchema` and `InitialAppSchema`.
- Inspect generated SQL before applying it. Migration code that mixes `identity` and `app` objects in one migration is a boundary violation.

### Database Role and Permissions Guidance

- The official PostgreSQL container only processes `/docker-entrypoint-initdb.d/` when the data directory is initialized for the first time. Validate the story against a fresh volume and document that expectation for local testing.
- Create separate login roles for the two runtime apps. The intended shape is:
  - IdentityServer runtime role owns and writes only `identity.*`.
  - Api runtime role owns and writes only `app.*`.
- Lock down `public` so later migrations cannot silently create objects there.
- Grant each role only the privileges it needs on its own schema and explicitly avoid cross-schema write permissions.
- If a bootstrap/admin credential is needed to create roles and schemas, keep it out of runtime app connection strings.

### Architecture Compliance Guardrails

- `Blinder.IdentityServer` writes only `identity.*`.
- `Blinder.Api` writes only `app.*`.
- `Blinder.AdminPanel` remains database-write-free in this story.
- Do not add cross-project references to share entities, `DbContext` types, or migrations between IdentityServer and Api.
- Keep migration folders physically separate inside each owning app.
- Api must not implement direct identity mutations; later identity interactions still go through explicit service/integration boundaries.

### File Structure Requirements

Expected implementation touchpoints:

```text
backend/
  .config/
    dotnet-tools.json                    # if a local dotnet-ef manifest is added
  src/
    Blinder.IdentityServer/
      Blinder.IdentityServer.csproj
      Program.cs
      Persistence/
        IdentityDbContext.cs
        Configurations/
        Migrations/
        DesignTime/
    Blinder.Api/
      Blinder.Api.csproj
      Program.cs
      Persistence/
        AppDbContext.cs
        Configurations/
        Migrations/
        DesignTime/
  tests/
    Blinder.ArchitectureTests/
      BoundaryTests.cs
    Blinder.IntegrationTests/
      ...database permission or migration smoke test...
docker-compose.yml
.env.example
deploy/hetzner/.env.example
postgres/
  init/
    010-schemas-and-roles.sql            # or equivalent tracked bootstrap script
```

Alternative names are acceptable only if ownership and separation remain obvious.

### Testing Requirements

- Run `dotnet tool restore` if a local tool manifest is added.
- Run `dotnet restore backend/Blinder.sln`.
- Run `dotnet build backend/Blinder.sln`.
- Run `dotnet ef migrations add ...` independently for `Blinder.IdentityServer` and `Blinder.Api`.
- Run `dotnet ef database update ...` independently for both apps against the same PostgreSQL instance.
- Run `dotnet test backend/Blinder.sln`.
- Validate Postgres bootstrap behavior from a fresh volume.
- Prove opposite-schema writes fail with permission errors.

### Risks and Anti-Patterns to Avoid

- Do not keep using the single shared `blinder_user` for both runtime applications.
- Do not let EF default objects into `public` or a shared migrations history location.
- Do not add placeholder future product tables solely to make the migration folders look busy.
- Do not hide migration execution inside application startup for this story.
- Do not mix `identity` and `app` DDL in the same migration.
- Do not put secrets into tracked config files.
- Do not add AdminPanel persistence work.

### Git Intelligence Summary

- Recent repository history shows story 1.2 landed the Docker/PostgreSQL baseline (`cdc855a`, `1.2 Dockler + database`). Story 1.3 should refine that baseline rather than replace it.
- `sprint-status.yaml` already marks stories 1.1 and 1.2 as `done`, so their file structure and deployment assets are the starting point for this story.

### Latest Technical Information

- Planning artifacts align on `.NET 10`, `EF Core 10`, `Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1`, and `PostgreSQL 18`.
- In a single physical database with two owning contexts, separate migrations history tables and schema-specific default schemas are required for clean independent `dotnet ef` workflows.

### Out of Scope

- Full OpenIddict persistence and OIDC workflow implementation beyond whatever minimal IdentityServer persistence scaffolding is required to establish isolated migrations.
- Business feature tables for onboarding, matching, messaging, moderation, or notifications beyond minimal app-schema bootstrap.
- AdminPanel data access.
- CI/CD automation for migration bundles or production rollout scripts.

### References

- Source: `_bmad-output/planning-artifacts/epics.md` (Epic 1, Story 1.3)
- Source: `_bmad-output/planning-artifacts/architecture.md` (Core Architectural Decisions; Data Architecture; Architectural Boundaries; File Organization Patterns; Decision Impact Analysis)
- Source: `_bmad-output/planning-artifacts/prd.md` (Technical Constraints; Integration Requirements; Risk Mitigations)
- Source: `_bmad-output/implementation-artifacts/1-2-docker-compose-production-stack-with-traefik-postgresql-and-minio.md`
- Source: `backend/tests/Blinder.ArchitectureTests/BoundaryTests.cs`
- Source: `.github/instructions/aspnet-rest-apis.instructions.md`
- Source: `.github/instructions/csharp.instructions.md`
- Source: `.github/skills/ef-core/SKILL.md`
- Source: `.github/skills/dotnet-best-practices/SKILL.md`
- Source: `.github/skills/nuget-manager/SKILL.md`

## Dev Agent Record

### Agent Model Used

GPT-5.4

### Debug Log References

- Story creation context captured on 2026-04-20.
- `docker-compose.yml` currently supplies the same PostgreSQL credential pattern to both `identityserver` and `api`; AC3 requires that to change.
- `Blinder.IdentityServer` and `Blinder.Api` currently contain no EF Core/Npgsql package references, no `Persistence/` folders, and no migrations.
- No repo-local `dotnet-ef` tool manifest exists under `backend/.config/`.
- No `project-context.md` file was found in the workspace.
- Added repo-local `backend/.config/dotnet-tools.json` and installed `dotnet-ef 10.0.5` for reproducible migration commands.
- Added isolated `IdentityDbContext` and `AppDbContext` implementations, schema marker configurations, and design-time factories in the owning apps.
- Generated `InitialIdentitySchema` and `InitialAppSchema` migrations under separate `Persistence/Migrations/` folders and inspected the emitted migration code for cross-schema leakage.
- Updated Compose and env examples to use `POSTGRES_ADMIN_*`, `IDENTITY_DB_*`, `API_DB_*`, and a tracked `postgres/init/010-schemas-and-roles.sql` bootstrap script.
- Added migration boundary assertions in `Blinder.ArchitectureTests` and a database permission smoke test in `Blinder.IntegrationTests`.
- `dotnet restore`, `dotnet build`, and `dotnet test` succeeded for `backend/Blinder.sln` after aligning EF Core versions in the test projects.
- Live PostgreSQL validation remains blocked in this environment because Docker is currently running Windows containers, so `postgres:18-alpine` cannot start and no local PostgreSQL instance is listening on `localhost:5432`.

### Completion Notes List

- Story context created from Epic 1, Story 1.3.
- Guardrails added for separate migrations history tables, separate runtime database roles, and explicit schema ownership.
- The story intentionally avoids over-specifying future identity or business tables so later stories can evolve the model without undoing premature schema choices.
- Runtime registration now resolves schema-scoped connection strings explicitly in `Blinder.IdentityServer` and `Blinder.Api` without auto-running migrations at startup.
- The migration bootstrap creates only `identity.schema_markers` and `app.schema_markers` plus schema-local `__EFMigrationsHistory` tables, keeping the first migration boundary-focused.
- `backend/MIGRATIONS.md` documents the exact per-project `dotnet tool restore`, `dotnet ef migrations add`, `dotnet ef migrations script`, and `dotnet ef database update` commands.
- The remaining unchecked validation task requires either switching Docker Desktop to Linux containers or providing a reachable PostgreSQL instance for the independent `dotnet ef database update` proof.

### File List

- `backend/.config/dotnet-tools.json`
- `backend/MIGRATIONS.md`
- `backend/src/Blinder.IdentityServer/Program.cs`
- `backend/src/Blinder.IdentityServer/appsettings.Development.json`
- `backend/src/Blinder.IdentityServer/Persistence/IdentityDbContext.cs`
- `backend/src/Blinder.IdentityServer/Persistence/IdentitySchemaMarker.cs`
- `backend/src/Blinder.IdentityServer/Persistence/Configurations/IdentitySchemaMarkerConfiguration.cs`
- `backend/src/Blinder.IdentityServer/Persistence/DesignTime/IdentityDbContextFactory.cs`
- `backend/src/Blinder.IdentityServer/Persistence/Migrations/20260420073123_InitialIdentitySchema.cs`
- `backend/src/Blinder.IdentityServer/Persistence/Migrations/20260420073123_InitialIdentitySchema.Designer.cs`
- `backend/src/Blinder.IdentityServer/Persistence/Migrations/IdentityDbContextModelSnapshot.cs`
- `backend/src/Blinder.Api/Program.cs`
- `backend/src/Blinder.Api/appsettings.Development.json`
- `backend/src/Blinder.Api/Persistence/AppDbContext.cs`
- `backend/src/Blinder.Api/Persistence/AppSchemaMarker.cs`
- `backend/src/Blinder.Api/Persistence/Configurations/AppSchemaMarkerConfiguration.cs`
- `backend/src/Blinder.Api/Persistence/DesignTime/AppDbContextFactory.cs`
- `backend/src/Blinder.Api/Persistence/Migrations/20260420073131_InitialAppSchema.cs`
- `backend/src/Blinder.Api/Persistence/Migrations/20260420073131_InitialAppSchema.Designer.cs`
- `backend/src/Blinder.Api/Persistence/Migrations/AppDbContextModelSnapshot.cs`
- `backend/tests/Blinder.ArchitectureTests/Blinder.ArchitectureTests.csproj`
- `backend/tests/Blinder.ArchitectureTests/BoundaryTests.cs`
- `backend/tests/Blinder.Api.Tests/Blinder.Api.Tests.csproj`
- `backend/tests/Blinder.IdentityServer.Tests/Blinder.IdentityServer.Tests.csproj`
- `backend/tests/Blinder.IntegrationTests/Blinder.IntegrationTests.csproj`
- `backend/tests/Blinder.IntegrationTests/DatabasePermissionSmokeTests.cs`
- `docker-compose.yml`
- `.env.example`
- `deploy/hetzner/.env.example`
- `deploy/hetzner/README.md`
- `postgres/init/010-schemas-and-roles.sql`

## Review Findings

### Decision Needed

- [x] [Review][Decision] EnsureSchema calls in migrations will fail at runtime — RESOLVED: replaced with `migrationBuilder.Sql()` assertion that validates schema exists (created by init script) without requiring `CREATE ON DATABASE`. Applied to `InitialAppSchema.cs` and `InitialIdentitySchema.cs`.

- [x] [Review][Decision] Hardcoded plaintext dev credentials in tracked appsettings.Development.json — ACCEPTED: treated as intentional local dev defaults, consistent with the existing `IdentityKeys:Password` dev default pattern. Not rotated secrets; safe for local development use.

### Patch

- [x] [Review][Patch] Init SQL psql variables not bridged from Docker env vars — FIXED: converted to `010-schemas-and-roles.sh` shell script with `--variable` flags; SQL replaced with placeholder comment [postgres/init/010-schemas-and-roles.sh]
- [x] [Review][Patch] ALTER ROLE executes unconditionally — FIXED: shell script validates all env vars are non-empty before calling psql; aborts with clear error if any are missing [postgres/init/010-schemas-and-roles.sh]
- [x] [Review][Patch] Init script has no transaction wrapping — FIXED: SQL wrapped in BEGIN/COMMIT inside heredoc [postgres/init/010-schemas-and-roles.sh]
- [x] [Review][Patch] `IncludeAssets` typo `organic` — FALSE POSITIVE: file on disk already has `native`; dismissed
- [x] [Review][Patch] DatabasePermissionSmokeTest silently returns (green) when DB is unavailable — FIXED: throws `Xunit.Sdk.SkipException`; added `[Trait("Category", "Integration")]` [backend/tests/Blinder.IntegrationTests/DatabasePermissionSmokeTests.cs]
- [x] [Review][Patch] Missing explicit Npgsql package in IntegrationTests.csproj — FIXED: added `Npgsql 9.0.4` PackageReference [backend/tests/Blinder.IntegrationTests/Blinder.IntegrationTests.csproj]
- [x] [Review][Patch] int.Parse on BLINDER_DB_PORT throws FormatException — FIXED: uses int.TryParse with fallback to DefaultPort [backend/tests/Blinder.IntegrationTests/DatabasePermissionSmokeTests.cs]
- [x] [Review][Patch] AssertWriteDeniedAsync throws confusing error on login failure — FIXED: connection.OpenAsync() wrapped in try-catch that rethrows as InvalidOperationException with diagnostic message [backend/tests/Blinder.IntegrationTests/DatabasePermissionSmokeTests.cs]
- [x] [Review][Patch] ALTER DEFAULT PRIVILEGES not set for admin role — FIXED: added 8 ALTER DEFAULT PRIVILEGES statements for ADMIN_USER in both schemas [postgres/init/010-schemas-and-roles.sh]
- [x] [Review][Patch] YAML `>-` folded scalar inserts space before `Username=` — FIXED: replaced with single-line double-quoted strings [docker-compose.yml]
- [x] [Review][Patch] Smoke test migrates without teardown — FIXED: added inline comment documenting fresh-volume requirement; migrations are idempotent so re-runs succeed if DB was not partially migrated [backend/tests/Blinder.IntegrationTests/DatabasePermissionSmokeTests.cs]
- [x] [Review][Patch] Missing newline at end of .env.example — FIXED [.env.example]

### Defer

- [x] [Review][Defer] BoundaryTests cannot detect wrong-schema DDL argument values inside a valid assembly [backend/tests/Blinder.ArchitectureTests/BoundaryTests.cs] — deferred, pre-existing architectural limitation of IL dependency analysis; NetArchTest cannot inspect runtime argument values
- [x] [Review][Defer] GetRequiredConnectionString called before builder.Build() — future config providers added after CreateBuilder won't be visible [backend/src/Blinder.Api/Program.cs:11] — deferred, theoretical concern; all standard sources (env vars, JSON) are loaded by CreateBuilder
- [x] [Review][Defer] GetRequiredConnectionString duplicated verbatim in both Program.cs files [backend/src/Blinder.Api/Program.cs, backend/src/Blinder.IdentityServer/Program.cs] — deferred, no shared infrastructure library in scope for this story
- [x] [Review][Defer] adminpanel depends_on postgres removed without a code comment explaining intent [docker-compose.yml] — deferred, correct per spec; AdminPanel has no DB access in this story
- [x] [Review][Defer] IntegrationTests project simultaneously references both persistence contexts with no architecture rule guarding it [backend/tests/Blinder.IntegrationTests] — deferred, intentional for smoke test; test-only cross-boundary reference
- [x] [Review][Defer] SetBasePath(Directory.GetCurrentDirectory()) fragile in design-time factories when dotnet ef runs from non-project directory [backend/src/Blinder.Api/Persistence/DesignTime/AppDbContextFactory.cs:20] — deferred, EF tooling generally handles this; MIGRATIONS.md documents the exact commands
- [x] [Review][Defer] Base appsettings.json lacks ConnectionStrings — design-time factory throws if ASPNETCORE_ENVIRONMENT is unset and no env vars are present [backend/src/Blinder.Api/appsettings.json] — deferred, env var fallback covers runtime; design-time workflow is documented
- [x] [Review][Defer] POSTGRES_HOST_PORT bound to 127.0.0.1 may not route correctly on Windows/WSL2 [docker-compose.yml:47] — deferred, correct security default; override with BLINDER_DB_HOST env var if needed

## Change Log

- 2026-04-20: Story 1.3 created and marked ready-for-dev.
- 2026-04-20: Implemented isolated EF Core persistence scaffolding, separate migration sets, schema-role bootstrap, migration workflow docs, and boundary tests; live PostgreSQL validation remains pending because Docker is in Windows-container mode.
- 2026-04-21: Code review completed. 2 decision-needed, 12 patch, 8 deferred, 4 dismissed.