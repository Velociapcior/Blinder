# Story 2.3: Social Federation via IdentityServer - Google, Apple, and Facebook

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a user,
I want to sign in with Google, Apple, or Facebook through IdentityServer,
so that external identity providers are federated through the single IdentityServer authority and never handled directly by the mobile app or Blinder.Api.

## Acceptance Criteria

1. **Given** IdentityServer is configured as the federation hub with Google, Apple, and Facebook as external providers
   **When** a user selects a social provider on the sign-in screen
   **Then** the mobile app initiates an Authorization Code + PKCE flow against IdentityServer, not directly against Google, Apple, or Facebook.

2. **Given** the external provider completes authentication successfully
   **When** IdentityServer receives the callback
   **Then** IdentityServer handles the external provider exchange, creates or links a local `identity.*` account, and returns IdentityServer-issued access, ID, and refresh tokens to the mobile client.

3. **Given** the federation flow completes
   **When** the mobile client resumes its OIDC flow
   **Then** the mobile app only ever holds IdentityServer-issued tokens and external provider tokens never reach the mobile client or `Blinder.Api`.

4. **Given** Apple sign-in is enabled
   **When** the provider registration is configured
   **Then** Sign in with Apple requests `name` and `email` scopes and uses a real HTTPS domain callback configuration suitable for App Store review.

5. **Given** an external provider returns an email address that already belongs to an existing local account
   **When** the user signs in for the first time with that provider
   **Then** IdentityServer prompts the user to explicitly link the provider to the existing account instead of silently creating a duplicate or silently attaching the login.

6. **Given** external login unlinking is needed by a later workflow
   **When** that capability is introduced
   **Then** it is implemented as an IdentityServer-owned command or endpoint and `Blinder.Api` never mutates `identity.*` directly.

## Tasks / Subtasks

- [x] Add upstream provider configuration to `Blinder.IdentityServer` only (AC: 1, 3, 4, 6)
  - [x] Add the OpenIddict web provider integration package(s) needed for upstream federation and keep all OpenIddict package versions aligned to `7.5.0`; do not downgrade to `7.4.0`.
  - [x] Extend `backend/src/Blinder.IdentityServer/Program.cs` with OpenIddict client configuration for upstream providers while preserving the existing OpenIddict server setup used by `/connect/*` endpoints.
  - [x] Register Google, Facebook, and Apple providers with unique callback paths per provider to reduce mix-up attack risk.
  - [x] Read provider credentials and Apple metadata from user secrets or environment variables; do not commit secrets to tracked `appsettings*.json` files.
  - [x] Preserve `app.UseForwardedHeaders()` before authentication so externally generated HTTPS callback URLs stay correct behind Traefik or another proxy.

- [x] Extend the IdentityServer account UI to initiate external sign-in without changing the mobile OIDC contract (AC: 1, 3)
  - [x] Reuse the existing `Pages/Account/Login` and `Pages/Account/Register` utility pages as the user-facing entry points for external sign-in instead of creating `Blinder.Api` auth endpoints.
  - [x] Enumerate configured external providers via the authentication stack instead of hardcoding button availability.
  - [x] Add page handlers or a small adjacent page model that challenges the selected provider using a safe local `returnUrl` and then resumes the existing `/connect/authorize?...` flow.
  - [x] Keep `asp-route-returnUrl` or equivalent safe-local-url handling so the original PKCE authorize request survives the external redirect round trip.
  - [x] Return generic UI errors for external login failures or provider cancellations; do not leak provider secrets or protocol internals to the user.

- [x] Implement local account resolution, creation, and explicit linking in IdentityServer (AC: 2, 5, 6)
  - [x] First check for an existing external login association using Identity APIs before creating any local account.
  - [x] If no login association exists and the external email is unused, create a local `ApplicationUser` and attach the external login through `UserManager`/`SignInManager` APIs.
  - [x] If the external email matches an existing local account, render an explicit confirmation step that requires the user to sign in or otherwise prove account ownership before `AddLoginAsync`; do not auto-link.
  - [x] Do not add new `ApplicationUser` columns or a new identity migration in this story; the required `AspNetUserLogins` table already exists from the initial identity migration.
  - [x] Request Apple `name` and `email` scopes, and if Apple returns profile data on first authorization, use it only within the current linking/creation flow unless a later story adds approved persistent fields.

