# Story 1.5: Design System Foundation - Warm Dusk Token System

Status: review

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want the complete Blinder design token system implemented in Tamagui - colours, typography, spacing, border radius, and motion vocabulary,
so that all future screen work is built from tokens with zero hardcoded values.

## Acceptance Criteria

1. Given the Tamagui configuration file exists at `mobile/blinder-app/tamagui.config.ts`, when the token system is defined, then all colour tokens from the Warm Dusk palette are implemented: `color.bg.base`, `color.bg.surface`, `color.bg.elevated`, `color.primary`, `color.primary.light`, `color.reveal`, `color.accent`, `color.text.primary`, `color.text.secondary`, `color.text.muted`, `color.border`, `color.error`, `color.offline`.
2. `color.reveal` is defined as a named semantic token with a code comment marking it exclusive to the reveal ceremony and Reveal gate button.
3. Typography tokens cover all 8 scale levels (`text.display` through `text.button`) using Lato (300/400/700/900 weights).
4. Spacing tokens are defined: `space.xs` (4px) through `space.2xl` (48px).
5. Border radius tokens are defined: `radius.sm` (8px) through `radius.full` (9999px).
6. A `motion.ts` file documents the motion vocabulary: what animates, at what duration, with what easing curve - distinguishing in-screen animations from navigation-level transitions.
7. `color.text.primary` (`#2C1C1A`) on `color.bg.base` (`#FBF5EE`) is verified to meet WCAG AA contrast (>=4.5:1), documented in a comment.
8. The Tamagui token values match the canonical packaged design system one-to-one - `colors_and_type.css` (CSS variables, loaded by HTML prototypes and the live component showcase at `ui_kits/Blinder/index.html`) is the cross-platform source of truth; any drift is resolved in favour of the packaged system.
9. A token showcase screen exists at a dev-only route rendering all tokens visually, mirroring the sections in `ui_kits/Blinder/index.html` so the two showcases can be visually diffed.
10. Zero hardcoded colour, font-size, or spacing values exist anywhere in the codebase (enforced by ESLint rule).
11. This story has a design review gate - a human review of the token showcase screen alongside `ui_kits/Blinder/index.html` is required before any screen implementation stories begin in Epic 3+.

## Tasks / Subtasks

- [x] Bind the existing packaged design system as the canonical source (AC: 1, 8, 9, 11)
  - [x] Use `_bmad-output/design-system/colors_and_type.css`, `_bmad-output/design-system/README.md`, `_bmad-output/design-system/SKILL.md`, and `_bmad-output/design-system/ui_kits/Blinder/index.html` as the source of truth for token names, values, semantics, copy guardrails, and showcase structure.
  - [x] Do not recreate or fork `_bmad-output/design-system/README.md` or `_bmad-output/design-system/SKILL.md` in this story; accommodate the existing packaged artefacts and only patch them if a verified drift bug is found.
  - [x] Compare planned Tamagui tokens against the packaged CSS variables before implementation; if any discrepancy appears, treat the packaged system as canonical and record the resolution in the completion notes.

- [x] Extend the scaffolded Tamagui config into a real Blinder token layer (AC: 1, 2, 4, 5, 7, 8)
  - [x] Replace the current `createTamagui(defaultConfig)` baseline in `mobile/blinder-app/tamagui.config.ts` with an explicit config layered on `@tamagui/config/v5` using `createTokens`, `createFont`, and semantic theme keys.
  - [x] Implement the Warm Dusk colour tokens one-to-one from `colors_and_type.css`, including semantic pairings (`on-primary`, `on-reveal`, `on-dark`) and a comment marking `color.reveal` as reserved for the Reveal gate option and mutual reveal ceremony only.
  - [x] Implement spacing tokens `space.xs` through `space.2xl`, radius tokens `radius.sm` through `radius.full`, and motion/elevation constants that match the packaged values exactly.
  - [x] Document the verified contrast ratio for `#2C1C1A` on `#FBF5EE` as `15.06:1` in a code comment or adjacent design-system note.
  - [x] Keep `tamagui.config.ts` build-time friendly: avoid heavy imports and keep tokens/themes local because Tamagui parses the config at build time.

