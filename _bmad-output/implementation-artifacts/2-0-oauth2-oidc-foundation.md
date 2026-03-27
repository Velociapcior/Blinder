# Story 2.0: OAuth2/OIDC Authentication Foundation (OpenIddict)

Status: ready-for-dev

## Story

As a developer and operator,
I want a complete OAuth2/OIDC token endpoint and grant handler infrastructure powered by OpenIddict,
So that all authentication flows (email/password, social login, future integrations) are built on a single, secure, standards-compliant foundation without reinventing what OpenIddict already provides.

## Context: Why This Story Exists After Story 2-1

Story 2-0 is being implemented **after** Story 2-1 (registration). This is intentional: registration was scaffolded first to unblock mobile development; Story 2-0 now establishes the token issuance infrastructure that Story 2-1's registration flow hands off to (registration returns 201, no token — token issuance is Story 2-0's responsibility). Story 2-2 (login) depends on this story and must be updated after 2-0 is done.

---

## Acceptance Criteria

### 1. OAuth2 Token Endpoint Operational (Blinder.IdentityServer)

**Given** a `POST /api/auth/oauth/token` request with valid parameters
**When** the token endpoint processes the request
**Then** the response returns: `access_token`, `refresh_token` (where applicable), `expires_in`, `token_type: "Bearer"` (RFC 6749 compliant)
**And** the endpoint is hosted in `Blinder.IdentityServer`, not `Blinder.Api`

---

### 2. Resource Owner Password Credentials (ROPC) Grant Implemented

**Given** a `POST /api/auth/oauth/token` request with `grant_type=password`, `client_id=blinder-mobile`, `username` (email), `password`
**When** credentials are validated via Identity `UserManager`
**Then**
- Access token is issued with **15-minute expiry** (not 30 days — see Dev Notes: Token Expiry Rationale)
- Refresh token is issued with **30-day rolling expiry** (OpenIddict rotation handles one-time use automatically)
- Both tokens returned in response
- On invalid credentials: 401 Unauthorized with RFC 6749 `invalid_grant` error response

---

### 3. Authorization Code Grant Framework Ready for Social Login

**Given** a `POST /api/auth/oauth/token` request with `grant_type=authorization_code`, `code`, `client_id`
**When** the grant handler processes the request
**Then**
- OpenIddict validates authorization code (one-time use, 10-min expiry) automatically
- `ISocialLoginTokenValidator` interface is defined for provider variance (Apple, Google, Facebook)
- Access token + refresh token issued on success
- Framework is extensible: Stories 2-3/2-4 plug in their `ISocialLoginTokenValidator` implementations — no changes to this story's controller required

---

### 4. Refresh Token Flow Implemented (OpenIddict-managed)

**Given** a `POST /api/auth/oauth/token` request with `grant_type=refresh_token`, `refresh_token`
**When** the refresh handler processes the request
**Then**
- OpenIddict validates and rotates refresh token automatically (marks old as `redeemed`, issues new)
- Replay attacks are rejected: a `redeemed` token reused returns `invalid_grant`
- New access token (15-min) + new refresh token (30-day) issued
- No custom `RefreshTokenRepository` needed — OpenIddict EF Core store manages all of this

---

### 5. Token Revocation Endpoint Operational

**Given** a `POST /api/auth/oauth/revoke` request with the token to revoke
**When** the revocation handler processes the request
**Then**
- OpenIddict's built-in revocation endpoint handles the request
- Specified token is marked `revoked` in the `OpenIddictTokens` table
- Revocation is idempotent
- Returns 200 OK

---

### 6. Refresh Token Storage Secured (OpenIddict Data Protection)

**Given** a refresh token is issued
**When** the token is persisted to database
**Then**
- Token payload is encrypted via ASP.NET Core Data Protection (automatic via OpenIddict) — never plaintext
- Stored in `OpenIddictTokens` table with: subject (user ID), status, creation/expiry dates
- No manual SHA-256 hashing or `RefreshTokens` table — OpenIddict's EF Core store handles this entirely

---

### 7. Mobile Token Storage Contract Defined

**Given** the mobile client receives tokens from the OAuth2 token endpoint
**When** `storageService.ts` persists tokens
**Then**
- `access_token` stored via `SecureStore.setItemAsync("access_token", ...)`
- `refresh_token` stored via `SecureStore.setItemAsync("refresh_token", ...)`
- Token expiry timestamp (from `expires_in` claim, converted to UTC milliseconds) stored for pre-refresh logic
- `AsyncStorage` is never used for auth tokens
- Methods provided: `getAccessToken()`, `getRefreshToken()`, `setTokens(access, refresh, expiresIn)`, `clearTokens()`

---

### 8. JWT Structure and Claims Standardized

