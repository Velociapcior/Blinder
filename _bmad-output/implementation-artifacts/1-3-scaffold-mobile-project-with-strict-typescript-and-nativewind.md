# Story 1.3: Scaffold Mobile Project with Strict TypeScript and NativeWind

Status: done

## Story

As a developer,
I want a fully scaffolded Expo SDK 55 React Native project with strict TypeScript, NativeWind styling, and EAS Build configured,
so that every mobile story has a convention-compliant base with no retrofitting of type safety or styling later.

## Acceptance Criteria

1. **Given** the repository is cloned fresh **When** the developer runs `npx expo start` from `mobile/` **Then** the Expo dev server starts without errors on both iOS and Android simulators

2. **Given** the mobile project is scaffolded **When** `tsconfig.json` is reviewed **Then** `"strict": true` is set and all additional strict flags from tech-preferences are present — TypeScript strict mode is active before the first component is written

3. **Given** the mobile project is scaffolded **When** the `mobile/` directory structure is inspected **Then** the following directories and files exist: `app/(auth)/`, `app/(tabs)/`, `components/`, `hooks/`, `services/apiClient.ts`, `services/signalrService.ts`, `services/storageService.ts`, `constants/errors.ts`, `constants/theme.ts`, `types/api/index.ts`, `types/signalr/index.ts`, `utils/dateFormat.ts`

4. **Given** the mobile project is scaffolded **When** the installed packages are reviewed **Then** `expo-router`, `expo-notifications`, `expo-image-picker`, `expo-secure-store`, `@microsoft/signalr`, `nativewind` are all installed

5. **Given** `services/storageService.ts` is implemented **When** a JWT token is stored and retrieved **Then** `expo-secure-store` is used exclusively — `AsyncStorage` is not imported anywhere in `storageService.ts`

6. **Given** `eas.json` is reviewed **When** it is verified it is committed to the repository **Then** it exists with at minimum `development`, `preview`, and `production` build profiles configured

7. **Given** an async hook is implemented **When** the return type is inspected **Then** it returns `AsyncState<T>` = `{ data: T | null; error: string | null; isLoading: boolean }` — never raw `try/catch` in components

## Tasks / Subtasks

- [x] Task 1: Initialize Expo SDK 55 project (AC: 1, 4)
  - [x] From repo root, run: `npx create-expo-app@latest mobile --template default@sdk-55`
  - [x] Verify `mobile/` is created and that `cd mobile && npx expo start` launches without errors
  - [x] Delete default template files that conflict with our structure (see "Files to Delete" in Dev Notes)
  - [x] Confirm `package.json` lists `"expo": "~55.0.0"` or similar SDK 55 constraint

- [x] Task 2: Configure TypeScript strict mode (AC: 2)
  - [x] Update `mobile/tsconfig.json` — add all strict compiler options per tech-preferences 10.1 (see exact content in Dev Notes)
  - [x] Verify `"strict": true` is set — this MUST be done before writing any code
  - [x] Confirm no TypeScript errors after configuration update: `npx tsc --noEmit` from `mobile/`

- [x] Task 3: Install all required packages (AC: 4)
  - [x] Run from `mobile/`: `npx expo install expo-router expo-notifications expo-image-picker expo-secure-store @microsoft/signalr nativewind`
  - [x] Run: `npx expo install tailwindcss react-native-reanimated react-native-safe-area-context` (NativeWind dependencies)
  - [x] Verify all packages appear in `mobile/package.json`

- [x] Task 4: Configure NativeWind v4 (AC: 1)
  - [x] Create `mobile/tailwind.config.ts` with Blinder design tokens (see exact content in Dev Notes)
  - [x] Update `mobile/babel.config.js` to add NativeWind preset (see exact content in Dev Notes)
  - [x] Create or update `mobile/metro.config.js` with `withNativeWind` wrapper (see exact content in Dev Notes)
  - [x] Create `mobile/global.css` with Tailwind directives
  - [x] Create `mobile/nativewind-env.d.ts` for TypeScript `className` prop support
  - [x] Update `mobile/app/_layout.tsx` to import `../global.css` as first import

- [x] Task 5: Configure EAS Build (AC: 6)
  - [x] Create `mobile/eas.json` with `development`, `preview`, `production` profiles (see exact content in Dev Notes)
  - [x] Verify `eas.json` is committed (not in `.gitignore`) — required by ARCH-24

- [x] Task 6: Create full directory structure (AC: 3)
  - [x] Restructure `app/` per project convention:
    - [x] Create `mobile/app/(auth)/` with `_layout.tsx`
    - [x] Ensure `mobile/app/(tabs)/` has correct `_layout.tsx` (clean up default template content)
    - [x] Create root `mobile/app/_layout.tsx` that imports `global.css` first (see exact content in Dev Notes)
  - [x] Create component subdirectories with `.gitkeep`:
    - [x] `mobile/components/chat/`
    - [x] `mobile/components/match/`
    - [x] `mobile/components/onboarding/`
    - [x] `mobile/components/shared/`
    - [x] `mobile/components/svg/`
    - [x] `mobile/components/moderation/` — `ReportButton.tsx` and `BlockConfirmation.tsx` belong here ONLY (never reimplemented inline)
  - [x] Create `mobile/hooks/` `.gitkeep`
  - [x] Create `mobile/utils/`
  - [x] Ensure `mobile/types/api/` and `mobile/types/signalr/` exist as separate directories