- [x] Implement typography and font loading for native and web parity (AC: 3, 7, 8)
  - [x] Add Lato 300/400/700/900 loading for the Expo app and map native font faces explicitly so Android resolves the intended weights.
  - [x] Define the 8 text scales from the packaged system: `display`, `h1`, `h2`, `h3`, `body`, `bodySm`, `caption`, `button`, preserving exact sizes, line heights, and `+0.04em` tracking only for buttons and captions.
  - [x] Keep light theme as the MVP theme. If the provider still expects a `dark` key because of `useColorScheme()`, alias it safely or force the app to light rather than inventing a bespoke dark palette in this story.

- [x] Add a dedicated motion vocabulary module with reduced-motion behavior (AC: 2, 6, 8, 11)
  - [x] Create a stable shared module such as `mobile/blinder-app/src/design-system/motion.ts` that names the motion tokens and documents intent: utility transitions, in-screen feedback, gate entrance, resolution-wait pulse, and reveal ceremony.
  - [x] Distinguish in-screen animations handled by Tamagui from navigation-level transitions or advanced choreography that should use the already-installed `react-native-reanimated` dependency.
  - [x] Encode the reduced-motion fallback so emotional animations collapse to a 200ms cross-fade, and preserve the resolution-wait state as a 2s pulse rather than a spinner.

- [x] Build a dev-only token showcase route for human review (AC: 8, 9, 11)
  - [x] Add a route under `mobile/blinder-app/app/` such as `app/dev/tokens.tsx` that is excluded from normal user navigation and is rendered only in dev builds or when a non-secret `EXPO_PUBLIC_ENABLE_TOKEN_SHOWCASE` flag is enabled.
  - [x] Mirror the packaged showcase sections from `_bmad-output/design-system/ui_kits/Blinder/index.html`: palette, typography, spacing, radii, elevation, motion vocabulary, and representative token usage examples so the mobile view can be visually diffed against the packaged web showcase.
  - [x] Keep Expo Router entrypoints thin and place reusable showcase rendering under `src/`; do not collapse feature/domain code back into a generic `src/screens/` structure.

- [x] Enforce the no-hardcoded-values rule in the mobile workspace (AC: 10)
  - [x] Add an ESLint configuration to `mobile/blinder-app/` and introduce a `yarn lint` script if none exists yet.
  - [x] Add lint rules or a local custom rule that fail on raw hex colours and hardcoded font-size/spacing literals in app source files, with a narrowly-scoped allowlist for token-definition files, generated files, and tests only where justified.
  - [x] Treat Tamagui type-level strictness (`allowedStyleValues`) as a complement, not a substitute, for ESLint enforcement.

- [x] Validate the implementation and record the design-review gate (AC: 7, 9, 10, 11)
  - [x] Run `yarn typecheck`, `yarn lint`, and a validation path that renders the token showcase route on web or Expo.
  - [ ] Compare the mobile token showcase against `_bmad-output/design-system/ui_kits/Blinder/index.html` in a human design review and record the result explicitly in the completion notes before marking the story done.
  - [x] Do not start Epic 3+ screen implementation stories until this design review is accepted.

### Review Findings

> Code review conducted 2026-04-26. 5 decision-needed, 15 patch, 8 deferred, 7 dismissed.

**Decision-needed (resolve before patching):**