**Given** all JWTs issued by the OpenIddict token endpoint
**When** a JWT is decoded
**Then**
- JWT includes claims: `sub` (user ID), `iss` (issuer = IdentityServer URL), `aud` ("blinder-mobile"), `exp`, `iat`
- All JWTs signed with RSA-256 signing certificate (from configuration — never hardcoded)
- Development: `options.AddDevelopmentSigningCertificate().AddDevelopmentEncryptionCertificate()`
- Production: certificate loaded from env variable base64 value (see Dev Notes: RSA Signing)

---

### 9. `blinder-mobile` Client Application Seeded

**Given** `Blinder.IdentityServer` starts up
**When** `OpenIddictSeeder` hosted service runs
**Then**
- `blinder-mobile` client application is registered if not already present
- Client has permissions for: Token endpoint, Revocation endpoint, Password grant, Refresh token grant, Authorization code grant
- Client is a public client (no secret — mobile ROPC)
- **Without this seeder, every token request returns `invalid_client` — this is the #1 gotcha**

---

### 10. `Blinder.Api` Validates Tokens Remotely via OIDC Discovery

**Given** a request arrives at `Blinder.Api` with an `Authorization: Bearer` header
**When** OpenIddict validation middleware processes the token
**Then**
- Token is validated against `Blinder.IdentityServer`'s OIDC discovery endpoint (`.well-known/openid-configuration`)
- Signing keys are fetched and cached via `UseSystemNetHttp()` — no per-request network call after warmup
- `JwtBearer` middleware is NOT used in `Blinder.Api` — OpenIddict remote validation is used instead (JwtBearer was never configured; it was deferred to Story 2.2 and is now replaced by this approach)
- Invalid/expired tokens return 401 Unauthorized Problem Details

---

### 11. Rate Limiting on Token Endpoint

**Given** malicious actors attempt credential stuffing
**When** a client makes repeated token requests
**Then**
- Token endpoint enforces rate limit via ASP.NET Core `AddRateLimiter()` (fixed window: 5 requests per minute per IP)
- Returns 429 Too Many Requests on limit exceeded
- Applied via `[EnableRateLimiting("token-endpoint")]` attribute on the controller action

---

## Tasks / Subtasks

### Task 1: Create `Blinder.IdentityServer` Project (AC: 1, 8)

- [ ] `dotnet new webapi -n Blinder.IdentityServer --use-controllers --framework net10.0`
- [ ] Add to `Blinder.sln`: `dotnet sln add backend/Blinder.IdentityServer/Blinder.IdentityServer.csproj`
- [ ] Add packages to `Blinder.IdentityServer`:
  ```xml
  <!-- OpenIddict server -->
  <PackageReference Include="OpenIddict.AspNetCore" Version="7.4.0" />
  <PackageReference Include="OpenIddict.EntityFrameworkCore" Version="7.4.0" />
  <!-- Shared user store from Blinder.Api -->
  <ProjectReference Include="../Blinder.Api/Blinder.Api.csproj" />
  <!-- Serilog (same as Blinder.Api) -->
  <PackageReference Include="Serilog.AspNetCore" Version="9.*" />
  ```
- [ ] Create `Infrastructure/Data/OpenIddictDbContext.cs` — a **separate, minimal DbContext for OpenIddict tables only**:
  ```csharp
  // This DbContext manages ONLY OpenIddict tables (4 tables).
  // ApplicationUser/Identity tables stay in Blinder.Api's AppDbContext.
  // Both point to the same PostgreSQL database — no table duplication.
  public class OpenIddictDbContext : DbContext
  {
      public OpenIddictDbContext(DbContextOptions<OpenIddictDbContext> options)
          : base(options) { }

      protected override void OnModelCreating(ModelBuilder builder)
      {
          base.OnModelCreating(builder);
          builder.UseOpenIddict(); // Only maps OpenIddict tables
          builder.HasDefaultSchema("public");
      }
  }
  ```
  **Why separate context?** `Blinder.Api`'s `AppDbContext` already manages AspNetUsers/Identity tables (Story 2-1 done). Using `AppDbContext` directly in IdentityServer would conflict EF migration histories between projects. A dedicated `OpenIddictDbContext` keeps OpenIddict migrations scoped to `Blinder.IdentityServer/Migrations/`.

- [ ] Configure `Program.cs` with **two separate DbContexts**:
  ```csharp
  var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

  // DbContext 1: AppDbContext (from Blinder.Api) — for Identity user store (UserManager)
  builder.Services.AddDbContextPool<AppDbContext>(options =>
      options.UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
             .UseSnakeCaseNamingConvention());

  // DbContext 2: OpenIddictDbContext — for OpenIddict token/auth tables only
  builder.Services.AddDbContextPool<OpenIddictDbContext>(options =>
      options.UseNpgsql(connectionString)
             .UseSnakeCaseNamingConvention()
             .UseOpenIddict());  // tells OpenIddict to manage its 4 tables here
  ```