- [x] Keep token ownership and boundary rules intact (AC: 2, 3, 6)
  - [x] `Blinder.IdentityServer` remains the only issuer of tokens consumed by mobile clients and `Blinder.Api`.
  - [x] Do not expose upstream provider access tokens, refresh tokens, or ID tokens to the mobile app, `Blinder.Api`, or shared contracts.
  - [x] Do not add `/api/auth`, `/api/external-login`, or other business-API authentication endpoints in `Blinder.Api`.
  - [x] Do not modify `backend/src/Blinder.IdentityServer/Workers/OpenIddictSeeder.cs` for this story unless a genuine mobile OIDC client registration bug is discovered.

- [x] Add focused tests and validation for the federation flow (AC: 1, 2, 3, 4, 5)
  - [x] Add unit tests under `backend/tests/Blinder.IdentityServer.Tests/` for external-login initiation, safe return URL handling, callback failure handling, new-user creation, existing-login reuse, and duplicate-email link confirmation.
  - [ ] Add at least one end-to-end manual validation script covering Google and Apple callback flow through IdentityServer and then back into `/connect/authorize`.
  - [x] Run `dotnet test backend/Blinder.sln` and keep existing auth tests green.

## Dev Notes

### Scope of This Story

This story delivers the backend-side federation hub inside `Blinder.IdentityServer`.

**In scope:**
- Upstream provider registration for Google, Facebook, and Apple inside `Blinder.IdentityServer`
- IdentityServer-hosted external login initiation and callback handling
- Local-account creation and explicit link-confirmation behavior for duplicate emails
- Preserving the existing mobile Authorization Code + PKCE contract against IdentityServer

**Out of scope:**
- Mobile social button UX and deep-link handling beyond the existing OIDC contract (Story 2.7)
- Age gate, policy acceptance, or extra `ApplicationUser` properties (Story 2.4)
- Account deletion or a full end-user unlink management UI
- Any authentication endpoint in `Blinder.Api`

### Current Implementation Seam

There is currently no external provider configuration in the repo. The nearest owning code path is the existing IdentityServer login flow built in Story 2.2:

- `backend/src/Blinder.IdentityServer/Pages/Connect/Authorize.cshtml.cs` already challenges unauthenticated users into the Identity UI and then issues IdentityServer tokens after authentication succeeds.
- `backend/src/Blinder.IdentityServer/Pages/Account/Login.cshtml.cs` and `Register.cshtml.cs` already preserve the safe local `returnUrl` required to resume `/connect/authorize?...`.
- `backend/src/Blinder.IdentityServer/Program.cs` already hosts ASP.NET Core Identity plus OpenIddict server middleware.

Build on those files. Do not invent a parallel auth pipeline.

### What Already Exists - Do Not Recreate

| Existing file | Relevant detail |
|---|---|
| `backend/src/Blinder.IdentityServer/Program.cs` | ASP.NET Core Identity, OpenIddict server endpoints, forwarded headers, authentication, authorization, Razor Pages, and health checks are already wired. |
| `backend/src/Blinder.IdentityServer/Pages/Connect/Authorize.cshtml.cs` | Existing OIDC authorize path resumes token issuance after the user is authenticated. Preserve this contract. |
| `backend/src/Blinder.IdentityServer/Pages/Account/Login.cshtml.cs` | Safe local return URL handling already exists and should be reused for external-login initiation. |
| `backend/src/Blinder.IdentityServer/Pages/Account/Register.cshtml.cs` | Existing local-account creation flow remains valid and should coexist with social federation. |
| `backend/src/Blinder.IdentityServer/Persistence/ApplicationUser.cs` | `ApplicationUser` is still a plain `IdentityUser`; do not add custom columns in this story. |
| `backend/src/Blinder.IdentityServer/Persistence/IdentityDbContext.cs` | `identity.*` schema ownership is already established and must remain IdentityServer-only. |
| `backend/src/Blinder.IdentityServer/Persistence/Migrations/20260428084114_AddIdentityAndOpenIddict.cs` | The standard ASP.NET Core Identity tables, including `AspNetUserLogins`, already exist. No new migration should be needed just to support external logins. |
| `backend/src/Blinder.IdentityServer/Workers/OpenIddictSeeder.cs` | The mobile/admin OIDC client registrations already exist; social federation is upstream of this and should not change the app's `blinder-mobile` contract. |

