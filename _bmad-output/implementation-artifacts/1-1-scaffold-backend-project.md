# Story 1.1: Scaffold Backend Project

Status: done

## Story

As a developer,
I want a fully scaffolded ASP.NET Core .NET 10 backend project committed to source control,
so that every subsequent story has a working, convention-compliant backend to build on.

## Acceptance Criteria

1. **Given** the repository is cloned fresh **When** the developer runs `dotnet build` inside `backend/` **Then** the solution builds with zero warnings or errors.

2. **Given** the backend project is scaffolded **When** the project structure is inspected **Then** `backend/Blinder.sln`, `backend/Blinder.Api/`, and `backend/Blinder.Tests/` exist with the following packages added to `Blinder.Api`: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`, `EFCore.NamingConventions`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Riok.Mapperly`, `FluentValidation.AspNetCore`, `Serilog.AspNetCore`, `Serilog.Sinks.File`, `FirebaseAdmin`, `dotAPNS`, `AWSSDK.S3`, `Coravel`, `MailKit`. Note: `Microsoft.AspNetCore.SignalR` and `Microsoft.AspNetCore.RazorPages` are part of the `Microsoft.AspNetCore.App` shared framework in .NET 10 and do not require an explicit NuGet package reference.

3. **Given** the project is scaffolded **When** `Program.cs` is reviewed **Then** `AddProblemDetails()` and `UseExceptionHandler()` are configured, Serilog replaces all default Microsoft logging providers (no `builder.Logging.AddConsole()`, no `Console.WriteLine` — Serilog's own console sink via `WriteTo.Console()` is permitted for local dev), and `AddValidatorsFromAssemblyContaining<Program>()` is registered.

4. **Given** `Blinder.Api/` is created **When** the directory structure is verified **Then** the following directories exist: `Controllers/`, `Hubs/`, `Pages/Admin/`, `Models/`, `DTOs/`, `Services/`, `Repositories/`, `Mappings/`, `Validators/`, `BackgroundJobs/`, `Errors/`, `Migrations/`, `Infrastructure/Data/`, `Infrastructure/Auth/`, `Infrastructure/Storage/`, `Infrastructure/Scanning/`, `Infrastructure/Email/`, `Infrastructure/Middleware/`.

5. **Given** the project is scaffolded **When** `Errors/AppErrors.cs` is reviewed **Then** it exists as the single source of truth for RFC 7807 problem type URIs — no error strings defined inline in any controller.

6. **Given** `ApplicationUser` is in `Models/ApplicationUser.cs` **When** the class definition is reviewed **Then** it inherits from `IdentityUser<Guid>`, includes `Gender` (enum `UserGender`: `Unspecified=0`, `Male=1`, `Female=2`, `NonBinary=3` — `Unspecified` is the sentinel default to distinguish unset gender from a valid choice, required by the female-invite enforcement rule in Story 2.5), `QuizCompletedAt` (`DateTimeOffset?`), `InviteLinkId` (Guid?, nullable FK placeholder), `IsOnboardingComplete` (bool, default false) — uses `DateTimeOffset` for all date fields, never `DateTime`.

7. **Given** `Blinder.Tests/` is created **When** the project is reviewed **Then** it references `Blinder.Api`, has `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `Moq`, `FluentAssertions` packages and mirrors the `Blinder.Api` folder structure (`Controllers/`, `Services/`, `Validators/`, `Integration/`).

8. **Given** `<Nullable>enable</Nullable>` is set **When** all `.csproj` files are reviewed **Then** nullable reference types are enabled in both `Blinder.Api.csproj` and `Blinder.Tests.csproj`.

## Tasks / Subtasks

- [x] Task 1: Create solution and project scaffolding (AC: 1, 2)
  - [x] Run `dotnet new sln -n Blinder` inside `backend/`
  - [x] Run `dotnet new webapi -n Blinder.Api --use-controllers --framework net10.0`
  - [x] Run `dotnet new xunit -n Blinder.Tests --framework net10.0`
  - [x] Run `dotnet sln add Blinder.Api/Blinder.Api.csproj`
  - [x] Run `dotnet sln add Blinder.Tests/Blinder.Tests.csproj`
  - [x] Add project reference: `dotnet add Blinder.Tests/Blinder.Tests.csproj reference Blinder.Api/Blinder.Api.csproj`
  - [x] Enable nullable reference types: add `<Nullable>enable</Nullable>` to both `.csproj` files

- [x] Task 2: Add all required NuGet packages to Blinder.Api (AC: 2)
  - [x] `dotnet add Blinder.Api package Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  - [x] `dotnet add Blinder.Api package Microsoft.AspNetCore.Authentication.JwtBearer`
  - [x] `dotnet add Blinder.Api package Npgsql.EntityFrameworkCore.PostgreSQL`
  - [x] `dotnet add Blinder.Api package Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite`
  - [x] `dotnet add Blinder.Api package EFCore.NamingConventions` (for `UseSnakeCaseNamingConvention`)
  - [x] `dotnet add Blinder.Api package Riok.Mapperly`
  - [x] `dotnet add Blinder.Api package FluentValidation.AspNetCore`
  - [x] `dotnet add Blinder.Api package Serilog.AspNetCore`
  - [x] `dotnet add Blinder.Api package Serilog.Sinks.File`
  - [x] `dotnet add Blinder.Api package FirebaseAdmin`
  - [x] `dotnet add Blinder.Api package dotAPNS`
  - [x] `dotnet add Blinder.Api package AWSSDK.S3`
  - [x] `dotnet add Blinder.Api package Coravel`
  - [x] `dotnet add Blinder.Api package MailKit`

- [x] Task 3: Add test packages to Blinder.Tests (AC: 7)
  - [x] `dotnet add Blinder.Tests package Moq`
  - [x] `dotnet add Blinder.Tests package FluentAssertions`
  - [x] `dotnet add Blinder.Tests package Microsoft.AspNetCore.Mvc.Testing`

- [x] Task 4: Create full `Blinder.Api/` directory structure (AC: 4)
  - [x] Create `Controllers/` (remove default WeatherForecastController.cs)
  - [x] Create `Hubs/`
  - [x] Create `Pages/Admin/`
  - [x] Create `Models/`
  - [x] Create `DTOs/`
  - [x] Create `Services/`
  - [x] Create `Repositories/`
  - [x] Create `Mappings/`
  - [x] Create `Validators/`
  - [x] Create `BackgroundJobs/`
  - [x] Create `Errors/`
  - [x] Create `Migrations/`
  - [x] Create `Infrastructure/Data/`
  - [x] Create `Infrastructure/Auth/`
  - [x] Create `Infrastructure/Storage/`
  - [x] Create `Infrastructure/Scanning/`
  - [x] Create `Infrastructure/Email/`
  - [x] Create `Infrastructure/Middleware/`

- [x] Task 5: Create mirror structure in Blinder.Tests (AC: 7)
  - [x] Create `Controllers/`, `Services/`, `Validators/`, `Integration/`

- [x] Task 6: Implement `Models/ApplicationUser.cs` (AC: 6)
  - [x] Inherit from `IdentityUser`
  - [x] Add `Gender` (enum `UserGender`: Male/Female/NonBinary), `QuizCompletedAt` (`DateTimeOffset?`), `InviteLinkId` (`Guid?`), `IsOnboardingComplete` (`bool`)
  - [x] Add `UserGender` enum to `Models/` (same file or separate `Enums/` — keep it in `Models/` for this story)
  - [x] Confirm all date properties use `DateTimeOffset`, not `DateTime`

- [x] Task 7: Implement `Infrastructure/Data/AppDbContext.cs` (AC: 1, 6)
  - [x] Inherit from `IdentityDbContext<ApplicationUser>`
  - [x] Override `OnModelCreating`: call `base.OnModelCreating(builder)`, `builder.UseSnakeCaseNamingConvention()`, `builder.UseNetTopologySuite()`
  - [x] Register in DI as `AddDbContextPool<AppDbContext>` in Program.cs

- [x] Task 8: Implement `Errors/AppErrors.cs` (AC: 5)
  - [x] Create static class with placeholder problem type URI constants
  - [x] Include: `UserNotFound`, `DuplicateEmail`, `InvalidInviteToken`, `RevealThresholdNotMet`, `ConversationLimitReached`, `Unauthorized`, `Forbidden` — all as `public const string`
  - [x] Base URI pattern: `"https://blinder.app/errors/{slug}"`

- [x] Task 9: Configure `Program.cs` (AC: 3)
  - [x] Replace default logging with Serilog via `UseSerilog()` on the host builder
  - [x] Configure Serilog with rolling file sink (`Serilog.Sinks.File`) and console sink for development
  - [x] Call `AddProblemDetails()` on the service collection
  - [x] Call `UseExceptionHandler()` in the middleware pipeline
  - [x] Register FluentValidation: `AddValidatorsFromAssemblyContaining<Program>()`
  - [x] Register Coravel: `services.AddScheduler()` and `services.AddQueue()`
  - [x] Register `AddRazorPages()` and map them (`MapRazorPages()`)
  - [x] Set `[AllowAnonymous]` / `[Authorize]` placeholders consistent with future stories
  - [x] Remove all default WeatherForecast-related code

- [x] Task 10: Verify build and validate all ACs (AC: 1)
  - [x] Run `dotnet build backend/` — confirm zero warnings or errors
  - [x] Run `dotnet test backend/` — confirm test project compiles (no actual tests yet, just compiles)

## Dev Notes

### Project Initialization Commands (Exact Sequence)

```bash
# From repo root
mkdir backend
cd backend

dotnet new sln -n Blinder
dotnet new webapi -n Blinder.Api --use-controllers --framework net10.0
dotnet new xunit -n Blinder.Tests --framework net10.0
dotnet sln add Blinder.Api/Blinder.Api.csproj
dotnet sln add Blinder.Tests/Blinder.Tests.csproj
dotnet add Blinder.Tests/Blinder.Tests.csproj reference Blinder.Api/Blinder.Api.csproj
```

**Always add packages via `dotnet add` CLI — never edit `.csproj` files by hand.**
See skill: [nuget-manager SKILL.md](../../.github/skills/nuget-manager/SKILL.md)

### Package Notes

| Package | Why |
|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` | **Separate package** from the base Npgsql EF package — provides PostGIS spatial type support via NetTopologySuite. Required for `UseNetTopologySuite()`. Don't skip this — PostGIS is mandatory from day one (ARCH-5). |
| `EFCore.NamingConventions` | Provides `UseSnakeCaseNamingConvention()`. This is a **separate NuGet package** (`EFCore.NamingConventions`), NOT part of `Npgsql.EntityFrameworkCore.PostgreSQL`. Forgetting this causes PascalCase column names in migrations — violates all schema conventions. |
| `Microsoft.AspNetCore.SignalR` | Core package; available as part of the ASP.NET Core shared framework in .NET 10 but should be explicitly referenced. |
| `FirebaseAdmin` | Official Google SDK for FCM push (Android). Stories 5.4+ use it. Wire up in this story just as a package reference; no configuration yet. |
| `dotAPNS` | APNs client for .NET (iOS push). Same — package reference only in this story. |
| `Coravel` | Registers `ScheduledJob` and `Queue` services in DI. Must call `services.AddScheduler()` and `services.AddQueue()` in `Program.cs`. |
| `Riok.Mapperly` | Source generator — generates mapping code at compile time. All entity↔DTO conversions must go through `[Mapper]` partial classes in `Mappings/`. No manual `new DTO { ... }`. |

### Program.cs — Critical Configuration Pattern

```csharp
// CORRECT Program.cs skeleton for this story

using Serilog;
using FluentValidation;
using Blinder.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Blinder.Api.Models;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .WriteTo.Console()
    .WriteTo.File("logs/blinder-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddProblemDetails();
builder.Services.AddSignalR();

// FluentValidation — MUST register from assembly, never manually
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// EF Core + Identity
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseNetTopologySuite()));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Coravel
builder.Services.AddScheduler();
builder.Services.AddQueue();

