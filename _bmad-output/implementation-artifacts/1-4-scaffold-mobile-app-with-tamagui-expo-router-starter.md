# Story 1.4: Scaffold Mobile App with Tamagui Expo Router Starter

Status: ready-for-dev

<!-- Note: Validation is optional. Run validate-create-story for quality check before dev-story. -->

## Story

As a developer,
I want a scaffolded Expo mobile app using the Tamagui Expo Router starter pruned into a domain-first module structure,
so that all mobile work starts from the correct navigation model and design system foundation.

## Acceptance Criteria

1. Given the mobile source directory exists under `mobile/blinder-app/`, when initialized with `yarn create tamagui@latest --template expo-router`, then the app runs on iOS and Android via Expo Go and development build without errors.
2. All Tamagui demo/example structure is removed.
3. The app is reorganized into domain-first modules under `src/features/`: `onboarding/`, `waiting/`, `match-entry/`, `conversation/`, `decision-gate/`, `reveal/`, `ending/`, `settings/`.
4. `src/services/` contains `auth/` and `realtime/` stubs.
5. Navigation uses Expo Router with a stack-based model and no bottom tab bar at any route.
6. TypeScript strict mode is enabled with zero type errors on `tsc --noEmit`.
7. Yarn 4.4.0+ is used as the package manager.
8. `expo doctor` reports no blocking issues.

## Tasks / Subtasks

- [ ] Initialize the mobile workspace from the approved starter (AC: 1, 7)
  - [ ] Create `mobile/blinder-app/` from the repo root using `yarn create tamagui@latest --template expo-router`.
  - [ ] Verify Yarn is `4.4.0+` before scaffolding; if not, upgrade with `yarn set version stable` and keep the generated Yarn configuration files in the repo.
  - [ ] Preserve the Expo SDK 55 + Tamagui starter baseline instead of mixing in a different Expo template.

- [ ] Prune starter demos and install the root app shell (AC: 1, 2, 5)
  - [ ] Remove starter demo/example routes, sample components, and any tabs-oriented route groups or layout assumptions.
  - [ ] Keep Expo Router `app/` files as thin route entrypoints and configure a stack-only root layout in `app/_layout.tsx`.
  - [ ] Wire the Tamagui provider at the app root, but keep the initial route visually neutral and structural only; do not implement branded product screens in this story.

- [ ] Reorganize into the required domain-first structure (AC: 3, 4, 5)
  - [ ] Create `src/features/onboarding/`, `src/features/waiting/`, `src/features/match-entry/`, `src/features/conversation/`, `src/features/decision-gate/`, `src/features/reveal/`, `src/features/ending/`, and `src/features/settings/`.
  - [ ] Create `src/services/auth/` and `src/services/realtime/` placeholder modules with clear stub entrypoints for later stories.
  - [ ] Keep feature-specific screens, hooks, and types inside the owning feature folder rather than creating a separate top-level `src/screens/` tree.

- [ ] Harden TypeScript and developer tooling (AC: 6, 7, 8)
  - [ ] Enable TypeScript strict mode and add a reproducible typecheck command for `tsc --noEmit`.
  - [ ] Add package scripts for local development and validation, including a cache-clearing Expo start command for first Tamagui bootstraps if needed.
  - [ ] Run Expo Doctor and resolve all blocking issues before marking the story complete.

- [ ] Validate the scaffold on the supported development paths (AC: 1, 6, 8)
  - [ ] Confirm the project starts in Expo Go on a real device or emulator.
  - [ ] Confirm a development build path works for Android and iOS; if the current host OS cannot complete one platform locally, document the remaining platform-specific validation explicitly instead of silently skipping it.
  - [ ] Run `tsc --noEmit` with zero errors and capture the final validation commands in the mobile README or story dev log.

## Dev Notes

### Story Intent

This story creates the mobile foundation only. It must establish the correct Expo + Expo Router + Tamagui baseline and the correct folder layout, but it must not drift into screen implementation, production auth wiring, real-time integration, or the full Warm Dusk token system. Story 1.5 owns the complete token mapping and design-review gate.

### Previous Story Intelligence