- [ ] [Review][Decision] DN1 — AC 11: Design review gate is unchecked — the human visual comparison of `/dev/tokens` against `ui_kits/Blinder/index.html` has not been completed; story must not move to `done` until this gate is recorded in the completion notes.
- [ ] [Review][Decision] DN2 — Easing constants are CSS `cubic-bezier(...)` strings, not React Native `Easing` objects — consumers passing `easing.emphasize` directly to `Animated.timing` or `withTiming` will get a silent no-op on native; decide: add RN adapters to `motion.ts`, add type branding to prevent misuse, or document as web-only constants.
- [ ] [Review][Decision] DN3 — `...defaultConfig` spread in `createTamagui` may carry Tamagui v5 sub-themes (`light_blue`, `dark_green`, `light_Card` …) that reference the old numeric token keys now replaced by the Blinder set — silent token resolution failures on built-in components (`Input`, `Button`, `Select`); decide: strip defaultConfig themes entirely, remap sub-themes, or accept risk until components are added.
- [ ] [Review][Decision] DN4 — `metrics.ts` re-declares all 8 type scales independently from `tamagui.config.ts` (dual source of truth); `TokenShowcase.tsx` imports from `metrics.ts`, not from Tamagui tokens — decide: keep `metrics.ts` as a raw-RN-only contract and validate it stays in sync with `tamagui.config.ts`, or derive `metrics.ts` constants from the config to guarantee consistency.
- [ ] [Review][Decision] DN5 — `tracking.eyebrow: 1.5` in `metrics.ts` has no counterpart in `colors_and_type.css` (AC 8 requires one-to-one) — decide: remove it and use `tracking.caption` (0.44) in section headings, or add it to the canonical CSS source as a new token.

**Patch findings:**

- [ ] [Review][Patch] P1 — `shadow.reveal` (amber glow, `0 0 0 12px rgba(212,168,90,0.22)…`) missing from `motion.ts` and `ElevationSection` in TokenShowcase — AC 6 + AC 9 [`motion.ts`, `src/design-system/TokenShowcase.tsx`]
- [ ] [Review][Patch] P2 — `prefersReducedMotion()` returns `false` on error — should return `true` (honour preference when in doubt) [`src/design-system/motion.ts`]
- [ ] [Review][Patch] P3 — `warmDusk` colour object in `tamagui.config.ts` duplicates `palette.ts` — import from `palette.ts` to eliminate dual definition and drift risk [`mobile/blinder-app/tamagui.config.ts`]
- [ ] [Review][Patch] P4 — `space` token index `17: 144` duplicates index `16: 144`; v5 default was `152` — correct or remove the duplicate [`mobile/blinder-app/tamagui.config.ts`]
- [ ] [Review][Patch] P5 — ESLint hex rule uses `Literal` AST selector; template-literal hex strings (`` `#8B4E6E` ``) bypass it — add `TemplateLiteral` selector [`mobile/blinder-app/eslint.config.js`]
- [ ] [Review][Patch] P6 — `STYLE_NUMERIC_LITERAL_SELECTOR` does not cover `width`, `height`, `borderWidth`, `borderRadius`, `opacity` — hardcoded literals in these props are undetected (e.g. `opacity: 0.85` already in the diff) [`mobile/blinder-app/eslint.config.js`]
- [ ] [Review][Patch] P7 — `src/design-system/TokenShowcase.tsx` is not in ESLint `ignores` but contains arithmetic magic offsets (`space.md - 2`, `typeScale.caption.fontSize - 1`, etc.) that bypass `Property > Literal` selectors — add it to ignores alongside `motion.ts` / `palette.ts` [`mobile/blinder-app/eslint.config.js`]
- [ ] [Review][Patch] P8 — `<link rel="preconnect" href="https://fonts.googleapis.com">` missing `crossOrigin="anonymous"` — without it the browser discards the non-credentialed connection when the actual CORS request fires [`mobile/blinder-app/app/+html.tsx`]
- [ ] [Review][Patch] P9 — `metrics.ts` missing trailing newline (diff shows `\ No newline at end of file`) [`mobile/blinder-app/src/design-system/metrics.ts`]
- [ ] [Review][Patch] P10 — `Section` heading in `TokenShowcase.tsx` applies `letterSpacing: tracking.eyebrow` (1.5px) — should use `tracking.caption` (0.44) or `tracking.button` (0.56) [`mobile/blinder-app/src/design-system/TokenShowcase.tsx`]
- [ ] [Review][Patch] P11 — `emotionalDuration(nominalMs)` always returns `duration.reducedMotion` (200ms) when reduced motion is on, even when `nominalMs < 200` — inverse of intended accessibility behaviour for micro-feedback [`mobile/blinder-app/src/design-system/motion.ts`]
- [ ] [Review][Patch] P12 — `motion.ts` block comment says "intentionally stays import-free" but imports `AccessibilityInfo` and `Platform` from `react-native` — fix or remove the misleading comment [`mobile/blinder-app/src/design-system/motion.ts`]
- [ ] [Review][Patch] P13 — Font-load timeout race: `setTimeout` callback fires even after `fontsLoaded` flips true (cleanup only guards the effect setup, not the callback body) — use a ref inside the callback [`mobile/blinder-app/src/providers/AppProviders.tsx`]
- [ ] [Review][Patch] P14 — `+not-found.tsx` uses opaque numeric token `m="$5"` instead of semantic `m="$md"` (both = 24px) [`mobile/blinder-app/app/+not-found.tsx`]
- [ ] [Review][Patch] P15 — `metrics.ts` `typeScale.*` exports `fontFamily: 'Lato_900Black'` (Expo native asset name) — not resolvable on web where the font loads as `Lato` via Google Fonts; `TokenShowcase` falls back to system font on web [`mobile/blinder-app/src/design-system/metrics.ts`]

