# Sprint Change Proposal — OAuth2/OIDC Authentication Foundation Restructuring

**Date:** 2026-03-25
**Author:** Piotr.palej
**Workflow:** Correct Course
**Status:** In Review

---

## Section 1: Issue Summary

### Problem Statement

Stories 2-1 (Email/Password Registration) and 2-2 (User Login & JWT Tokens) implement **custom email/password authentication with direct JWT issuance**, bypassing OAuth2/OIDC patterns entirely. This creates three critical issues:

1. **Security Risk:** Direct JWT issuance on email/password login deviates from industry best practices. OAuth2/OIDC provides standardized grant types, refresh token patterns, and revocation mechanisms that the custom flow lacks.

2. **Architectural Inversion:** Stories 2-3 (Apple Sign In) and 2-4 (Google/Facebook Social Login) plan to retrofit OAuth2/OIDC **on top** of the custom flow. This creates parallel authentication code paths:
   - Email/password: Direct JWT
   - Social login: OAuth2/OIDC token exchange
   - Maintenance burden: Two auth patterns in one system

3. **Microsoft.Identity Packages Unexplored:** ASP.NET Core offers `Microsoft.AspNetCore.Authentication.OpenIdConnect`, `Microsoft.IdentityModel.*`, and related packages designed for exactly this use case, but the current stories don't leverage them. Instead, the approach is home-grown.

### Discovery Context

Identified during Correct Course review when user flagged that Stories 2-1/2-2 don't align with Architecture's requirement for "Social login uses `ExternalLoginAsync` flows in Identity" and the PRD's FR2 requirement for "registration via email/password **and** social login (Apple, Google, Facebook)."

The Architecture document explicitly states:
- "Social provider ID tokens validated server-side; backend issues its own JWT — no OAuth redirect dance"
- "Social login...must be explicitly planned in the social login implementation stories"

But there is no foundational OAuth2/OIDC story before social login stories. The custom email/password flow doesn't set up the preconditions for proper social login integration.

### Evidence

- **Story 2-2 Task 1:** "Generate JWT with inactivity timeout policy" — custom JWT generation logic, not OAuth2 standard patterns
- **Story 2-2 AC 1:** "Credentials are verified then a JWT access token is returned" — direct issuance, no token grant framework
- **Architecture ARCH-16:** "Social login... ExternalLoginAsync wiring in `SocialLoginHandler.cs` — not covered by default Identity scaffolding templates. **Explicit dedicated stories are required.**" — This means OAuth2 foundation must precede or run parallel with social login stories.
- **Story 2-3 AC 1:** "Apple identity token is verified... against `https://appleid.apple.com/auth/keys` (JWKS)" — This is OAuth2/OIDC token validation, but there's no baseline OAuth2 pattern story before it.

---

## Section 2: Impact Analysis

### Epic Impact

| Epic | Impact | Severity |
|---|---|---|
| **Epic 2 (Auth)** | **Directly affected.** Stories 2-1 and 2-2 must be redesigned to establish OAuth2/OIDC foundation. Stories 2-3 and 2-4 become streamlined implementations on that foundation, not retrofits. | **CRITICAL** |
| Epic 3 (Onboarding) | Indirect — onboarding depends on working auth from Epic 2. Once auth is restructured, no Epic 3 changes needed. | Low |
| Epic 4 (Matching) | No impact. | Low |
| Epic 5 (Real-Time Chat) | No impact. | Low |
| Epic 6 (Reveal) | No impact. | Low |
| Epic 7 (Subscriptions) | Indirect — subscription webhooks (Apple IAP, Google Play) require OAuth2 token validation patterns. If auth foundation is solid, webhook validation follows naturally. | Medium |
| Epic 8 (Moderation) | No impact. | Low |
| Epic 9 (Analytics/GDPR) | No direct impact. | Low |

### Story Impact

