# Story 2.2: User Login and Logout

Status: ready-for-dev

**Updated 2026-03-25:** Redesigned to use OpenIddict (Story 2-0). All token issuance is handled by `Blinder.IdentityServer`'s ROPC grant endpoint. This story is primarily a **mobile-side story**: login screen, authService, auto-refresh interceptor, and logout flow. No new backend login endpoint is required — the OAuth2 token endpoint is the login endpoint.

---

## Story

As a registered user,
I want to log in with my email and password, stay logged in automatically, and log out securely,
So that I can access Blinder without re-entering credentials and know my session is secure.

---

## Acceptance Criteria

### 1. Login Screen Implemented

**Given** a user navigates to the login screen
**When** the screen renders
**Then**
- `TextField` for email (keyboard: `email-address`, autocomplete: `email`)
- `TextField` for password (`secureTextEntry`, autocomplete: `current-password`)
- `Button` with "Log in" label + `isLoading` state during request
- `ErrorBanner` displaying error message on failure
- Link/button to registration screen
- All components from shared library (`mobile/components/shared/`) — no inline primitives

---

### 2. Login Calls OAuth2 ROPC Endpoint (Blinder.IdentityServer)

**Given** a user submits valid email + password on the login screen
**When** `authService.login()` executes
**Then**
- `POST /api/auth/oauth/token` is called with:
  ```
  grant_type=password
  client_id=blinder-mobile
  username=<email>
  password=<password>
  scope=email api
  ```
- `scope=email api` is required — the `api` scope causes IdentityServer to put `"blinder-api"` in the token's `aud` claim, which `Blinder.Api` validates; omitting it produces tokens that `Blinder.Api` rejects
- Response contains `access_token`, `refresh_token`, `expires_in`, `token_type: "Bearer"`
- Both tokens stored via `storageService.setTokens(access, refresh, expiresIn)` (SecureStore)
- User navigated to home screen on success

---

### 3. Invalid Credentials Return Error

**Given** a user submits wrong email or password
**When** the token endpoint returns 401 `invalid_grant`
**Then**
- `ErrorBanner` shows "Invalid email or password" (no user enumeration hint)
- Form is not cleared (user can correct credentials)
- No stack trace or server error detail shown

---

### 4. Automatic Token Refresh (5-minute Pre-expiry Buffer)

**Given** the access token will expire within 5 minutes
**When** `apiClient.ts` intercepts an outgoing request
**Then**
- `storageService.isTokenExpiringSoon()` returns `true`
- `authService.refreshToken()` is called: `POST /api/auth/oauth/token` with `grant_type=refresh_token`
- New token pair is stored via `storageService.setTokens()`
- Original request is retried with the new access token
- Concurrent requests during refresh are queued (not each triggering a separate refresh)

---

### 5. Expired Token Handled (401 Recovery)

**Given** an access token has actually expired and the server returns 401
**When** `apiClient.ts` intercepts the 401 response
**Then**
- Token refresh is attempted once (if refresh token is still valid)
- If refresh succeeds: original request is retried
- If refresh fails (refresh token expired/revoked): `storageService.clearTokens()` + navigate to login screen
- No retry loop — exactly one refresh attempt before re-login

---

### 6. Logout Revokes Tokens and Clears Local Storage