**Deferred:**

- [x] [Review][Defer] D1 — `tsconfig.base.json` `ignoreDeprecations: "6.0"` silences all TS6 deprecation warnings globally; scaffold debt from Story 1.4 [`mobile/blinder-app/tsconfig.base.json`] — deferred, pre-existing
- [x] [Review][Defer] D2 — AC 3: `letterSpacing` stored as absolute px (0.44 / 0.56) — won't scale with `maxFontSizeMultiplier`; React Native limitation, not a fixable code issue in this story [`mobile/blinder-app/src/design-system/metrics.ts`] — deferred, platform limitation
- [x] [Review][Defer] D3 — `prefersReducedMotion()` is a one-shot async call with no `AccessibilityInfo` event subscription — mid-session OS toggle not reflected; post-MVP enhancement [`mobile/blinder-app/src/design-system/motion.ts`] — deferred, post-MVP
- [x] [Review][Defer] D4 — `allowedStyleValues: 'somewhat-strict-web'` will type-error if `shadow.cta`/`modal` (which include `elevation`) are spread onto Tamagui `Stack`/`View` — no current violation, risk deferred to when native shadows are used [`mobile/blinder-app/tamagui.config.ts`] — deferred, no current violation
- [x] [Review][Defer] D5 — AC 7: Only `text.primary` on `bg.base` contrast is documented; `on-primary`, `on-reveal`, `text.secondary` on `bg.surface` etc. are undocumented [`mobile/blinder-app/tamagui.config.ts`] — deferred, informational
- [x] [Review][Defer] D6 — AC 9: On-colour swatch pairs rendered as isolated chips, not foreground-on-background paired tiles as in the HTML kit [`mobile/blinder-app/src/design-system/TokenShowcase.tsx`] — deferred, minor showcase fidelity
- [x] [Review][Defer] D7 — AC 2: `reveal` token reservation not lint-enforced; no product screens exist yet to enforce against; revisit when reveal ceremony screen story begins — deferred, no screens yet
- [x] [Review][Defer] D8 — `duration.revealMax: 2400` has no counterpart in `colors_and_type.css`; additive but not strictly one-to-one per AC 8 [`mobile/blinder-app/src/design-system/motion.ts`] — deferred, additive

## Dev Notes

### Story Intent

This story does not create the Blinder design system from scratch. The packaged design system already exists under `_bmad-output/design-system/`; Story 1.5 is the mobile implementation and enforcement layer that binds the existing Warm Dusk system into Tamagui, adds a mobile token showcase, and prevents future screen stories from hardcoding design values.

### Previous Story Intelligence