| Story | Current Status | Change Required | Impact |
|---|---|---|---|
| **2-1** | In review | **Redesign:** Establish OAuth2/OIDC foundation + email/password as first auth implementation | Email/password no longer "custom" — becomes OAuth2 Resource Owner Password Credentials (ROPC) or simple direct auth + JWT issued by OAuth2 token endpoint |
| **2-2** | Ready-for-dev | **Redesign:** Refocus on JWT token management & refresh patterns (not generation) | JWT management becomes responsibility of OAuth2 token service. Story 2-2 focuses on token storage, refresh, and revocation. |
| **2-3 (Apple)** | Planned | **Simplify:** Becomes straightforward `ExternalLoginAsync` → JWT mapping on solid OAuth2 foundation | ~20% less code; no retrofitting. |
| **2-4 (Google/Facebook)** | Planned | **Simplify:** Same as 2-3 | ~20% less code; no retrofitting. |

### Artifact Conflicts & Changes Required

| Artifact | Conflict | Change |
|---|---|---|
| **epics.md** | Story sequence is wrong (custom auth before OAuth2 foundation) | Reorder Epic 2 stories: add **Story 2-0: OAuth2/OIDC Foundation** before 2-1 and 2-2 — establishes token endpoint patterns & grant types |
| **2-1-email-password-user-registration.md** | Assumes custom registration logic | Refocus: "Use OAuth2 ROPC grant for email/password. Identity scaffolding + token service integration." De-emphasize custom JWT. |
| **2-2-user-login-jwt-tokens-and-logout.md** | Generated JWT directly; no OAuth2 context | Major redesign: "User logs in → OAuth2 token endpoint issues tokens → mobile stores JWT + refresh token via secure store → logout revokes tokens." |
| **architecture.md** | OAuth2/OIDC foundation assumed but not explicitly planned | Add: "Email/password authentication uses OAuth2 ROPC grant; social logins use OAuth2 Authorization Code flow; all token issuance through shared OAuth2 token endpoint; backend issues JWT wrapping OAuth2 tokens." |
| **project-context.md** | No auth-specific rules documented | Add enforced rules: "Never issue JWT directly from credential validation. OAuth2 token endpoint always. Refresh tokens must be present for stateful auth. All token storage uses expo-secure-store on mobile." |

### Technical Impact

**Codebase Changes:**
- New `Infrastructure/Auth/OAuth2TokenService.cs` — centralized token endpoint
- New `Infrastructure/Auth/OAuth2GrantHandler.cs` — handles ROPC and Authorization Code grant types
- Modified `Program.cs` — OAuth2 middleware configuration
- Modified `AuthController` — endpoints delegate to OAuth2 token service
- Enhanced `storageService.ts` (mobile) — manages both access tokens and refresh tokens

**Dependencies to Add:**
- `Microsoft.AspNetCore.Authentication.OpenIdConnect` (OIDC middleware)
- `Microsoft.IdentityModel.Protocols.OpenIdConnect` (OIDC protocol support)
- `Microsoft.AspNetCore.Authentication.OAuth` (OAuth2 middleware)
- `Jose-JWT` or similar for JWT token signing consistency (optional but recommended)

**No Breaking Changes:**
- Existing `IdentityUser` scaffolding remains
- Database schema unchanged (no new tables)
- API contract evolves but remains backward-compat within Epic 2

---

## Section 3: Recommended Approach

**Selected: Option 1 — Direct Adjustment with Insertion of Foundation Story**

### Rationale

1. **No Rollback Required:** Story 2-1 is in review but not yet completed; Story 2-2 is ready-for-dev. Pivot now before implementation begins.
2. **Effort Justified:** Adding OAuth2 foundation story (2-0) is ~1 story sprint of effort. It yields:
   - Proper token lifecycle (access + refresh + revocation)
   - Simplified social login stories (2-3, 2-4) reduce effort by ~20%
   - Eliminates technical debt and parallel code paths
