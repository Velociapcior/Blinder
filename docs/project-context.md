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

---