- [ ] Add `IdentityRole<Guid>` — **must match `Blinder.Api/Program.cs` exactly** which uses `IdentityRole<Guid>`:
  ```csharp
  // CRITICAL: Blinder.Api uses IdentityRole<Guid> (not default IdentityRole with string key)
  builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
      .AddEntityFrameworkStores<AppDbContext>(); // Identity users live in Blinder.Api's AppDbContext
  ```

- [ ] Add to OpenIddict server configuration (see Dev Notes: DI Wiring Pattern)
- [ ] Add `Blinder.IdentityServer` to `docker-compose.yml`:
  - New container `identityserver` with its own port (e.g., 5001 internal)
  - Same `db` service connection string
- [ ] **Add Nginx routing** to `nginx/nginx.conf`:
  ```nginx
  # OAuth2/OIDC token endpoints → Blinder.IdentityServer
  location /api/auth/oauth/ {
      proxy_pass http://identityserver:5001;
      proxy_http_version 1.1;
      proxy_set_header Host $host;
      proxy_set_header X-Real-IP $remote_addr;
  }

  # Well-known OpenID config → Blinder.IdentityServer
  location /.well-known/ {
      proxy_pass http://identityserver:5001;
  }

  # All other /api/ → Blinder.Api
  location /api/ {
      proxy_pass http://api:5000;
  }
  ```
  **Why this matters:** Mobile calls a single base URL (Nginx). Nginx routes `/api/auth/oauth/` to IdentityServer and all other `/api/` to Blinder.Api. Mobile never directly addresses either container.

- [ ] Generate EF Core migration for `OpenIddictDbContext`:
  ```bash
  # Run inside backend/Blinder.IdentityServer/ directory
  dotnet ef migrations add AddOpenIddictSchema --context OpenIddictDbContext
  dotnet ef migrations script --idempotent --context OpenIddictDbContext -o ../../migrations/latest-identity.sql
  ```
  This adds 4 OpenIddict tables: `OpenIddictApplications`, `OpenIddictAuthorizations`, `OpenIddictTokens`, `OpenIddictScopes`.
  **CRITICAL: Run `dotnet ef` in `Blinder.IdentityServer/`, NOT `Blinder.Api/`**

---

### Task 2: Implement `OpenIddictSeeder` Hosted Service (AC: 9)