### Architecture Guardrails

- IdentityServer is the only application allowed to create or modify `identity.*` data.
- `Blinder.Api` is a resource server only. It validates IdentityServer-issued access tokens and must not issue tokens or mutate identity-owned tables.
- If later business workflows require unlinking or other identity changes, they must call an IdentityServer-owned integration surface rather than sharing a DbContext or writing directly to `identity.*`.
- Keep the current Authorization Code + PKCE flow shape intact: mobile talks to IdentityServer, IdentityServer talks to upstream providers, and `Blinder.Api` only sees IdentityServer tokens.

### Recommended Technical Approach

Use OpenIddict's upstream client/web-provider integration for new provider work instead of mixing multiple provider stacks:

- Keep the existing server configuration in `Program.cs`.
- Add client-side upstream provider registration in the same host so Google, Facebook, and Apple are federated inside IdentityServer.
- Use unique callback endpoints per provider such as `callback/login/google`, `callback/login/facebook`, and `callback/login/apple`.
- Keep all OpenIddict packages on `7.5.0` to match the repo's current version.

This aligns with current repo dependencies and avoids a split design where some providers use ASP.NET handlers and others use unrelated third-party packages.

### Provider and Platform Constraints

- Apple callback URIs must use a real domain over HTTPS. Apple does not allow `localhost` or raw IP addresses for the redirect host.
- Apple returns the `user` payload only on the first successful authorization. If name/email information is needed during first-time account creation or linking, consume it during that initial callback.
- Request Apple `name` and `email` scopes explicitly. This is a launch-blocking requirement for the iOS product path described in the epic.
- Forwarded headers matter because the secure request scheme affects generated redirect URIs for external providers when the app is behind Traefik.
- Provider secrets belong in local user secrets for development and environment/secret-store configuration for deployed environments, never in tracked config files.

### Testing Requirements

#### Automated tests

Target project: `backend/tests/Blinder.IdentityServer.Tests/`

Add tests for:
- external-login handler rejects non-local return URLs
- external-login handler challenges the selected provider and preserves the OIDC return URL
- callback reuses an existing external login mapping without creating a duplicate user
- callback creates a new local user when the provider email is unused
- callback shows an explicit link/confirm path when provider email matches an existing local account
- callback handles provider cancellation or remote error with a generic message

#### Manual validation

1. Configure Google, Facebook, and Apple provider credentials using local secrets or environment variables.
2. Run IdentityServer behind a domain-based HTTPS host suitable for callbacks. For local work, prefer the existing Traefik host such as `https://auth.blinder.local` instead of `localhost` when validating Apple.
3. Start an OIDC authorize flow for `blinder-mobile` with PKCE.
4. From the IdentityServer login screen, choose each external provider and complete the upstream authentication flow.
5. Verify the browser returns to IdentityServer first, then resumes the original `/connect/authorize?...` request, then issues IdentityServer's code/tokens.
6. Verify the mobile contract stays unchanged: the final token exchange is still against `/connect/token` on IdentityServer.
7. Verify first-time social sign-in creates exactly one local identity row and one external-login association row.
8. Verify signing in again with the same provider reuses the same local account.
9. Verify a provider returning an email that already belongs to a local-password account produces an explicit link-confirmation path instead of a duplicate account.
10. Run `dotnet test backend/Blinder.sln`.

### Project Structure Notes

- Prefer extending existing Razor Page models in `backend/src/Blinder.IdentityServer/Pages/Account/` or adding a small adjacent page model there; avoid introducing a new auth subsystem elsewhere.
- Keep any new auth-only service abstractions under `backend/src/Blinder.IdentityServer/` rather than `Blinder.Api`.
- Keep tests in `backend/tests/Blinder.IdentityServer.Tests/` near the current login and register tests.
- No changes should be required in `mobile/blinder-app/` for this story beyond consuming the existing IdentityServer OIDC flow later in Story 2.7.

### References