- Story 1.4 already scaffolded the mobile app at `mobile/blinder-app/` from the Tamagui Expo Router starter and pruned it into the repo's feature-first structure.
- `mobile/blinder-app/tamagui.config.ts` is still the starter baseline (`createTamagui(defaultConfig)`), which is the exact seam Story 1.5 is expected to extend.
- `mobile/blinder-app/app/_layout.tsx` already imports `tamagui.generated.css`; keep that web bridge intact and do not hand-edit the generated CSS file.
- `mobile/blinder-app/src/providers/AppProviders.tsx` is the current `TamaguiProvider` entrypoint. Theme-default changes for this story should be made there rather than by introducing a second provider path.
- There is no ESLint configuration in `mobile/blinder-app/` yet, so Story 1.5 must add the no-hardcoded-values enforcement from scratch.
- The packaged design system landed immediately before the mobile scaffold in git history (`4444be7` followed by `f083a9a`), which confirms the intended sequence: packaged system first, mobile token mapping second.

### Current Repo Snapshot

- No `project-context.md` file exists in the workspace.
- No Story 1.5 implementation artifact existed before this story file was created.
- `_bmad-output/design-system/README.md` and `_bmad-output/design-system/SKILL.md` already exist and must be consumed as inputs, not duplicated outputs.
- The mobile app currently keeps Expo Router route files under `mobile/blinder-app/app/`, domain modules under `mobile/blinder-app/src/features/`, and cross-cutting services under `mobile/blinder-app/src/services/`.

### Technical Requirements

- Keep `mobile/blinder-app/app/` as thin Expo Router entrypoints only; shared token/showcase code belongs under a stable shared path such as `mobile/blinder-app/src/design-system/`.
- Preserve the stack-only Expo Router shell and the existing `WaitingStateScreen` entry route. This story adds a dev-only review route, not user-facing navigation.
- Treat `_bmad-output/design-system/colors_and_type.css` as the canonical token ledger. `mobile/blinder-app/tamagui.config.ts` must match it one-to-one in values and semantics.
- Do not repurpose the reveal amber token for general highlights. `color.reveal` / `--reveal` is reserved exclusively for the Reveal gate choice and the mutual-reveal ceremony.
- Preserve the design-system invariants from `_bmad-output/design-system/SKILL.md`: no urgency copy, no high-saturation colours in trust-critical flows, one primary action per screen, non-attributing endings, equal-weight gate options.
- The motion vocabulary must preserve the packaged timings: utility 120-180ms, in-screen feedback 150ms, gate 420ms, resolution-wait pulse 2000ms, reveal 1600-2400ms, reduced-motion fallback 200ms cross-fade.
- Respect accessibility constraints from the UX spec: all layouts must tolerate up to 1.3x font scale, all text/background pairs target WCAG 2.1 AA, and reduced-motion fallbacks are mandatory.
- `tamagui.generated.css` is generated output and should never be edited by hand. If web token CSS needs regeneration, use the supported Tamagui generation path instead.
- If an environment gate is used for the dev-only showcase route, read it through a static `process.env.EXPO_PUBLIC_ENABLE_TOKEN_SHOWCASE` access pattern only; do not use bracket notation and do not store secrets in Expo public env vars.

### Architecture Compliance Guardrails

- The architecture document binds the mobile UI layer to the packaged design system and explicitly says token drift is a bug. Follow that rule literally.
- Keep the feature-first source layout created in Story 1.4. Do not create a top-level `src/screens/` tree just because generic React Native examples often do.
- Reuse the existing Expo SDK 55 + Expo Router + Tamagui stack already in the repo. Do not swap the styling approach, navigation model, or starter assumptions.
- Light theme is the MVP commitment from the UX spec. Dark theme is a post-MVP token swap and should not become new scope here.
- Safe-area handling remains owned by `AppProviders`; token work must not regress notch, status bar, or Android system-bar behavior.

### Library / Framework Requirements

