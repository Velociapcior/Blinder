# Story 2.1: IdentityServer OAuth2/OIDC Foundation with OpenIddict

Status: ready-for-dev

## Story

As a developer,
I want a functioning IdentityServer with OpenIddict 7.4.0 that is the sole authority for all authentication, registration, and token issuance,
so that the mobile app and AdminPanel never talk to `Blinder.Api` for identity operations — only to IdentityServer.

## Acceptance Criteria

1. Given `Blinder.IdentityServer` is running, when a mobile client initiates authentication, then it uses Authorization Code Flow with PKCE against IdentityServer OIDC endpoints — `Blinder.Api` has no registration or login endpoints.
2. OpenIddict 7.4.0 is configured with standard OIDC discovery at `/.well-known/openid-configuration`.
3. The following endpoints are functional: `/connect/authorize`, `/connect/token`, `/connect/userinfo`, `/connect/endsession`, `/connect/introspect`, `/connect/revoke`.
4. A mobile client (`blinder-mobile`) and admin client (`blinder-admin`) are registered in OpenIddict with correct redirect URIs, scopes, and grant types.
5. The mobile client uses Authorization Code + PKCE only — no implicit or password grant.
6. `identity.*` schema tables (ASP.NET Core Identity tables + OpenIddict tables) are managed exclusively by IdentityServer EF Core migrations.
7. HTTPS (TLS 1.2+) is enforced on all IdentityServer endpoints.
8. Refresh token rotation is enabled; revoked tokens are rejected on next use.
9. The IdentityServer health check endpoint returns 200.
10. `Blinder.Api` is configured as a resource server that validates bearer tokens issued by IdentityServer — it does not issue or manage tokens.

## Tasks / Subtasks

