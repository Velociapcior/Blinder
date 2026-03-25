# Story 2.2: User Login, JWT Tokens, and Logout

Status: ready-for-dev

## Story

As a registered user,
I want to log in with my credentials and have my session managed securely,
so that I can access Blinder without re-entering my credentials and know my tokens are stored safely.

## Acceptance Criteria

1. Given a `POST /api/auth/login` request with valid email and password
When credentials are verified
Then a JWT access token is returned; the mobile client stores it exclusively via `expo-secure-store` (mapped to iOS Keychain / Android Keystore)

2. Given login is supported by both web/admin and mobile channels
When authentication logic is reviewed
Then scaffolded Identity login flow and API login endpoint share one Identity-backed validation/sign-in ruleset; mobile does not consume scaffolded Razor pages directly

3. Given the mobile client receives a JWT
When `storageService.ts` persists the token
Then `SecureStore.setItemAsync` is called; `AsyncStorage` is never used for token storage

4. Given a JWT token is issued
When the token's expiry is inspected
Then the token expires after 30 days of inactivity (NFR10); the `exp` claim reflects this

5. Given a `POST /api/auth/logout` request is made
When the logout succeeds
Then the device token for this session is removed from the `DeviceTokens` table and the client clears the stored JWT from `expo-secure-store`

6. Given an API request is made with an expired or invalid token
When the JwtBearer middleware processes the request
Then a 401 Unauthorized Problem Details response is returned; no stack trace is exposed

## Tasks / Subtasks

- [ ] Task 1: Implement shared Identity-backed login service (AC: 1, 2, 4)
- [ ] Create `ILoginService` and `LoginService` in backend auth/service layer using `SignInManager<ApplicationUser>` and `UserManager<ApplicationUser>`
- [ ] Ensure API login path and Razor PageModel path call the same shared login service (single ruleset)
- [ ] Generate JWT with inactivity timeout policy of 30 days and include proper `exp` claim
- [ ] Use `DateTimeOffset` for all token timestamp calculations

- [ ] Task 2: Add API login endpoint and contract validation (AC: 1, 2, 6)
- [ ] Add `POST /api/auth/login` endpoint under auth controllers
- [ ] Create `LoginRequest` DTO and `LoginRequestValidator` (`FluentValidation`) for required email/password format
- [ ] Return RFC 7807 Problem Details for invalid credentials (401) and bad input (400)
- [ ] Ensure no stack traces/raw exceptions in responses

- [ ] Task 3: Implement API logout endpoint and token/device cleanup (AC: 5)
- [ ] Add `POST /api/auth/logout` endpoint requiring authenticated user
- [ ] Remove current session device token from `DeviceTokens` table using authenticated user/session context
- [ ] Return success response and ensure idempotent behavior for missing/already-deleted token record

- [ ] Task 4: Mobile login flow integration with secure token storage (AC: 1, 3, 5)
- [ ] Implement/update mobile auth service login method to call `POST /api/auth/login`
- [ ] Persist JWT only through `SecureStore.setItemAsync` in `storageService.ts`
- [ ] Ensure logout flow calls `POST /api/auth/logout` and then clears JWT via `SecureStore.deleteItemAsync`
- [ ] Verify `AsyncStorage` is not imported/used for auth token operations

- [ ] Task 5: JwtBearer failure behavior and Problem Details consistency (AC: 6)
- [ ] Ensure expired/invalid token requests yield 401 Problem Details shape
- [ ] Verify `AddProblemDetails()` + `UseExceptionHandler()` behavior remains intact for auth failures
- [ ] Keep problem type URIs centralized through `AppErrors.cs`

- [ ] Task 6: Tests (AC: 1-6)
- [ ] Add backend tests for login success/failure, invalid credentials, and token expiry claim content
- [ ] Add backend tests confirming API and Razor auth paths share the same login ruleset/service
- [ ] Add backend tests for logout removing `DeviceTokens` row and idempotent behavior
- [ ] Add mobile tests for `storageService`/auth service ensuring SecureStore-only usage and logout token clearing
- [ ] Add/adjust integration tests for 401 Problem Details on invalid/expired JWT

## Dev Notes

### Story Scope and Intent

This story establishes secure login/logout session management on top of the Identity foundation implemented in Story 2.1 and the shared component baseline from Story 2-1-A. The core risk to avoid is auth rules drift between web/admin Identity flows and mobile API flows.

### Architecture Compliance Guardrails

- Identity auth logic is single-source. Razor PageModels and API endpoints must share one Identity-backed ruleset/service.
- Mobile UI remains native React Native; never consume scaffolded Razor pages in mobile.
- Store auth tokens only with `expo-secure-store`; `AsyncStorage` is prohibited for auth tokens.
- All date/time fields and token time calculations use `DateTimeOffset`.
- All auth input validation uses FluentValidation, not inline controller checks.
- Errors follow RFC 7807 Problem Details for all 4xx/5xx responses.
- Keep `AppErrors.cs` as the single source for problem type URIs.
- Use existing DTO mapping patterns with Mapperly where entity/DTO conversion is required.

### Technical Requirements

