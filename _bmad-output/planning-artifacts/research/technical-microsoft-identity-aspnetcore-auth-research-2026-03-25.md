---
stepsCompleted: [1, 2, 3, 4, 5, 6]
inputDocuments: []
workflowType: 'research'
lastStep: 1
research_type: 'technical'
research_topic: 'Microsoft Identity Libraries for ASP.NET Core Authentication'
research_goals: 'Understand what Microsoft provides out-of-the-box for OAuth2/OIDC foundation (Microsoft.AspNetCore.Identity, Microsoft.Identity.*, OpenIddict) to replace the custom OAuth2 implementation planned in Story 2-0 of the Blinder project'
user_name: 'Piotr.palej'
date: '2026-03-25'
web_research_enabled: true
source_verification: true
---

# Research Report: technical

**Date:** 2026-03-25
**Author:** Piotr.palej
**Research Type:** technical

---

## Technical Research Scope Confirmation

**Research Topic:** Microsoft Identity Libraries for ASP.NET Core Authentication
**Research Goals:** Understand what Microsoft provides out-of-the-box for OAuth2/OIDC foundation (Microsoft.AspNetCore.Identity, Microsoft.Identity.*, OpenIddict) to replace the custom OAuth2 implementation planned in Story 2-0 of the Blinder project.

**Technical Research Scope:**

- Architecture Analysis - design patterns, frameworks, system architecture
- Implementation Approaches - development methodologies, coding patterns
- Technology Stack - languages, frameworks, tools, platforms
- Integration Patterns - APIs, protocols, interoperability
- Performance Considerations - scalability, optimization, patterns

**Research Methodology:**

- Current web data with rigorous source verification
- Multi-source validation for critical technical claims
- Confidence level framework for uncertain information
- Comprehensive technical coverage with architecture-specific insights

**Scope Confirmed:** 2026-03-25

## Research Overview

This research was conducted to answer a specific architectural question: does Story 2-0 of the Blinder project need to hand-roll its OAuth2 infrastructure, or can Microsoft's identity ecosystem — and the broader .NET open-source ecosystem — provide it ready-made?

The answer is decisive. **OpenIddict**, combined with **ASP.NET Core Identity**, provides every grant handler, token endpoint, revocation mechanism, and refresh token rotation primitive that Story 2-0 planned to build from scratch. The research also confirmed that Microsoft's own packages (`Microsoft.Identity.Web`, the `.NET 10` Identity API endpoints) are not applicable to a self-hosted OAuth2 server scenario — they serve different purposes. OpenIddict is the correct, free, actively maintained choice.

The research further resolved two significant architectural questions: (1) the authorization server should be a **separate `Blinder.IdentityServer` project**, not co-located with `Blinder.Api`; and (2) Story 2-0's **30-day access token expiry is incompatible** with remote token validation — 15-minute access tokens with 30-day rolling refresh tokens is the correct pattern. See the Research Synthesis section for the full executive summary, Story 2-0 revised task breakdown, and implementation recommendations.

---

<!-- Content will be appended sequentially through research workflow steps -->

## Technology Stack Analysis

### Programming Languages

C# is the primary language for all packages in this stack. All libraries support .NET 8 (LTS) and .NET 9/10.

_Popular Languages:_ C# / .NET
_Language Evolution:_ .NET 8 LTS is the production-stable target; .NET 9+ supported by all packages
_Performance Characteristics:_ Native AOT compatibility improving across packages; OpenIddict 7.x fully supports .NET 8+
_Source:_ https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication

---

### Development Frameworks and Libraries

The Microsoft identity ecosystem for ASP.NET Core consists of three distinct layers with separate responsibilities:

**Layer 1 — User & Credential Management: `Microsoft.AspNetCore.Identity`**

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.8 | User/role store via EF Core |
| `Microsoft.Extensions.Identity.Core` | 8.0.17 | Core identity abstractions |
| `Microsoft.AspNetCore.Identity.UI` | 8.0.6 | Optional Razor Pages UI |

- Provides: `UserManager<T>`, `SignInManager<T>`, password hashing, 2FA, email confirmation, external login support
- **Does NOT provide**: Standard OAuth2 token endpoints, JWT generation, RFC 6749 compliance
- The built-in `.NET 8 Identity API endpoints` (`AddIdentityApiEndpoints`) generate proprietary bearer tokens (NOT JWT), intended for simple SPA scenarios only
- _Source:_ https://devblogs.microsoft.com/dotnet/improvements-auth-identity-aspnetcore-8/

**Layer 2 — OAuth2/OIDC Server: OpenIddict**

| Package | Version | Purpose |
|---|---|---|
| `OpenIddict` | 7.4.0 (2026-03-12) | Metapackage |
| `OpenIddict.Core` | 7.2.0 | Core abstractions |
| `OpenIddict.AspNetCore` | 7.4.0 | ASP.NET Core integration |
| `OpenIddict.Server.AspNetCore` | 7.2.0 | OAuth2/OIDC server endpoints |
| `OpenIddict.EntityFrameworkCore` | 7.2.0 | EF Core token/authorization stores |
| `OpenIddict.Validation.AspNetCore` | 7.2.0 | Token validation middleware |
| `OpenIddict.Client` | 7.2.0 | OAuth2 client (for consuming external providers) |