**Given** a user taps "Log out"
**When** `authService.logout()` executes
**Then**
- `POST /api/auth/oauth/revoke` called on `Blinder.IdentityServer` with the **refresh token** (not the access token — revoking the refresh token triggers OpenIddict's cascade revocation of all related access tokens; revoking only the access token has minimal effect since it expires in 15 minutes and no cascade occurs)
- `storageService.clearTokens()` called regardless of revocation result (client always clears)
- User navigated to login screen
- Revocation is best-effort: if network is unavailable, local tokens are still cleared

---

### 7. No `AsyncStorage` for Tokens

**Given** any auth token operation (store, read, delete)
**When** the code is inspected
**Then**
- `AsyncStorage` is never imported in `storageService.ts` or `authService.ts`
- All token operations go through `expo-secure-store` exclusively
- `storageService` API from Story 2-0 (`getAccessToken`, `getRefreshToken`, `setTokens`, `clearTokens`, `isTokenExpiringSoon`) is used — not reimplemented inline

---

### 8. Authenticated State Persisted Across App Restarts

**Given** a user has logged in and closes + reopens the app
**When** the app launches
**Then**
- `storageService.getAccessToken()` returns a non-null token
- If token is not expiring soon: user goes directly to home screen (no login required)
- If token is expiring soon: refresh is attempted before navigation
- If no tokens stored: user goes to login screen

---

## Tasks / Subtasks

### Task 1: Create Login Screen (AC: 1, 3)

- [ ] Create `mobile/app/(auth)/login.tsx`:
  - Use `TextField` (email + password), `Button`, `ErrorBanner` from shared library
  - `AsyncState<void>` via `useLogin()` hook (never raw try/catch in component)
  - On success: navigate to `/(tabs)/` (authenticated root)
  - On failure: display error in `ErrorBanner`
  - Accessibility: `accessibilityRole`, `accessibilityLabel` on all interactive elements
  - Keyboard: `returnKeyType="next"` on email field, `returnKeyType="done"` on password

### Task 2: Implement `useLogin` Hook (AC: 2, 3)

- [ ] Create `mobile/hooks/useLogin.ts`:
  ```typescript
  export function useLogin() {
    const [state, setState] = useState<AsyncState<void>>({
      data: null, error: null, isLoading: false
    });

    async function login(email: string, password: string) {
      setState({ data: null, error: null, isLoading: true });
      const result = await authService.login(email, password);
      if (result.ok) {
        setState({ data: undefined, error: null, isLoading: false });
        // navigation handled by caller
      } else {
        setState({ data: null, error: result.error, isLoading: false });
      }
    }

    return { ...state, login };
  }
  ```
- [ ] Error strings from `constants/errors.ts` only (no inline string literals)

### Task 3: Implement `authService.login()` and `authService.logout()` (AC: 2, 6)

- [ ] Add to `mobile/services/authService.ts`:
  ```typescript
  export const authService = {
    // existing: register()

    async login(email: string, password: string): Promise<{ ok: true } | { ok: false; error: string }> {
      try {
        const response = await fetch(`${API_BASE_URL}/api/auth/oauth/token`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: new URLSearchParams({
            grant_type: 'password',
            client_id: 'blinder-mobile',
            username: email,
            password,
            scope: 'email api',  // "api" scope puts "blinder-api" in aud; required for Blinder.Api to accept tokens
          }).toString(),
        });

        if (!response.ok) {
          return { ok: false, error: ERRORS.INVALID_CREDENTIALS };
        }

        const data = await response.json();
        await storageService.setTokens(data.access_token, data.refresh_token, data.expires_in);
        return { ok: true };
      } catch {
        return { ok: false, error: ERRORS.NETWORK_ERROR };
      }
    },

    async logout(): Promise<void> {
      const refreshToken = await storageService.getRefreshToken();
      if (refreshToken) {
        // Best-effort revocation of the REFRESH TOKEN (not access token).
        // Revoking the refresh token triggers OpenIddict cascade revocation of all
        // related access tokens. Revoking only the access token has minimal effect
        // since it expires in 15 minutes and causes no cascade.
        fetch(`${API_BASE_URL}/api/auth/oauth/revoke`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
          },
          body: new URLSearchParams({ token: refreshToken }).toString(),
        }).catch(() => {}); // Ignore network errors — local clear is authoritative
      }
      await storageService.clearTokens();
    },

    async refreshToken(): Promise<boolean> {
      const refreshToken = await storageService.getRefreshToken();
      if (!refreshToken) return false;

      try {
        const response = await fetch(`${API_BASE_URL}/api/auth/oauth/token`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: new URLSearchParams({
            grant_type: 'refresh_token',
            client_id: 'blinder-mobile',
            refresh_token: refreshToken,
            // scope is preserved from the original token by OpenIddict automatically
          }).toString(),
        });

        if (!response.ok) {
          await storageService.clearTokens();
          return false;
        }

        const data = await response.json();
        await storageService.setTokens(data.access_token, data.refresh_token, data.expires_in);
        return true;
      } catch {
        return false;
      }
    },
  };
  ```
- [ ] Add `INVALID_CREDENTIALS` and `NETWORK_ERROR` to `constants/errors.ts`
- [ ] `API_BASE_URL` from environment config — not hardcoded

### Task 4: Add Auth Interceptor to `apiClient.ts` (AC: 4, 5)

- [ ] Update `mobile/services/apiClient.ts` to attach Bearer token and handle refresh:
  ```typescript
  // Token refresh lock — prevents concurrent refresh stampede
  let isRefreshing = false;
  let refreshQueue: Array<(token: string | null) => void> = [];

  async function getValidAccessToken(): Promise<string | null> {
    if (await storageService.isTokenExpiringSoon()) {
      if (isRefreshing) {
        // Queue this request until refresh completes
        return new Promise(resolve => refreshQueue.push(resolve));
      }
      isRefreshing = true;
      const success = await authService.refreshToken();
      const newToken = success ? await storageService.getAccessToken() : null;
      refreshQueue.forEach(resolve => resolve(newToken));
      refreshQueue = [];
      isRefreshing = false;
      return newToken;
    }
    return storageService.getAccessToken();
  }
  ```
- [ ] Attach `Authorization: Bearer <token>` header on every authenticated request
- [ ] On 401 response: attempt one refresh, retry request, then clear tokens + trigger re-login on second 401
- [ ] Unauthenticated requests (register, login) bypass the interceptor

### Task 5: App Startup Auth State Check (AC: 8)

- [ ] Update `mobile/app/index.tsx` (or root layout):
  - On app launch: check `storageService.getAccessToken()`
  - If token exists + not expiring soon → navigate to `/(tabs)/`
  - If token expiring soon → `authService.refreshToken()` → navigate to `/(tabs)/` or `/(auth)/login`
  - If no tokens → navigate to `/(auth)/login`
  - Show `LoadingIndicator` during check (never a blank screen)

### Task 6: Tests (AC: 1-8)

- [ ] Mobile unit tests:
  - `authService.login()`: valid credentials → tokens stored, invalid → error returned
  - `authService.logout()`: revocation called (best-effort), tokens always cleared
  - `authService.refreshToken()`: valid refresh → new tokens stored, invalid refresh → tokens cleared + returns false
  - Refresh queue: concurrent requests during refresh → only one refresh call made
- [ ] Integration tests (against real IdentityServer, or `WebApplicationFactory`):
  - ROPC login → 200 with tokens
  - Invalid credentials → 401 `invalid_grant`
  - Refresh token → new token pair
  - Refresh replay → 401 (OpenIddict rejects reused refresh token)
  - Revocation → revoked token rejected on next API request

---

## Dev Notes

### Architecture: Mobile → Nginx → IdentityServer / Blinder.Api

```
Mobile App
  ├── POST /api/auth/oauth/token (login)          → Nginx → Blinder.IdentityServer
  ├── POST /api/auth/oauth/token (refresh)        → Nginx → Blinder.IdentityServer
  ├── POST /api/auth/oauth/revoke (logout)        → Nginx → Blinder.IdentityServer
  │     body: token=<refresh_token>               ← refresh token, not access token
  └── GET/POST /api/** (resource calls)           → Nginx → Blinder.Api
```

Mobile uses ONE base URL (Nginx). Nginx routes OAuth2 paths to IdentityServer, everything else to Blinder.Api. The mobile app never addresses containers directly.

### Why No `POST /api/auth/login` Endpoint in Blinder.Api

The OAuth2 ROPC token endpoint **is** the login endpoint. A thin wrapper in `Blinder.Api` that proxies to IdentityServer would add latency and complexity without benefit. Story 2-2 embraces this: login = calling IdentityServer ROPC directly via Nginx routing.

Device push token cleanup (clearing `DeviceTokens` on logout) is deferred to Story 5.4 where the feature is introduced. When Story 5.4 is done, either:
a) A `POST /api/auth/logout` endpoint is added to Blinder.Api (calls revocation + clears device token)
b) Or the mobile explicitly calls both endpoints