- JWT must use configured issuer/audience/signing key with explicit `exp` and enforce 30-day inactivity policy.
- JwtBearer middleware should return 401 for expired or invalid tokens without leaking internal details.
- Consider explicit token validation clock skew configuration to avoid hidden expiry drift.
- Logout must clean up session-related device token record to prevent stale push routing.
- Logout path should be safe to call repeatedly (idempotent from client perspective).

### Library and Framework Requirements

- Backend: ASP.NET Core Identity, JwtBearer, FluentValidation, Mapperly, Problem Details middleware.
- Mobile: Expo SDK 55 auth flow using `expo-secure-store` (`setItemAsync`, `getItemAsync`, `deleteItemAsync`).
- Do not introduce alternate auth/token libraries unless required by existing architecture constraints.

### File Structure Requirements

Expected backend touchpoints:

- `backend/Blinder.Api/Controllers/Auth/` (login/logout endpoints)
- `backend/Blinder.Api/Services/` (shared login/logout service)
- `backend/Blinder.Api/Validators/` (login request validation)
- `backend/Blinder.Api/Errors/AppErrors.cs`
- `backend/Blinder.Api/Program.cs` (auth/JwtBearer setup updates only if required)
- `backend/Blinder.Tests/` (unit/integration auth tests)

Expected mobile touchpoints:

- `mobile/services/authService.ts`
- `mobile/services/storageService.ts`
- `mobile/app/(auth)/login.tsx` (if present/added in this story implementation)
- `mobile/types/api/index.ts`

### Testing Requirements

- Unit tests for login service credential validation and JWT generation claims.
- Integration tests for `POST /api/auth/login` and `POST /api/auth/logout` happy/error paths.
- Auth middleware tests proving expired/invalid JWT returns 401 Problem Details.
- Mobile tests proving token is written and deleted through SecureStore only.
- Regression checks to ensure Story 2.1 registration/auth foundation behavior remains intact.

### Previous Story Intelligence

From Story 2.1 (`2-1-email-password-user-registration.md`):

- Scaffolded Identity output may introduce incorrect types (`IdentityUser` vs `ApplicationUser`) and duplicate data context files. Reconcile immediately with existing `AppDbContext` and custom user model.
- Shared service pattern (`IRegistrationService`) was introduced to prevent rules drift. Follow the same pattern for login/logout (`ILoginService` or equivalent).
- Duplicate email and validation handling already standardized through `AppErrors` + Problem Details. Reuse that error architecture for invalid credentials/session failures.
- Existing test suite style in `backend/Blinder.Tests/Registration/*` should be mirrored for auth tests.

From Story 2-1-A (`2-1-a-shared-mobile-component-library.md`):

- UI components should come from shared library; do not add inline form primitives for login screen.
- Rule 18 in project context prohibits inline UI component definitions in screens.

### Git Intelligence Summary

Recent commits show sequence: registration screen implementation, tech reference fixes, register layout correction, and shared component library completion (`story 2-1-A`).

Actionable implications:

- Keep Story 2.2 focused on auth/session behavior, not reworking component architecture.
- Reuse established patterns from immediate prior commits instead of creating parallel auth implementations.

### Latest Technical Information

- Expo SecureStore remains the correct secure token storage layer for SDK 55 (`expo-secure-store` bundled ~55.0.9), with async APIs (`setItemAsync`, `getItemAsync`, `deleteItemAsync`) preferred for non-blocking operations.
- On mobile platforms, SecureStore maps to encrypted storage (iOS Keychain / Android Keystore-backed storage path), aligning with project security constraints.
- ASP.NET token validation supports explicit `ClockSkew` configuration (`TokenValidationParameters.ClockSkew`); set intentionally to avoid accidental grace-period behavior that weakens expiry semantics.

### Project Context Reference

Key enforced rules for this story:

- Rule 3: `DateTimeOffset` only
- Rule 5: FluentValidation required
- Rule 6: SecureStore only for auth tokens
- Rule 10: No raw exception details
- Rule 17: Shared Identity auth logic across Razor and mobile API
- Rule 18: No inline UI components in screen files

## References

- `_bmad-output/planning-artifacts/epics.md` (Epic 2, Story 2.2)
- `_bmad-output/planning-artifacts/architecture.md` (Identity/JWT, Problem Details, SecureStore constraints)
- `_bmad-output/planning-artifacts/prd.md` (NFR10 token inactivity expiry)
- `docs/project-context.md` (Rules 3, 5, 6, 10, 17, 18)
- `_bmad-output/implementation-artifacts/2-1-email-password-user-registration.md`
- `_bmad-output/implementation-artifacts/2-1-a-shared-mobile-component-library.md`

## Story Completion Status

- Story context generated with cross-artifact analysis and implementation guardrails.
- Status set to `ready-for-dev`.
- Completion note: Ultimate context engine analysis completed - comprehensive developer guide created.

## Dev Agent Record

### Agent Model Used

GPT-5.3-Codex

### Debug Log References

- N/A (story creation artifact)

### Completion Notes List

- Story created from Epic 2.2 with architecture, UX, and prior-story constraints integrated.
- Added explicit anti-drift rule for Identity-backed auth between Razor and API/mobile paths.
- Included test strategy and regression guardrails for token expiry/logout semantics.

### File List

- `_bmad-output/implementation-artifacts/2-2-user-login-jwt-tokens-and-logout.md`