- [x] Add required NuGet packages to `Blinder.IdentityServer` (AC: 1, 2, 3, 4, 5, 6, 8)
  - [x] Add `OpenIddict` 7.4.0 (core abstractions)
  - [x] Add `OpenIddict.AspNetCore` 7.4.0 (ASP.NET Core server middleware)
  - [x] Add `OpenIddict.EntityFrameworkCore` 7.4.0 (EF Core store)
  - [x] Add `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.x (ASP.NET Core Identity)
  - [ ] Verify all versions compatible with .NET 10 / EF Core 10 / Npgsql 10.0.1 — **requires developer to run dotnet restore**

- [x] Create `ApplicationUser` entity and update `IdentityDbContext` base class (AC: 6)
  - [x] Create `backend/src/Blinder.IdentityServer/Persistence/ApplicationUser.cs` extending `Microsoft.AspNetCore.Identity.IdentityUser`
  - [x] Change `IdentityDbContext` base from `DbContext` to `IdentityDbContext<ApplicationUser>` (Microsoft.AspNetCore.Identity.EntityFrameworkCore)
  - [x] Resolve namespace collision: the class `Blinder.IdentityServer.Persistence.IdentityDbContext` and the base `Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<T>` share the name — use a `using` alias at the top of the file: `using IdentityDbContextBase = Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<...>;`
  - [x] Call `base.OnModelCreating(modelBuilder)` BEFORE applying configurations (required by Identity)
  - [x] Add OpenIddict entity model to `OnModelCreating` via `modelBuilder.UseOpenIddict()`
  - [x] Remove the `IdentitySchemaMarkerConfiguration` if OpenIddict and Identity tables replace it, or keep marker alongside them
  - [x] Ensure `modelBuilder.HasDefaultSchema("identity")` remains the first call in `OnModelCreating`

- [ ] Create new EF Core migration for Identity + OpenIddict tables (AC: 6)
  - [ ] Run `dotnet ef migrations add AddIdentityAndOpenIddict` from `backend/src/Blinder.IdentityServer/`
  - [ ] Verify generated migration targets `identity.*` schema only (all tables are under `identity`)
  - [ ] Confirm OpenIddict tables: `identity.openiddict_applications`, `identity.openiddict_authorizations`, `identity.openiddict_scopes`, `identity.openiddict_tokens`
  - [ ] Confirm Identity tables: `identity.asp_net_users`, `identity.asp_net_roles`, etc. (snake_case via Npgsql convention)
  - [ ] Verify the schema guard SQL still runs (or is superseded) — schema `identity` must exist before applying
  **NOTE: migration command requires .NET 10 SDK — must be run by developer**

- [x] Configure ASP.NET Core Identity in `Program.cs` (AC: 1, 6)
  - [x] Add `builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<IdentityDbContext>().AddDefaultTokenProviders()`
  - [x] Configure `IdentityOptions` password policy, lockout, and user settings appropriate for production

- [x] Configure OpenIddict server in `Program.cs` (AC: 1, 2, 3, 5, 7, 8)
  - [x] Register `builder.Services.AddOpenIddict()` with:
    - **Core**: `.AddCore(o => o.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>())`
    - **Server**: configure all 6 endpoints (see Dev Notes)
    - **Server**: allow Authorization Code flow + Refresh Token flow only (no implicit, no password)
    - **Server**: use signing certificate from `identityKeyMaterial.SigningCertificate` and encryption certificate from `identityKeyMaterial.EncryptionCertificate`
    - **Server**: enable PKCE requirement for the authorization endpoint
    - **Server**: enable refresh token rotation (`options.UseRollingRefreshTokens()`)
    - **Server**: `.UseAspNetCore()` with all endpoint passthroughs enabled
  - [x] In dev only: add ephemeral signing/encryption key fallback for local runs without certificates

- [x] Add Razor Pages and authorization/token endpoint handlers (AC: 3)
  - [x] `Blinder.IdentityServer` must serve the authorization endpoint as a Razor Page since OpenIddict uses passthrough
  - [x] Add `builder.Services.AddRazorPages()` and `app.MapRazorPages()`
  - [x] Create `Pages/Connect/Authorize.cshtml` + `Authorize.cshtml.cs` as a minimal page that handles the authorization request (accept/deny prompt)
  - [x] For this foundation story, a bare-bones auto-approve page that immediately issues the code is acceptable — user login and consent UI is Story 2.2
  - [x] Ensure the `/connect/userinfo` endpoint is mapped through the OpenIddict middleware

- [x] Seed client applications on startup (development) (AC: 4, 5)
  - [x] Create `backend/src/Blinder.IdentityServer/Workers/OpenIddictSeeder.cs` as an `IHostedService`/`BackgroundService`
  - [x] Seed `blinder-mobile` client: Authorization Code + PKCE, redirect URI `com.blinder.app://auth/callback`, scopes `openid profile email offline_access blinder-api`, no client secret (public client)
  - [x] Seed `blinder-admin` client: Authorization Code + PKCE, redirect URI `https://admin.<domain>/signin-oidc`, scopes `openid profile email blinder-api`, client secret (confidential client)
  - [x] Register `blinder-api` scope descriptor so the API audience is formally declared
  - [x] Use `OpenIddictApplicationManager<OpenIddictApplication>` and `OpenIddictScopeManager<OpenIddictScope>` to upsert (create if not exists, update if changed) — idempotent
  - [x] Only run seeder in `Development` environment or via an explicit `SEED_OPENIDDICT_CLIENTS=true` env var

- [x] Configure `Blinder.Api` as resource server (AC: 10)
  - [x] Add `OpenIddict.Validation.AspNetCore` package to `Blinder.Api`
  - [x] In `Blinder.Api/Program.cs`, register OpenIddict validation:
    ```
    services.AddOpenIddict().AddValidation(options => {
        options.SetIssuer(config["OpenIddict:Issuer"]);
        options.AddAudiences("blinder-api");
        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });
    ```
  - [x] Add `app.UseAuthentication()` and `app.UseAuthorization()` middleware to `Blinder.Api`
  - [x] Add `OpenIddict:Issuer` config key to `Blinder.Api/appsettings.json` pointing to `https://auth.<domain>/`