- [ ] Create `Infrastructure/Auth/OpenIddictSeeder.cs`:
  ```csharp
  public class OpenIddictSeeder : IHostedService
  {
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
- [ ] Register: `builder.Services.AddHostedService<OpenIddictSeeder>();`

---

### Task 3: Implement `OAuth2Controller` with Passthrough Mode (AC: 1, 2, 3, 4, 5)

- [ ] Create `Controllers/Auth/OAuth2Controller.cs`:
  ```csharp
  [ApiController]
  public class OAuth2Controller : ControllerBase
  {
      [HttpPost("/api/auth/oauth/token")]
      [EnableRateLimiting("token-endpoint")]
      public async Task<IActionResult> Exchange()
      {
          var request = HttpContext.GetOpenIddictServerRequest()
              ?? throw new InvalidOperationException("OpenIddict server request not found.");

          if (request.IsPasswordGrantType())  // ROPC (AC2)
          {
              var user = await _userManager.FindByEmailAsync(request.Username!);
              if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password!))
                  return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

              var identity = new ClaimsIdentity(
                  OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                  Claims.Name, Claims.Role);

              identity.SetClaim(Claims.Subject, user.Id)
                      .SetClaim(Claims.Email, user.Email)
                      .SetDestinations(GetDestinations);

              return SignIn(new ClaimsPrincipal(identity),
                  OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
          }

          if (request.IsRefreshTokenGrantType())  // Refresh (AC4) — OpenIddict handles rotation
          {
              var result = await HttpContext.AuthenticateAsync(
                  OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
              return SignIn(result.Principal!,
                  OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
          }

          if (request.IsAuthorizationCodeGrantType())  // Social login (AC3, AC9)
          {
              // OpenIddict already validated: code is one-time use, not expired
              // Plug in ISocialLoginTokenValidator in Stories 2-3/2-4
              var result = await HttpContext.AuthenticateAsync(
                  OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
              return SignIn(result.Principal!,
                  OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
          }

          return BadRequest();
      }
  }
  ```
- [ ] Implement `GetDestinations()` helper — claims must declare destinations:
  ```csharp
  private static IEnumerable<string> GetDestinations(Claim claim) =>
      claim.Type switch
      {
          Claims.Subject or Claims.Email => new[] { Destinations.AccessToken, Destinations.IdentityToken },
          _ => new[] { Destinations.AccessToken }
      };
  ```
  **NOTE:** Using old 3-arg `AddClaim(type, value, destination)` is REMOVED in OpenIddict v4+. Always use `SetDestinations()`.

---

### Task 4: Define `ISocialLoginTokenValidator` Interface (AC: 3, 9)

- [ ] Create `Infrastructure/Auth/ISocialLoginTokenValidator.cs`:
  ```csharp
  public interface ISocialLoginTokenValidator
  {
      string ProviderName { get; }
      Task<SocialLoginPrincipal?> ValidateAsync(string idToken, CancellationToken ct);
  }

  public record SocialLoginPrincipal(string ProviderId, string Email, string? DisplayName);
  ```
- [ ] This interface is the only addition needed — Stories 2-3/2-4 implement it
- [ ] Stub `TestSocialLoginTokenValidator` for integration tests

---

### Task 5: Add OpenIddict Remote Token Validation to `Blinder.Api` (AC: 10)

**Current state in `Blinder.Api/Program.cs`:** `UseAuthentication()` and `UseAuthorization()` are already in the middleware pipeline. JwtBearer was NOT configured yet — the comment says "JWT Bearer auth → Story 2.2". Story 2-0 replaces that plan: instead of JwtBearer in 2.2, we add OpenIddict remote validation here.

- [ ] Add packages to `Blinder.Api/Blinder.Api.csproj`:
  ```xml
  <PackageReference Include="OpenIddict.Validation.AspNetCore" Version="7.4.0" />
  <PackageReference Include="OpenIddict.Validation.SystemNetHttp" Version="7.4.0" />
  ```
- [ ] Add to `Blinder.Api/Program.cs` (in the services section, replace the deferred comment):
  ```csharp
  // --------------------------------------------------------------------
  // OpenIddict remote token validation
  // Tokens issued by Blinder.IdentityServer; validated here via OIDC discovery.
  // JwtBearer is NOT used — OpenIddict validation replaces it.
  // --------------------------------------------------------------------
  builder.Services.AddOpenIddict()
      .AddValidation(options =>
      {
          options.SetIssuer(builder.Configuration["Auth:IdentityServerUrl"]!);
          options.UseSystemNetHttp();   // fetches .well-known/openid-configuration, caches JWKS
          options.UseAspNetCore();
      });

  builder.Services.AddAuthentication(
      OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
  ```
- [ ] Add `Auth:IdentityServerUrl` to `appsettings.json`:
  ```json
  "Auth": {
    "IdentityServerUrl": "http://identityserver:5001"
  }
  ```
- [ ] `UseAuthentication()` and `UseAuthorization()` already exist in the middleware pipeline — no change needed there
- [ ] Remove the deferred comment `// JWT Bearer auth → Story 2.2` from `Program.cs`
- [ ] Any future `[Authorize]` attributes in `Blinder.Api` will now use `OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme` automatically since it is set as the default scheme above

---

### Task 6: Rate Limiting on Token Endpoint (AC: 11)

- [ ] Add to `Blinder.IdentityServer/Program.cs`:
  ```csharp
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
  // In middleware pipeline:
  app.UseRateLimiter();
  ```
- [ ] Apply `[EnableRateLimiting("token-endpoint")]` on `OAuth2Controller.Exchange()` (already shown in Task 3)

---

### Task 7: Mobile Token Storage Contract (AC: 7)

- [ ] Update `mobile/services/storageService.ts`:
  ```typescript
  export const storageService = {
    async getAccessToken(): Promise<string | null> {
      return SecureStore.getItemAsync('access_token');
    },
    async getRefreshToken(): Promise<string | null> {
      return SecureStore.getItemAsync('refresh_token');
    },
    async setTokens(
      accessToken: string,
      refreshToken: string,
      expiresIn: number
    ): Promise<void> {
      const expiry = Date.now() + expiresIn * 1000;
      await Promise.all([
        SecureStore.setItemAsync('access_token', accessToken),
        SecureStore.setItemAsync('refresh_token', refreshToken),
        SecureStore.setItemAsync('token_expiry', String(expiry)),
      ]);
    },
    async clearTokens(): Promise<void> {
      await Promise.all([
        SecureStore.deleteItemAsync('access_token'),
        SecureStore.deleteItemAsync('refresh_token'),
        SecureStore.deleteItemAsync('token_expiry'),
      ]);
    },
    async isTokenExpiringSoon(): Promise<boolean> {
      const expiry = await SecureStore.getItemAsync('token_expiry');
      if (!expiry) return true;
      const fiveMinutes = 5 * 60 * 1000;
      return Date.now() > Number(expiry) - fiveMinutes;
    },
  };
  ```
- [ ] Scan codebase: ensure no `AsyncStorage` import in `storageService.ts` or `authService.ts`

---

### Task 8: Update `docs/project-context.md` Section 18 (Required)

Section 18 of `docs/project-context.md` was written based on the OLD Story 2-0 design (custom `OAuth2TokenService`, SHA-256 hashing, 30-day tokens). It MUST be updated to reflect OpenIddict:

- [ ] **Remove** rules 19–24 (all reference `OAuth2TokenService`, SHA-256 hashing, manual `RefreshTokens` table)
- [ ] **Replace** with:
  - Rule 19: OpenIddict is the single source of all token issuance — no ad-hoc JWT generation
  - Rule 20: Access tokens are 15 minutes (JWKS-validated, no revocation check per request); refresh tokens are 30 days rolling (OpenIddict rotation)
  - Rule 21: Refresh tokens stored encrypted via OpenIddict Data Protection — never plaintext, no manual `RefreshTokens` table
  - Rule 22: Token revocation via `POST /api/auth/oauth/revoke` (OpenIddict built-in)
  - Rule 23: Social login uses `ISocialLoginTokenValidator` — same OAuth2 token endpoint as ROPC
  - Rule 24: `Blinder.Api` validates tokens remotely via OIDC discovery (`.well-known/openid-configuration`) — no `JwtBearer` middleware

---

### Task 9: Comprehensive Tests (AC: 1–11)

- [ ] Integration tests for `Blinder.IdentityServer` using `WebApplicationFactory<Program>` + in-memory EF:
  ```csharp
  // In-memory setup (from research):
  services.RemoveAll<DbContextOptions<IdentityDbContext>>();
  services.AddDbContext<IdentityDbContext>(o =>
      o.UseInMemoryDatabase("test").UseOpenIddict());
  services.Configure<OpenIddictServerOptions>(o =>
      o.DisableTransportSecurityRequirement()); // tests run over HTTP
  ```
- [ ] Test: ROPC valid credentials → returns `access_token` + `refresh_token`
- [ ] Test: ROPC invalid password → 401 `invalid_grant`
- [ ] Test: Refresh token rotation → old token is `redeemed`, new pair returned
- [ ] Test: Refresh token replay → second use of same token returns 401
- [ ] Test: Token revocation → revoked token rejected on subsequent request
- [ ] Test: Rate limiting → 6th request in 60s returns 429
- [ ] Test: `blinder-mobile` client not seeded → `invalid_client` (validates seeder logic)
- [ ] Mobile tests: `storageService.ts` uses SecureStore only (mock `expo-secure-store`)

---

### Task 10: Documentation & Environment Variables

- [ ] Add to `.env.example`:
  ```
  # Identity Server
  IDENTITY_SERVER_URL=http://identityserver:5001
  AUTH_SIGNING_CERT_BASE64=  # RSA-256 signing cert, base64-encoded (production)
  AUTH_SIGNING_CERT_PASSWORD= # Cert password (production)
  AUTH_ENCRYPTION_CERT_BASE64=  # Encryption cert, base64-encoded (production)
  AUTH_ENCRYPTION_CERT_PASSWORD= # Cert password (production)
  ```
- [ ] Create `docs/authentication.md` (or update if exists):
  - OAuth2 token endpoint contract (grant types, request/response formats)
  - Two-project topology diagram (IdentityServer ↔ Api ↔ Mobile)
  - Token lifetime rationale (15-min access + 30-day rolling refresh)
  - RSA certificate setup for dev (AddDevelopmentSigningCertificate) and production
  - `blinder-mobile` client seeding explanation
- [ ] Keep `README.md` high-level — detailed auth content goes in `docs/authentication.md`

---

## Dev Notes

### Architecture: Two-Project Solution Layout

```
Blinder.sln
├── backend/
│   ├── Blinder.IdentityServer/        ← NEW: OpenIddict OAuth2 authorization server
│   │   ├── Controllers/
│   │   │   └── OAuth2Controller.cs    ← ROPC + refresh + auth code (passthrough)
│   │   ├── Infrastructure/
│   │   │   ├── Auth/
│   │   │   │   ├── ISocialLoginTokenValidator.cs
│   │   │   │   └── OpenIddictSeeder.cs
│   │   │   └── Data/
│   │   │       └── OpenIddictDbContext.cs  ← Manages OpenIddict tables ONLY (4 tables)
│   │   ├── Migrations/                ← EF migrations for OpenIddict tables only
│   │   └── Program.cs                 ← Two DbContexts: AppDbContext (Identity) + OpenIddictDbContext
│   │                                     References Blinder.Api.csproj for ApplicationUser + AppDbContext
│   │
│   ├── Blinder.Api/                   ← EXISTING: Resource server
│   │   ├── Models/ApplicationUser.cs  ← Stays here; IdentityServer references this project
│   │   ├── Infrastructure/Data/
│   │   │   └── AppDbContext.cs        ← Stays here; manages Identity + domain tables
│   │   └── Program.cs                 ← ADD OpenIddict remote validation (was deferred to 2.2)
│   │
│   └── Blinder.Tests/                 ← Tests for both projects
│
├── migrations/
│   ├── latest.sql                     ← From Blinder.Api (Identity + domain tables — existing)
│   └── latest-identity.sql            ← From Blinder.IdentityServer (OpenIddict tables only — NEW)
│
└── mobile/
    └── services/storageService.ts     ← Updated with full token contract
```

### DI Wiring Pattern (Blinder.IdentityServer Program.cs)

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext 1: AppDbContext (from Blinder.Api) — Identity user store (UserManager reads from here)
builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
           .UseSnakeCaseNamingConvention());

// DbContext 2: OpenIddictDbContext — manages ONLY the 4 OpenIddict tables
builder.Services.AddDbContextPool<OpenIddictDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention()
           .UseOpenIddict());

// Identity — MUST use IdentityRole<Guid> to match Blinder.Api (not default IdentityRole string key)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>();

// OpenIddict server — uses OpenIddictDbContext (NOT AppDbContext)
builder.Services.AddOpenIddict()
    .AddCore(options =>
        options.UseEntityFrameworkCore()
               .UseDbContext<OpenIddictDbContext>())  // OpenIddict tables only
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/api/auth/oauth/token");
        options.SetRevocationEndpointUris("/api/auth/oauth/revoke");

        options.AllowPasswordFlow()          // ROPC (AC2)
               .AllowRefreshTokenFlow()      // Refresh (AC4)
               .AllowAuthorizationCodeFlow();// Social login (AC3)

        // Access token: 15 minutes; refresh token: 30 days
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(30));

        if (builder.Environment.IsDevelopment())
        {
            options.AddDevelopmentSigningCertificate();
            options.AddDevelopmentEncryptionCertificate();
            options.DisableTransportSecurityRequirement(); // Allow HTTP in dev
        }
        else
        {
            options.AddSigningCertificate(LoadCert(config["Auth:SigningCertBase64"], config["Auth:SigningCertPassword"]));
            options.AddEncryptionCertificate(LoadCert(config["Auth:EncryptionCertBase64"], config["Auth:EncryptionCertPassword"]));
        }

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableRevocationEndpointPassthrough();
    });