3. **Risk Reduction:** OAuth2 foundation is tested infrastructure. Reduces risk vs. custom JWT patterns.
4. **Timeline Impact:** +1 story cycle (2-0 inserted). Stories 2-1, 2-2 then proceed with cleaner implementation. 2-3, 2-4 actually complete **faster** on solid foundation.
5. **Long-term:** This is foundational. Deferred OAuth2 work compounds across Episodes 5/7 (chat, subscriptions) which also need token validation patterns.

### Implementation Plan

**Step 1: Insert Story 2-0 (OAuth2/OIDC Foundation)** — new story before 2-1
- Establish OAuth2 token endpoint with ROPC and Authorization Code grant support
- Set up refresh token patterns
- Token storage contract for mobile (access + refresh tokens)
- Zero end-user-facing features; pure foundation

**Step 2: Redesign Story 2-1** — focus on email/password as first ROPC grant use case
- Leverage existing Identity registration flow
- Route authentication through OAuth2 token endpoint
- De-emphasize custom JWT

**Step 3: Redesign Story 2-2** — focus on token lifecycle management, not JWT generation
- User logs in → OAuth2 token endpoint issues tokens
- Mobile stores both access and refresh tokens
- Logout revokes tokens
- Token refresh flow implemented

**Step 4: Stories 2-3 & 2-4 (Social Login)** — become simpler on OAuth2 foundation
- Apple Sign In → OAuth2 Authorization Code grant variant
- Google/Facebook → OAuth2 Authorization Code grant
- All route through same token endpoint

---

## Section 4: Detailed Change Proposals

### Change 4.1 — New Story 2-0: OAuth2/OIDC Foundation

**Story: 2-0: OAuth2/OIDC Authentication Foundation**

```
As a developer,
I want a complete OAuth2/OIDC token endpoint and grant handler infrastructure,
So that all authentication flows (email/password, social login, future integrations) 
are built on a single, secure, standards-compliant foundation.

## Acceptance Criteria

1. OAuth2 token endpoint operational at POST /api/auth/oauth/token
   - Accepts grant_type parameter: "password" (ROPC) or "authorization_code" (social)
   - Returns access_token, refresh_token, expires_in, token_type: "Bearer"

2. Resource Owner Password Credentials (ROPC) grant implemented
   - POST /api/auth/oauth/token with grant_type=password, username, password
   - Validates credentials via Identity UserManager
   - Issues JWT access token (30-day expiry) + refresh token (90-day expiry)
   - Returns RFC 7807 Problem Details on invalid credentials (401)

3. Authorization Code grant framework implemented (for social login)
   - POST /api/auth/oauth/token with grant_type=authorization_code, code, provider
   - Framework accepts provider-specific token validation logic
   - Issues JWT access token + refresh token on successful validation
   - Stories 2-3, 2-4 will implement provider-specific token validators

4. Refresh token flow implemented
   - POST /api/auth/oauth/token with grant_type=refresh_token, refresh_token
   - Issues new access token (30-day expiry) + new refresh token (90-day expiry)
   - Invalidates old refresh token on issue

5. Token revocation endpoint operational
   - POST /api/auth/oauth/revoke with token parameter (access or refresh token)
   - Removes token from valid tokens. Logout calls this endpoint.

6. Refresh token storage secured
   - Refresh tokens stored in PostgreSQL RefreshTokens table with user_id + hashed token + expiry_date
   - Tokens validated by looking up hashed token in DB, not decoded from JWT

7. Mobile token storage contract defined
   - Mobile stores access_token in secure store under key "access_token"
   - Mobile stores refresh_token in secure store under key "refresh_token"
   - Mobile automatically calls refresh endpoint before access token expiry
   - storageService.ts implements getAccessToken(), getRefreshToken(), setTokens(access, refresh), clearTokens()

8. JWT structure and claims standardized
   - All JWTs include: sub (user ID), iss (issuer), aud (audience), exp, iat
   - All JWTs signed with private key from configuration (PKCS8 RSA private key)
   - All JWTs can be validated server-side by JwtBearer middleware

9. Authorization Code for social login ready for use
   - SocialLoginHandler.cs validates provider ID token (JWKS, iss, aud)
   - Returns authorization code (short-lived, one-time use, 10-minute expiry)
   - Client exchanges authorization code + code_verifier for tokens via ROPC-like endpoint
   - Stories 2-3, 2-4 provide provider-specific validators

## Tasks Breakdown

- Task 1: OAuth2TokenService infrastructure
  - Create OAuth2TokenService with methods: IssueTokenAsync(userId, grantType), RefreshTokenAsync(), RevokeTokenAsync()
  - Create RefreshToken entity and repository
  - Migration to create refresh_tokens table

- Task 2: ROPC grant implementation
  - Create ResourceOwnerPasswordCredentialsGrantHandler
  - Validates email + password via UserManager
  - Returns tokens on success

- Task 3: Authorization Code framework
  - Create AuthorizationCodeGrantHandler (interface for provider-specific validation)
  - Create SocialLoginTokenValidator interface for provider-specific implementations (to be filled in 2-3, 2-4)

- Task 4: Token refresh and revocation
  - RefreshTokenGrantHandler
  - TokenRevocationHandler

- Task 5: API endpoint wiring
  - AuthController.OAuth2TokenEndpoint (POST /api/auth/oauth/token)
  - AuthController.TokenRevocationEndpoint (POST /api/auth/oauth/revoke)

- Task 6: Mobile token storage contract
  - Update storageService.ts with OAuth2 token management
  - Implement automatic refresh token fetch before expiry

- Task 7: Tests for all grant types and token lifecycle

## Dev Notes

- No user-facing features yet. This is pure infrastructure.
- Email/password login (Story 2-1) will call OAuth2TokenService after this story completes.
- Stories 2-3, 2-4 extend OAuth2 framework with provider-specific validators.
- This foundation also prepares for future OAuth2 resource server scenarios on mobile (e.g., third-party integrations).
```