### Token Endpoint Request Format (application/x-www-form-urlencoded)

OpenIddict requires **form-encoded body**, not JSON. This is RFC 6749 compliant. The `Content-Type` header must be `application/x-www-form-urlencoded`. Sending JSON will return `unsupported_media_type`.

### storageService Contract (from Story 2-0)

`storageService` was fully implemented in Story 2-0. Do NOT reimplement any part of it in this story:
- `getAccessToken()` — reads from SecureStore
- `getRefreshToken()` — reads from SecureStore
- `setTokens(access, refresh, expiresIn)` — stores all three (access, refresh, expiry timestamp)
- `clearTokens()` — removes all three
- `isTokenExpiringSoon()` — checks 5-minute buffer against stored expiry timestamp

### Refresh Token Rotation (OpenIddict Automatic)

OpenIddict marks the old refresh token as `redeemed` on every use. The client receives a new refresh token on every successful refresh call. **Store the new refresh token immediately** (already done via `storageService.setTokens()`). If the new refresh token is lost, the user must re-authenticate.

### Rate Limiting (Already Configured in Story 2-0)

The token endpoint has a 5-requests-per-minute rate limit (configured in IdentityServer, Story 2-0 Task 6). The mobile client should handle 429 responses gracefully — show a "Too many attempts, try again later" error in `ErrorBanner`.

