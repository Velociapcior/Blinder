# Story 1.1: Scaffold ASP.NET Core 10 Three-Project Backend Solution

Status: review

## Story

As a developer,
I want a scaffolded backend solution with IdentityServer, Api, and AdminPanel projects,
so that all backend work has a consistent, boundary-enforcing structure from day one.

## Acceptance Criteria

1. Given the backend source directory exists under `backend/src/`, when the solution is initialized, then it contains `Blinder.IdentityServer`, `Blinder.Api`, `Blinder.AdminPanel`, `Blinder.SharedKernel`, and `Blinder.Contracts` projects targeting .NET 10.
2. Each project compiles cleanly with `dotnet build`.
3. `Blinder.SharedKernel` contains no business logic, only foundational primitives: `Result<T>`, `Error`, `BlinderException`, and `CorrelationId`.
4. `Blinder.Contracts` contains only intentionally shared cross-process contracts (empty at this stage — structure only).
5. Test projects exist under `backend/tests/` for each app plus `Blinder.IntegrationTests` and `Blinder.ArchitectureTests`.
6. All app projects have health check endpoints registered and returning HTTP 200 on `/health`.
7. `appsettings.json` and `appsettings.Development.json` are present per app and tracked config contains no secrets.
8. A `global.json` at `backend/` pins the SDK to `10.0.103`.
9. A `Directory.Build.props` at `backend/` sets `<TargetFramework>net10.0</TargetFramework>`, `<Nullable>enable</Nullable>`, and `<ImplicitUsings>enable</ImplicitUsings>` for all projects.
10. `Blinder.ArchitectureTests` contains passing boundary tests using `NetArchTest.Fluent`.

## Tasks / Subtasks

- [x] Clean up existing empty directory (pre-condition)
  - [x] Remove the empty `backend/Blinder.Api/` directory that currently exists before creating the proper structure.

- [x] Initialize backend root and solution skeleton (AC: 1, 2, 8, 9)
  - [x] Create `backend/src/` and `backend/tests/` folders.
  - [x] Create `backend/global.json` pinning SDK version `10.0.103`.
  - [x] Create `backend/Directory.Build.props` with shared TFM, nullable, and implicit usings settings.
  - [x] Create solution `backend/Blinder.sln`.
  - [x] Scaffold projects using the correct `dotnet new` templates:
    - `backend/src/Blinder.IdentityServer/` → `dotnet new webapi --use-controllers false` (minimal API; OpenIddict wired in story 2.1)
    - `backend/src/Blinder.Api/` → `dotnet new webapi --use-controllers false`
    - `backend/src/Blinder.AdminPanel/` → `dotnet new webapp` (Razor Pages; authenticates via OIDC in story 2.2)
    - `backend/src/Blinder.SharedKernel/` → `dotnet new classlib`
    - `backend/src/Blinder.Contracts/` → `dotnet new classlib`
  - [x] Add all projects to the solution and verify `dotnet restore` and `dotnet build` pass.

- [x] Establish project references and boundaries (AC: 1, 3, 4)
  - [x] Wire the explicit reference graph (see Dev Notes → Project Reference Graph below).
  - [x] Scaffold the four primitives in `Blinder.SharedKernel`: `Result<T>`, `Error`, `BlinderException`, `CorrelationId`.
  - [x] Leave `Blinder.Contracts` structurally present but with no types yet.
  - [x] Add a `README.md` in `Blinder.SharedKernel` and `Blinder.Contracts` documenting allowed content.
  - [x] Verify no circular references exist via `dotnet build`.

- [x] Add app startup baselines for three app hosts (AC: 2, 6, 7)
  - [x] Add `Microsoft.Extensions.Diagnostics.HealthChecks` and register `/health` endpoint in each app's `Program.cs`.
  - [x] Ensure each app has `appsettings.json` and `appsettings.Development.json`.
  - [x] Keep placeholder config values only; keep secrets in environment variables/user secrets (`dotnet user-secrets`).