OpenIddict is the recommended open-source framework for building an OAuth2/OIDC authorization server on ASP.NET Core when Microsoft's cloud-based Entra ID is not used.

- Supports all grant types: Authorization Code (+ PKCE), ROPC, Client Credentials, Refresh Token, Device Authorization, Token Exchange
- Built-in token endpoints: `/connect/token`, `/connect/revoke`, `/connect/introspect`, `.well-known/openid-configuration`
- Built-in refresh token rotation with "redeemed" status tracking (detects replay attacks)
- Built-in cascade revocation: revoking refresh token invalidates all related access tokens
- Reference token mode: stores opaque identifier in DB instead of full payload
- Integrates with `ASP.NET Core Identity` for user credential validation via `UserManager`
- Supported through July 2027 (v7.x)
- _Source:_ https://github.com/openiddict/openiddict-core, https://documentation.openiddict.com

**Layer 3 — Token Validation (Resource Server): `Microsoft.AspNetCore.Authentication.JwtBearer`**

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.x / 10.0.5 | Validate JWT bearer tokens on APIs |
| `System.IdentityModel.Tokens.Jwt` | 8.16.0 | JWT creation and parsing |

- Used on the API side to validate access tokens issued by OpenIddict
- .NET 8 breaking change: `TokenValidatedContext.SecurityToken` now returns `JsonWebToken` (not `JwtSecurityToken`) — use `JsonWebToken` type
- Does NOT include built-in token revocation — requires custom blacklist or versioning pattern
- _Source:_ https://learn.microsoft.com/en-us/dotnet/core/compatibility/aspnet-core/8.0/securitytoken-events

