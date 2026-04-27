# Deferred Work

## Deferred from: code review of 1-5-design-system-foundation-warm-dusk-token-system (2026-04-26)

- **D1 — tsconfig.base.json ignoreDeprecations: "6.0"** — silences all TypeScript 6.0 deprecation warnings globally; deprecated options (`baseUrl`, `importHelpers`, `moduleResolution: node` etc.) will fail hard when eventually removed. Scaffold debt from Story 1.4.
- **D2 — letterSpacing not em-relative (React Native platform limitation)** — AC 3 requires `+0.04em` tracking; implementation uses absolute px (0.44 / 0.56). RN does not support em units for letterSpacing, so values don't scale with `maxFontSizeMultiplier: 1.3`. Not fixable without a custom Animated solution.
- **D3 — prefersReducedMotion has no OS change subscription** — `prefersReducedMotion()` is a one-shot async call; mid-session toggle of the OS reduced-motion setting is not reflected. Requires `AccessibilityInfo.addEventListener('reduceMotionChanged', …)` in animated components. Post-MVP accessibility enhancement.
- **D4 — allowedStyleValues: 'somewhat-strict-web' type-errors for elevation on Tamagui components** — `shadow.cta` / `shadow.modal` include the Android `elevation` prop; spreading them onto Tamagui `Stack`/`View` will produce type errors under `somewhat-strict-web`. No current violation; risk surfaces when native shadow use on Tamagui components begins.
- **D5 — AC 7: secondary contrast pairs undocumented** — Only `text.primary` (#2C1C1A) on `bg.base` (#FBF5EE) has a documented contrast ratio. `on-primary` on `primary`, `on-reveal` on `reveal`, `text.secondary` on `bg.surface` etc. should be verified and commented. Informational; WCAG compliance likely met given the palette.
- **D6 — AC 9: on-colour swatch pairs shown as isolated chips** — The `PaletteSection` renders `on-primary`, `on-reveal`, `on-dark` as standalone colour swatches rather than foreground-on-background paired tiles as shown in the HTML kit. Minor showcase fidelity gap.
- **D7 — AC 2: reveal token reservation not lint-enforced** — `$reveal` / `palette.reveal` can be used anywhere with no automated check. Adding a `no-restricted-syntax` ESLint rule is the right fix, but no product screens exist yet to enforce against. Revisit when reveal ceremony screen story (Epic 4+) begins.
- **D8 — duration.revealMax (2400ms) has no CSS source counterpart** — `colors_and_type.css` defines only `--dur-reveal: 1600ms`. The `revealMax: 2400` constant is additive (spec prose mentions 1600–2400ms range) but is not strictly one-to-one per AC 8. Acceptable as documented design intent.

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