- [x] Create test project structure (AC: 5)
  - [x] Scaffold unit test projects using `dotnet new xunit`:
    - `backend/tests/Blinder.IdentityServer.Tests/`
    - `backend/tests/Blinder.Api.Tests/`
    - `backend/tests/Blinder.AdminPanel.Tests/`
  - [x] Scaffold `backend/tests/Blinder.IntegrationTests/` using `dotnet new xunit`; add `Microsoft.AspNetCore.Mvc.Testing` package.
  - [x] Scaffold `backend/tests/Blinder.ArchitectureTests/` using `dotnet new xunit`; add `NetArchTest.Fluent` package.
  - [x] Add all test projects to the solution and verify restore/build.

- [x] Add architecture guardrail tests (AC: 3, 4, 10)
  - [x] In `Blinder.ArchitectureTests`, add a project reference to each `src/` project so NetArchTest can resolve assemblies.
  - [x] Write a test that fails if `Blinder.SharedKernel` references any project outside itself.
  - [x] Write a test that fails if any `src/` project references `Blinder.Contracts` for internal DTOs (i.e., internal types must not live in Contracts).
  - [x] Write a test that fails if `Blinder.AdminPanel` has a direct project reference to `Blinder.IdentityServer` or `Blinder.Api`.

- [x] Validate completion checklist (AC: 1-10)
  - [x] `dotnet build backend/Blinder.sln` passes with zero warnings (treat warnings as errors).
  - [x] `/health` returns 200 for each app host when run with `dotnet run`.
  - [x] Required config files exist per app.
  - [x] No secrets committed in any `appsettings*.json`.
  - [x] All expected test projects are present and `dotnet test` passes.
  - [x] `backend/global.json` contains `"version": "10.0.103"`.
  - [x] `backend/Directory.Build.props` is picked up by all projects (confirm via `dotnet build -v:n`).

## Dev Notes

### Story Intent

This story creates the backend foundation only. Do not implement business features, identity flows, or database access yet. The output must be a clean, enforceable starting architecture that prevents cross-boundary drift in all subsequent stories.

### Technical Requirements

- Runtime/framework: .NET 10 / ASP.NET Core 10 (SDK `10.0.103` confirmed installed on dev machine).
- Project templates:
  - `Blinder.IdentityServer` → `webapi` (minimal API; no OpenIddict yet — that is story 2.1).
  - `Blinder.Api` → `webapi` (minimal API).
  - `Blinder.AdminPanel` → `webapp` (Razor Pages; no OIDC wiring yet — that is story 2.2).
  - `Blinder.SharedKernel` → `classlib`.
  - `Blinder.Contracts` → `classlib`.
  - All test projects → `xunit`.
- Health checks: use the built-in `Microsoft.Extensions.Diagnostics.HealthChecks`; map `/health` with `MapHealthChecks`.
- Architecture test library: `NetArchTest.Fluent` (latest stable compatible with .NET 10).
- Integration tests: `Microsoft.AspNetCore.Mvc.Testing`; no live services wired yet.
- Warnings as errors: add `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` to `Directory.Build.props`.

### Project Reference Graph

Only the following references are permitted at this stage:

```
Blinder.IdentityServer → Blinder.SharedKernel, Blinder.Contracts
Blinder.Api            → Blinder.SharedKernel, Blinder.Contracts
Blinder.AdminPanel     → Blinder.SharedKernel, Blinder.Contracts

Blinder.IdentityServer.Tests → Blinder.IdentityServer
Blinder.Api.Tests            → Blinder.Api
Blinder.AdminPanel.Tests     → Blinder.AdminPanel
Blinder.IntegrationTests     → Blinder.IdentityServer, Blinder.Api, Blinder.AdminPanel
Blinder.ArchitectureTests    → Blinder.IdentityServer, Blinder.Api, Blinder.AdminPanel,
                               Blinder.SharedKernel, Blinder.Contracts
```

No other references are permitted. AdminPanel must not reference IdentityServer or Api directly.

### SharedKernel Primitives to Scaffold

Minimal stubs only — no implementation depth needed in this story:

```csharp
// Result<T> — discriminated union for operation outcomes
public sealed class Result<T> { ... }

// Error — structured error descriptor (code + message)
public sealed class Error { ... }

// BlinderException — base exception type for domain errors
public abstract class BlinderException : Exception { ... }

// CorrelationId — strongly-typed wrapper for request correlation
public readonly struct CorrelationId { ... }
```

### Directory Structure

```
backend/
├── global.json
├── Directory.Build.props
├── Blinder.sln
├── src/
│   ├── Blinder.IdentityServer/
│   ├── Blinder.Api/
│   ├── Blinder.AdminPanel/
│   ├── Blinder.SharedKernel/
│   └── Blinder.Contracts/
└── tests/
    ├── Blinder.IdentityServer.Tests/
    ├── Blinder.Api.Tests/
    ├── Blinder.AdminPanel.Tests/
    ├── Blinder.IntegrationTests/
    └── Blinder.ArchitectureTests/
```

### Architecture Compliance Guardrails

- `Blinder.SharedKernel` is foundational primitives only; no domain features, no feature logic, no EF Core references.
- `Blinder.Contracts` is only for intentionally shared cross-process contracts; leave empty in this story.
- App identity boundaries:
  - Identity concerns go in `Blinder.IdentityServer`.
  - Business concerns go in `Blinder.Api`.
  - Staff UI workflows go in `Blinder.AdminPanel`.
- AdminPanel calls Api over HTTP, never via project reference in production. The project reference in `Blinder.ArchitectureTests` is for boundary assertion only.
- No secrets in tracked config files; use `dotnet user-secrets` for local dev overrides.

### Testing Requirements

- Build validation: `dotnet build backend/Blinder.sln` must pass with zero errors and zero warnings.
- Health endpoint validation: each app responds 200 on `/health` with `dotnet run`.
- Boundary validation: `Blinder.ArchitectureTests` enforces SharedKernel/Contracts intent via `NetArchTest.Fluent`.
- Config hygiene: `appsettings*.json` files present in each app with no secret values.

### Risks and Anti-Patterns to Avoid

- Do not place business/domain logic into `Blinder.SharedKernel`.
- Do not place app-internal DTOs/entities into `Blinder.Contracts`.
- Do not skip `Blinder.ArchitectureTests`; boundary enforcement is required early.
- Do not commit secrets in any `appsettings*.json`.
- Do not add Identity or OIDC endpoints into `Blinder.Api` — all auth is owned by IdentityServer.
- Do not add OpenIddict, EF Core, or database packages in this story — those belong to stories 2.1 and 1.3.
- Do not use `--use-controllers` (`webapi` with controllers) — use minimal API style for Api and IdentityServer.

### References

- Source: `_bmad-output/planning-artifacts/epics.md` (Epic 1, Story 1.1 acceptance criteria)
- Source: `_bmad-output/planning-artifacts/architecture.md` (Core Architectural Decisions; Architectural Boundaries; File Organization Patterns; Infrastructure & Deployment; Implementation Patterns & Consistency Rules)
- Source: `_bmad-output/planning-artifacts/prd.md` (Technical success criteria and server-authoritative constraints)

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6 (2026-04-08)

### Debug Log References

- `dotnet new sln` in .NET 10 defaults to `.slnx` format; used `--format sln` flag to create the traditional `.sln` as required by the AC (`dotnet build backend/Blinder.sln`).
- `NetArchTest.Fluent` is not a valid NuGet package name — the correct package is `NetArchTest.Rules` v1.3.2, which provides the `NetArchTest.Fluent` API namespace referenced in the story.
- IdentityServer/Api HTTP-to-HTTPS redirect warning on `--launch-profile http` is expected behavior (no HTTPS port configured for HTTP-only profile); `/health` returns 200 regardless.
- `Blinder.Contracts` is intentionally empty; assembly loading via `Assembly.Load(new AssemblyName("Blinder.Contracts"))` used in ArchitectureTests since no anchor type exists.

### Completion Notes List