- Story 1.3 finished the backend database-boundary foundation and explicitly keeps the mobile app as an independent client that will live under `mobile/blinder-app/`.
- Recent git history shows the packaged Blinder design system landed immediately before this story. Treat that packaged system as the canonical visual and copy reference rather than inventing temporary product styling.
- The current repository already has backend, Docker, PostgreSQL, MinIO, and deployment baselines. This story should add the mobile root cleanly without disturbing those foundations.

### Current Repo Snapshot

- No `mobile/` directory exists yet in the workspace.
- No `project-context.md` file exists.
- The packaged design system already exists under `_bmad-output/design-system/` and is the source of truth for future tokens, motion rules, copy guardrails, and component intent.
- `sprint-status.yaml` still marks this story as `backlog`; creating this file should move it to `ready-for-dev`.

### Technical Requirements

- Use the Tamagui Expo Router starter exactly as selected in architecture: `yarn create tamagui@latest --template expo-router`.
- Keep Yarn on `4.4.0+`; this is a current Tamagui starter requirement.
- Preserve the Expo SDK 55 starter baseline generated by Tamagui rather than swapping to Expo's default tabs template or `blank-typescript`.
- Keep Expo Router stack-based with no bottom tabs and no `(tabs)` route group anywhere in the app.
- Root provider wiring belongs in `app/_layout.tsx`; Tamagui should be mounted there so later stories inherit one consistent shell.
- Keep the initial scaffold compatible with both Expo Go and development builds. Expo's current guidance treats Expo Go as the quickest learning/test path and development builds as the intended path for production-grade apps; Blinder needs both because later stories add notifications and native integrations.
- Clear the Expo cache on the first Tamagui run if the starter behaves inconsistently (`yarn dlx expo start -c` is the current documented recovery step).
- Keep TypeScript strict mode enabled and require a clean `tsc --noEmit` run.
- Keep state management lean at scaffold time. Do not introduce heavyweight client-state patterns just to satisfy folder structure; add only the structural stubs needed for `auth` and `realtime`.
- `src/services/realtime/` is future SignalR infrastructure. It must remain a synchronization layer only; later stories must still recover authoritative state through normal API reads.
- Respect safe-area and font-scaling requirements in the root shell so future screens are not boxed into a layout that breaks Dynamic Type, notches, or Android system chrome.
- Do not invent interim brand colours, alternate typography, or throwaway demo UI. The scaffold can be neutral, but it must leave an obvious place for Story 1.5 to map Tamagui tokens one-to-one from the packaged design system.

### Architecture Compliance Guardrails

- `mobile/blinder-app/app/` is the routing shell; domain code belongs under `mobile/blinder-app/src/features/`.
- Feature folder names must match architecture exactly: `onboarding`, `waiting`, `match-entry`, `conversation`, `decision-gate`, `reveal`, `ending`, `settings`.
- Keep `src/services/auth/` and `src/services/realtime/` separate from feature modules because they are cross-cutting integration surfaces.
- Preserve the single-focus navigation model from UX: no persistent bottom navigation now or later.
- All future screens must be safe-area aware and compatible with `1.3x` font scaling.
- Do not hardcode visual tokens in the scaffold. Story 1.5 is the hard gate for token implementation and design review.

### Library / Tooling Requirements

- Expo SDK 55 scaffold from the Tamagui Expo Router starter.
- Expo Router file-based routing.
- Tamagui root provider wired from the starter rather than replaced with a custom navigation stack.
- Yarn `4.4.0+`.
- TypeScript `strict` mode.
- Expo Doctor must pass with no blocking issues.

### File Structure Requirements

Expected implementation touchpoints:

```text
mobile/
  blinder-app/
    app/
      _layout.tsx
      index.tsx
      ...route entrypoints...
    src/
      features/
        onboarding/
        waiting/
        match-entry/
        conversation/
        decision-gate/
        reveal/
        ending/
        settings/
      services/
        auth/
        realtime/
    assets/
    tamagui.config.ts
    package.json
    tsconfig.json
```

The exact internal file names can vary, but the route shell versus domain-module split must remain obvious.

### Testing Requirements

