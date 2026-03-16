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
- Android Studio emulator and/or Xcode simulator

## Environment

Set API URL via environment variable:

```bash
EXPO_PUBLIC_API_URL=http://localhost/api
```

See root `.env.example` for current defaults.

## Install and Run

From `mobile/`:

```bash
npm install
npx expo start
```

Shortcuts:

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
      _layout.tsx
      (auth)/
         _layout.tsx
      (tabs)/
         _layout.tsx
   components/
      chat/
      match/
      moderation/
      onboarding/
      shared/
      svg/
   constants/
      errors.ts
      theme.ts
   hooks/
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

## Build Profiles (EAS)

`eas.json` is committed and includes:

- `development`
- `preview`
- `production`

## Notes

- App is configured for dark mode (`app.json` -> `userInterfaceStyle: "dark"`).
- Deep-link scheme is `blinder`.