- [x] Task 7: Implement `services/storageService.ts` (AC: 5)
  - [x] Wrap `expo-secure-store` for JWT and refresh token storage (see exact content in Dev Notes)
  - [x] **NEVER import `AsyncStorage`** — even as a fallback — this is ARCH-7 and project-context rule 6
  - [x] Confirm: no `@react-native-async-storage/async-storage` import anywhere in `storageService.ts`

- [x] Task 8: Implement `services/apiClient.ts`
  - [x] Create HTTP client wrapper using native `fetch` with auth header injection (see exact content in Dev Notes)
  - [x] Import `storageService` for token retrieval — never read token in raw components
  - [x] Handle RFC 7807 Problem Details error extraction — never expose raw status codes to callers
  - [x] Environment base URL from `process.env.EXPO_PUBLIC_API_URL` with localhost fallback

- [x] Task 9: Implement `services/signalrService.ts`
  - [x] Create singleton SignalR connection manager (see exact content in Dev Notes)
  - [x] Use `@microsoft/signalr` package — never `ws` or raw WebSocket
  - [x] Singleton is module-level — NOT exported as class, NOT instantiated per component (UX-DR20)
  - [x] `withAutomaticReconnect()` MUST be called — covers VPS restarts and network drops

- [x] Task 10: Implement `types/api/index.ts` and `types/signalr/index.ts` (AC: 3)
  - [x] Create `types/api/index.ts` with `ProblemDetails` interface and stub comment (see exact content in Dev Notes)
  - [x] Create `types/signalr/index.ts` with `HubMethods` constant and stub payload types (see exact content in Dev Notes)
  - [x] Create `types/index.ts` with `AsyncState<T>` definition (AC: 7)
  - [x] **IMPORTANT:** `types/api/` and `types/signalr/` are separate namespaces — drift between them causes silent bugs (tech-preferences rule)

- [x] Task 11: Implement `constants/theme.ts` and `constants/errors.ts` (AC: 3)
  - [x] Create `constants/theme.ts` with complete Blinder color palette per UX spec (see exact content in Dev Notes)
  - [x] Create `constants/errors.ts` with all user-facing error strings — never expose raw exception messages (project-context rule 10)
  - [x] Delete `constants/Colors.ts` if generated by template — replace entirely with `constants/theme.ts`

- [x] Task 12: Implement `utils/dateFormat.ts` (AC: 3)
  - [x] Create date formatter using `Intl.DateTimeFormat` with `pl-PL` locale (primary locale per tech-preferences 16)
  - [x] All input dates are ISO 8601 UTC strings — never Unix timestamps
  - [x] Export `formatMessageTime` and `formatConversationDate` functions

- [x] Task 13: Update `app.json` configuration
  - [x] Set `"userInterfaceStyle": "dark"` — dark mode only at MVP (UX-DR1)
  - [x] Set `"scheme": "blinder"` — required for deep links (invite landing flow in Story 2.5)
  - [x] Add required Expo plugin entries for `expo-router`, `expo-secure-store`, `expo-notifications`, `expo-image-picker`
  - [x] Set `"experiments": { "typedRoutes": true }` — Expo Router typed routes

- [x] Task 14: Verify project starts and TypeScript passes (AC: 1, 2)
  - [x] Run `npx expo start` from `mobile/` — confirm dev server starts without errors
  - [x] Run `npx tsc --noEmit` from `mobile/` — confirm zero TypeScript errors
  - [x] Confirm NativeWind `className` prop works on a `View` in `app/_layout.tsx` without TS errors

## Dev Notes

### Critical Architecture Rules for This Story

| Rule | Requirement |
|---|---|
| ARCH-1 | Mobile starter: `npx create-expo-app@latest --template default@sdk-55` |
| ARCH-7 | `expo-secure-store` mandatory for ALL token storage — `AsyncStorage` explicitly prohibited for auth tokens |
| ARCH-24 | `eas.json` committed from the first commit — EAS Build is the only path to App Store / Play Store |
| ARCH-25 | `strict: true` in `tsconfig.json` before first component — retrofitting is costly |
| project-context rule 6 | `expo-secure-store` for all token storage — `AsyncStorage` prohibited |
| project-context rule 9 | `AsyncState<T>` from all async hooks — never raw `try/catch` in components |
| tech-preferences 10.2 | No inline styles — NativeWind utility classes only. No anonymous functions in `renderItem` or event handlers |
| tech-preferences 10.4 | `signalrService.ts` is singleton — never instantiated per component |
| tech-preferences 16 | Polish (`pl-PL`) primary locale — `Intl.DateTimeFormat` with `'pl-PL'` |

### Files to Delete from Default Template

The `create-expo-app default@sdk-55` template generates content that conflicts with our structure. Delete these before committing:

```
mobile/
├── app/(tabs)/index.tsx          ← delete (default Home screen)
├── app/(tabs)/explore.tsx        ← delete (default Explore screen)
├── components/Collapsible.tsx    ← delete
├── components/ExternalLink.tsx   ← delete
├── components/HapticTab.tsx      ← delete
├── components/HelloWave.tsx      ← delete
├── components/ParallaxScrollView.tsx ← delete
├── components/ThemedText.tsx     ← delete
├── components/ThemedView.tsx     ← delete
├── constants/Colors.ts           ← delete (replaced by constants/theme.ts)
├── hooks/useColorScheme.ts       ← delete
├── hooks/useColorScheme.web.ts   ← delete
├── hooks/useThemeColor.ts        ← delete
└── assets/images/               ← replace with minimal placeholder assets
```