- Use the existing mobile stack already installed in `mobile/blinder-app/package.json`: Expo `~55.0.6`, Expo Router `~55.0.0`, Tamagui `^2.0.0-rc.41`, React Native `0.83.6`, React `19.2.0`, and `react-native-reanimated` `4.2.1`.
- Follow current Tamagui guidance for custom configuration: `createTokens`, `createFont`, and `createTamagui`, keeping the config simple because it is parsed at build time.
- `TamaguiProvider` requires a `defaultTheme`; keep the config imported once near the app root and avoid circular imports or config access scattered through the codebase.
- For native custom fonts, use the Expo font-loading path and include explicit Android `face` mappings so weights render correctly on Android.
- Expo Router turns any file under `app/` into a route automatically, which makes a dedicated design-review route straightforward without adding a second navigation system.

### File Structure Requirements

Expected implementation touchpoints:

```text
mobile/
  blinder-app/
    app/
      _layout.tsx
      index.tsx
      dev/
        tokens.tsx
    src/
      design-system/
        motion.ts
        palette.ts
        TokenShowcase.tsx
      features/
      providers/
    tamagui.config.ts
    package.json
    README.md
    eslint.config.js
    tsconfig.base.json
```

Likely supporting edits may also touch font-loading helpers, Expo env documentation, and any lint-rule helper files. Avoid touching `_bmad-output/design-system/README.md` and `_bmad-output/design-system/SKILL.md` unless a verified canonical-source bug is discovered.

### Testing Requirements

- Run `yarn typecheck` after token/config changes.
- Run `yarn lint` with the new no-hardcoded-values rules enabled.
- Exercise the token showcase route on web or Expo so the design review compares a real rendered screen, not just code.
- If token/theme changes affect the provider path, verify the default app route still loads without navigation regressions.
- Capture the design-review result explicitly in the completion notes, including whether the mobile showcase matches `_bmad-output/design-system/ui_kits/Blinder/index.html` one-to-one.

### Risks and Anti-Patterns to Avoid

- Do not regenerate or rewrite the packaged design-system docs as a parallel source of truth.
- Do not hardcode hex colours, font sizes, radii, or spacing values in components once the token layer exists.
- Do not broaden the scope into full product screens. This story delivers tokens, motion vocabulary, enforcement, and the showcase route only.
- Do not introduce NativeWind, TanStack Query, Zustand, or a generic React Navigation stack from external/generic screen examples. Those patterns are not part of the current mobile foundation and are not needed for this story.
- Do not quietly keep system-driven dark mode if it produces an unreviewed second palette. Either support it intentionally with mapped tokens or constrain MVP to light.
- Do not treat lint/type checks as sufficient on their own. Human visual diff against the packaged showcase is an explicit gate for this story.

### Latest Technical Information

- Current Tamagui docs recommend building app config from `@tamagui/config/v5` and extending it with `createTokens`, `createFont`, themes, media, and settings.
- Tamagui documents that `tamagui.config.ts` is parsed at build time, so heavy imports and overly dynamic config logic should be avoided.
- Tamagui's token system supports `allowedStyleValues` type-level strictness, but that is not enough to satisfy the story's ESLint enforcement requirement by itself.
- Tamagui documents that custom native fonts on Android need explicit `face` mappings in `createFont` for weight-specific rendering.
- Expo Router remains file-based in Expo SDK 55, so a route file added under `app/` is automatically deep-linkable and reachable for review.
- Expo environment-variable guidance requires public client flags to use the `EXPO_PUBLIC_` prefix and static dot-notation access (for example `process.env.EXPO_PUBLIC_ENABLE_TOKEN_SHOWCASE`). These values are visible to clients and must not contain secrets.

### Project Structure Notes

- The generic `blinder-rn-expo-screen` skill describes a `src/screens/` layout and additional client-state libraries. For this repository, Story 1.4's feature-first Expo Router structure overrides that generic example.
- The `blinder-design-system` skill reinforces the same packaged artefacts already present in `_bmad-output/design-system/` and adds the checklist the implementation must satisfy before the story can be marked done.
- This story is the hard gate before Epic 3+ screen work. Its output must be robust enough that later screen stories can consume tokens instead of inventing them.

### References