**Effort Estimate:** Medium (1 story sprint)
**Risk:** Low — OAuth2 is well-established pattern; infrastructure-only, no business logic changes

---

### Change 4.2 — Modified Story 2-1: Email/Password Registration (Redesign)

**Current Problem:** References custom JWT generation and doesn't establish OAuth2.

**New Direction:** Focus on user creation + invitation flow, with authentication routed through OAuth2 token service.

```yaml
REPLACE IN: 2-1-email-password-user-registration.md
OLD SECTION: "Acceptance Criteria / AC 1"
  "Given a registration endpoint POST /api/auth/register receives email, password, gender, over18Declaration
   When validation succeeds
   Then a new user is created..."

NEW SECTION: "Acceptance Criteria / AC 1"
  "Given POST /api/auth/register receives email, password, gender, over18Declaration
   When validation succeeds and ApplicationUser is created
   Then no JWT is issued by this endpoint. Instead, a 201 Created response with user_id is returned.
   The client must then call POST /api/auth/oauth/token with grant_type=password to obtain JWT."

RATIONALE: Separates user creation (registration) from token issuance (authentication). 
Follows OAuth2 separation of concerns.
```

**Key Changes to Story 2-1:**
1. Registration endpoint creates user only, doesn't generate JWT
2. API response includes user_id for client reference
3. Client flows immediately to login endpoint to obtain tokens
4. UX remains unchanged (user experience is still register → auto-log-in)
5. Backend delegates authentication to OAuth2 token service (Story 2-0)

---

### Change 4.3 — Modified Story 2-2: User Login, JWT Tokens, and Logout (Major Redesign)

**Current Problem:** Generates JWT directly on successful credential verification. No refresh token pattern. No OAuth2 context.

**New Focus:**
1. **OAuth2 Token Endpoint Integration** — login delegates to OAuth2 token service
2. **Refresh Token Lifecycle** — manage both access and refresh tokens
3. **Token Revocation** — secure logout via token revocation