Keep and update:
- `app/_layout.tsx` — update to import `global.css` and configure providers
- `app/(tabs)/_layout.tsx` — clean up to empty tab shell
- `package.json`, `tsconfig.json`, `app.json` — update, don't replace

### TypeScript Configuration (Required)

```json
{
  "extends": "expo/tsconfig.base",
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "exactOptionalPropertyTypes": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["./*"]
    }
  },
  "include": [
    "**/*.ts",
    "**/*.tsx",
    ".expo/types/**/*.d.ts",
    "expo-env.d.ts",
    "nativewind-env.d.ts"
  ]
}
```

**Note:** `expo/tsconfig.base` already enables many strict flags. Explicit overrides ensure all flags from tech-preferences 10.1 are active regardless of `expo/tsconfig.base` version changes.

### NativeWind v4 Configuration

NativeWind v4 is required for Expo SDK 55 (React Native 0.83). Setup differs significantly from NativeWind v2.

**`mobile/tailwind.config.ts`** — complete with all Blinder design tokens:

```typescript
import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./app/**/*.{js,jsx,ts,tsx}",
    "./components/**/*.{js,jsx,ts,tsx}",
  ],
  presets: [require("nativewind/preset")],
  theme: {
    extend: {
      colors: {
        // Blinder design tokens (UX-DR1, UX spec dark mode palette)
        // Dark mode only at MVP — no light mode toggle
        background: {
          primary: "#1A1814",   // App background — warm-tinted, not pure black
          surface: "#252219",   // Cards, received chat bubbles, modals
          input: "#2E2922",     // Input fields, secondary surfaces
          reveal: "#0F0D0B",   // Reveal screen ONLY — deliberate exception
        },
        accent: {
          primary: "#C8833A",   // CTAs, reveal trigger affordance, active states (amber)
          secondary: "#8A5A28", // Secondary actions
          reveal: "#D4A843",    // Reveal screen gold — more luminous, paired ONLY with background.reveal
        },
        text: {
          primary: "#F2EDE6",  // Body text, names — warm white (13.4:1 contrast ✅ AAA)
          secondary: "#9E9790", // Timestamps, labels, captions (5.1:1 contrast ✅ AA)
          muted: "#635D57",    // Placeholder text, disabled states
        },
        safety: "#4A9E8A",    // Consent statements, privacy indicators — calm, not urgent (4.6:1 ✅ AA)
        danger: "#D94F4F",    // Report button ONLY — never decorative (4.58:1 ✅ AA)
      },
      fontFamily: {
        // DM Sans — brand typeface. Never substitute with system-ui without explicit approval.
        sans: ["DM Sans", "SF Pro Text", "Roboto"],
      },
    },
  },
  plugins: [],
};

export default config;
```

**`mobile/babel.config.js`:**

```javascript
module.exports = function (api) {
  api.cache(true);
  return {
    presets: [
      ["babel-preset-expo", { jsxImportSource: "nativewind" }],
      "nativewind/babel",
    ],
  };
};
```

**`mobile/metro.config.js`:**

```javascript
const { getDefaultConfig } = require("expo/metro-config");
const { withNativeWind } = require("nativewind/metro");

const config = getDefaultConfig(__dirname);

module.exports = withNativeWind(config, { input: "./global.css" });
```

**`mobile/global.css`:**

```css
@tailwind base;
@tailwind components;
@tailwind utilities;
```

**`mobile/nativewind-env.d.ts`:**

```typescript
/// <reference types="nativewind/types" />
```

**Verification**: After setup, `<View className="bg-background-primary flex-1" />` should compile without TypeScript errors.

### Root Layout (`app/_layout.tsx`)

```tsx
import "../global.css";

import { SafeAreaProvider } from "react-native-safe-area-context";
import { Stack } from "expo-router";

// AccessibilityContext comes in Story 1.5 — placeholder structure here
export default function RootLayout() {
  return (
    <SafeAreaProvider>
      <Stack
        screenOptions={{
          headerShown: false,
          contentStyle: { backgroundColor: "#1A1814" },
        }}
      />
    </SafeAreaProvider>
  );
}
```

**Critical:** `import "../global.css"` MUST be the first import. NativeWind requires this to process Tailwind classes.

### Route Group Layouts

**`mobile/app/(auth)/_layout.tsx`:**

```tsx
import { Stack } from "expo-router";

export default function AuthLayout() {
  return (
    <Stack
      screenOptions={{
        headerShown: false,
        contentStyle: { backgroundColor: "#1A1814" },
      }}
    />
  );
}
```

**`mobile/app/(tabs)/_layout.tsx`:**

```tsx
import { Tabs } from "expo-router";

// Tab bar configuration comes in later stories when actual screens exist
export default function TabsLayout() {
  return (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarStyle: { backgroundColor: "#1A1814" },
      }}
    />
  );
}
```

### `app.json` Configuration