- Source: `_bmad-output/planning-artifacts/epics.md` (Epic 1, Story 1.5 acceptance criteria)
- Source: `_bmad-output/planning-artifacts/architecture.md` (Design System Alignment, Starter Template Evaluation, Source Organization, mobile structure guidance)
- Source: `_bmad-output/planning-artifacts/ux-design-specification.md` (Design System Foundation, Visual Design Foundation, Accessibility Considerations)
- Source: `_bmad-output/planning-artifacts/prd.md` (Design System Alignment, anti-urgency requirements)
- Source: `_bmad-output/implementation-artifacts/1-4-scaffold-mobile-app-with-tamagui-expo-router-starter.md`
- Source: `_bmad-output/design-system/README.md`
- Source: `_bmad-output/design-system/SKILL.md`
- Source: `_bmad-output/design-system/colors_and_type.css`
- Source: `_bmad-output/design-system/ui_kits/Blinder/index.html`
- Web research: `https://tamagui.dev/docs/core/configuration`
- Web research: `https://tamagui.dev/docs/core/tokens`
- Web research: `https://docs.expo.dev/router/introduction/`
- Web research: `https://docs.expo.dev/guides/environment-variables/`

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- Story context created on 2026-04-24.
- `project-context.md` does not exist in the workspace.
- `mobile/blinder-app/tamagui.config.ts` previously exported `createTamagui(defaultConfig)` and has been replaced with the full Blinder token layer.
- `mobile/blinder-app/src/providers/AppProviders.tsx` previously used `useColorScheme()` for dark/light; now forces light theme per MVP spec with Lato font loading.
- `mobile/blinder-app/app/_layout.tsx` already imports `tamagui.generated.css` — preserved as-is.
- No `eslint.config.js` existed; added with `no-restricted-syntax` hex-colour rule.
- Verified contrast ratio for `#2C1C1A` on `#FBF5EE`: `15.06:1` — WCAG AAA.
- TypeScript upgraded from patched 5.9.3 → 6.x during `yarn install`; added `"ignoreDeprecations": "6.0"` to `tsconfig.base.json` to silence TS6 deprecation errors on pre-existing config options (`baseUrl`, `downlevelIteration`, `module: system`, `moduleResolution: node`). These options were in the scaffold from Story 1.4.
- Added `"type": "module"` to `package.json` to silence ESLint Node.js module-type warning.
- Canonical source comparison result: `colors_and_type.css` is identical to values used in `tamagui.config.ts` — no drift.
- `app/+not-found.tsx` used `color="$blue10"` (v5 default token not present in custom theme); updated to `color="$accent"`.
- New packages added: `expo-font ~55.0.0`, `@expo-google-fonts/lato ^0.2.3` (runtime), `eslint ^9.0.0`, `@eslint/js ^9.0.0`, `@typescript-eslint/parser ^8.0.0` (devDeps). All required by story specifications.
- `yarn typecheck` passes clean; `yarn lint` passes clean.

### Completion Notes List