```yaml
REPLACE IN: 2-2-user-login-jwt-tokens-and-logout.md
OLD AC 1: "Given a POST /api/auth/login request with valid email and password
          When credentials are verified
          Then a JWT access token is returned"

NEW AC 1: "Given a POST /api/auth/login request with valid email and password (DEPRECATED endpoint)
          When credentials are verified by LoginController
          Then LoginController delegates to OAuth2TokenService.IssueTokenAsync(userId, grantType: 'password')
          OAuth2TokenService issues access_token (30-day) + refresh_token (90-day)
          Both tokens are returned in response body"

RATIONALE: OAuth2 token endpoint is single source of token issuance. 
Login endpoint becomes thin wrapper that validates identity and calls token service.
```

**Key Changes to Story 2-2:**
1. Rename current `/api/auth/login` → `/api/auth/oauth/token` (OAuth2 standard)
   - Old endpoint deprecated but may remain temporarily for backward compat
2. Token response now includes: `access_token`, `refresh_token`, `expires_in`, `token_type`
3. Mobile stores both tokens via new `storageService` methods
4. Logout calls `/api/auth/oauth/revoke` (not just clear storage)
5. AC 4 (30-day expiry) now explicit: refresh tokens 90 days, access tokens 30 days
6. New AC: Refresh token endpoint operational — client can fetch new access token before expiry

---

### Change 4.4 — Modified Story 2-3: Apple Sign In (Simplified)

**Current Problem:** Plans to implement Apple OIDC token validation independently.

**New Direction:** Apple OIDC token validation + OAuth2 Authorization Code grant on solid foundation (Story 2-0).

```yaml
REPLACE IN: (future story file 2-3-apple-sign-in.md)
OLD PATTERN: Apple validation logic in SocialLoginHandler, then custom auth flow

NEW PATTERN: 
1. Mobile obtains Apple ID token (native Apple Sign In UI)
2. Mobile POSTs to POST /api/auth/oauth/token with:
   grant_type: "authorization_code"
   provider: "apple"
   token: <Apple ID token>
3. Backend calls SocialLoginTokenValidator.ValidateAppleTokenAsync(token)
4. Validation succeeds → create/find ApplicationUser
5. OAuth2TokenService issues access_token + refresh_token
6. Same token response structure as email/password login

BENEFIT: One token endpoint, consistent behavior, no code duplication.
```

---

### Change 4.5 — Modified Story 2-4: Google & Facebook Social Login (Simplified)

Same pattern as Story 2-3 (Apple). OAuth2 Authorization Code + provider validators.

---

### Change 4.6 — Architecture Document Update

**Add Section: OAuth2/OIDC Authentication Architecture**

```markdown
## Authentication Architecture: OAuth2/OIDC Foundation

All authentication flows (email/password, social login, future third-party) route through 
a unified OAuth2/OIDC token endpoint. This ensures:

1. Centralized token policy enforcement (expiry, revocation, refresh patterns)
2. Consistent JWT structure and claims across all grant types
3. Standardized error responses (RFC 6749 OAuth2 errors + RFC 7807 Problem Details)
4. Simplified social login implementation (provider validators plug into Authorization Code flow)

### Grant Types Supported

**Resource Owner Password Credentials (ROPC) — Email/Password Auth**
- User provides username (email) + password
- Backend validates via ASP.NET Core Identity UserManager
- Issues access_token (30-day expiry) + refresh_token (90-day expiry)
- RFC 6749 compliant; ROPC is acceptable for first-party mobile/web clients

**Authorization Code — Social Login**
- Mobile obtains ID token from social provider (Apple, Google, Facebook)
- Mobile exchanges ID token + authorization_code for tokens via /auth/oauth/token
- Backend validates ID token signature (JWKS endpoint) + claims
- Issues same access_token + refresh_token structure
- Supports future OAuth2 Authorization Code + PKCE flows with external IDPs

**Refresh Token — Token Renewal**
- Client exchanges refresh_token for new access_token + refresh_token pair
- Refresh token single-use; old token invalidated on issue
- Backend validates refresh token by DB lookup (hashed token comparison)

### Token Storage & Security

- **Server-Side:** Refresh tokens hashed and stored in PostgreSQL with expiry + user_id
- **Mobile:** Both access and refresh tokens stored in `expo-secure-store` (iOS Keychain / Android Keystore)
- **No Session Tables:** Stateless token-based auth; stored only for revocation tracking

### JWT Structure

All JWTs signed with RSA-256 (private key in appsettings.json, never in code):

```json
{
  "sub": "user-guid",
  "iss": "https://blinder-api.{region}.{domain}",
  "aud": "blinder-mobile",
  "exp": 1735689600,
  "iat": 1735603200,
  "auth_time": 1735603200,
  "given_name": "Optional",
  "email": "user@example.com"
}
```

All dates: Unix epoch (seconds).

### Token Lifecycle on Mobile

1. Login flow obtains access_token + refresh_token
2. storageService stores both in secure store
3. Before each API request, apiClient checks token age:
   - If access_token expires within 5 minutes → silently refresh
   - Call POST /auth/oauth/token with grant_type=refresh_token → get new pair
   - Update secure store with new tokens
4. On 401 Unauthorized → notify UI, require explicit re-login
5. On logout → call POST /auth/oauth/revoke once per token, then clear secure store
```

