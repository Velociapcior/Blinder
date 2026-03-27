# Authentication Architecture

Established in Story 2-0 (OAuth2/OIDC Foundation — OpenIddict 7.x).

---

## Two-Project Topology

```
Mobile App
    │  POST /api/auth/oauth/token  (ROPC, refresh, auth code)
    │  POST /api/auth/oauth/revoke (token revocation)
    │  GET  /.well-known/openid-configuration
    ▼
  Nginx (port 80)
    ├── /api/auth/oauth/*  →  Blinder.IdentityServer:8080  (token issuance)
    ├── /.well-known/*     →  Blinder.IdentityServer:8080  (OIDC discovery)
    └── /api/*             →  Blinder.Api:8080             (resource server)

Blinder.IdentityServer (authorization server)
    • Issues all tokens via OpenIddict
    • Owns: OpenIddictDbContext (4 OpenIddict tables)
    • Uses: AppDbContext (read-only, for UserManager credential validation)

Blinder.Api (resource server)
    • Validates tokens remotely via OIDC discovery
    • Never issues tokens
    • Uses: AppDbContext (Identity + domain tables)
```

---

## OAuth2 Token Endpoint

### ROPC (email/password login)

```
POST /api/auth/oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=password
&client_id=blinder-mobile
&username=user@example.com
&password=MyPassword1!
```

**Response:**
```json
{
  "access_token": "<JWT>",
  "refresh_token": "<opaque>",
  "expires_in": 900,
  "token_type": "Bearer"
}
```

### Refresh Token

```
POST /api/auth/oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token
&client_id=blinder-mobile
&refresh_token=<refresh_token>
```

Returns a new token pair. The used refresh token is immediately marked `redeemed` by OpenIddict (replay protection).

### Token Revocation (logout)

```
POST /api/auth/oauth/revoke
Content-Type: application/x-www-form-urlencoded

token=<access_or_refresh_token>
&client_id=blinder-mobile
```

Returns `200 OK`. Idempotent. The mobile client must also call `storageService.clearTokens()` regardless of the revocation response.

---

## Token Lifetimes

| Token | Lifetime | Rationale |
|---|---|---|
| Access token | **15 minutes** | `Blinder.Api` validates via cached JWKS — no per-request revocation check. Short lifetime bounds the compromise window. |
| Refresh token | **30 days rolling** | OpenIddict rotates on every use: old token marked `redeemed`, new pair issued. Rolling means active users never expire. |

**Do not increase access token lifetime** without understanding that revocation cannot be enforced mid-window (tokens are validated offline via JWKS).

---

## OIDC Discovery

`Blinder.IdentityServer` exposes:

```
GET /.well-known/openid-configuration   → issuer metadata
GET /.well-known/jwks                   → JSON Web Key Set (signing keys)
```

`Blinder.Api` fetches and caches the JWKS on first request. Subsequent token validations use cached keys (no per-request network call after warmup). Keys are refreshed automatically by `UseSystemNetHttp()`.

---

## JWT Structure

All JWTs issued by the token endpoint include:

| Claim | Value |
|---|---|
| `sub` | User ID (GUID string) |
| `email` | User email address |
| `iss` | `Blinder.IdentityServer` URL |
| `aud` | `blinder-mobile` |
| `exp` | Unix epoch expiry (15 min from issuance) |
| `iat` | Unix epoch issuance time |

Signed with RSA-256 (development: auto-generated dev certificate; production: PFX loaded from env var).

---

## Mobile Token Storage (`storageService.ts`)

All token storage uses `expo-secure-store` (Keychain/Android Keystore). `AsyncStorage` is prohibited for auth tokens.

```typescript
// After successful login (ROPC or refresh):
await storageService.setTokens(access_token, refresh_token, expires_in);

// Before each API call — pre-refresh if expiring soon:
if (await storageService.isTokenExpiringSoon()) {
  // call POST /api/auth/oauth/token with grant_type=refresh_token
}

// On logout:
await storageService.clearTokens();
// Also call POST /api/auth/oauth/revoke
```

---

## `blinder-mobile` Client Application

OpenIddict requires every token request to identify a registered client via `client_id`. The `blinder-mobile` client is a **public client** (no secret — mobile apps cannot safely store secrets).

Registered by `OpenIddictSeeder` hosted service on startup with permissions for:
- Token endpoint
- Revocation endpoint
- Password grant (ROPC)
- Refresh token grant
- Authorization code grant (for future social login — Stories 2-3/2-4)

**Without the seeder running, every token request returns `invalid_client`.**

---

## Refresh Token Storage

Refresh tokens are stored **encrypted** in the `openiddict_tokens` table via ASP.NET Core Data Protection. There is no manual `RefreshTokens` table, no SHA-256 hashing. OpenIddict manages the full token lifecycle:

- Issued → stored as `valid`
- Used → marked `redeemed`, new token issued
- Expired → not accepted (validated against `expiry_date` column)
- Revoked → marked `revoked` via `POST /api/auth/oauth/revoke`

---

## Social Login Extension Point (Stories 2-3, 2-4)

The `ISocialLoginTokenValidator` interface in `Blinder.IdentityServer` is the extension point for Apple/Google/Facebook login. Provider-specific implementations validate the provider's ID token and return a normalized `SocialLoginPrincipal`. The `OAuth2Controller` dispatches via this interface within the authorization code grant — no changes to the controller are required when adding new providers.

---

## Certificate Setup

### Development

```csharp
// auto-generated and stored in user secrets — no manual setup needed
options.AddDevelopmentSigningCertificate();
options.AddDevelopmentEncryptionCertificate();
```

### Production

Generate a self-signed RSA-256 certificate and export as PFX:

```bash
openssl req -x509 -newkey rsa:4096 -keyout signing.key -out signing.crt -days 3650 -nodes \
  -subj "/CN=Blinder Identity Server Signing"
openssl pkcs12 -export -out signing.pfx -inkey signing.key -in signing.crt \
  -passout pass:your-cert-password

# Base64-encode for environment variable:
base64 -w 0 signing.pfx
```

Set the following environment variables (see `.env.example`):

```
AUTH_SIGNING_CERT_BASE64=<base64-encoded PFX>
AUTH_SIGNING_CERT_PASSWORD=<pfx password>
AUTH_ENCRYPTION_CERT_BASE64=<base64-encoded PFX>
AUTH_ENCRYPTION_CERT_PASSWORD=<pfx password>
```

---

## Database Schema

`OpenIddictDbContext` manages four tables in the `public` schema:

| Table | Purpose |
|---|---|
| `openiddict_applications` | Registered OAuth2 clients (`blinder-mobile`) |
| `openiddict_authorizations` | Authorization grants |
| `openiddict_tokens` | Issued tokens (access, refresh) with lifecycle state |
| `openiddict_scopes` | OAuth2 scopes |

EF Core migrations for these tables are generated from `Blinder.IdentityServer/` only. The idempotent SQL deployment script is at `migrations/latest-identity.sql`.

---

## ROPC Trade-off Note

ROPC (Resource Owner Password Credentials) is retained for email/password flows. **OAuth 2.1 formally deprecates ROPC.** This is a deliberate MVP decision: Blinder is a first-party app with no current MFA requirement. If MFA becomes a requirement, ROPC must be replaced with Authorization Code + PKCE.