```json
{
  "expo": {
    "name": "Blinder",
    "slug": "blinder",
    "version": "1.0.0",
    "orientation": "portrait",
    "scheme": "blinder",
    "userInterfaceStyle": "dark",
    "ios": {
      "supportsTablet": false,
      "bundleIdentifier": "com.yourcompany.blinder"
    },
    "android": {
      "adaptiveIcon": {
        "foregroundImage": "./assets/images/adaptive-icon.png",
        "backgroundColor": "#1A1814"
      },
      "package": "com.yourcompany.blinder"
    },
    "plugins": [
      "expo-router",
      "expo-secure-store",
      "expo-notifications",
      [
        "expo-image-picker",
        {
          "photosPermission": "Blinder potrzebuje dostępu do biblioteki zdjęć, aby przesłać zdjęcie profilowe."
        }
      ]
    ],
    "experiments": {
      "typedRoutes": true
    }
  }
}
```

**Notes:**
- `"scheme": "blinder"` — required for invite deep links in Story 2.5. Do NOT skip.
- `"userInterfaceStyle": "dark"` — dark mode only at MVP (UX-DR1). Dating apps used primarily at night.
- `"typedRoutes": true` — enables Expo Router's TypeScript type generation for `href` params.

### EAS Build Configuration (`eas.json`)

```json
{
  "cli": {
    "version": ">= 12.0.0"
  },
  "build": {
    "development": {
      "developmentClient": true,
      "distribution": "internal"
    },
    "preview": {
      "distribution": "internal",
      "android": {
        "buildType": "apk"
      }
    },
    "production": {
      "autoIncrement": true
    }
  },
  "submit": {
    "production": {}
  }
}
```

**Why development profile:** `expo-secure-store` and future native modules (push notifications, image picker) require a development client build to function correctly. Use `eas build --profile development` for device testing with native modules.

### AsyncState Type Definition (`types/index.ts`)

This type MUST be defined exactly as specified and used consistently across all hooks:

```typescript
// types/index.ts — shared types used across all mobile code

/**
 * Standard async state shape for all custom hooks.
 * - ARCH-7 / project-context rule 9: NEVER use raw try/catch in components
 * - data and error are NEVER both non-null simultaneously
 * - error strings come from constants/errors.ts ONLY — never raw exception messages
 */
export type AsyncState<T> = {
  data: T | null;
  error: string | null;
  isLoading: boolean;
};
```

### `services/storageService.ts` — Complete Implementation

```typescript
import * as SecureStore from "expo-secure-store";

// ARCH-7, project-context rule 6: NEVER use AsyncStorage for auth tokens.
// AsyncStorage is unencrypted. expo-secure-store maps to:
//   - iOS: Keychain Services
//   - Android: Android Keystore System

const KEYS = {
  JWT_TOKEN: "blinder_jwt_token",
  REFRESH_TOKEN: "blinder_refresh_token",
} as const;

export const storageService = {
  async saveToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(KEYS.JWT_TOKEN, token);
  },

  async getToken(): Promise<string | null> {
    return SecureStore.getItemAsync(KEYS.JWT_TOKEN);
  },

  async deleteToken(): Promise<void> {
    await SecureStore.deleteItemAsync(KEYS.JWT_TOKEN);
  },

  async saveRefreshToken(token: string): Promise<void> {
    await SecureStore.setItemAsync(KEYS.REFRESH_TOKEN, token);
  },

  async getRefreshToken(): Promise<string | null> {
    return SecureStore.getItemAsync(KEYS.REFRESH_TOKEN);
  },

  async deleteRefreshToken(): Promise<void> {
    await SecureStore.deleteItemAsync(KEYS.REFRESH_TOKEN);
  },

  async clearAll(): Promise<void> {
    await Promise.all([
      SecureStore.deleteItemAsync(KEYS.JWT_TOKEN),
      SecureStore.deleteItemAsync(KEYS.REFRESH_TOKEN),
    ]);
  },
} as const;
```

### `services/apiClient.ts` — Complete Implementation

```typescript
import { storageService } from "./storageService";
import { ERRORS } from "../constants/errors";

// All API calls go through this client — NEVER raw fetch in components or hooks
// tech-preferences 10.4: All API calls through apiClient.ts

const API_BASE_URL =
  process.env.EXPO_PUBLIC_API_URL ?? "http://localhost/api";

type RequestOptions = Omit<RequestInit, "body"> & {
  body?: unknown;
};

async function request<T>(
  path: string,
  options: RequestOptions = {}
): Promise<T> {
  const token = await storageService.getToken();

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token !== null ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  });

  if (!response.ok) {
    // RFC 7807 Problem Details — extract title for user display
    // Never expose raw status codes or stack traces to components (project-context rule 10)
    const problem = await response
      .json()
      .catch(() => ({ title: ERRORS.UNEXPECTED_ERROR }));
    throw new Error(
      (problem as { title?: string }).title ?? ERRORS.UNEXPECTED_ERROR
    );
  }

  if (response.status === 204) {
    return undefined as unknown as T;
  }

  return response.json() as Promise<T>;
}

export const apiClient = {
  get: <T>(
    path: string,
    options?: Omit<RequestOptions, "method" | "body">
  ) => request<T>(path, { ...options, method: "GET" }),

  post: <T>(
    path: string,
    body?: unknown,
    options?: Omit<RequestOptions, "method" | "body">
  ) => request<T>(path, { ...options, method: "POST", body }),

  put: <T>(
    path: string,
    body?: unknown,
    options?: Omit<RequestOptions, "method" | "body">
  ) => request<T>(path, { ...options, method: "PUT", body }),

  patch: <T>(
    path: string,
    body?: unknown,
    options?: Omit<RequestOptions, "method" | "body">
  ) => request<T>(path, { ...options, method: "PATCH", body }),

  delete: <T>(
    path: string,
    options?: Omit<RequestOptions, "method" | "body">
  ) => request<T>(path, { ...options, method: "DELETE" }),
} as const;
```