---

### Change 4.7 — project-context.md: Add Authentication Rules

**Add Section: Authentication & Token Management Rules**

```markdown
### Authentication & Token Management Rules

26. **OAuth2 Token Endpoint as Single Source:** All token issuance flows (email/password, social login, 
    token refresh) route through `POST /api/auth/oauth/token`. Never issue JWT directly from a credential 
    validation endpoint. The OAuth2 token service is the canonical token generator.

27. **Refresh Token Pattern Mandatory:** All stateful auth flows must include refresh tokens. 
    Access tokens expire after 30 days of inactivity; refresh tokens expire after 90 days. 
    Clients refresh access tokens automatically before expiry (mobile: 5-minute buffer).

28. **Refresh Token Storage:** Backend stores hashed refresh tokens in PostgreSQL `refresh_tokens` table. 
    Mobile stores both access_token and refresh_token in `expo-secure-store` only (never AsyncStorage, 
    never in-memory state).

29. **Token Revocation on Logout:** Logout endpoint calls `POST /api/auth/oauth/revoke` for each token 
    (access + refresh) before clearing device storage. Revoked tokens marked invalid in DB.

30. **OAuth2 Authorization Code for Social Login:** Apple, Google, Facebook login use the OAuth2 
    Authorization Code flow. Provider-specific token validators implement `ISocialLoginTokenValidator`. 
    All routes through the same token endpoint (`POST /api/auth/oauth/token` with grant_type=authorization_code).

31. **No Custom JWT Signing:** JWT signing is centralized in OAuth2TokenService. No controller-level, 
    service-level, or ad-hoc JWT generation outside OAuth2TokenService.
```

---

## Section 5: Implementation Handoff

### Change Scope Classification

**MAJOR** — Fundamental authentication architecture replan. Requires:
- Product Owner coordination (verify this aligns with PRD intent)
- Architect review (OAuth2 design confirmation)
- Developer team consensus (impacts all future auth work)

### Scope Details

| Category | Scope |
|---|---|
| **Code Changes** | New: `OAuth2TokenService`, `OAuth2GrantHandlers`, `RefreshToken` entity, migrations; Modified: `AuthController`, `storageService.ts`, `Program.cs`, multiple story acceptance criteria |
| **Story Impact** | 4 stories modified (2-0 new, 2-1 redesigned, 2-2 redesigned, 2-3/2-4 simplified) |
| **Team Involved** | Architect (OAuth2 design), Backend Lead (token service impl), Mobile Lead (token storage), PM (prioritization) |
| **Timeline Impact** | +1 story for foundation (2-0), but 2-1/2-2 execute faster with cleaner design, 2-3/2-4 ~20% simpler. Net: no schedule delay; smoother execution. |
| **Risk Level** | Medium — OAuth2 is standard, but refactor of core auth affects all downstream work. Mitigation: defer any Epic 3+ work until auth foundation proven solid. |