var app = builder.Build();

app.UseExceptionHandler();  // Must come before routing
app.UseStatusCodePages();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
// SignalR hub mapping will come in Story 5.1:
// app.MapHub<ChatHub>("/hubs/chat");

app.Run();
```

**DO NOT** add `app.UseSwagger()` / `app.UseSwaggerUI()` boilerplate — .NET 10 uses the new `Microsoft.AspNetCore.OpenApi` package which is configured differently. Skip Swagger for this story.

### ApplicationUser — Required Shape

```csharp
// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace Blinder.Api.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public UserGender Gender { get; set; }
    public DateTimeOffset? QuizCompletedAt { get; set; }
    public Guid? InviteLinkId { get; set; }
    public bool IsOnboardingComplete { get; set; } = false;
}

public enum UserGender
{
    Male,
    Female,
    NonBinary
}
```

**Key points:**
- Inherit from `IdentityUser<Guid>` (typed key) — consistent with `IdentityRole<Guid>` and `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- `DateTimeOffset?` for `QuizCompletedAt` — never `DateTime`
- `InviteLinkId` is a nullable `Guid?` here — the `InviteLink` entity and FK constraint come in Story 2.5
- Never add columns directly to `IdentityUser` — always via the subclass (ARCH-6)

### AppDbContext — Required Shape