### `services/signalrService.ts` — Complete Implementation

```typescript
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { storageService } from "./storageService";

// SINGLETON — module-level, never instantiated per component (UX-DR20, tech-preferences 10.4)
// Components NEVER manage connection state directly.
// Components subscribe via hooks ONLY: useConversation, useRevealState

let connection: HubConnection | null = null;

const HUB_URL = `${
  (process.env.EXPO_PUBLIC_API_URL ?? "http://localhost/api").replace(
    /\/api$/,
    ""
  )
}/hubs/chat`;

export const signalrService = {
  async start(): Promise<void> {
    if (connection?.state === HubConnectionState.Connected) return;

    const token = await storageService.getToken();

    connection = new HubConnectionBuilder()
      .withUrl(HUB_URL, {
        accessTokenFactory: () => token ?? "",
      })
      .withAutomaticReconnect() // Handles VPS restarts and network drops
      .configureLogging(LogLevel.Warning)
      .build();

    await connection.start();
  },

  async stop(): Promise<void> {
    await connection?.stop();
    connection = null;
  },

  getConnection(): HubConnection | null {
    return connection;
  },

  isConnected(): boolean {
    return connection?.state === HubConnectionState.Connected;
  },
} as const;
```

**Critical:** The `/hubs/chat` path matches the Nginx WebSocket location block configured in Story 1.2 (ARCH-3). Do NOT change this path.

### `types/api/index.ts`

```typescript
// All API response types must match backend DTOs exactly.
// Backend uses DateTimeOffset → ISO 8601 strings in API responses (project-context rule 3).
// Populated story-by-story as endpoints are implemented.

/**
 * RFC 7807 Problem Details — shape returned by backend on all 4xx/5xx errors.
 * Backend uses AddProblemDetails() + AppErrors.cs (ARCH-10).
 */
export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
}

// Future types added per story:
// Story 2.1: AuthTokenDto, RegisterRequest, LoginRequest
// Story 3.1: QuizResponse, QuizSubmissionRequest
// Story 4.x: MatchDto, ConversationSummaryDto
// Story 5.x: MessageDto, SendMessageRequest
// Story 6.x: RevealStateDto, RevealReadyRequest
```

### `types/signalr/index.ts`

```typescript
// SignalR hub method names and payload types.
// Hub method names are PascalCase on server; must match backend Hubs/ChatHub.cs EXACTLY.
// IMPORTANT: types/api/ and types/signalr/ are SEPARATE namespaces.
// Drift between them causes silent bugs (tech-preferences section 1).

/** Server-to-client hub method names. Must match ChatHub.cs exactly. */
export const HubMethods = {
  // Story 5.1: ReceiveMessage
  RECEIVE_MESSAGE: "ReceiveMessage",
  // Story 6.3: RevealStateUpdated — broadcast when BOTH reveal_ready flags are set (ARCH-13)
  REVEAL_STATE_UPDATED: "RevealStateUpdated",
  // Story 4.3: MatchAssigned — triggered by MatchGenerationJob
  MATCH_ASSIGNED: "MatchAssigned",
} as const;

export type HubMethodName = (typeof HubMethods)[keyof typeof HubMethods];

// Payload stubs — full types implemented per story:

/** Story 5.1 */
export interface ReceiveMessagePayload {
  conversationId: string;
  messageId: string;
  senderId: string;
  content: string;
  sentAt: string; // ISO 8601 UTC
}

/** Story 6.3 */
export interface RevealStateUpdatedPayload {
  conversationId: string;
  userAReady: boolean;
  userBReady: boolean;
}

/** Story 4.3 */
export interface MatchAssignedPayload {
  conversationId: string;
  matchedAt: string; // ISO 8601 UTC
}
```

### `constants/theme.ts` — Complete Design Tokens

