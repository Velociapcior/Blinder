# Blinder Mobile (Expo SDK 55)

Mobile app for Blinder, built with Expo Router, strict TypeScript, NativeWind, and SignalR.

## Stack

- Expo SDK 55
- React Native 0.83
- Expo Router (file-based navigation)
- TypeScript strict mode
- NativeWind v4 + Tailwind CSS
- `expo-secure-store` for auth token storage
- `@microsoft/signalr` for realtime chat/reveal events

## Prerequisites

- Node.js 20+
- npm 10+
- Android Studio (includes emulator + bundled JDK 17)
- JDK 17 (use Android Studio's bundled JDK at `C:\Program Files\Android\Android Studio\jbr`)

## Environment

Create `mobile/.env` (git-ignored, never the root `.env`):

```bash
# Android emulator — 10.0.2.2 resolves to host machine
EXPO_PUBLIC_API_URL=http://10.0.2.2/api

# iOS simulator — localhost works fine
# EXPO_PUBLIC_API_URL=http://localhost/api
```

> Expo loads `.env` from the same directory as `package.json` (`mobile/`), not from the project root.
> `EXPO_PUBLIC_` prefix is required for Expo to include the variable in the client bundle.
> Changes to `.env` require a full native rebuild (`npx expo run:android`), not just a Metro reload.

## Install and Run

From `mobile/`:

```bash
npm install
```

**First run or after `app.json` / native config changes** (required to rebuild the native app):

```bash
# Set JAVA_HOME to Android Studio's bundled JDK first (Windows):
$env:JAVA_HOME="C:\Program Files\Android\Android Studio\jbr"
npx expo run:android
```

**Daily development** (JS-only changes, app already installed):

```bash
npx expo start
```

Shortcuts in Metro:

- `a` open Android emulator
- `i` open iOS simulator
- `w` open web

## Scripts

```bash
npm run start
npm run android
npm run ios
npm run web
npm run lint
```

Type-check:

```bash
npx tsc --noEmit
```

## Project Structure

```text
mobile/
   app/
      _layout.tsx          # Root layout — SafeAreaProvider > AccessibilityProvider > Stack
      (auth)/
         _layout.tsx
      (tabs)/
         _layout.tsx
   components/
      shared/
         AccessiblePressable.tsx  # Enforces 44×44 touch target + hitSlop; required a11y props
         ThemedText.tsx           # Enforces allowFontScaling={true}; variant system
      chat/
      match/
      moderation/
      onboarding/
      svg/
   contexts/
      AccessibilityContext.tsx    # Provides { reduceMotion, fontScale, isScreenReaderEnabled }
   constants/
      errors.ts
      theme.ts
   hooks/
      useAccessibility.ts         # Re-exports useAccessibility from contexts/
      useResponsiveLayout.ts      # compact <375 / regular 375-427 / expanded >=428
   services/
      apiClient.ts
      signalrService.ts
      storageService.ts
   types/
      api/
      signalr/
      index.ts
   utils/
      dateFormat.ts
```

## Conventions

- Do not use `AsyncStorage` for auth tokens. Use `expo-secure-store` only.
- Do not call `fetch` directly from components. Use `services/apiClient.ts`.
- Keep SignalR connection lifecycle in `services/signalrService.ts` (singleton).
- Keep user-facing errors in `constants/errors.ts`.
- Keep API and SignalR types under `types/api` and `types/signalr` (separate namespaces).

## Accessibility

All components must follow these patterns (WCAG 2.1 AA — non-negotiable):

- **Never** call `AccessibilityInfo` directly in components. Use `useAccessibility()` from `hooks/useAccessibility` instead.
- Use `<ThemedText>` instead of raw `<Text>` — `allowFontScaling={true}` is always on.
- Use `<AccessiblePressable>` for icon-only controls — enforces 44×44 touch target and `hitSlop`.
- For animated components, collapse duration when reduce motion is enabled:
  ```tsx
  const { reduceMotion } = useAccessibility();
  const duration = reduceMotion ? 0 : motion.standard;
  ```
- `accessibilityRole` and `accessibilityLabel` are required on every interactive element.

## Responsive Layout

- `useResponsiveLayout()` exposes three width buckets for future screens and modals:
- `compact`: width < 375
- `regular`: 375 <= width < 428
- `expanded`: width >= 428

## Build Profiles (EAS)

`eas.json` is committed and includes:

- `development`
- `preview`
- `production`

## Notes

- App is configured for dark mode (`app.json` -> `userInterfaceStyle: "dark"`).
- Deep-link scheme is `blinder`.