- Verify Yarn version before scaffolding.
- Install dependencies with Yarn and keep the lockfile/package-manager artifacts reproducible.
- Run the app through Expo Go.
- Run a development build validation path.
- Run `tsc --noEmit` with zero errors.
- Run Expo Doctor and clear all blocking issues.
- If one platform cannot be validated from the current machine, log the remaining platform validation as an explicit follow-up rather than treating it as complete.

### Risks and Anti-Patterns to Avoid

- Do not use Expo's default tabs starter and then try to back out of its navigation model.
- Do not leave Tamagui demo/example structure in place; it will create immediate drift from the product architecture.
- Do not create a generic top-level `src/screens/` folder just because the React Native screen skill shows a generic example. In this repository, the feature-first architecture wins.
- Do not build real product screens yet. This story is structural foundation, not UX implementation.
- Do not introduce temporary colours, fonts, or button styles that later stories must unwind.
- Do not skip explicit iOS validation notes just because the current machine is Windows. Expo Go can still cover part of the path, but iOS development builds require a macOS-capable validation step.

### Latest Technical Information

- Tamagui's current Expo guide still recommends `yarn create tamagui@latest --template expo-router` and still requires Yarn `4.4.0+`.
- Tamagui's current installation docs call out React Native `0.81+`, React `19+`, and TypeScript `5+` as the baseline the starter is expected to satisfy.
- Expo's current guidance positions Expo Go as a quick validation path and development builds as the intended long-term path for production apps.
- Expo Router remains the preferred Expo-native routing solution for new apps and keeps every route deep-linkable, which fits Blinder's future notification/deep-link requirements.

### Project Structure Notes

- The `blinder-rn-expo-screen` skill includes a generic `src/screens/...` example. For Blinder, treat that as an intra-feature pattern only. Screen components, hooks, and screen-local types should live inside each feature module under `src/features/...`, with Expo Router route files delegating into those modules.
- The `blinder-design-system` skill and `_bmad-output/design-system/README.md` are already active constraints for this story even though Story 1.5 owns the full token layer. The point of Story 1.4 is to leave the app in a state where Story 1.5 can map tokens cleanly without undoing route or folder decisions.

### References

- Source: `_bmad-output/planning-artifacts/epics.md` (Epic 1 overview, Story 1.4 acceptance criteria, UX-DR37, UX-DR38)
- Source: `_bmad-output/planning-artifacts/architecture.md` (Starter Template Evaluation, Requirements to Structure Mapping, Source Organization, Frontend Architecture, Implementation Handoff)
- Source: `_bmad-output/planning-artifacts/ux-design-specification.md` (Design System Foundation, Navigation Architecture, Accessibility - MVP Scope, Implementation Guidelines)
- Source: `_bmad-output/planning-artifacts/prd.md` (Project Classification, Mobile App Specific Requirements, Technical Constraints)
- Source: `_bmad-output/design-system/README.md`
- Source: `_bmad-output/design-system/SKILL.md`
- Source: `.github/skills/blinder-rn-expo-screen/SKILL.md`
- Source: `.github/skills/blinder-design-system/SKILL.md`
- Web research: `https://tamagui.dev/docs/guides/expo`, `https://tamagui.dev/docs/intro/installation`, `https://docs.expo.dev/router/introduction/`, `https://docs.expo.dev/get-started/set-up-your-environment/`

## Dev Agent Record

### Agent Model Used

GPT-5.4

### Debug Log References

- Story context created on 2026-04-21.
- `mobile/` does not exist yet in the workspace.
- `project-context.md` does not exist in the workspace.
- Existing recent commits relevant to this story: `e0332ef` (`1-3 Postgres migrations`), `4444be7` (`Add Blinder Design System from Claude Design handoff`), `cdc855a` (`1.2 Dockler + database`).
- Upstream docs checked: current Tamagui Expo guide, Tamagui installation guide, Expo Router introduction, Expo environment setup.

### Completion Notes List

- Story context compiled from planning artifacts, design-system artifacts, repo state, and current upstream starter guidance.
- No implementation work was performed as part of story creation.
- Story is ready for a dev agent to execute.

### File List

- `_bmad-output/implementation-artifacts/1-4-scaffold-mobile-app-with-tamagui-expo-router-starter.md`