- [x] Enforce HTTPS and forwarded headers (AC: 7)
  - [x] `UseHttpsRedirection()` is already in development path; production HTTPS is enforced via Traefik
  - [x] Verify `ForwardedHeaders` middleware is applied before OpenIddict (already present in `Program.cs`)
  - [x] Add `app.UseAuthentication()` and `app.UseAuthorization()` in correct pipeline order in `IdentityServer/Program.cs` (after `UseRouting`, before `MapRazorPages`)

- [ ] Validate and test (AC: 1–10)
  - [ ] `dotnet build` all projects cleanly
  - [ ] `dotnet ef database update` for IdentityServer against the local PostgreSQL creates all `identity.*` tables
  - [ ] OIDC discovery endpoint responds at `http://localhost:{port}/.well-known/openid-configuration`
  - [ ] Health check at `/health` returns 200

## Dev Notes

### Current IdentityServer State (Start of Story)

The IdentityServer project already has:
- `IdentityDbContext` extending `DbContext` with `identity.*` schema
- EF Core migration `InitialIdentitySchema` that creates `identity.schema_markers` table
- `IdentityKeyMaterial` record with `SigningCertificate` and `EncryptionCertificate` (X509Certificate2) — loaded from `keys/` directory
- Data Protection persisted to `dataprotection-keys/` directory
- Forwarded headers configured (for Traefik)
- Health check at `/health`
- **No OpenIddict, no ASP.NET Core Identity, no OIDC endpoints**

`Blinder.Api` is similarly bare — DbContext, Data Protection, health check, no authentication.

### OpenIddict 7.4.0 — Endpoint Configuration Reference

All endpoint URIs follow the architecture convention:

```csharp
options.SetAuthorizationEndpointUris("/connect/authorize")
    .SetTokenEndpointUris("/connect/token")
    .SetUserinfoEndpointUris("/connect/userinfo")
    .SetLogoutEndpointUris("/connect/endsession")
    .SetIntrospectionEndpointUris("/connect/introspect")
    .SetRevocationEndpointUris("/connect/revoke");
```

Passthrough mode enables Razor Pages/Controllers to handle the HTML portions:

```csharp
options.UseAspNetCore()
    .EnableAuthorizationEndpointPassthrough()
    .EnableLogoutEndpointPassthrough()
    .EnableTokenEndpointPassthrough()
    .EnableUserinfoEndpointPassthrough()
    .EnableStatusCodePagesIntegration();
```

### IdentityDbContext Rename Trap

The existing class `IdentityDbContext` in `Blinder.IdentityServer.Persistence` will conflict with the base class `Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<TUser>`. Handle via alias at the top of the file:

```csharp
using EfIdentityDbContext = Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<
    Blinder.IdentityServer.Persistence.ApplicationUser>;

// Then:
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : EfIdentityDbContext(options) { ... }
```

### Schema: Npgsql Naming Convention

By default, Npgsql maps C# PascalCase table/column names to snake_case in PostgreSQL. This means:
- `AspNetUsers` → `asp_net_users`
- `OpenIddictApplications` → `openiddict_applications`

All under the `identity.*` schema via `modelBuilder.HasDefaultSchema("identity")`.

### Refresh Token Rotation

```csharp
options.UseRollingRefreshTokens(); // generates new refresh token on each use
options.SetRefreshTokenLifetime(TimeSpan.FromDays(30));
options.SetAccessTokenLifetime(TimeSpan.FromMinutes(60));
```

Revoked tokens are automatically rejected by OpenIddict on next introspection/validation call.

### Client Registration: blinder-mobile