### Handoff Recipients & Responsibilities

1. **Product Owner / Scrum Master:**
   - Confirm OAuth2 foundation aligns with business intent (security requirements, user experience)
   - Approve reordering of stories (insert 2-0, resurface 2-1/2-2)
   - Backlog reorganization: update sprint-status.yaml and epics.md
   - Schedule coordination meetings with development leads before sprint kickoff

2. **Solution Architect:**
   - Review OAuth2 token service design in this proposal
   - Confirm JWT structure, claims, expiry policies align with infrastructure
   - Approve grant types (ROPC, Authorization Code)
   - Sign off on refresh token storage pattern
   - Available for design review sessions with backend/mobile developers

3. **Backend Development Lead:**
   - **Strategy & Coordination:**
     - Assign Story 2-0 implementation ownership to senior backend developer
     - Create task breakdown for OAuth2TokenService subcomponents
     - Establish code review process for token service (high-security code)
     - Coordinate Story 2-1/2-2 refactor sequencing
     - Liaison with Mobile Lead on token storage contract sign-off
   
   **Developer Team Responsibilities:**
   - **Senior Backend Developer (Story 2-0 Owner):**
     - Implement `Infrastructure/Auth/OAuth2TokenService.cs` with ROPC and Authorization Code grant handling
     - Create `Infrastructure/Auth/OAuth2GrantHandler.cs` abstract base + concrete handlers
     - Implement `RefreshToken` entity, repository, and migration
     - Setup JWT signing/validation with RSA keys from appsettings
     - Create comprehensive unit tests for all grant types and token lifecycle
     - Code review: sign-off from architect and lead before Story 2-0 completion
   
   - **Backend Developers (Stories 2-1 & 2-2):**
     - Update `AuthController.RegisterAsync()` to use OAuth2TokenService for token issuance (Story 2-1)
     - Refactor `AuthController.LoginAsync()` to delegate to OAuth2TokenService (Story 2-2)
     - Implement `POST /api/auth/oauth/revoke` endpoint for logout (Story 2-2)
     - Create `TokenRefreshRequest/Response` DTOs and validators
     - Implement automatic token expiry policy in refresh logic
     - Add migration for `RefreshTokens` table
     - Unit/integration tests for login/logout/refresh flows
     - Cross-team testing with mobile developers on token storage contract

4. **Mobile Development Lead:**
   - **Strategy & Coordination:**
     - Assign `storageService.ts` refactor to TypeScript-strong developer
     - Review access/refresh token storage implementation
     - Establish automatic token refresh pattern for all API client calls
     - Coordinate Story 2-3/2-4 implementation on OAuth2 foundation
   
   **Developer Team Responsibilities:**
   - **Mobile Developer (Token Storage & Refresh):**
     - Refactor `mobile/services/storageService.ts` to manage both access and refresh tokens
       - `getAccessToken()`, `getRefreshToken()`, `setTokens(access, refresh)`, `clearTokens()`
       - Implement token expiry tracking (store `exp` timestamp)
     - Create `mobile/services/authService.ts` with automatic refresh logic
       - Before each API request, check if access token expires within 5 minutes
       - If so, silently call `POST /api/auth/oauth/token` with `grant_type=refresh_token`
       - On 401 Unauthorized, require explicit user re-login (show login screen)
     - Update `mobile/services/apiClient.ts` to use refreshed tokens
     - Add `AsyncStorage` scan to ensure NO auth tokens stored outside secure store
     - Unit tests: mock OAuth2 token endpoint, test refresh flow, test expiry logic
     - Integration tests: test against Story 2-0 OAuth2 token endpoint
   
   - **Mobile Developers (Stories 2-3 & 2-4 Support):**
     - When social login stories begin, extend `authService.ts` to support Authorization Code grant
     - Implement provider-specific token validators (Apple, Google, Facebook)
     - Ensure social login tokens flow through same OAuth2 endpoint
     - Test simultaneous email/password + social login flows