### Architecture Compliance Guardrails

- `authService.ts` calls `/api/auth/oauth/token` and `/api/auth/oauth/revoke` directly — not a custom `POST /api/auth/login`
- All token storage via `storageService` (SecureStore) — never `AsyncStorage`
- Login screen uses shared components from `mobile/components/shared/` (Rule 18)
- `AsyncState<T>` from `useLogin` hook — no raw try/catch in `login.tsx` (Rule 9)
- Error strings from `constants/errors.ts` — no inline string literals
- `DateTimeOffset` / ISO 8601 not applicable to mobile-side code but `expiresIn` (seconds) converted to `Date.now() + expiresIn * 1000` for expiry tracking

### Previous Story Intelligence

**From Story 2-0:**
- `storageService.ts` is fully implemented — use it, don't duplicate
- `Blinder.IdentityServer` is running with `blinder-mobile` client seeded
- Token endpoint: `POST /api/auth/oauth/token` (via Nginx)
- Revocation endpoint: `POST /api/auth/oauth/revoke` (via Nginx)
- Access token: 15-minute expiry; Refresh token: 30-day rolling
- `OpenIddict.Validation.AspNetCore` is configured in `Blinder.Api` — `[Authorize]` works on resource endpoints

**From Story 2-1-A:**
- `TextField`, `Button`, `ErrorBanner`, `LoadingIndicator` are all in `mobile/components/shared/`
- Rule 18: no inline component definitions in screen files

### File Touchpoints

Backend:
- `backend/Blinder.Api/Program.cs` — no changes (OpenIddict validation already done in Story 2-0)
- `backend/Blinder.IdentityServer/` — no changes (token endpoint done in Story 2-0)

Mobile:
- `mobile/app/(auth)/login.tsx` — NEW: login screen
- `mobile/hooks/useLogin.ts` — NEW: login hook
- `mobile/services/authService.ts` — UPDATED: add `login()`, `logout()`, `refreshToken()`
- `mobile/services/apiClient.ts` — UPDATED: auth interceptor with refresh queue
- `mobile/app/index.tsx` — UPDATED: startup auth check
- `mobile/constants/errors.ts` — UPDATED: add `INVALID_CREDENTIALS`, `NETWORK_ERROR`

---

## Definition of Done

- [ ] Login screen renders with accessible shared components
- [ ] `authService.login()` calls ROPC endpoint, stores tokens
- [ ] `authService.logout()` calls revocation + clears tokens
- [ ] `authService.refreshToken()` called by apiClient interceptor on expiry
- [ ] Concurrent refresh requests are queued (not stampeded)
- [ ] App startup routes correctly based on stored token state
- [ ] Invalid credentials show user-friendly error in `ErrorBanner`
- [ ] 429 (rate limit) handled gracefully in UI
- [ ] `AsyncStorage` is not used anywhere for auth tokens (verified by search)
- [ ] Unit tests for authService and refresh interceptor
- [ ] Integration tests against IdentityServer token endpoint
- [ ] Code review complete before Stories 2-3, 2-4 begin

---

## References

- Story 2-0: `_bmad-output/implementation-artifacts/2-0-oauth2-oidc-foundation.md`
- storageService: `mobile/services/storageService.ts`
- authService (current): `mobile/services/authService.ts`
- Shared components: `mobile/components/shared/`
- Errors: `mobile/constants/errors.ts`
- Project rules: `docs/project-context.md` (Rules 6, 9, 10, 18)