// Middleware pipeline ORDER MATTERS:
app.UseCors();            // Must be before UseAuthentication
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();     // After routing
app.MapControllers();
```

### ROPC Trade-off (Documented Decision)

ROPC is retained for email/password login (Stories 2-1, 2-2). **OAuth 2.1 formally deprecates ROPC.** This is a conscious decision for MVP: Blinder is a first-party app with no current MFA requirement. The trade-off is acceptable.

**If MFA becomes a requirement in a future sprint**, ROPC must be replaced with Authorization Code + PKCE. This note exists so that choice is never accidental technical debt.

### Token Expiry Rationale (Critical — Do Not Change Without Reading This)

| Token | Expiry | Reason |
|---|---|---|
| Access token | **15 minutes** | `Blinder.Api` validates tokens via cached JWKS keys — no revocation check per request. A compromised 30-day token cannot be invalidated. 15 minutes bounds the compromise window. |
| Refresh token | **30 days rolling** | OpenIddict rotates on every use: old token marked `redeemed`, new token issued. Replay detection is automatic. |

NFR10 ("tokens expire after 30 days of inactivity") is satisfied by refresh token rotation: as long as the user authenticates at least every 30 days, they get a new refresh token. Inactive accounts expire after 30 days of no refresh token use.

**The original Story 2-0 had 30-day access tokens. This was changed because the two-project topology (separate IdentityServer) makes remote JWT validation the only option. Do not revert to 30-day access tokens.**

### OpenIddict Is Not Turnkey — Passthrough Mode Required

OpenIddict does NOT auto-handle grant logic. It owns: token issuance, response serialization, code lifecycle, rotation, revocation. You own: the controller action that validates credentials and calls `SignIn()`.

**Reference implementation:** Clone and study the Zirku sample (two-project topology) from [https://github.com/openiddict/openiddict-samples](https://github.com/openiddict/openiddict-samples).

### Known Pitfalls Checklist

- [ ] **`IdentityRole<Guid>` — not default `IdentityRole`**: `Blinder.Api/Program.cs` uses `AddIdentity<ApplicationUser, IdentityRole<Guid>>()`. IdentityServer must use the exact same type parameters or Identity DI will fail.
- [ ] **Two DbContexts, same DB**: `AppDbContext` (from Blinder.Api) manages Identity tables; `OpenIddictDbContext` manages OpenIddict tables. Both point to the same PostgreSQL connection string. `dotnet ef` targets must specify `--context OpenIddictDbContext` when running from IdentityServer.
- [ ] **Seed `blinder-mobile` client before any token request** — `OpenIddictSeeder` hosted service. Without it: `invalid_client` on every request.
- [ ] **Call `UseOpenIddict()` on DbContext options** — not just `UseEntityFrameworkCore()`.
- [ ] **Middleware order**: `UseCors()` → `UseAuthentication()` → `UseAuthorization()`.
- [ ] **Use `DisableTransportSecurityRequirement()` in development/test only** — never production.
- [ ] **Token endpoint requires `EnableTokenEndpointPassthrough()`** — otherwise controller is never reached.
- [ ] **Use `SetDestinations()` on claims** — old 3-arg `AddClaim(type, value, destination)` removed in OpenIddict v4+.
- [ ] **`Blinder.Api` uses `OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme`** on `[Authorize]`, not the default cookie scheme.
- [ ] **Run `dotnet ef migrations add` in `Blinder.IdentityServer/`** — not in `Blinder.Api/`.
- [ ] **`IdentityDbContext` must call `UseOpenIddict()` in options** — not in `OnModelCreating` directly.
- [ ] **`.NET 10 breaking change:** `TokenValidatedContext.SecurityToken` returns `JsonWebToken`, not `JwtSecurityToken`** — use `JsonWebToken` type if custom token validation is needed.