5. **QA / Test Automation:**
   - **Test Lead Responsibilities:**
     - Define test matrix for OAuth2 stories (ROPC, AuthCode, refresh, revocation, edge cases)
     - Establish test data (valid/invalid credentials, expired tokens, tampered tokens)
     - Coordinate with developers on test environment setup (OAuth2 token endpoint must be testable)
   
   - **Test Automation Engineers:**
     - **Backend Tests:**
       - ROPC grant: valid credentials, invalid credentials, missing parameters, expired tokens
       - Authorization Code grant framework: placeholder providers, valid/invalid codes, expired codes
       - Token refresh: valid refresh token, expired refresh token, reused refresh token (revocation), missing refresh token
       - Token revocation: revoke access token, revoke refresh token, idempotent revocation
       - Edge cases: concurrent token refresh, token expiry boundary conditions, clock skew scenarios
     
     - **Mobile Tests:**
       - Token storage: SecureStore-only validation, no AsyncStorage for auth tokens
       - Automatic refresh: trigger refresh at 5-minute boundary, verify tokens updated in storage
       - Login flow: email/password → receive tokens → store automatically
       - Logout flow: call revoke endpoint → clear storage, verify tokens gone
       - Error scenarios: 401 on expired token, automatic re-login prompt, silent refresh failure handling
     
     - **Integration Tests:**
       - End-to-end: mobile login → OAuth2 token endpoint → token storage → API request with token
       - Refresh flow: access token expiry → automatic refresh → API request succeeds
       - Logout: mobile → revoke endpoint → storage cleared → next API request → 401 → re-login required

6. **DevOps / Infrastructure (Prerequisite):**
   - Ensure RSA key pair generation for JWT signing available in `appsettings.json`
   - Confirm PostgreSQL connection and migration tooling ready for `RefreshTokens` table
   - Staging environment ready for OAuth2 testing before production deployment

---

## Section 6: Success Criteria for Implementation

Before Stories 2-1, 2-2 proceed to development:

- [ ] Architect approves OAuth2 design from Section 4.6
- [ ] Project context rules added (Section 4.7)
- [ ] epics.md updated with Story 2-0 insertion
- [ ] Story 2-0, 2-1, 2-2, (2-3 preview), (2-4 preview) acceptance criteria finalized
- [ ] Backend + Mobile leads confirm token storage contract from Section 4.1 (AC 7)
- [ ] Risk mitigation plan agreed: no Epic 3+ work starts until Story 2-0 complete + 2-1/2-2 tested

---

## Appendix: Quick Reference — OAuth2 Token Endpoint Contract

### POST /api/auth/oauth/token

**Email/Password (ROPC):**
```
POST /api/auth/oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&username=user@example.com&password=SecurePassword123

RESPONSE (200):
{
  "access_token": "eyJhbGciOiJSUzI1NiIs...",
  "refresh_token": "eyJhbGciOiJSUzI1NiIs...",
  "expires_in": 2592000,
  "token_type": "Bearer"
}
```

**Social Login (Authorization Code):**
```
POST /api/auth/oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code&provider=apple&code=<auth_code>&token=<apple_id_token>

RESPONSE (200):
{
  "access_token": "eyJhbGciOiJSUzI1NiIs...",
  "refresh_token": "eyJhbGciOiJSUzI1NiIs...",
  "expires_in": 2592000,
  "token_type": "Bearer"
}
```

**Token Refresh:**
```
POST /api/auth/oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=refresh_token&refresh_token=<stored_refresh_token>

RESPONSE (200):
{
  "access_token": "eyJhbGciOiJSUzI1NiIs...",
  "refresh_token": "eyJhbGciOiJSUzI1NiIs...",
  "expires_in": 2592000,
  "token_type": "Bearer"
}
```

### POST /api/auth/oauth/revoke

**Token Revocation:**
```
POST /api/auth/oauth/revoke
Content-Type: application/x-www-form-urlencoded
Authorization: Bearer <access_token>

token=<access_or_refresh_token>

RESPONSE (200):
{
  "message": "Token revoked successfully"
}
```

---