```csharp
// Infrastructure/Data/AppDbContext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Blinder.Api.Models;

namespace Blinder.Api.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);           // Identity schema first
        builder.UseSnakeCaseNamingConvention();  // All tables/columns snake_case
        builder.HasPostgresExtension("postgis"); // PostGIS extension (ARCH-5)
        // UseNetTopologySuite() is configured on the Npgsql options builder, not here
    }
}
```

**Important:** `UseNetTopologySuite()` is called on the **Npgsql options builder** (inside `UseNpgsql(conn, o => o.UseNetTopologySuite())`), not on the `ModelBuilder`. `HasPostgresExtension("postgis")` in `OnModelCreating` causes the migration to emit `CREATE EXTENSION IF NOT EXISTS postgis` — required by ARCH-5.

### AppErrors.cs — Required Shape

```csharp
// Errors/AppErrors.cs
namespace Blinder.Api.Errors;

/// <summary>
/// Single source of truth for all RFC 7807 Problem Details type URIs.
/// Every 4xx/5xx response in the application must reference a constant from this class.
/// Never define error type strings inline in controllers.
/// </summary>
public static class AppErrors
{
    private const string Base = "https://blinder.app/errors";

    public const string UserNotFound            = $"{Base}/user-not-found";
    public const string DuplicateEmail          = $"{Base}/duplicate-email";
    public const string InvalidInviteToken      = $"{Base}/invalid-invite-token";
    public const string RevealThresholdNotMet   = $"{Base}/reveal-threshold-not-met";
    public const string ConversationLimitReached = $"{Base}/conversation-limit-reached";
    public const string InsufficientPermissions = $"{Base}/insufficient-permissions";
    public const string PhotoScanFailed         = $"{Base}/photo-scan-failed";
    public const string InviteRequiredForFemale = $"{Base}/invite-required-for-female";
    public const string WebhookVerificationFailed = $"{Base}/webhook-verification-failed";
}
```