```typescript
/**
 * Blinder design tokens — single source of truth for all visual decisions.
 * Dark mode only at MVP (UX-DR1). No light mode toggle.
 * Source: UX Design Specification + tech-preferences section 12.
 *
 * Amber bookend (UX-DR2): accent.primary (#C8833A) appears at TWO moments only:
 *   1. Onboarding entry (within first 3 seconds of app open)
 *   2. Reveal trigger screen
 * This bookend creates a subconscious visual through-line.
 */
export const colors = {
  background: {
    primary: "#1A1814",  // App background
    surface: "#252219",  // Cards, received bubbles, modals
    input: "#2E2922",    // Input fields
    reveal: "#0F0D0B",   // Reveal screen ONLY — deliberate exception (darker for reveal intimacy)
  },
  accent: {
    primary: "#C8833A",  // CTAs, active states, reveal trigger (amber)
    secondary: "#8A5A28",
    reveal: "#D4A843",   // Reveal screen gold — paired ONLY with background.reveal
  },
  text: {
    primary: "#F2EDE6",  // 13.4:1 on background.primary ✅ AAA
    secondary: "#9E9790", // 5.1:1 on background.primary ✅ AA
    muted: "#635D57",
  },
  safety: "#4A9E8A",    // 4.6:1 on background.primary ✅ AA — consent, privacy
  danger: "#D94F4F",    // 4.58:1 on background.primary ✅ AA — report only, never decorative
} as const;

/**
 * Typography scale — DM Sans typeface.
 * NEVER substitute with system-ui without explicit product approval.
 * DM Sans is brand delivery, not decoration (~85KB bundle, acceptable at MVP).
 */
export const typography = {
  family: {
    primary: "DM Sans",
    fallback: ["SF Pro Text", "Roboto", "system-ui", "sans-serif"],
  },
  size: {
    displayXl: 32,  // Reveal screen headline, onboarding hero
    displayLg: 26,  // Section headlines, match arrival
    titleMd: 20,    // Screen titles, name display
    titleSm: 17,    // Card titles, conversation name
    bodyLg: 16,     // Chat messages — primary reading size
    bodyMd: 15,     // Descriptions, quiz answers
    bodySm: 14,     // Supporting text
    captionMd: 12,  // Timestamps, metadata
    captionSm: 11,  // Never below 11px at default scale
    labelMd: 14,    // Button text, navigation labels
    labelSm: 12,    // Tags, small buttons
  },
  lineHeight: {
    tight: 1.2,
    snug: 1.35,
    normal: 1.5,
    relaxed: 1.6,   // Chat messages — generous for emotional readability
  },
} as const;

/**
 * Spacing — 4px base unit.
 * All spacing derived from this unit for consistency.
 */
export const spacing = {
  1: 4,
  2: 8,
  3: 12,
  4: 16,   // Standard component padding
  5: 20,
  6: 24,
  8: 32,
  10: 40,  // Generous breathing room — reveal screen, onboarding
  12: 48,
  16: 64,  // Screen-level top/bottom padding
} as const;

/**
 * Border radii.
 */
export const radii = {
  sm: 8,
  md: 12,
  lg: 16,   // Cards, modals
  full: 9999,
  bubbleSent: {
    topLeft: 18,
    topRight: 18,
    bottomRight: 4,  // WhatsApp-conventional
    bottomLeft: 18,
  },
  bubbleReceived: {
    topLeft: 18,
    topRight: 18,
    bottomRight: 18,
    bottomLeft: 4,
  },
} as const;

/**
 * Motion durations (ms).
 * All animations MUST respect AccessibilityInfo.isReduceMotionEnabled() (UX-DR13, project-context rule 15).
 * When reduce motion is enabled: fade-only variant, duration → 0ms or opacity fade.
 */
export const motion = {
  fast: 150,      // Micro-interactions (button press, input focus)
  standard: 300,  // Screen transitions, modal entry
  reveal: 700,    // The mutual reveal moment — ceremonial, 600–800ms range
} as const;

/**
 * Touch target minimums — WCAG 2.1 AA compliance (project-context rule 15).
 */
export const touchTarget = {
  minSize: 44,         // All interactive elements: minHeight: 44, minWidth: 44
  hitSlop: {           // hitSlop on icon-only controls (back button, report icon)
    top: 8,
    bottom: 8,
    left: 8,
    right: 8,
  },
} as const;
```

### `constants/errors.ts`

```typescript
// All user-facing error message strings.
// project-context rule 10: NEVER expose raw exception messages or stack traces in UI.
// Error strings come from here ONLY — never inline string literals in components.
// Future stories add domain-specific error keys.

export const ERRORS = {
  NETWORK_ERROR: "Brak połączenia. Sprawdź połączenie z internetem.",
  UNEXPECTED_ERROR: "Coś poszło nie tak. Spróbuj ponownie.",
  SESSION_EXPIRED: "Sesja wygasła. Zaloguj się ponownie.",
  INVALID_CREDENTIALS: "Nieprawidłowy e-mail lub hasło.",
  EMAIL_ALREADY_REGISTERED: "Konto z tym adresem e-mail już istnieje.",
  INVITE_REQUIRED: "Rejestracja kobiet wymaga ważnego linku zaproszenia.",
  INVALID_INVITE: "Ten link zaproszenia jest nieprawidłowy lub został już użyty.",
  CONVERSATION_LIMIT: "Osiągnąłeś maksymalną liczbę aktywnych rozmów.",
  REVEAL_THRESHOLD_NOT_MET: "Potrzeba więcej wiadomości, zanim ujawnienie będzie dostępne.",
  PHOTO_SCAN_FAILED: "Nie można przetworzyć zdjęcia. Spróbuj inne zdjęcie.",
  UPLOAD_FAILED: "Przesyłanie zdjęcia nie powiodło się. Spróbuj ponownie.",
} as const;

export type ErrorKey = keyof typeof ERRORS;
```

**Note:** Polish strings per tech-preferences section 16 (`pl-PL` primary locale). English fallback translations will be added via i18n architecture in a future story.

### `utils/dateFormat.ts`

```typescript
// All datetimes from API are ISO 8601 UTC strings (project-context rule 3).
// Display formatting uses Polish locale (tech-preferences section 16).
// NEVER accept Unix timestamps — always ISO 8601 input.

/**
 * Format a message timestamp for display in chat.
 * Returns HH:MM in Polish locale.
 */
export function formatMessageTime(iso8601: string): string {
  return new Intl.DateTimeFormat("pl-PL", {
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(iso8601));
}

/**
 * Format a conversation's last activity date.
 * Today: returns time only (HH:MM).
 * Earlier: returns date (DD.MM.YYYY).
 */
export function formatConversationDate(iso8601: string): string {
  const date = new Date(iso8601);
  const today = new Date();

  const isToday =
    date.getDate() === today.getDate() &&
    date.getMonth() === today.getMonth() &&
    date.getFullYear() === today.getFullYear();

  if (isToday) {
    return new Intl.DateTimeFormat("pl-PL", {
      hour: "2-digit",
      minute: "2-digit",
    }).format(date);
  }

  return new Intl.DateTimeFormat("pl-PL", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(date);
}
```

