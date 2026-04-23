# Blinder Mobile App

This app was scaffolded from the official Tamagui Expo Router starter and then pruned into Blinder's mobile foundation.

## Commands

- One-time setup on a new machine: run `corepack enable` so `yarn` resolves to the app's pinned Yarn 4 version instead of a global Yarn 1 install.
- `yarn start` starts Expo normally.
- `yarn start:clear` starts Expo with a cleared cache for first-run Tamagui recovery.
- `yarn android` runs the Android development build path.
- `yarn ios` runs the iOS development build path.
- `yarn web` starts the web target.
- `yarn typecheck` runs `tsc --noEmit`.
- `yarn doctor` runs Expo Doctor.
- `yarn validate` runs the required static validation commands.
- `yarn test:web` runs the lightweight web smoke test.

## Structure

- `app/` contains thin Expo Router entrypoints only.
- `src/features/` contains domain-first feature modules.
- `src/services/auth/` and `src/services/realtime/` are cross-cutting integration stubs for later stories.

## Validation Notes

- Android and Expo Go validation can be executed from Windows when the required SDK or devices are available locally.
- iOS development builds require a macOS-capable validation path and are tracked explicitly until that host is available.
- If `yarn` still resolves to Yarn 1 in your shell, either restart the terminal after `corepack enable` or use `corepack yarn <command>` for that session.
- `tamagui.generated.css` is a build-time artefact (gitignored). On a fresh checkout the web target renders without Tamagui styles until the file exists. Run `yarn start:clear` once to generate it; subsequent `yarn web` starts will work normally.