### Directory Structure to Create

All directories below must exist (create `.gitkeep` files where needed so they're committed):

```
backend/
├── Blinder.sln
├── Blinder.Api/
│   ├── Controllers/         ← .gitkeep
│   ├── Hubs/                ← .gitkeep
│   ├── Pages/
│   │   └── Admin/           ← .gitkeep
│   ├── Models/              ← ApplicationUser.cs, UserGender enum
│   ├── DTOs/                ← .gitkeep
│   ├── Services/            ← .gitkeep
│   ├── Repositories/        ← .gitkeep
│   ├── Mappings/            ← .gitkeep
│   ├── Validators/          ← .gitkeep
│   ├── BackgroundJobs/      ← .gitkeep
│   ├── Errors/              ← AppErrors.cs
│   ├── Migrations/          ← .gitkeep (EF migrations added in Story 1.4)
│   └── Infrastructure/
│       ├── Data/            ← AppDbContext.cs
│       ├── Auth/            ← .gitkeep (SocialLoginHandler comes in Story 2.3)
│       ├── Storage/         ← .gitkeep (S3ClientFactory comes in Story 3.2)
│       ├── Scanning/        ← .gitkeep (ContentScanningClient comes in Story 3.3)
│       ├── Email/           ← .gitkeep (SmtpSettings comes in Story 5.4)
│       └── Middleware/      ← .gitkeep
├── Blinder.Tests/
│   ├── Controllers/         ← .gitkeep
│   ├── Services/            ← .gitkeep
│   ├── Validators/          ← .gitkeep
│   └── Integration/         ← .gitkeep
└── Dockerfile               ← Multi-stage (comes in Story 1.2)
```

### Clean Up Default Scaffolding

The `dotnet new webapi` template generates several files that must be **deleted** before commit:
- `Controllers/WeatherForecastController.cs`
- `WeatherForecast.cs`
- Remove the `MapGet("/weatherforecast", ...)` minimal API route if present in `Program.cs`

### Non-Negotiable Rules from project-context.md

Rules most relevant to this story:
1. `UseSnakeCaseNamingConvention()` — **must** be in `OnModelCreating`, Story 1.4 migrations depend on it
2. RFC 7807 Problem Details — `AddProblemDetails()` + `UseExceptionHandler()` from this very first story
3. `DateTimeOffset` in C# / `timestamptz` in PG — set the pattern in `ApplicationUser.cs`
4. No PII in structured log properties — Serilog configuration must never include automatic request body logging
5. `<Nullable>enable</Nullable>` in both `.csproj` files — retrofitting is expensive

### Serilog appsettings.json Section

Add this to `appsettings.json` so the file sink config is externalized (not hardcoded):

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.File", "Serilog.Sinks.Console"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/blinder-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 14
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Packages NOT in this story

Do NOT configure the following in `Program.cs` in this story — they come in later stories:
- JWT Bearer auth configuration (Story 2.1)
- EF Core connection string / migration pipeline (Story 1.4)
- SignalR `/hubs/chat` mapping (Story 5.1)
- S3 client factory setup (Story 3.2)
- Firebase / APNs credential configuration (Story 5.4)
- AppSettings seeding (Story 4.1)

**Register them as packages only. No wiring up.**

### Project Structure Notes

- All infrastructure clients (`ContentScanningClient`, `S3ClientFactory`, `SocialLoginHandler`, etc.) go in `Infrastructure/` subdirectories — **never in `Services/`**. `Services/` is for domain orchestration only.
- `BackgroundJobs/` is exclusively for Coravel `IInvocable` classes. No other logic.
- `Repositories/` — only add repository classes here if EF's `DbContext` direct access is insufficient. For this MVP, direct `AppDbContext` injection in services is acceptable.
- The `.gitkeep` files in empty directories ensure the structure is committed to source control and visible to future story authors.

### References

- [Source: docs/tech-preferences.md#1-repository--project-structure] — Full canonical directory structure
- [Source: docs/tech-preferences.md#2-technology-stack-version-pinned] — Version-pinned package table
- [Source: _bmad-output/planning-artifacts/architecture.md#Selected-Starters] — Exact scaffold commands
- [Source: _bmad-output/planning-artifacts/architecture.md#Validation] — FluentValidation decision
- [Source: _bmad-output/planning-artifacts/architecture.md#Logging] — Serilog decision
- [Source: _bmad-output/planning-artifacts/architecture.md#Object-Mapping] — Mapperly decision
- [Source: _bmad-output/planning-artifacts/architecture.md#Background-Processing] — Coravel decision
- [Source: _bmad-output/planning-artifacts/epics.md#Story-1.1] — Original AC source
- [Source: docs/project-context.md#17-non-negotiable-enforcement-rules] — Critical rules
- ARCH-1: Greenfield scaffold commands
- ARCH-4: EF Core migration pipeline (MigrateAsync explicitly prohibited)
- ARCH-5: PostGIS from day one (`HasPostgresExtension` in first migration)
- ARCH-6: `ApplicationUser : IdentityUser` from the first story
- ARCH-8: Mapperly for all DTO mapping
- ARCH-9: FluentValidation for all input
- ARCH-10: RFC 7807 Problem Details (`AddProblemDetails()` + `UseExceptionHandler()`)
- ARCH-22: Serilog structured logging (replaces default logger)

## Dev Agent Record

### Agent Model Used

Claude Sonnet 4.6

### Debug Log References

- `UseSnakeCaseNamingConvention()` is an extension on `DbContextOptionsBuilder`, not `ModelBuilder`. It is called in `Program.cs` on the options builder (`.UseSnakeCaseNamingConvention()` chained after `.UseNpgsql(...)`), not inside `OnModelCreating`.
- `Microsoft.AspNetCore.SignalR` NuGet package resolves to the old 1.2.9 ASP.NET Core 2.x metapackage — incompatible with .NET 10. SignalR is part of the shared framework; `AddSignalR()` / `MapHub()` work without an explicit package reference.
- `Microsoft.AspNetCore.RazorPages` has no NuGet listing — it is also part of the shared framework. `AddRazorPages()` works without an explicit package.
- `dotnet new sln` defaults to `.slnx` format in .NET 10. Kept as `.slnx` per user preference.
- `<Nullable>enable</Nullable>` is set by default in the .NET 10 `webapi` and `xunit` templates — no manual edit needed.

### Completion Notes List

- ✅ `backend/Blinder.slnx` created with both projects referenced.
- ✅ All 14 NuGet packages added to `Blinder.Api` via `dotnet add` CLI only (never edited .csproj by hand).
- ✅ `Moq`, `FluentAssertions`, `Microsoft.AspNetCore.Mvc.Testing` added to `Blinder.Tests`.
- ✅ All 18 required directories created with `.gitkeep` files; default `WeatherForecast*` files removed.
- ✅ `Models/ApplicationUser.cs` inherits `IdentityUser<Guid>`, all dates `DateTimeOffset`, `UserGender` enum in same file.
- ✅ `Infrastructure/Data/AppDbContext.cs` inherits `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`, PostGIS extension declared, snake_case on options builder.
- ✅ `Errors/AppErrors.cs` — static class, all 9 RFC 7807 type URI constants, no inline strings.
- ✅ `Program.cs` — Serilog bootstrap + host logger, `AddProblemDetails()`, `UseExceptionHandler()`, `AddValidatorsFromAssemblyContaining<Program>()`, Coravel scheduler + queue, RazorPages, Identity, DbContextPool.
- ✅ `appsettings.json` — Serilog section with console + rolling file sinks, Microsoft/System overrides.
- ✅ `dotnet build` → 0 warnings, 0 errors.
- ✅ `dotnet test` → 1 passed, 0 failed.

### File List

- `backend/Blinder.slnx`
- `backend/Blinder.Api/Blinder.Api.csproj`
- `backend/Blinder.Api/Program.cs`
- `backend/Blinder.Api/appsettings.json`
- `backend/Blinder.Api/Models/ApplicationUser.cs`
- `backend/Blinder.Api/Infrastructure/Data/AppDbContext.cs`
- `backend/Blinder.Api/Errors/AppErrors.cs`
- `backend/Blinder.Api/Controllers/.gitkeep`
- `backend/Blinder.Api/Hubs/.gitkeep`
- `backend/Blinder.Api/Pages/Admin/.gitkeep`
- `backend/Blinder.Api/Models/.gitkeep`
- `backend/Blinder.Api/DTOs/.gitkeep`
- `backend/Blinder.Api/Services/.gitkeep`
- `backend/Blinder.Api/Repositories/.gitkeep`
- `backend/Blinder.Api/Mappings/.gitkeep`
- `backend/Blinder.Api/Validators/.gitkeep`
- `backend/Blinder.Api/BackgroundJobs/.gitkeep`
- `backend/Blinder.Api/Errors/.gitkeep`
- `backend/Blinder.Api/Migrations/.gitkeep`
- `backend/Blinder.Api/Infrastructure/Data/.gitkeep`
- `backend/Blinder.Api/Infrastructure/Auth/.gitkeep`
- `backend/Blinder.Api/Infrastructure/Storage/.gitkeep`
- `backend/Blinder.Api/Infrastructure/Scanning/.gitkeep`
- `backend/Blinder.Api/Infrastructure/Email/.gitkeep`
- `backend/Blinder.Api/Infrastructure/Middleware/.gitkeep`
- `backend/Blinder.Tests/Blinder.Tests.csproj`
- `backend/Blinder.Tests/Controllers/.gitkeep`
- `backend/Blinder.Tests/Services/.gitkeep`
- `backend/Blinder.Tests/Validators/.gitkeep`
- `backend/Blinder.Tests/Integration/.gitkeep`