### Final Directory Structure to Create

```
mobile/
├── app/
│   ├── _layout.tsx              ← import global.css FIRST + SafeAreaProvider
│   ├── (auth)/
│   │   └── _layout.tsx          ← auth stack shell
│   └── (tabs)/
│       └── _layout.tsx          ← tabs shell (screens added in later stories)
├── components/
│   ├── chat/
│   │   └── .gitkeep             ← ChatBubble, ChatInput added in Story 5.1
│   ├── match/
│   │   └── .gitkeep             ← EmptyMatchState (UX-DR7) added in Story 4.5
│   ├── onboarding/
│   │   └── .gitkeep             ← Quiz components added in Story 3.1
│   ├── shared/
│   │   └── .gitkeep             ← Button, Input, Modal, Typography added as needed
│   ├── svg/
│   │   └── .gitkeep             ← PascalCase SVG exports, no default exports
│   └── moderation/
│       └── .gitkeep             ← ReportButton.tsx, BlockConfirmation.tsx belong HERE ONLY
├── hooks/
│   └── .gitkeep                 ← useAuth, useConversation, useRevealState added per story
├── services/
│   ├── apiClient.ts             ← HTTP client with auth header injection
│   ├── signalrService.ts        ← Singleton SignalR connection manager
│   └── storageService.ts        ← expo-secure-store wrapper (JWT tokens ONLY)
├── constants/
│   ├── errors.ts                ← All user-facing error strings
│   └── theme.ts                 ← Complete design tokens
├── types/
│   ├── index.ts                 ← AsyncState<T> and shared types
│   ├── api/
│   │   └── index.ts             ← API response/request types (match backend DTOs)
│   └── signalr/
│       └── index.ts             ← Hub method names and payload types
├── utils/
│   └── dateFormat.ts            ← ISO 8601 → display string formatters
├── app.json                     ← scheme: "blinder", dark mode, plugins
├── babel.config.js              ← NativeWind babel preset
├── eas.json                     ← EAS Build profiles (dev, preview, production)
├── global.css                   ← @tailwind directives
├── metro.config.js              ← withNativeWind wrapper
├── nativewind-env.d.ts          ← className prop types
├── package.json
└── tsconfig.json                ← strict: true + all strict flags
```

### What NOT to Build in This Story

These features belong to later stories — do NOT add them here:

| Feature | Story |
|---|---|
| AccessibilityContext (`{ reduceMotion, fontScale, isScreenReaderEnabled }`) | Story 1.5 |
| DM Sans font loading (`expo-font`) | Story 1.5 |
| Auth screens (`login.tsx`, `register.tsx`) | Story 2.1 |
| JWT Bearer auth token refresh logic | Story 2.1 |
| Push notification registration | Story 5.4 |
| Tab bar icons and actual tab screens | Story 4.5+ |
| `invite-landing.tsx` | Story 2.5 |
| `ReportButton.tsx` actual implementation | Story 8.2 |

Create `.gitkeep` files in component subdirectories to establish the structure without implementing anything prematurely.

### EXPO_PUBLIC_API_URL Environment Variable

Create `mobile/.env` (git-ignored). Expo loads `.env` from the same directory as `package.json` — the root `.env` is not read by Expo.

```
# Android emulator — 10.0.2.2 resolves to host machine
EXPO_PUBLIC_API_URL=http://10.0.2.2/api

# iOS simulator — use localhost instead
# EXPO_PUBLIC_API_URL=http://localhost/api
```

Add to root `.env.example` (project-context rule 12):

```
# Mobile (Android emulator: 10.0.2.2 = host machine; iOS simulator: localhost)
EXPO_PUBLIC_API_URL=http://10.0.2.2/api
```

Notes:
- `EXPO_PUBLIC_` prefix is required for Expo to include the variable in the client bundle.
- Changes to `.env` require a full native rebuild (`npx expo run:android`), not just a Metro reload.
- Android 9+ blocks cleartext HTTP by default — `android.usesCleartextTraffic: true` must be set in `app.json` for local dev.

### Previous Story Learnings Applied (Stories 1.1 and 1.2)

From Story 1.1:
- Backend at `backend/Blinder.Api/` — mobile is a completely separate project in `mobile/`
- `backend/` and `mobile/` are sibling directories at repo root

From Story 1.2:
- Nginx routes `/hubs/` with WebSocket headers (ARCH-3) — `signalrService.ts` must connect to `/hubs/chat`, not `/hubs/chat` directly on port 8080
- Docker is in Windows containers mode on the dev machine — this does NOT affect `npx expo start` (runs directly on host, not via Docker)
- `.env.example` must be kept in sync — if `EXPO_PUBLIC_API_URL` is added to `.env.example`, document it there in the same commit

### Testing Approach for This Story

This is a scaffolding story — no business logic to unit test. Verification is:
1. `npx tsc --noEmit` from `mobile/` → 0 errors
2. `npx expo start` → dev server starts
3. Manual check: `className` prop works on View without TS error
4. Manual check: `storageService.saveToken("test")` compiles and doesn't import AsyncStorage