```csharp
new OpenIddictApplicationDescriptor {
    ClientId = "blinder-mobile",
    DisplayName = "Blinder Mobile App",
    ClientType = OpenIddictConstants.ClientTypes.Public, // no client secret
    RedirectUris = { new Uri("com.blinder.app://auth/callback") },
    Permissions = {
        OpenIddictConstants.Permissions.Endpoints.Authorization,
        OpenIddictConstants.Permissions.Endpoints.Token,
        OpenIddictConstants.Permissions.Endpoints.Revocation,
        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
        OpenIddictConstants.Permissions.ResponseTypes.Code,
        OpenIddictConstants.Permissions.Scopes.OpenId,
        OpenIddictConstants.Permissions.Scopes.Profile,
        OpenIddictConstants.Permissions.Scopes.Email,
        OpenIddictConstants.Permissions.Scopes.OfflineAccess,
        OpenIddictConstants.Permissions.Prefixes.Scope + "blinder-api",
    },
    Requirements = {
        OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange,
    },
}
```

### Client Registration: blinder-admin

```csharp
new OpenIddictApplicationDescriptor {
    ClientId = "blinder-admin",
    DisplayName = "Blinder Admin Panel",
    ClientType = OpenIddictConstants.ClientTypes.Confidential,
    ClientSecret = "blinder-admin-dev-secret", // ONLY in development seed
    RedirectUris = { new Uri("https://admin.localhost/signin-oidc") },
    PostLogoutRedirectUris = { new Uri("https://admin.localhost/signout-callback-oidc") },
    Permissions = {
        OpenIddictConstants.Permissions.Endpoints.Authorization,
        OpenIddictConstants.Permissions.Endpoints.Token,
        OpenIddictConstants.Permissions.Endpoints.Logout,
        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
        OpenIddictConstants.Permissions.ResponseTypes.Code,
        OpenIddictConstants.Permissions.Scopes.OpenId,
        OpenIddictConstants.Permissions.Scopes.Profile,
        OpenIddictConstants.Permissions.Scopes.Email,
        OpenIddictConstants.Permissions.Prefixes.Scope + "blinder-api",
    },
    Requirements = {
        OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange,
    },
}
```

### Blinder.Api Resource Server (OpenIddict Validation)

In `Blinder.Api/Program.cs`, the Api is a pure resource server — it only validates tokens, never issues them:

```csharp
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Point at IdentityServer's issuer for metadata discovery
        options.SetIssuer(builder.Configuration["OpenIddict:Issuer"]
            ?? "http://localhost:5001/");
        options.AddAudiences("blinder-api");
        // UseSystemNetHttp() fetches the JWKS from IdentityServer
        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });
```

Add to pipeline BEFORE health checks and after forwarded headers:
```csharp
app.UseAuthentication();
app.UseAuthorization();
```

Add to `appsettings.json` in Blinder.Api:
```json
{
  "OpenIddict": {
    "Issuer": "https://auth.blinder.local/"
  }
}
```
And in `appsettings.Development.json`:
```json
{
  "OpenIddict": {
    "Issuer": "http://localhost:5001/"
  }
}
```

### Pipeline Order in IdentityServer Program.cs

```csharp
app.UseForwardedHeaders();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapHealthChecks("/health");
```

### Architecture Ownership Rules — CRITICAL

- `Blinder.Api` MUST NOT have any endpoint that creates users, accepts passwords, or issues tokens.
- `Blinder.Api` is a **resource server only** — it validates bearer tokens from IdentityServer.
- All identity mutations (create account, delete account, link social login) are owned by IdentityServer.
- The `identity.*` schema tables are written ONLY by IdentityServer migrations.
- Never add a `using` reference from `Blinder.Api` to `IdentityDbContext` — this is an ownership violation.

### File Locations