### RSA Signing Keys

Development: `options.AddDevelopmentSigningCertificate().AddDevelopmentEncryptionCertificate()` — auto-generated, stored in user secrets.

Production (in Program.cs):
```csharp
static X509Certificate2 LoadCert(string? base64, string? password) =>
    new X509Certificate2(Convert.FromBase64String(base64!), password);

options.AddSigningCertificate(LoadCert(config["Auth:SigningCertBase64"], config["Auth:SigningCertPassword"]));
options.AddEncryptionCertificate(LoadCert(config["Auth:EncryptionCertBase64"], config["Auth:EncryptionCertPassword"]));
```

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
                services.RemoveAll<DbContextOptions<IdentityDbContext>>();
                services.AddDbContext<IdentityDbContext>(o =>
                    o.UseInMemoryDatabase("test").UseOpenIddict());
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
        body.GetProperty("expires_in").GetInt32().Should().Be(900); // 15 min
    }
}
```

### Dependency Chain

- **Blocks:** Stories 2-2 (login/logout), 2-3 (Apple login), 2-4 (Google/Facebook login)
  - **Story 2-2 MUST be updated** after this story: it references old `POST /api/auth/login` and 30-day JWT patterns — these are now handled by OpenIddict ROPC grant
- **Unblocked By:** No blocking dependencies (Story 2-1 registration is already done — registration correctly returns 201 without token)
- **Related:** `docs/project-context.md` Section 18 rules 19–24 MUST be updated (Task 8)

### Packages Summary

`Blinder.IdentityServer`:
- `OpenIddict.AspNetCore` 7.4.0
- `OpenIddict.EntityFrameworkCore` 7.4.0
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.*
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.*
- `FluentValidation.AspNetCore` 11.*
- `Serilog.AspNetCore` 9.*

`Blinder.Api` additions/changes:
- ADD: `OpenIddict.Validation.AspNetCore` 7.4.0
- ADD: `OpenIddict.Validation.SystemNetHttp` 7.4.0
- REMOVE: `Microsoft.AspNetCore.Authentication.JwtBearer` (if added — not in original scaffold)

### Architecture Compliance Guardrails

- **Single Token Source:** OpenIddict in `Blinder.IdentityServer` is the ONLY source of token issuance. `Blinder.Api` never issues tokens.
- **No Session Tables:** Only OpenIddict's `OpenIddictTokens` table for token lifecycle (no custom `RefreshTokens` table).
- **RFC 6749 & RFC 7807:** OAuth2 token endpoint is RFC 6749 compliant via OpenIddict. All errors use RFC 7807 Problem Details.
- **Mobile Token Storage:** Both access and refresh tokens via `expo-secure-store`. `AsyncStorage` prohibited.
- **DateTimeOffset everywhere:** `DateTimeOffset` in C#, `timestamptz` in PostgreSQL, ISO 8601 in API — no exceptions.
- **Snake_case DB:** `IdentityDbContext` must use `UseSnakeCaseNamingConvention()`.
- **ARCH-4 Migrations:** Migration deployment via `migrations/latest-identity.sql` (idempotent script) — no `Database.MigrateAsync()` at startup except in Development environment.

---

## Definition of Done

- [ ] `Blinder.IdentityServer` project compiles and starts without errors
- [ ] `POST /api/auth/oauth/token` with ROPC returns 15-minute access token + 30-day refresh token
- [ ] Refresh token rotation: second use of old refresh token returns `invalid_grant`
- [ ] Token revocation endpoint operational (`POST /api/auth/oauth/revoke`)
- [ ] OpenIddict 4 tables created via EF migration in `IdentityDbContext`
- [ ] `OpenIddictSeeder` registers `blinder-mobile` client on startup
- [ ] `Blinder.Api` validates tokens remotely via OIDC discovery (no JwtBearer)
- [ ] Rate limiting active on token endpoint (429 after 5 failed attempts/minute)
- [ ] Mobile `storageService.ts` contract complete with expiry tracking
- [ ] `docs/project-context.md` Section 18 rules updated (Task 8 complete)
- [ ] `docs/authentication.md` created with OAuth2 architecture and token lifecycle
- [ ] `.env.example` updated with all new variables
- [ ] All acceptance criteria validated via integration tests
- [ ] Code review approved before Story 2-2 implementation begins

---

## References

- Research: `_bmad-output/planning-artifacts/research/technical-microsoft-identity-aspnetcore-auth-research-2026-03-25.md`
- Epics: `_bmad-output/planning-artifacts/epics.md` (Epic 2, Story 2.0)
- Architecture: `_bmad-output/planning-artifacts/architecture.md`
- Previous: `_bmad-output/implementation-artifacts/2-1-email-password-user-registration.md`
- Project rules: `docs/project-context.md`
- OpenIddict samples: [https://github.com/openiddict/openiddict-samples](https://github.com/openiddict/openiddict-samples) (Zirku = two-project topology)
- OpenIddict docs: [https://documentation.openiddict.com](https://documentation.openiddict.com)

---

## Dev Agent Record

### Implementation Plan

Two-project OAuth2 topology implemented: `Blinder.IdentityServer` (new) issues tokens via OpenIddict; `Blinder.Api` validates them remotely via OIDC discovery.

### Debug Log

- Build errors identified on first compile (2026-03-25): `GetOpenIddictServerRequest` missing using directive; `DisableTransportSecurityRequirement` and `EnableRevocationEndpointPassthrough` have different API signatures in OpenIddict 7.4 vs. the story's code patterns (v5-era samples). `AddFixedWindowLimiter` needs `using Microsoft.AspNetCore.RateLimiting;`. `X509Certificate2(byte[], string?)` is obsolete in .NET 10 — use `X509CertificateLoader`. Developer is resolving these compilation errors manually.

### Completion Notes

All structural files created. Build errors in `OAuth2Controller.cs` and `Program.cs` (IdentityServer) left for developer to resolve per their instruction. EF Core migration for `OpenIddictDbContext` pending build fix.

---

## File List

- `backend/Blinder.IdentityServer/Blinder.IdentityServer.csproj` (new)
- `backend/Blinder.IdentityServer/Program.cs` (new)
- `backend/Blinder.IdentityServer/appsettings.json` (new)
- `backend/Blinder.IdentityServer/appsettings.Development.json` (new)
- `backend/Blinder.IdentityServer/Dockerfile` (new)
- `backend/Blinder.IdentityServer/Infrastructure/Data/OpenIddictDbContext.cs` (new)
- `backend/Blinder.IdentityServer/Infrastructure/Data/HostExtensions.cs` (new)
- `backend/Blinder.IdentityServer/Infrastructure/Auth/ISocialLoginTokenValidator.cs` (new)
- `backend/Blinder.IdentityServer/Infrastructure/Auth/OpenIddictSeeder.cs` (new)
- `backend/Blinder.IdentityServer/Controllers/Auth/OAuth2Controller.cs` (new)
- `backend/Blinder.slnx` (modified — added IdentityServer project)
- `backend/Blinder.Api/Blinder.Api.csproj` (modified — replaced JwtBearer with OpenIddict validation packages)
- `backend/Blinder.Api/Program.cs` (modified — added OpenIddict remote validation)
- `backend/Blinder.Api/appsettings.json` (modified — added Auth:IdentityServerUrl)
- `backend/Blinder.Tests/Blinder.Tests.csproj` (modified — added InMemory EF and IdentityServer reference)
- `backend/Blinder.Tests/IdentityServer/OAuth2TokenEndpointTests.cs` (new)
- `docker-compose.yml` (modified — added identityserver service, removed obsolete JWT env vars)
- `nginx/nginx.conf` (modified — added /api/auth/oauth/ and /.well-known/ routing to identityserver)
- `mobile/services/storageService.ts` (modified — full OAuth2 token contract with expiry tracking)
- `.env.example` (modified — added identity server cert vars, removed obsolete JWT vars)
- `docs/authentication.md` (new)

---

## Change Log

- 2026-03-25: Implemented Story 2-0 OAuth2/OIDC Foundation — created `Blinder.IdentityServer` project with OpenIddict 7.4, `OAuth2Controller` (ROPC + refresh + auth code passthrough), `OpenIddictSeeder`, `ISocialLoginTokenValidator` interface, `OpenIddictDbContext`; added OpenIddict remote validation to `Blinder.Api`; updated nginx routing, docker-compose, mobile storageService.ts token contract, .env.example, docs/authentication.md. Build errors in controller/Program.cs pending developer fix.

---

## Story Completion Status

- Story context generated with cross-artifact analysis, research synthesis, and implementation guardrails.
- Status set to `ready-for-dev`.
- Completion note: Updated from custom OAuth2 implementation to OpenIddict 7.4 per research report 2026-03-25. Token expiries corrected (15-min access, 30-day rolling refresh). Blinder.IdentityServer architecture applied. Known pitfalls and full code patterns included.
