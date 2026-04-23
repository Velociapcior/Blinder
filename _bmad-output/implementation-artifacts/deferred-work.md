# Deferred Work

## Deferred from: code review of 1-4-scaffold-mobile-app-with-tamagui-expo-router-starter (2026-04-23)

- **Android package name placeholder** — `app.json` sets `"android.package": "com.anonymous.blinderapp"` (Expo default stub). Must be replaced with the real reverse-domain identifier (e.g. `com.blinder.app`) before any Play Store submission, APK signing, or deep-link registration. All downstream stories that add push notifications or deep links depend on this being correct.
- **Android / iOS native-build validation** — `expo run:android` failed on Windows (ReadableStream unpacking error); `expo run:ios` blocked by macOS requirement. Accepted as host-constraint; full native-build validation must complete via EAS Build or a macOS CI agent before any release build is cut.

## Deferred from: code review of 1-3-postgresql-schema-separation-and-ef-core-migrations-pipeline (2026-04-21)

- BoundaryTests cannot detect wrong-schema DDL argument values inside a valid assembly — NetArchTest IL analysis cannot inspect runtime argument values; a migration in Blinder.Api could call `CreateTable(schema: "identity", ...)` and no test would catch it.
- GetRequiredConnectionString called before builder.Build() — future configuration providers added after `CreateBuilder` (e.g. Azure Key Vault) won't be visible when the connection string is resolved; theoretical concern with current stack.
- GetRequiredConnectionString duplicated verbatim in both Program.cs files — identical static local function; no shared infrastructure library is in scope for this story.
- adminpanel depends_on postgres removed without a code comment explaining intent — correct per spec (AdminPanel has no DB access in this story), but undocumented in the compose file.
- IntegrationTests project simultaneously references both persistence contexts with no architecture rule guarding it — intentional for the smoke test; test-only cross-boundary reference has no guardrail.
- SetBasePath(Directory.GetCurrentDirectory()) fragile in design-time factories — breaks if dotnet ef is run from outside the project directory; MIGRATIONS.md documents the exact commands as mitigation.
- Base appsettings.json lacks ConnectionStrings — design-time factory throws if ASPNETCORE_ENVIRONMENT is unset and no env vars are present; env var fallback covers normal usage.
- POSTGRES_HOST_PORT bound to 127.0.0.1 may not route correctly on Windows/WSL2 — correct security default; override with BLINDER_DB_HOST env var if needed.