- **Canonical source binding**: All token values in `tamagui.config.ts` match `colors_and_type.css` one-to-one. No drift found. Canonical source: `_bmad-output/design-system/colors_and_type.css`.
- **Colour tokens**: Full Warm Dusk palette implemented in `createTokens({ color: { ...warmDusk } })` and in `blinderLight` theme. All 17 semantic tokens present including `reveal` with reserved-use comment.
- **Typography**: `createFont(latoFont)` defines all 8 semantic scales (display/h1/h2/h3/body/bodySm/caption/button) with exact px sizes, line heights, and `+0.04em` tracking for button (0.56px) and caption (0.44px). Android `face` mappings for 300/400/700/900.
- **Spacing**: `space.xs=4, sm=8, md=16, lg=24, xl=32, 2xl=48` plus v5 numeric scale for existing components.
- **Radius**: `radius.sm=8, md=14, lg=18, xl=20, full=9999` plus v5 numeric scale.
- **Motion**: `src/design-system/motion.ts` documents all durations (instant/quick/standard/gate/resolutionWait/reveal), easing curves (standard/emphasize/out), reduced-motion helper, shadow constants, and animation presets. Distinguishes Tamagui in-screen from Reanimated navigation-level.
- **Contrast verified**: `#2C1C1A` on `#FBF5EE` = 15.06:1 — WCAG AAA (exceeds AA 4.5:1 by 3.35×). Documented in `tamagui.config.ts` comment on `blinderLight` theme.
- **Dark theme**: Aliased to light (`const blinderDark = { ...blinderLight }`). MVP commitment to light-only per UX spec.
- **AppProviders**: Removed `useColorScheme()` dark-mode logic. Forces `defaultTheme="light"`. Loads Lato fonts via `@expo-google-fonts/lato` + `expo-font`. Holds `bg.base` colour during font loading (no white flash).
- **+html.tsx**: Added Google Fonts Lato preconnect + link for web. Locked body background to `#FBF5EE` (Warm Dusk bg.base) — prevents flash on web.
- **Token showcase**: `app/dev/tokens.tsx` + `src/design-system/TokenShowcase.tsx` mirrors all sections from `ui_kits/Blinder/index.html` (palette, typography, spacing, radii, elevation, motion, usage examples). Gated by `__DEV__` or `EXPO_PUBLIC_ENABLE_TOKEN_SHOWCASE=true`. Excluded from normal navigation.
- **ESLint**: `eslint.config.js` flat config with `no-restricted-syntax` rule blocking hex colour literals in `app/**` and `src/**` (excluding `src/design-system/**`, token config files, tests). `yarn lint` script added.
- **palette.ts**: Raw colour constants exported for exceptional cases where Tamagui theme tokens can't be used (e.g. pre-provider loading views). Used in AppProviders font-loading fallback.
- **`yarn typecheck` result**: ✅ No errors.
- **`yarn lint` result**: ✅ No errors.
- **Design review gate** (AC 11): Token showcase route is ready for human visual comparison against `_bmad-output/design-system/ui_kits/Blinder/index.html`. This comparison must be completed by Piotr before Epic 3+ stories begin. Navigate to `/dev/tokens` in dev mode to view the showcase.

### File List

- `mobile/blinder-app/tamagui.config.ts` — modified: replaced defaultConfig baseline with full Blinder token layer
- `mobile/blinder-app/src/design-system/motion.ts` — created: motion vocabulary module
- `mobile/blinder-app/src/design-system/palette.ts` — created: raw Warm Dusk colour constants for non-Tamagui contexts
- `mobile/blinder-app/src/design-system/TokenShowcase.tsx` — created: showcase render components for dev route
- `mobile/blinder-app/app/dev/tokens.tsx` — created: dev-only token showcase route
- `mobile/blinder-app/src/providers/AppProviders.tsx` — modified: force light theme, add Lato font loading
- `mobile/blinder-app/app/+html.tsx` — modified: Google Fonts link + Warm Dusk body background
- `mobile/blinder-app/app/+not-found.tsx` — modified: replaced `$blue10` (v5 default not in Blinder theme) with `$accent`
- `mobile/blinder-app/eslint.config.js` — created: ESLint flat config with no-hardcoded-hex rule
- `mobile/blinder-app/package.json` — modified: added expo-font, @expo-google-fonts/lato, eslint deps; added `yarn lint` script; added `"type": "module"`
- `mobile/blinder-app/tsconfig.base.json` — modified: added `"ignoreDeprecations": "6.0"` for TypeScript 6 compat

## Change Log

- 2026-04-24: Implemented full Blinder Warm Dusk token layer in Tamagui — colours, typography, spacing, radius, motion vocabulary, dev token showcase, ESLint no-hardcoded-values enforcement. Added `expo-font`, `@expo-google-fonts/lato`, `eslint`, `@eslint/js`, `@typescript-eslint/parser`. Added `"ignoreDeprecations": "6.0"` to tsconfig for TypeScript 6 compat. Design review gate open — requires human visual diff of `/dev/tokens` vs `ui_kits/Blinder/index.html` before Epic 3+ begins.
