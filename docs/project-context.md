# project-context.md

See [tech-preferences.md](./tech-preferences.md) for full stack standards.

---

## 17. Non-Negotiable Enforcement Rules

Every developer and every AI agent **must** follow these without exception:

1. `snake_case` DB names via `UseSnakeCaseNamingConvention()` — never `PascalCase` columns in migrations
2. Direct response body on success; RFC 7807 Problem Details on all 4xx and 5xx — no wrapper objects
3. `DateTimeOffset` in C#, `timestamptz` in PostgreSQL, ISO 8601 in API contracts — no exceptions
4. Mapperly for all Entity ↔ DTO mapping — no manual property copying
5. FluentValidation for all input validation — no inline validation in controllers
6. `expo-secure-store` for all token storage on mobile — `AsyncStorage` prohibited for auth tokens
7. Never log PII in structured log properties
8. Never auto-apply EF Core migrations on startup outside local development — `Development` may create/migrate the DB automatically, but all shared environments must use the checked-in idempotent SQL script via `docker compose exec -T db psql`
9. `AsyncState<T>` from all async hooks in mobile — never raw `try/catch` in components
10. Never expose raw exception messages or stack traces in API responses or mobile UI
11. Never run any backend service directly on the host OS — Docker Compose always
12. Keep `.env.example` in sync — new variable = update `.env.example` in the same commit
13. `docker compose down -v` is **prohibited** in production — destroys `db-data` volume
14. `SkipWebhookVerification` must never be `true` in production — startup assertion enforces this
15. WCAG 2.1 AA on all core flows — not optional, not post-launch
16. Every story that changes public API surface, architecture, setup steps, environment variables, or operational behaviour **must** include a documentation update in the same commit: update `README.md` for general project-level information (how to run, configure, or operate the project), or create/update a dedicated `docs/*.md` file for focused topics (e.g. `docs/database.md`, `docs/api.md`, `docs/architecture.md`). `README.md` stays high-level; detailed domain-specific content belongs in its own file
17. Identity auth logic is single-source: scaffolded Razor PageModels and mobile API auth endpoints must share one Identity-backed ruleset; mobile UI must remain native and must not consume scaffolded Razor pages directly
18. Never define UI components inline in screen files. Before building any UI element, check `mobile/components/` first — if a matching component exists, use it. If it does not exist, create it in the correct subdirectory (`shared/`, `chat/`, `match/`, `moderation/`, or `onboarding/`) with a typed props interface, theme tokens, and accessibility attributes, then register it in `docs/component-library.md`. Inline `TextInput`, `Pressable`-as-button, local loading animations, or ad-hoc error displays inside screen files are prohibited after Story 2-1-A.

---

## 18. OAuth2/OIDC Authentication Enforcement Rules (Story 2-0+)

Established in Story 2-0 (OAuth2/OIDC Foundation — OpenIddict); **enforced across all authentication flows**:

19. **OpenIddict in `Blinder.IdentityServer` is the single source of all token issuance.** No ad-hoc JWT generation in controllers, services, or any other code path. All token endpoint requests (ROPC, social login authorization code, refresh, revocation) go through OpenIddict's `OAuth2Controller` passthrough. No custom `OAuth2TokenService` class exists.

20. **Token lifetimes:** Access tokens are **15 minutes**. Refresh tokens are **30 days rolling** (OpenIddict rotates on every use: old token marked `redeemed`, new pair issued). A compromised 15-minute access token cannot be revoked mid-window — the short lifetime is the protection. Do not increase access token lifetime without understanding the remote validation architecture.

21. **Refresh tokens are stored encrypted by OpenIddict** (ASP.NET Core Data Protection) in the `OpenIddictTokens` table. There is no manual SHA-256 hashing or custom `RefreshTokens` table. Replay protection is automatic: OpenIddict rejects any `redeemed` or `revoked` token.

22. **Logout = revocation at `Blinder.IdentityServer`:** All logout flows call `POST /api/auth/oauth/revoke` (OpenIddict's built-in endpoint). The mobile client additionally calls `storageService.clearTokens()` — always, regardless of revocation result. `Blinder.Api` does NOT have a `/api/auth/logout` endpoint until device push token cleanup is implemented (Story 5.4).

23. **Social login (Stories 2-3, 2-4) plugs into the same token endpoint via `ISocialLoginTokenValidator`:** Provider-specific validators implement `ISocialLoginTokenValidator` and are injected into `OAuth2Controller`. Authorization codes issued server-side are 10-minute expiry, one-time use. Token response format is identical to ROPC (access + refresh).

24. **`Blinder.Api` validates tokens remotely via OpenIddict OIDC discovery** — NOT JwtBearer middleware. `Blinder.Api` never issues tokens. Signing keys are fetched and cached via `UseSystemNetHttp()` from `Blinder.IdentityServer`'s `.well-known/openid-configuration`. Any addition of JwtBearer to `Blinder.Api` is a violation.

25. **Two DbContexts, same database:** `AppDbContext` (in `Blinder.Api`) manages Identity and domain tables. `OpenIddictDbContext` (in `Blinder.IdentityServer`) manages only the 4 OpenIddict tables. Both contexts share the same PostgreSQL connection string. EF migrations for OpenIddict tables run from `Blinder.IdentityServer/` only.

---