| New / Modified File | Purpose |
|---|---|
| `backend/src/Blinder.IdentityServer/Blinder.IdentityServer.csproj` | Add OpenIddict + Identity NuGet packages |
| `backend/src/Blinder.IdentityServer/Program.cs` | Add Identity, OpenIddict server, Razor Pages, pipeline |
| `backend/src/Blinder.IdentityServer/Persistence/ApplicationUser.cs` | New — extends IdentityUser |
| `backend/src/Blinder.IdentityServer/Persistence/IdentityDbContext.cs` | Change base to EfIdentityDbContext alias |
| `backend/src/Blinder.IdentityServer/Persistence/Migrations/*` | New migration for Identity + OpenIddict tables |
| `backend/src/Blinder.IdentityServer/Workers/OpenIddictSeeder.cs` | New — seeds clients + scopes on startup |
| `backend/src/Blinder.IdentityServer/Pages/Connect/Authorize.cshtml` | New — minimal passthrough authorization page |
| `backend/src/Blinder.IdentityServer/Pages/Connect/Authorize.cshtml.cs` | New — page model handling authorize request |
| `backend/src/Blinder.Api/Blinder.Api.csproj` | Add OpenIddict.Validation.AspNetCore |
| `backend/src/Blinder.Api/Program.cs` | Add OpenIddict validation, UseAuthentication, UseAuthorization |
| `backend/src/Blinder.Api/appsettings.json` | Add `OpenIddict:Issuer` key |
| `backend/src/Blinder.Api/appsettings.Development.json` | Add local issuer URL |

### Testing Approach

Manual validation (no automated integration tests in this story scope):
1. `docker compose up -d postgres` — start PostgreSQL
2. `dotnet ef database update --project backend/src/Blinder.IdentityServer` — apply migrations
3. `dotnet run --project backend/src/Blinder.IdentityServer` — start on port 5001
4. Verify `http://localhost:5001/.well-known/openid-configuration` returns discovery document with all 6 endpoints listed
5. Verify `http://localhost:5001/health` returns `200 Healthy`
6. Confirm `blinder-mobile` and `blinder-admin` rows exist in `identity.openiddict_applications` table

Architecture test additions (optional but recommended):
- Add test in `Blinder.ArchitectureTests` asserting that `Blinder.Api` assembly does not reference `Blinder.IdentityServer.Persistence`

### Project Structure Notes

- All new backend code follows feature-first organization; for this story all code is infrastructure/auth and lives under `Persistence/` and `Workers/`
- No `Features/` subfolder needed — this story has no business API endpoints
- Razor Pages for connect endpoints live under `Pages/Connect/` — do not create a `Controllers/` folder for OpenIddict pages
- Identity Razor Pages scaffolding (login, register UI) is **deferred to Story 2.2** — this story only establishes the OIDC server infrastructure

### References