Jest and React Native Testing Library setup comes in Story 1.5 or first story that adds testable logic.

### References

- ARCH-1: Mobile starter command — `npx create-expo-app@latest --template default@sdk-55`
- ARCH-7: expo-secure-store mandatory for all token storage
- ARCH-24: EAS Build — `eas.json` committed from first commit
- ARCH-25: TypeScript `strict: true` from day one
- UX-DR1: Design token system in `constants/theme.ts`, NativeWind, dark mode only
- UX-DR2: Amber bookend principle (`#C8833A` at two moments only)
- UX-DR12: AccessibilityContext at root (Story 1.5)
- UX-DR20: `signalrService.ts` singleton — components never manage connection state
- UX-DR21: AsyncState<T> shape from all async hooks
- project-context rules 6, 9, 10, 15
- tech-preferences sections 10, 12, 13, 16
- [Source: docs/tech-preferences.md#1-repository--project-structure]
- [Source: docs/tech-preferences.md#2-technology-stack-version-pinned]
- [Source: docs/tech-preferences.md#10-mobile--typescript-rules]

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- SDK 55 template now generates `src/` directory structure (not flat `app/` at root). Deleted `mobile/src/` and `mobile/scripts/` entirely — all files created fresh at story-specified locations.
- `exactOptionalPropertyTypes: true` caused TS error in `apiClient.ts`: changed `body: undefined` to `body: null` in fetch call to satisfy `BodyInit | null` type requirement.
- Template asset file is `android-icon-foreground.png` (not `adaptive-icon.png`). Updated `app.json` accordingly.
- `npx expo install expo-secure-store` auto-added the config plugin to `app.json` plugins array.
- NativeWind v4.2.3 installed (compatible with Expo SDK 55 / React Native 0.83.2).

### Completion Notes List

- Initialized `mobile/` with `create-expo-app@latest --template default@sdk-55`, expo v55.0.6.
- Deleted template `src/` structure; rebuilt all files at root-level paths per story spec.
- TypeScript: `strict`, `noImplicitAny`, `strictNullChecks`, `noUnusedLocals`, `noUnusedParameters`, `exactOptionalPropertyTypes` all active. `npx tsc --noEmit` → 0 errors.
- NativeWind v4 configured: `tailwind.config.ts`, `babel.config.js`, `metro.config.js`, `global.css`, `nativewind-env.d.ts`.
- EAS Build: `eas.json` committed with `development`, `preview`, `production` profiles (ARCH-24).
- All 6 required packages installed: `expo-router`, `expo-notifications`, `expo-image-picker`, `expo-secure-store`, `@microsoft/signalr`, `nativewind`.
- `storageService.ts`: uses `expo-secure-store` exclusively — no `AsyncStorage` import (ARCH-7).
- `apiClient.ts`: RFC 7807 Problem Details error handling, `EXPO_PUBLIC_API_URL` env var, auth header injection.
- `signalrService.ts`: module-level singleton, `withAutomaticReconnect()`, connects to `/hubs/chat` (matches ARCH-3 Nginx config from Story 1.2).
- `types/index.ts`: `AsyncState<T>` shape for all async hooks (project-context rule 9).
- `types/api/index.ts` and `types/signalr/index.ts`: separate namespaces as required.
- `constants/theme.ts`: full Blinder design token system (colors, typography, spacing, radii, motion, touchTarget).
- `constants/errors.ts`: all Polish-locale user-facing error strings (pl-PL, project-context rule 10).
- `utils/dateFormat.ts`: `formatMessageTime` and `formatConversationDate` using `Intl.DateTimeFormat('pl-PL')`.
- `app.json`: name=Blinder, scheme=blinder, userInterfaceStyle=dark, all required plugins, typedRoutes=true.
- `.env.example` updated with `EXPO_PUBLIC_API_URL` (project-context rule 12).

### File List

- mobile/app/_layout.tsx
- mobile/app/(auth)/_layout.tsx
- mobile/app/(tabs)/_layout.tsx
- mobile/components/chat/.gitkeep
- mobile/components/match/.gitkeep
- mobile/components/onboarding/.gitkeep
- mobile/components/shared/.gitkeep
- mobile/components/svg/.gitkeep
- mobile/components/moderation/.gitkeep
- mobile/hooks/.gitkeep
- mobile/services/storageService.ts
- mobile/services/apiClient.ts
- mobile/services/signalrService.ts
- mobile/types/index.ts
- mobile/types/api/index.ts
- mobile/types/signalr/index.ts
- mobile/constants/theme.ts
- mobile/constants/errors.ts
- mobile/utils/dateFormat.ts
- mobile/app.json
- mobile/tsconfig.json
- mobile/package.json
- mobile/babel.config.js
- mobile/metro.config.js
- mobile/tailwind.config.ts
- mobile/global.css
- mobile/nativewind-env.d.ts
- mobile/eas.json
- mobile/.env.local
- .env.example

## Change Log

- 2026-03-16: Story 1.3 created — comprehensive mobile scaffold story with Expo SDK 55, strict TypeScript, NativeWind v4 design tokens, EAS Build, and all service stubs.
- 2026-03-16: Story 1.3 implemented — mobile/ scaffold complete. All 14 tasks done, `npx tsc --noEmit` passes with zero errors. Ready for review.