**Layer 4 — Microsoft Cloud Identity (Entra ID): `Microsoft.Identity.Web`**

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.Identity.Web` | 4.6.0 (2026-03-20) | Microsoft Entra ID / Azure AD integration |

- Built on MSAL — exclusively for Microsoft Entra ID (Azure AD), NOT for self-hosted identity servers
- **Not applicable** to Blinder's self-hosted OAuth2 server scenario
- _Source:_ https://www.nuget.org/packages/Microsoft.Identity.Web

_Ecosystem Maturity:_ OpenIddict is mature, actively maintained, and widely used in production ASP.NET Core apps requiring a self-hosted OAuth2 server. ASP.NET Core Identity for user management is the de-facto standard.

---

### Database and Storage Technologies

**OpenIddict Token Storage (EF Core):**
- OpenIddict creates DB tables for: applications, authorizations, tokens, scopes
- Token records store: subject, client ID, creation/expiration dates, status (`valid`, `redeemed`, `revoked`)
- Token **payload** is NOT stored by default (stateless access tokens remain JWTs)
- Exception: authorization codes, device codes, user codes, and state tokens DO store payload
- **Reference token mode**: stores a 256-bit random identifier → full payload stored server-side (replaces JWT with opaque token)
- Strongly recommended: column-level encryption or ASP.NET Core Data Protection for the Payload column
- Refresh token cascade revocation is automatic: revoking one token revokes all related tokens

**Refresh Token Revocation Strategies (JwtBearer):**

| Strategy | Mechanism | Performance |
|---|---|---|
| Token Blacklist | Store revoked `jti` in Redis/DB; check on each request | Moderate (one cache lookup/request) |
| Token Versioning | Store user version in DB; increment on logout | Low DB reads (user record only) |
| Short-lived Access Tokens | 5–15 min expiry; no revocation check needed | High (no check required) |
| Hybrid (Recommended) | Short-lived access tokens + DB-backed refresh tokens | Optimal |

_Recommended for Blinder:_ Hybrid approach — short-lived access tokens (no revocation check per request) + OpenIddict-managed refresh tokens in PostgreSQL via EF Core.

_Source:_ https://documentation.openiddict.com/configuration/token-storage

---

### Development Tools and Platforms

- **IDE:** Visual Studio 2022 / VS Code + C# Dev Kit
- **NuGet:** All packages available on nuget.org
- **EF Core Migrations:** Required for OpenIddict tables (`dotnet ef migrations add`)
- **Testing:** OpenIddict ships official sample projects (openiddict-samples GitHub repo) covering all grant types including ROPC
- **Build Systems:** Standard MSBuild / dotnet CLI

---

### Cloud Infrastructure and Deployment

- All packages are cloud-provider agnostic — deployable to any host supporting .NET 8+
- Token signing key management: RSA-256 key from configuration (environment variable / Azure Key Vault / AWS Secrets Manager)
- OpenIddict supports distributed deployments with shared database for token storage
- Rate limiting: ASP.NET Core built-in `AddRateLimiter()` applies independently of OpenIddict

---

### Technology Adoption Trends

- **OpenIddict replacing IdentityServer:** Microsoft removed Duende IdentityServer from .NET SPA templates in .NET 8 due to licensing cost. OpenIddict is the recommended free alternative.
- **ROPC deprecation trend:** OAuth 2.0 security best practices (OAuth 2.1 draft) formally deprecate ROPC. However, for first-party mobile apps (like Blinder) it remains acceptable and widely used in practice — Story 2-0 already uses it for email/password login.
- **Reference tokens gaining adoption:** Growing preference for opaque reference tokens over self-contained JWTs in scenarios requiring immediate revocation.
- **Microsoft.Identity.Web** targets cloud/Entra scenarios only — not relevant for self-hosted identity.

_Source:_ https://kevinchalet.com/2023/10/04/can-you-use-the-asp-net-core-identity-api-endpoints-with-openiddict/, https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth-ropc

---

## Integration Patterns Analysis

### Story 2-0 AC Mapping: Built-in vs Custom

This is the core of the research. The table below maps every Story 2-0 acceptance criterion to what OpenIddict provides out-of-the-box versus what still requires custom code.

| AC | Requirement | OpenIddict Built-in | Custom Code Required |
|---|---|---|---|
| AC1 | `POST /api/auth/oauth/token` RFC 6749 response | ✅ Token endpoint, response serialization | Controller action to handle grant dispatch |
| AC2 | ROPC grant (email + password) | ✅ Grant type routing, error responses | `CheckPasswordAsync` via `UserManager` in controller |
| AC3 | Authorization Code grant framework | ✅ Grant type, code generation, one-time use | `ISocialLoginTokenValidator` hook in controller |
| AC4 | Refresh token flow | ✅ Rotation, one-time use, new token pair | None — fully automatic |
| AC5 | Token revocation endpoint `/api/auth/oauth/revoke` | ✅ `/connect/revoke` endpoint, idempotent 200 OK | Route alias if `/api/auth/oauth/revoke` path required |
| AC6 | Refresh token hashed storage | ✅ Encrypted at rest (Data Protection), DB record | None — automatic with EF Core store |
| AC7 | Mobile token storage contract | ❌ Not applicable (mobile-side) | `storageService.ts` as planned |
| AC8 | JWT claims standardization (sub, iss, aud, exp, iat) | ✅ Standard claims automatic | Set `aud` = "blinder-mobile" in claims identity |
| AC9 | Social login framework (`ISocialLoginTokenValidator`) | ✅ Event handler architecture, custom grant types | Define interface + wire into authorization controller |
| AC10 | Rate limiting on token endpoint | ❌ Not OpenIddict's concern | ASP.NET Core `AddRateLimiter()` as planned |

**Net result:** Tasks 1–5 in Story 2-0 collapse significantly. The custom `OAuth2TokenService`, `RefreshTokenGrantHandler`, `TokenRevocationHandler`, and `RefreshTokenRepository` become unnecessary — replaced by OpenIddict configuration.

---

### DI Wiring Pattern (Program.cs)

```csharp
// Layer 1: ASP.NET Core Identity (user store)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Layer 2: OpenIddict (OAuth2 server + validation)
builder.Services.AddOpenIddict()
    .AddCore(options => options
        .UseEntityFrameworkCore()
        .UseDbContext<ApplicationDbContext>())
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/api/auth/oauth/token");
        options.SetRevocationEndpointUris("/api/auth/oauth/revoke");

        options.AllowPasswordFlow()       // ROPC (AC2)
               .AllowRefreshTokenFlow()   // Refresh (AC4)
               .AllowAuthorizationCodeFlow(); // Social login (AC3, AC9)

        // RSA-256 signing from configuration (AC8)
        options.AddSigningCertificate(LoadRsaCertFromConfig(builder.Configuration));
        options.AddEncryptionCertificate(LoadEncCertFromConfig(builder.Configuration));

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableRevocationEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// Rate limiting (AC10)
builder.Services.AddRateLimiter(options => { /* token-endpoint policy */ });
```

_Source:_ https://documentation.openiddict.com/guides/getting-started/creating-your-own-server-instance

---

### Authorization Controller Pattern (What You Still Write)

OpenIddict uses **passthrough mode** — the token endpoint routes to your controller action. You own the grant logic; OpenIddict owns the token issuance and response serialization.

```csharp
[ApiController]
public class OAuth2Controller : ControllerBase
{
    [HttpPost("/api/auth/oauth/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();

        if (request.IsPasswordGrantType())          // ROPC (AC2)
        {
            var user = await _userManager.FindByEmailAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = BuildClaimsIdentity(user, request.GetScopes());
            return SignIn(new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsAuthorizationCodeGrantType())  // Social login (AC3, AC9)
        {
            // Validate provider ID token via ISocialLoginTokenValidator
            // OpenIddict already validated the authorization code is one-time use
            ...
        }

        if (request.IsRefreshTokenGrantType())       // Refresh (AC4) — OpenIddict handles fully
        {
            // Usually just re-sign with updated claims; rotation is automatic
            var principal = (await HttpContext.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest();
    }
}
```

_Source:_ https://github.com/openiddict/openiddict-samples (Hollastin sample), https://documentation.openiddict.com/integrations/aspnet-core

---

### EF Core Database Tables

OpenIddict adds 4 tables via EF Core migrations:

| Table | Purpose |
|---|---|
| `OpenIddictApplications` | Registered OAuth2 clients |
| `OpenIddictAuthorizations` | Authorization grants |
| `OpenIddictTokens` | Token records (status: valid/redeemed/revoked) |
| `OpenIddictScopes` | Registered scopes |

Run: `dotnet ef migrations add AddOpenIddictSchema`

Refresh tokens are stored **encrypted** (ASP.NET Core Data Protection) — never plaintext. Rotation marks old token as `redeemed`; replay attempts are rejected automatically.

_Source:_ https://documentation.openiddict.com/integrations/entity-framework-core, https://documentation.openiddict.com/configuration/token-storage

---

### RSA Signing Key Configuration

Production setup with two separate certificates (signing + encryption):

```csharp
// Load from appsettings / environment variable (never hardcoded)
options.AddSigningCertificate(new X509Certificate2(
    Convert.FromBase64String(config["Auth:SigningCertBase64"]),
    config["Auth:SigningCertPassword"]));

options.AddEncryptionCertificate(new X509Certificate2(
    Convert.FromBase64String(config["Auth:EncryptionCertBase64"]),
    config["Auth:EncryptionCertPassword"]));
```

Development: `options.AddDevelopmentSigningCertificate().AddDevelopmentEncryptionCertificate()`

_Source:_ https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html

---

### Deployment Topology: Separate Identity Server

**Architectural decision (Piotr, 2026-03-25):** The OpenIddict authorization server will be a **separate ASP.NET Core Web Application project** (`Blinder.IdentityServer`), distinct from the main `Blinder.Api` resource server.

This changes the validation pattern: `UseLocalServer()` is NOT applicable. The API project validates tokens **remotely** via OIDC discovery or introspection.

#### Identity Server Project (`Blinder.IdentityServer`)

Owns: OpenIddict server + `AddIdentity` user store + EF Core + `OAuth2Controller`

```csharp
builder.Services.AddOpenIddict()
    .AddCore(o => o.UseEntityFrameworkCore().UseDbContext<IdentityDbContext>())
    .AddServer(o =>
    {
        o.SetTokenEndpointUris("/api/auth/oauth/token");
        o.SetRevocationEndpointUris("/api/auth/oauth/revoke");
        o.AllowPasswordFlow().AllowRefreshTokenFlow().AllowAuthorizationCodeFlow();
        o.AddSigningCertificate(...).AddEncryptionCertificate(...);
        o.UseAspNetCore()
         .EnableTokenEndpointPassthrough()
         .EnableRevocationEndpointPassthrough();
    });
```

#### Main API Project (`Blinder.Api`)

Owns: Resource endpoints only. Validates tokens remotely via OIDC discovery:

```csharp
builder.Services.AddOpenIddict()
    .AddValidation(o =>
    {
        o.SetIssuer("https://identity.blinder.app/");  // identity server base URL
        o.UseSystemNetHttp();  // fetches .well-known/openid-configuration for keys
        o.UseAspNetCore();
    });
```

Or with **introspection** for real-time revocation checking per request:

```csharp
o.UseIntrospection()
 .SetClientId("blinder-api")
 .SetClientSecret("...");
```

| Validation Mode | Revocation | Performance | Complexity |
|---|---|---|---|
| OIDC discovery (JWT) | On token expiry only | High (keys cached) | Low |
| Introspection | Real-time per request | Lower (network call) | Medium |
| Hybrid (short-lived JWTs + refresh introspection) | Effective | High | Medium |

**Recommendation for Blinder MVP:** OIDC discovery with short-lived access tokens (15–30 min). Real-time revocation via introspection can be added post-MVP.

_Source:_ https://documentation.openiddict.com/guides/getting-started/integrating-with-a-remote-server-instance, https://documentation.openiddict.com/guides/getting-started/implementing-token-validation-in-your-apis

---

## Architectural Patterns and Design

### System Architecture: Two-Project Solution Layout

```
Blinder.sln
├── backend/
│   ├── Blinder.IdentityServer/          ← NEW: OpenIddict OAuth2 authorization server
│   │   ├── Controllers/
│   │   │   └── OAuth2Controller.cs      ← ROPC + refresh + auth code grant handling
│   │   ├── Infrastructure/
│   │   │   ├── Auth/
│   │   │   │   └── ISocialLoginTokenValidator.cs
│   │   │   └── Data/
│   │   │       └── IdentityDbContext.cs ← IdentityDbContext + UseOpenIddict()
│   │   ├── Migrations/                  ← OpenIddict + Identity EF Core migrations
│   │   └── Program.cs                   ← OpenIddict server + Identity DI setup
│   │
│   └── Blinder.Api/                     ← EXISTING: Resource server
│       └── Program.cs                   ← AddOpenIddictValidation (remote, OIDC discovery)
│
└── mobile/                              ← Unchanged
```

**Key design decisions:**
- `Blinder.IdentityServer` owns: user store (`ApplicationUser`), OpenIddict server, EF Core migrations for both Identity and OpenIddict tables
- `Blinder.Api` owns: resource endpoints, remote token validation only — no user management
- No shared library needed for MVP: API discovers signing keys via `.well-known/openid-configuration`
- CORS required: `Blinder.Api` → `Blinder.IdentityServer` token endpoint (for mobile app direct calls to identity server, no CORS needed; CORS needed only if a browser-based client calls the identity server)

_Source:_ https://github.com/openiddict/openiddict-samples (Zirku sample — two-project auth server + API), https://medium.com/@faruk.akyapak/building-a-centralized-authentication-and-authorization-system-with-openid-in-net-2101cb531356

---

### Design Principles and Best Practices

**Single Responsibility:** `Blinder.IdentityServer` is the single source of truth for token issuance. `Blinder.Api` never issues tokens.

**DbContext strategy:** `IdentityDbContext` in `Blinder.IdentityServer` with `UseOpenIddict()` — manages all four OpenIddict tables plus ASP.NET Core Identity tables in one schema. `Blinder.Api` has no user tables.

**Signing key sharing:** Identity server publishes public keys via JWKS (`/.well-known/jwks`). API fetches them on startup via OIDC discovery — no out-of-band key distribution required.

**Secrets management:** Signing/encryption certificates stored as environment variables or Key Vault references. Never in `appsettings.json`. Development uses `AddDevelopmentSigningCertificate()`.

_Source:_ https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html

---

### Security Architecture Patterns

#### Token Lifetime Recommendations (vs Story 2-0)

| Token | Story 2-0 (current) | Research Recommendation | Rationale |
|---|---|---|---|
| Access token | 30 days | **15 minutes** | Industry standard for remote validation without introspection; minimises compromise window |
| Refresh token | 90 days | **30 days rolling** | OpenIddict rotation on every use; 90 days acceptable with rotation, 30 days tighter |

**Decision for Story 2-0:** Change access token expiry from 30 days → **15 minutes**. Refresh token stays at 30 days (reduced from 90 days) with OpenIddict's built-in rolling rotation.

_Source:_ https://duendesoftware.com/learn/best-practices-using-jwts-with-web-and-mobile-apps, https://www.oauth.com/oauth2-servers/access-tokens/access-token-lifetime/, https://will.townsend.io/2024/jwt-tokens-mobile-apps

#### ROPC Security Posture (Important)

**OAuth 2.1 formally deprecates ROPC.** The security community (OWASP, Microsoft, Scott Brady) recommends against it even for first-party apps — primary concern is that it prevents MFA adoption and exposes credentials to the client.

**However:** Story 2-0's ROPC usage (email/password login from Blinder's own mobile app) is the most common legitimate exception, and remains widely deployed in production in 2025.

**Pragmatic position for Blinder:**
- Keep ROPC for the email/password flow (Stories 2-1/2-2) as planned
- Accept the trade-off explicitly: document that ROPC blocks future MFA
- Plan an architectural upgrade path: Authorization Code + PKCE for future MFA support
- If MFA becomes a requirement in a future sprint, ROPC must be replaced

_Source:_ https://blog.logto.io/deprecated-ropc-grant-type, https://www.scottbrady.io/oauth/why-the-resource-owner-password-credentials-grant-type-is-not-authentication-nor-suitable-for-modern-applications, https://cheatsheetseries.owasp.org/cheatsheets/OAuth2_Cheat_Sheet.html

#### Rate Limiting Architecture

OpenIddict has no built-in brute-force protection. Layer ASP.NET Core's native rate limiting middleware:

```csharp
// In Blinder.IdentityServer Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("token-endpoint", o =>
    {
        o.PermitLimit = 5;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
});

// Apply to token endpoint controller action
[EnableRateLimiting("token-endpoint")]
[HttpPost("/api/auth/oauth/token")]
public async Task<IActionResult> Exchange() { ... }
```

_Source:_ https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0

---

### Scalability and Performance Patterns

- **Stateless access tokens** (JWT, 15 min): API validates via cached JWKS keys — zero DB calls per request
- **OpenIddict token DB** used only for: refresh token lookup, revocation status, authorization code validation
- **OIDC discovery caching**: `UseSystemNetHttp()` caches signing keys; near-zero overhead after warmup
- **Horizontal scaling**: Both `Blinder.IdentityServer` and `Blinder.Api` are stateless — scale independently
- **Single DB bottleneck**: OpenIddict token table is the only shared state; index on `Subject` + `Status` for refresh token lookups (OpenIddict does this automatically)

---

### Deployment and Operations Architecture

| Concern | Identity Server | API |
|---|---|---|
| Deployment | Separate container / App Service | Separate container / App Service |
| Database | Shared PostgreSQL (Identity + OpenIddict tables) | No auth DB — reads from token |
| Public endpoint | Yes — mobile app calls `/api/auth/oauth/token` directly | Yes — mobile app calls resource endpoints |
| CORS | Configure for any browser clients | Not needed for mobile |
| Certificates | Signing + encryption certs (Key Vault) | Not needed — keys fetched via JWKS |
| Migrations | Owns all Identity + OpenIddict EF migrations | No auth migrations |

_Source:_ https://iamjeremie.me/post/2025-01/openiddict-dotnet-core-8-app-on-azure/, https://documentation.openiddict.com/guides/getting-started/integrating-with-a-remote-server-instance

---

## Implementation Approaches and Technology Adoption

### NuGet Package List — `Blinder.IdentityServer`

```xml
<!-- OAuth2 server -->
<PackageReference Include="OpenIddict.AspNetCore" Version="7.4.0" />
<PackageReference Include="OpenIddict.EntityFrameworkCore" Version="7.4.0" />

<!-- User management -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.*" />

<!-- EF Core + PostgreSQL -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.*" />

<!-- Validation -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.*" />
```

### NuGet Package List — `Blinder.Api` (changes only)

```xml
<!-- Replace or supplement JwtBearer with OpenIddict remote validation -->
<PackageReference Include="OpenIddict.Validation.AspNetCore" Version="7.4.0" />
<PackageReference Include="OpenIddict.Validation.SystemNetHttp" Version="7.4.0" />
<!-- Remove: Microsoft.AspNetCore.Authentication.JwtBearer (if previously added) -->
```

_Source:_ https://www.nuget.org/packages/OpenIddict.AspNetCore, https://www.nuget.org/packages/OpenIddict.EntityFrameworkCore

---

### Revised Story 2-0 Task Breakdown

Mapping original tasks against OpenIddict — what collapses, what remains.

| Original Task | Status | Replacement |
|---|---|---|
| Task 1: `OAuth2TokenService.cs` (custom JWT issuance) | ❌ Removed | OpenIddict server handles all token issuance |
| Task 2: `ResourceOwnerPasswordCredentialsGrantHandler.cs` | ✅ Reduced | ~30 lines in `OAuth2Controller.Exchange()` using `UserManager` |
| Task 3: `AuthorizationCodeGrantHandler.cs` + `ISocialLoginTokenValidator` | ✅ Reduced | Interface + ~40 lines in controller; OpenIddict handles code lifecycle |
| Task 4: `RefreshTokenGrantHandler.cs` + `RefreshTokenRepository.cs` + migration | ❌ Removed | OpenIddict EF store + automatic rotation |
| Task 5: `TokenRevocationHandler.cs` | ❌ Removed | Built-in `/connect/revoke` endpoint |
| Task 6: `OAuth2Controller.cs` wiring | ✅ Simplified | One controller, passthrough mode, no manual routing logic |
| Task 7: Mobile `storageService.ts` | ✅ Unchanged | As planned |
| Task 8: Rate limiting | ✅ Unchanged | `AddRateLimiter()` on identity server |
| Task 9: Tests | ✅ Changed | Integration tests via `WebApplicationFactory` + in-memory EF Core |
| Task 10: Documentation | ✅ Updated | Document OpenIddict setup, cert config, two-project topology |
| **NEW Task**: `Blinder.IdentityServer` project scaffold | ➕ Added | New ASP.NET Core Web App project, Program.cs, DbContext, migrations |
| **NEW Task**: Client application seeding | ➕ Added | `IHostedService` to register `blinder-mobile` client on startup |
| **NEW Task**: `Blinder.Api` validation wiring | ➕ Added | Replace JwtBearer with `AddOpenIddictValidation` + remote issuer |

---

### Client Application Seeding (Required — Common Gotcha)

OpenIddict will return `invalid_client` on every token request until the client app is registered. Seed it via `IHostedService`:

```csharp
public class OpenIddictSeeder : IHostedService
{
    private readonly IServiceProvider _provider;
    public OpenIddictSeeder(IServiceProvider provider) => _provider = provider;

    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = _provider.CreateAsyncScope();
        var manager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("blinder-mobile", ct) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "blinder-mobile",
                // Public client — no secret (mobile ROPC)
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Revocation,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                }
            }, ct);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

Register: `builder.Services.AddHostedService<OpenIddictSeeder>();`

_Source:_ https://dev.to/robinvanderknaap/setting-up-an-authorization-server-with-openiddict-part-iii-client-credentials-flow-55lp, https://documentation.openiddict.com/guides/getting-started/creating-your-own-server-instance

---

### Integration Testing Pattern

```csharp
public class OAuth2TokenEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OAuth2TokenEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
            builder.ConfigureServices(services =>
            {
                // Swap to in-memory EF Core — no migrations needed in tests
                services.RemoveAll<DbContextOptions<IdentityDbContext>>();
                services.AddDbContext<IdentityDbContext>(o =>
                    o.UseInMemoryDatabase("test")
                     .UseOpenIddict());

                // Disable HTTPS enforcement for tests
                services.Configure<OpenIddictServerOptions>(o =>
                    o.DisableTransportSecurityRequirement());
            }))
            .CreateClient();
    }

    [Fact]
    public async Task ROPC_ValidCredentials_ReturnsTokenPair()
    {
        var response = await _client.PostAsync("/api/auth/oauth/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = "test@blinder.app",
                ["password"] = "ValidPass1!",
                ["client_id"] = "blinder-mobile"
            }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("access_token").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("refresh_token").GetString().Should().NotBeNullOrEmpty();
    }
}
```

_Source:_ https://renatogolia.com/2025/08/01/testing-aspnet-core-endpoints-with-fake-jwt-tokens-and-webapplicationfactory/

---

### Known Pitfalls Checklist

- [ ] Seed `blinder-mobile` client before any token requests (use `IHostedService`)
- [ ] Call `UseOpenIddict()` on `DbContext` options — not just `UseEntityFrameworkCore()`
- [ ] Middleware order: `UseCors()` → `UseAuthentication()` → `UseAuthorization()`
- [ ] Use `DisableTransportSecurityRequirement()` in development/test only
- [ ] Token endpoint requires `EnableTokenEndpointPassthrough()` to reach controller
- [ ] Use `SetDestinations()` — old 3-arg `AddClaim()` syntax removed in v4+
- [ ] `Blinder.Api` uses `OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme` on `[Authorize]`, not the default cookie scheme
- [ ] Run `dotnet ef migrations add` in `Blinder.IdentityServer`, not `Blinder.Api`

_Source:_ https://documentation.openiddict.com/integrations/aspnet-core, https://github.com/openiddict/openiddict-samples

---

## Research Synthesis

### Executive Summary

Story 2-0 as currently written plans to build ~10 tasks worth of custom OAuth2 infrastructure: a token service, five grant handlers, a refresh token repository with hashed storage, a revocation handler, and a custom authorization code framework. This research demonstrates that **OpenIddict 7.4 on .NET 10 provides all of this out-of-the-box**, reducing the story to scaffolding one new project, writing one ~100-line controller, and configuring a hosted service to seed the client application.

Two architectural decisions emerged from this research. First, the authorization server should be a **dedicated `Blinder.IdentityServer` ASP.NET Core Web Application**, not co-located with `Blinder.Api`. This is both the OpenIddict-recommended topology for non-trivial deployments and a clean separation of concerns. Second, **the 30-day access token expiry in Story 2-0 must change to 15 minutes**. With a separate identity server, the API validates tokens remotely via OIDC discovery — long-lived access tokens in this topology create an irrevocable window if a token is compromised. 15-minute access tokens with 30-day rolling refresh tokens (rotated on every use by OpenIddict automatically) is the correct and industry-standard pattern for this setup.

ROPC remains in the design as a conscious, documented trade-off. OAuth 2.1 formally deprecates it, but for a first-party mobile app with no current MFA requirement it is the most pragmatic path. The decision is recorded here so it does not become technical debt by accident.

**Key Technical Findings:**

- `Microsoft.Identity.Web` is for Microsoft Entra ID (Azure AD) consumption only — not applicable to Blinder's self-hosted server
- ASP.NET Core Identity's built-in `.NET 10` token endpoints issue proprietary non-JWT tokens — not RFC 6749 compliant
- OpenIddict 7.4 is free, MIT-licensed, .NET 10 compatible, and actively maintained through July 2027
- OpenIddict replaces Tasks 1, 4, and 5 of Story 2-0 entirely; Tasks 2, 3, and 6 reduce to ~1/4 of their original scope
- The `blinder-mobile` client application must be seeded via `IHostedService` before the token endpoint will accept any request — this is the #1 implementation gotcha

**Technical Recommendations:**

1. **Adopt OpenIddict 7.4** as the OAuth2 server framework for `Blinder.IdentityServer`
2. **Create `Blinder.IdentityServer`** as a new ASP.NET Core Web Application project in the solution
3. **Change access token expiry** from 30 days → 15 minutes in Story 2-0 AC2/AC8
4. **Change refresh token expiry** from 90 days → 30 days rolling (OpenIddict handles rotation automatically)
5. **Do not use `Microsoft.Identity.Web`** — it serves a different purpose and adds no value here

---

### Story 2-0 Rewrite Specification

The following changes should be applied to Story 2-0 before development begins.

#### Architectural Changes

**Add:** `Blinder.IdentityServer` — new ASP.NET Core Web Application project
**Modify:** `Blinder.Api` — replace JwtBearer with OpenIddict remote validation

#### Acceptance Criteria Changes

| AC | Change | From | To |
|---|---|---|---|
| AC1 | Endpoint host | `Blinder.Api` hosts `/api/auth/oauth/token` | `Blinder.IdentityServer` hosts `/api/auth/oauth/token` |
| AC2 | Access token expiry | 30-day expiry | **15-minute expiry** |
| AC4 | Refresh token expiry | 90-day expiry | **30-day rolling expiry** (rotation automatic via OpenIddict) |
| AC6 | Token storage mechanism | SHA-256 + manual salt | OpenIddict Data Protection encryption (automatic) |
| AC8 | JWT signing | RSA-256, custom config | OpenIddict signing certificate, same config pattern |

#### Task Changes

**Remove entirely:**
- Task 1: `OAuth2TokenService.cs` → replaced by OpenIddict server configuration
- Task 4: `RefreshTokenGrantHandler.cs`, `RefreshTokenRepository.cs`, EF migration → replaced by OpenIddict EF Core store
- Task 5: `TokenRevocationHandler.cs` → replaced by built-in `/connect/revoke`

**Reduce scope:**
- Task 2: `ResourceOwnerPasswordCredentialsGrantHandler` → 1 `if` block in `OAuth2Controller.Exchange()` using `UserManager.CheckPasswordAsync()`
- Task 3: `AuthorizationCodeGrantHandler` → `ISocialLoginTokenValidator` interface + ~40 lines in controller; OpenIddict owns code lifecycle
- Task 6: `OAuth2Controller` wiring → one passthrough controller; no manual token construction

**Add new tasks:**
- **Task NEW-1:** Scaffold `Blinder.IdentityServer` project (Web App, `IdentityDbContext`, `UseOpenIddict()`, EF migrations for Identity + OpenIddict tables)
- **Task NEW-2:** `OpenIddictSeeder` hosted service — register `blinder-mobile` client on startup with ROPC, refresh, and authorization code permissions
- **Task NEW-3:** Update `Blinder.Api` Program.cs — replace `AddAuthentication().AddJwtBearer()` with `AddOpenIddict().AddValidation(o => { o.SetIssuer(...); o.UseSystemNetHttp(); o.UseAspNetCore(); })`

#### Dev Notes Additions

```
### ROPC Trade-off (Documented Decision)
ROPC is retained for email/password login (Stories 2-1, 2-2). OAuth 2.1 deprecates ROPC.
This is a conscious decision for MVP: Blinder is a first-party app with no current MFA
requirement. If MFA is required in a future sprint, ROPC must be replaced with
Authorization Code + PKCE. Do not add ROPC without revisiting this note.

### Token Expiry Rationale
Access tokens are 15 minutes because Blinder.IdentityServer and Blinder.Api are
separate processes. The API validates tokens locally via cached JWKS keys — there is
no revocation check per request. A 30-day token that is compromised cannot be
invalidated until expiry. 15-minute tokens bound the compromise window to acceptable
levels. Refresh tokens are 30 days rolling with automatic rotation (OpenIddict
marks old tokens as `redeemed` and rejects replays).

### OpenIddict Not a Turnkey Solution
OpenIddict is a framework, not a drop-in server. You must implement OAuth2Controller
with passthrough mode. Clone the Hollastin or Zirku samples from
https://github.com/openiddict/openiddict-samples as a reference starting point.
```

---

### Source Index

| Source | URL | Used For |
|---|---|---|
| OpenIddict Documentation | https://documentation.openiddict.com | All OpenIddict configuration and patterns |
| OpenIddict Samples (Zirku) | https://github.com/openiddict/openiddict-samples | Two-project topology reference |
| OpenIddict NuGet | https://www.nuget.org/packages/OpenIddict.AspNetCore | Package versions |
| Kévin Chalet Blog | https://kevinchalet.com | OpenIddict maintainer — authoritative source |
| ASP.NET Core .NET 10 What's New | https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0 | .NET 10 identity changes |
| JWT Bearer .NET 10 | https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication | JwtBearer vs OpenIddict validation |
| OAuth.com Token Lifetime | https://www.oauth.com/oauth2-servers/access-tokens/access-token-lifetime/ | Access token expiry guidance |
| Duende JWT Best Practices | https://duendesoftware.com/learn/best-practices-using-jwts-with-web-and-mobile-apps | Mobile token lifetime |
| Scott Brady ROPC | https://www.scottbrady.io/oauth/why-the-resource-owner-password-credentials-grant-type-is-not-authentication-nor-suitable-for-modern-applications | ROPC security posture |
| ASP.NET Core Rate Limiting | https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-10.0 | Rate limiting on token endpoint |
| OpenIddict Token Storage | https://documentation.openiddict.com/configuration/token-storage | Refresh token encryption |
| OpenIddict Signing Credentials | https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html | RSA cert configuration |

---

**Research Completed:** 2026-03-25
**Research Period:** Current — all sources from 2024–2026
**Technical Confidence Level:** High — verified across official docs, NuGet, and multiple independent sources
**Decision Status:** Architectural decisions confirmed by Piotr.palej during research session

_This document serves as the technical reference for Story 2-0 refactoring and the `Blinder.IdentityServer` project creation._