- All 10 acceptance criteria satisfied.
- `dotnet build Blinder.sln` passes with 0 warnings, 0 errors (TreatWarningsAsErrors=true enforced).
- `/health` returns HTTP 200 on all three app hosts (IdentityServer:5041, Api:5164, AdminPanel:5046).
- `dotnet test` passes: 13 SharedKernel primitive unit tests in Blinder.Api.Tests, 3 architecture boundary tests in Blinder.ArchitectureTests.
- Architecture guardrail tests cover: SharedKernel isolation, Contracts isolation from app projects, AdminPanel not depending on IdentityServer/Api.
- No secrets in any tracked appsettings files; all app configs contain only logging/AllowedHosts defaults.
- `backend/global.json` pins SDK `10.0.103` with `rollForward: disable`.
- `Directory.Build.props` sets net10.0, Nullable=enable, ImplicitUsings=enable, TreatWarningsAsErrors=true for all projects.

### File List

- backend/global.json
- backend/Directory.Build.props
- backend/Blinder.sln
- backend/src/Blinder.IdentityServer/Blinder.IdentityServer.csproj
- backend/src/Blinder.IdentityServer/Program.cs
- backend/src/Blinder.IdentityServer/appsettings.json
- backend/src/Blinder.IdentityServer/appsettings.Development.json
- backend/src/Blinder.IdentityServer/Properties/launchSettings.json
- backend/src/Blinder.Api/Blinder.Api.csproj
- backend/src/Blinder.Api/Program.cs
- backend/src/Blinder.Api/appsettings.json
- backend/src/Blinder.Api/appsettings.Development.json
- backend/src/Blinder.Api/Properties/launchSettings.json
- backend/src/Blinder.AdminPanel/Blinder.AdminPanel.csproj
- backend/src/Blinder.AdminPanel/Program.cs
- backend/src/Blinder.AdminPanel/appsettings.json
- backend/src/Blinder.AdminPanel/appsettings.Development.json
- backend/src/Blinder.AdminPanel/Properties/launchSettings.json
- backend/src/Blinder.AdminPanel/Pages/Index.cshtml
- backend/src/Blinder.AdminPanel/Pages/Index.cshtml.cs
- backend/src/Blinder.AdminPanel/Pages/Privacy.cshtml
- backend/src/Blinder.AdminPanel/Pages/Privacy.cshtml.cs
- backend/src/Blinder.AdminPanel/Pages/Error.cshtml
- backend/src/Blinder.AdminPanel/Pages/Error.cshtml.cs
- backend/src/Blinder.SharedKernel/Blinder.SharedKernel.csproj
- backend/src/Blinder.SharedKernel/Result.cs
- backend/src/Blinder.SharedKernel/Error.cs
- backend/src/Blinder.SharedKernel/BlinderException.cs
- backend/src/Blinder.SharedKernel/CorrelationId.cs
- backend/src/Blinder.SharedKernel/README.md
- backend/src/Blinder.Contracts/Blinder.Contracts.csproj
- backend/src/Blinder.Contracts/README.md
- backend/tests/Blinder.IdentityServer.Tests/Blinder.IdentityServer.Tests.csproj
- backend/tests/Blinder.Api.Tests/Blinder.Api.Tests.csproj
- backend/tests/Blinder.Api.Tests/SharedKernelPrimitivesTests.cs
- backend/tests/Blinder.AdminPanel.Tests/Blinder.AdminPanel.Tests.csproj
- backend/tests/Blinder.IntegrationTests/Blinder.IntegrationTests.csproj
- backend/tests/Blinder.ArchitectureTests/Blinder.ArchitectureTests.csproj
- backend/tests/Blinder.ArchitectureTests/BoundaryTests.cs
- _bmad-output/implementation-artifacts/1-1-scaffold-asp-net-core-10-three-project-backend-solution.md

## Change Log

- 2026-04-08: Story 1.1 implemented — scaffolded ASP.NET Core 10 backend solution with 5 src projects, 5 test projects, SharedKernel primitives, health check endpoints on all app hosts, architecture guardrail tests, and full build/test validation.