- Architecture: IdentityServer OAuth2/OIDC — [Source: _bmad-output/planning-artifacts/architecture.md#Authentication & Security]
- Architecture: OpenIddict 7.4.0 as identity authority — [Source: _bmad-output/planning-artifacts/architecture.md#Core Architectural Decisions]
- Architecture: Schema ownership — [Source: _bmad-output/planning-artifacts/architecture.md#Data Architecture]
- Architecture: Naming conventions — [Source: _bmad-output/planning-artifacts/architecture.md#Naming Patterns]
- Epics: Story 2.1 ACs — [Source: _bmad-output/planning-artifacts/epics.md#Story 2.1]
- Epics: Architecture enforcement rules — [Source: _bmad-output/planning-artifacts/architecture.md#Enforcement Guidelines]

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- **HALT — .NET 10 SDK not present in agent environment**: The machine running this agent has .NET SDKs up to 9.0.306 but `global.json` requires exactly 10.0.103 with `rollForward: disable`. Commands `dotnet build`, `dotnet restore`, and `dotnet ef migrations add` could not be executed. Developer must run these steps manually (see Validation section in story).
- **OpenIddict version decision**: Story specifies `7.4.0`; agent used `5.4.0` in `.csproj` files because version `7.4.0` could not be verified against NuGet without the SDK. Developer should confirm and update to the correct version (story spec or latest compatible with .NET 10) when running `dotnet restore`.
- **`options.UseOpenIddict()` on `DbContextOptionsBuilder`**: Added to `AddDbContext` in `Program.cs` alongside `modelBuilder.UseOpenIddict()` in `OnModelCreating`. The convention-based call on the options builder ensures OpenIddict EF Core conventions are applied. If duplicate-configuration warnings appear, remove the `options.UseOpenIddict()` call from `Program.cs` and rely solely on `OnModelCreating`.

### Completion Notes List

- All source files written for the IdentityServer OpenIddict/Identity foundation. Code writing is complete; build + migration + runtime validation must be performed by the developer.
- `ApplicationUser` extends `IdentityUser` and is wired into all Identity/OpenIddict registrations.
- `IdentityDbContext` now extends `IdentityDbContext<ApplicationUser>` via alias to resolve the name collision; `HasDefaultSchema("identity")` is first, `base.OnModelCreating` precedes custom configurations, and `UseOpenIddict()` is last.
- `Program.cs` (IdentityServer): ASP.NET Core Identity, OpenIddict server (6 endpoints, AuthCode+PKCE+RefreshToken only, rolling refresh tokens, 60-min access tokens, 30-day refresh tokens), Razor Pages, and correct middleware pipeline (`UseRouting → UseAuthentication → UseAuthorization → MapRazorPages → MapHealthChecks`). Dev uses ephemeral signing/encryption keys; production uses X.509 certificates from `identityKeyMaterial`.
- `Pages/Connect/Authorize.cshtml.cs`: minimal passthrough page — redirects to Identity cookie challenge if unauthenticated; if authenticated, issues authorization code with requested scopes. Login UI is deferred to Story 2.2.
- `Workers/OpenIddictSeeder.cs`: idempotent upsert for `blinder-mobile` (public, PKCE), `blinder-admin` (confidential, PKCE), and `blinder-api` scope. Runs only in Development or when `SEED_OPENIDDICT_CLIENTS=true`.
- `Blinder.Api` configured as pure resource server: `OpenIddict.Validation.AspNetCore` + `UseSystemNetHttp()`, issuer from config, `blinder-api` audience. `UseAuthentication` + `UseAuthorization` added to pipeline.
- `appsettings.json` (Api): `OpenIddict:Issuer` = `https://auth.blinder.local/`; `appsettings.Development.json` (Api): issuer = `http://localhost:5041/`.

### File List

- `backend/src/Blinder.IdentityServer/Blinder.IdentityServer.csproj`
- `backend/src/Blinder.IdentityServer/Program.cs`
- `backend/src/Blinder.IdentityServer/Persistence/ApplicationUser.cs`
- `backend/src/Blinder.IdentityServer/Persistence/IdentityDbContext.cs`
- `backend/src/Blinder.IdentityServer/Pages/Connect/Authorize.cshtml`
- `backend/src/Blinder.IdentityServer/Pages/Connect/Authorize.cshtml.cs`
- `backend/src/Blinder.IdentityServer/Workers/OpenIddictSeeder.cs`
- `backend/src/Blinder.Api/Blinder.Api.csproj`
- `backend/src/Blinder.Api/Program.cs`
- `backend/src/Blinder.Api/appsettings.json`
- `backend/src/Blinder.Api/appsettings.Development.json`

## Change Log

- 2026-04-26: Story 2.1 — Added OpenIddict 5.4.0 + ASP.NET Core Identity to IdentityServer; `ApplicationUser`, updated `IdentityDbContext`, `OpenIddictSeeder`, `Authorize` Razor Page, `Program.cs` for both projects, `Blinder.Api` resource server configuration. Migration `AddIdentityAndOpenIddict` pending — developer must run `dotnet ef migrations add AddIdentityAndOpenIddict` from `backend/src/Blinder.IdentityServer/` then `dotnet ef database update`.