- Epics: `_bmad-output/planning-artifacts/epics.md` - Epic 2, Story 2.3
- Architecture: `_bmad-output/planning-artifacts/architecture.md` - Authentication & Security, Architectural Boundaries
- Previous story: `_bmad-output/implementation-artifacts/2-2-local-account-registration-and-sign-in-via-identityserver-oidc.md`
- Identity host setup: `backend/src/Blinder.IdentityServer/Program.cs`
- OIDC authorize flow: `backend/src/Blinder.IdentityServer/Pages/Connect/Authorize.cshtml.cs`
- Current login/register flow: `backend/src/Blinder.IdentityServer/Pages/Account/Login.cshtml.cs`, `backend/src/Blinder.IdentityServer/Pages/Account/Register.cshtml.cs`
- Identity persistence boundary: `backend/src/Blinder.IdentityServer/Persistence/IdentityDbContext.cs`, `backend/src/Blinder.IdentityServer/Persistence/ApplicationUser.cs`
- Existing identity migration: `backend/src/Blinder.IdentityServer/Persistence/Migrations/20260428084114_AddIdentityAndOpenIddict.cs`
- Current OpenIddict/OIDC client registration: `backend/src/Blinder.IdentityServer/Workers/OpenIddictSeeder.cs`
- Official guidance: Microsoft Learn, "External provider authentication in ASP.NET Core Identity" (updated 2026-04-28)
- Official guidance: OpenIddict documentation, "Web providers" and "Integrating with a remote server instance"
- Official guidance: Apple Developer documentation, "Configuring your webpage for Sign in with Apple"

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- Story context created on 2026-05-04.
- Implementation completed on 2026-05-04.

### Completion Notes List

- Created a ready-for-dev story file for upstream social federation in `Blinder.IdentityServer`.
- Captured repo-specific guardrails from Stories 2.1 and 2.2 so dev work stays inside the existing IdentityServer auth flow.
- Added current-version and provider-specific constraints for OpenIddict 7.5.0, forwarded headers, and Apple callback rules.
- Added `OpenIddict.Client.AspNetCore`, `OpenIddict.Client.SystemNetHttp`, and `OpenIddict.Client.WebIntegration` packages at 7.5.0.
- Restructured `Program.cs` to conditionally register the OpenIddict client block only when at least one social provider has credentials configured; prevents startup validation failure in environments with no provider secrets.
- Extended `Login.cshtml.cs` with `IAuthenticationSchemeProvider` injection, `ExternalProviders` population in `OnGetAsync`, and `OnPostExternalLogin` handler.
- Added social sign-in buttons section to `Login.cshtml`.
- Created `ExternalLogin.cshtml.cs` callback handler: tries existing login mapping, creates new user if email is unused, redirects to `LinkAccount` on email collision.
- Created `ExternalLogin.cshtml` minimal error/processing view.
- Created `LinkAccount.cshtml.cs` explicit link-confirmation page using `CheckPasswordSignInAsync` before `AddLoginAsync`.
- Created `LinkAccount.cshtml` Bootstrap 5 form.
- Added 7 unit tests in `ExternalLoginModelTests.cs` covering all callback scenarios.
- Updated `LoginModelTests.cs` for the new `IAuthenticationSchemeProvider` constructor parameter.
- All 45 tests across all projects pass with 0 failures.

### File List

- `_bmad-output/implementation-artifacts/2-3-social-federation-via-identityserver-google-apple-and-facebook.md`
- `backend/src/Blinder.IdentityServer/Blinder.IdentityServer.csproj`
- `backend/src/Blinder.IdentityServer/Program.cs`
- `backend/src/Blinder.IdentityServer/Pages/Account/Login.cshtml`
- `backend/src/Blinder.IdentityServer/Pages/Account/Login.cshtml.cs`
- `backend/src/Blinder.IdentityServer/Pages/Account/ExternalLogin.cshtml`
- `backend/src/Blinder.IdentityServer/Pages/Account/ExternalLogin.cshtml.cs`
- `backend/src/Blinder.IdentityServer/Pages/Account/LinkAccount.cshtml`
- `backend/src/Blinder.IdentityServer/Pages/Account/LinkAccount.cshtml.cs`
- `backend/tests/Blinder.IdentityServer.Tests/ExternalLoginModelTests.cs`
- `backend/tests/Blinder.IdentityServer.Tests/LoginModelTests.cs`

### Change Log

| Date | Change | Author |
|---|---|---|
| 2026-05-04 | Initial implementation: social federation via OpenIddict web providers, ExternalLogin and LinkAccount Razor Pages, 7 new unit tests, all 45 tests green | claude-sonnet-4-6 |