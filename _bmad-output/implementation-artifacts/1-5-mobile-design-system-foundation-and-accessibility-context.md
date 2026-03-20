# Story 1.5: Mobile Design System Foundation and Accessibility Context

Status: done

## Story

As a user,
I want the app to use Blinder's dark, warm visual identity from the very first screen, with full accessibility support built in from the start,
So that every subsequent screen is consistent, accessible, and never requires design retrofitting.

## Acceptance Criteria

1. **Given** `constants/theme.ts` is implemented
   **When** the colour tokens are reviewed
   **Then** the following tokens exist: background `#1A1814`, primary text `#F2EDE6`, secondary text `#9E9790`, amber accent `#C8833A`, safety teal `#4A9E8A`, danger red `#D94F4F`, and inactive `#635D57` — dark mode only at MVP; no light mode toggle

2. **Given** a screen renders
   **When** WCAG AA contrast is checked between all text/background pairs
   **Then** amber on background = 4.82:1 ✅, primary text on background = 13.4:1 ✅, secondary text on background = 5.1:1 ✅

3. **Given** `AccessibilityContext` is implemented at app root
   **When** any component accesses the context
   **Then** `{ reduceMotion: boolean, fontScale: number, isScreenReaderEnabled: boolean }` are available — no per-component `AccessibilityInfo` calls needed

4. **Given** `reduceMotion` is `true` in `AccessibilityContext`
   **When** any animated component renders
   **Then** all transitions snap to 0ms duration or use opacity-fade-only — no motion-based transitions fire

5. **Given** any interactive element renders
   **When** its touch target dimensions are measured
   **Then** minimum `minHeight: 44, minWidth: 44` logical pixels are applied; icon-only controls have `hitSlop={{ top: 8, bottom: 8, left: 8, right: 8 }}`

6. **Given** any interactive element renders
   **When** VoiceOver or TalkBack inspects it
   **Then** `accessibilityRole`, `accessibilityLabel`, and `accessibilityHint` (where non-obvious) are present on every element

7. **Given** the app is running with system font size at ×2.0
   **When** any screen is rendered
   **Then** `allowFontScaling={true}` is set on all `Text` components, no text is truncated, and no fixed-height text containers clip content

---

## Dev Notes

### Critical State: What Story 1.3 Already Built

**DO NOT re-implement any of the following — it already exists and is correct:**

| Component | Location | Status |
|---|---|---|
| `constants/theme.ts` — full color palette, typography, spacing, radii, motion, touchTarget | `mobile/constants/theme.ts` | ✅ Done |
| `constants/errors.ts` — user-facing error strings in Polish | `mobile/constants/errors.ts` | ✅ Done |
| `services/apiClient.ts` — typed fetch wrapper | `mobile/services/apiClient.ts` | ✅ Done |
| `services/signalrService.ts` — SignalR singleton | `mobile/services/signalrService.ts` | ✅ Done |
| `services/storageService.ts` — expo-secure-store wrapper | `mobile/services/storageService.ts` | ✅ Done |
| `app/_layout.tsx` — SafeAreaProvider + Stack with bg `#1A1814` | `mobile/app/_layout.tsx` | ✅ Done (needs AccessibilityContext added) |
| Component subdirectories scaffolded | `mobile/components/{chat,match,moderation,onboarding,shared,svg}/` | ✅ Done (contain .gitkeep) |
| `hooks/` directory | `mobile/hooks/` | ✅ Done (empty, .gitkeep) |

**AC 1 (theme.ts tokens) and AC 2 (WCAG contrast) are already satisfied by Story 1.3's implementation.**

The theme.ts exports: `colors`, `typography`, `spacing`, `radii`, `motion`, `touchTarget`.

Existing contrast ratios (already verified):
- `colors.text.primary` (#F2EDE6) on `colors.background.primary` (#1A1814): **13.4:1 ✅ AAA**
- `colors.text.secondary` (#9E9790) on `colors.background.primary` (#1A1814): **5.1:1 ✅ AA**
- `colors.accent.primary` (#C8833A) on `colors.background.primary` (#1A1814): **4.82:1 ✅ AA**

### What This Story Builds

This story delivers three concrete deliverables:

1. **`contexts/AccessibilityContext.tsx`** — React context providing `{ reduceMotion, fontScale, isScreenReaderEnabled }` at app root
2. **`app/_layout.tsx` update** — wrap the tree with `AccessibilityProvider`
3. **Two base shared components** that enforce accessibility patterns for all future components:
   - `components/shared/AccessiblePressable.tsx` — enforces 44×44 touch target + hitSlop (AC 5)
   - `components/shared/ThemedText.tsx` — enforces `allowFontScaling={true}` (AC 7)

These three things collectively satisfy ACs 3–7 and give every subsequent story a proven foundation to build on. ACs 5, 6, and 7 are demonstrated through these base components, not by creating full feature screens.

### Architecture Rules for This Story

| Rule | Requirement |
|---|---|
| UX-DR12 | `AccessibilityContext` at app root — provides `{ reduceMotion, fontScale, isScreenReaderEnabled }`. Components NEVER call `AccessibilityInfo` directly. |
| UX-DR13 | Reduce motion: all animations snap to 0ms or opacity-fade-only when `reduceMotion === true`. The `motion.reveal` duration (700ms) collapses to 0 for reduce-motion users. |
| UX-DR14 | Touch targets: `minHeight: 44, minWidth: 44` on all `Pressable`/`TouchableOpacity`. `hitSlop={{ top: 8, bottom: 8, left: 8, right: 8 }}` on icon-only controls. |
| UX-DR15 | Every interactive element declares `accessibilityRole`, `accessibilityLabel`, `accessibilityHint` (where non-obvious), `accessibilityState`. |
| UX-DR16 | `allowFontScaling={true}` on every `Text`. No fixed-height text containers. Test at ×1.0, ×1.3, ×2.0. |
| UX-DR17 | `useResponsiveLayout` hook exposes `compact` (<375px), `regular` (375–427px), `expanded` (≥428px) breakpoints via `useWindowDimensions`. |
| ARCH-25 | TypeScript `strict: true` — no `any`, no `as unknown`, full return types on all functions. |
| project-context rule 9 | All async hooks return `AsyncState<T>` — but the AccessibilityContext reads are synchronous; no `AsyncState` shape needed here. |
| project-context rule 15 | WCAG 2.1 AA — non-negotiable, not optional. |

### Implementation: `contexts/AccessibilityContext.tsx`

Create this file at `mobile/contexts/AccessibilityContext.tsx`.

**Key React Native APIs to use:**
- `AccessibilityInfo.isReduceMotionEnabled()` — returns `Promise<boolean>`, call once on mount
- `AccessibilityInfo.addEventListener('reduceMotionChanged', handler)` — listen for system changes
- `AccessibilityInfo.isBoldTextEnabled()` — NOT needed for this story
- `AccessibilityInfo.isScreenReaderEnabled()` — returns `Promise<boolean>`, call once on mount
- `AccessibilityInfo.addEventListener('screenReaderChanged', handler)` — listen for changes
- `useWindowDimensions().fontScale` — synchronous, gets current system font scale

**Context shape:**
```typescript
type AccessibilityContextValue = {
  reduceMotion: boolean;
  fontScale: number;
  isScreenReaderEnabled: boolean;
};
```

**Provider pattern:**
```typescript
// Reads AccessibilityInfo ONCE on mount, then listens for changes
// Default values while loading: { reduceMotion: false, fontScale: 1, isScreenReaderEnabled: false }
// This is safe — animations will run on first paint but stop if user has reduce motion on
// A better default if you want to be conservative: reduceMotion: true (no motion until confirmed safe)
// DECISION: default reduceMotion: true to be accessibility-first
```

**Export both the context and a provider component:**
```typescript
export const AccessibilityContext = React.createContext<AccessibilityContextValue>(...)
export function AccessibilityProvider({ children }: { children: React.ReactNode }) { ... }
export function useAccessibility(): AccessibilityContextValue { ... }
```

The `useAccessibility()` hook must throw if called outside `AccessibilityProvider` — this prevents silent bugs where context reads return defaults instead of real values.

### Implementation: Update `app/_layout.tsx`

Current state (Story 1.3):
```tsx
// AccessibilityContext comes in Story 1.5 — placeholder structure here
export default function RootLayout() {
  return (
    <SafeAreaProvider>
      <Stack screenOptions={{ headerShown: false, contentStyle: { backgroundColor: "#1A1814" } }} />
    </SafeAreaProvider>
  );
}
```

Updated state (this story):
```tsx
export default function RootLayout() {
  return (
    <SafeAreaProvider>
      <AccessibilityProvider>
        <Stack screenOptions={{ headerShown: false, contentStyle: { backgroundColor: "#1A1814" } }} />
      </AccessibilityProvider>
    </SafeAreaProvider>
  );
}
```

Remove the placeholder comment. Remove the `global.css` import and replace only if NativeWind still requires it (it was in the 1.3 scaffold). Keep `import "../global.css"` if it exists and is used by NativeWind.

### Implementation: `components/shared/AccessiblePressable.tsx`

A thin wrapper over React Native's `Pressable` that enforces:
- `minHeight: 44, minWidth: 44` (touch target — AC 5)
- `hitSlop={{ top: 8, bottom: 8, left: 8, right: 8 }}` applied by default, overrideable
- `accessibilityRole` required (not optional) in props (AC 6)
- `accessibilityLabel` required in props (AC 6)
- `accessibilityState` optional (disabled, selected, checked states)

**Props interface:**
```typescript
type AccessiblePressableProps = Omit<PressableProps, 'hitSlop'> & {
  accessibilityRole: AccessibilityRole;    // required — no default
  accessibilityLabel: string;              // required — no default
  accessibilityHint?: string;              // optional
  hitSlop?: Insets;                        // optional override; default from touchTarget.hitSlop
  style?: StyleProp<ViewStyle>;
};
```

Use `colors`, `touchTarget` from `constants/theme.ts` — never hardcode values.

This component is for icon-only buttons and small interactive elements. Larger, labeled buttons (text buttons) will be a `Button` component in a future story.

### Implementation: `components/shared/ThemedText.tsx`

A thin wrapper over React Native's `Text` that enforces:
- `allowFontScaling={true}` — always (AC 7)
- Default color from `colors.text.primary`
- Default font family from `typography.family.primary`
- `style` prop accepted as override

**Props interface:**
```typescript
type ThemedTextProps = Omit<TextProps, 'allowFontScaling'> & {
  variant?: 'displayXl' | 'displayLg' | 'titleMd' | 'titleSm' | 'bodyLg' | 'bodyMd' | 'bodySm' | 'captionMd' | 'captionSm' | 'labelMd' | 'labelSm';
  color?: string;  // overrides default, use colors.* values
};
```

Map `variant` to `typography.size[variant]` from theme.ts. Default variant: `bodyMd`.

**CRITICAL:** `allowFontScaling` is NEVER set to `false` anywhere in this component or in any component built on it. Fixed-height containers that clip text at ×2.0 scale are a WCAG violation.

### Implementation: `hooks/useAccessibility.ts`

This is just the re-export of `useAccessibility()` from `AccessibilityContext.tsx` for import ergonomics. Components import from `hooks/useAccessibility` (consistent with project convention of `hooks/` for custom hooks).

```typescript
// mobile/hooks/useAccessibility.ts
export { useAccessibility } from '../contexts/AccessibilityContext';
```

### Implementation: `hooks/useResponsiveLayout.ts`

Exposes breakpoints via `useWindowDimensions()` (UX-DR17). Used by onboarding and reveal modal in future stories.

```typescript
type LayoutBreakpoint = 'compact' | 'regular' | 'expanded';

type ResponsiveLayout = {
  breakpoint: LayoutBreakpoint;
  width: number;
  height: number;
};

// compact: width < 375
// regular: 375 <= width < 428
// expanded: width >= 428
```

### File Locations — Exact Paths

| File | Action | Path |
|---|---|---|
| `AccessibilityContext.tsx` | **CREATE NEW** | `mobile/contexts/AccessibilityContext.tsx` |
| `_layout.tsx` | **MODIFY** | `mobile/app/_layout.tsx` |
| `AccessiblePressable.tsx` | **CREATE NEW** | `mobile/components/shared/AccessiblePressable.tsx` |
| `ThemedText.tsx` | **CREATE NEW** | `mobile/components/shared/ThemedText.tsx` |
| `useAccessibility.ts` | **CREATE NEW** | `mobile/hooks/useAccessibility.ts` |
| `useResponsiveLayout.ts` | **CREATE NEW** | `mobile/hooks/useResponsiveLayout.ts` |

**Delete** `.gitkeep` from `mobile/components/shared/` once `AccessiblePressable.tsx` and `ThemedText.tsx` are added.
**Delete** `.gitkeep` from `mobile/hooks/` once hook files are added.

### Files NOT to Modify

- `mobile/constants/theme.ts` — complete; do not touch
- `mobile/constants/errors.ts` — complete; do not touch
- `mobile/services/` — complete; do not touch
- Any component in `mobile/components/chat/`, `mobile/components/match/`, `mobile/components/moderation/`, `mobile/components/onboarding/`, `mobile/components/svg/` — these are empty and belong to future stories

### Import Conventions

```typescript
// Correct: import from hooks/
import { useAccessibility } from '../hooks/useAccessibility';
import { useResponsiveLayout } from '../hooks/useResponsiveLayout';

// Correct: import from contexts/ only in _layout.tsx (the provider)
import { AccessibilityProvider } from '../contexts/AccessibilityContext';

// Correct: import theme values from constants/
import { colors, typography, spacing, touchTarget, motion } from '../constants/theme';

// WRONG: never import AccessibilityContext directly in components
// WRONG: never call AccessibilityInfo directly in components
```

### Reduce Motion Usage Pattern (for ALL future animated components)

Every animated component in the app must follow this pattern:

```typescript
const { reduceMotion } = useAccessibility();

// For Animated.timing / Animated.spring:
const duration = reduceMotion ? 0 : motion.standard;

// For reveal animation specifically:
const revealDuration = reduceMotion ? 0 : motion.reveal; // 700ms → 0ms

// For opacity-fade-only fallback (UX-DR13):
// When reduceMotion is true: use opacity fade only, no translation/scale
// When reduceMotion is false: full motion treatment
```

Document this pattern in a comment in the context provider file so future stories can copy the pattern.

### Touch Target Pattern (for ALL future interactive components)

```typescript
// Standard interactive element (button with text label):
<Pressable style={{ minHeight: 44, minWidth: 44, justifyContent: 'center' }}>
  <ThemedText>Label</ThemedText>
</Pressable>

// Icon-only control (back button, report icon):
<AccessiblePressable
  accessibilityRole="button"
  accessibilityLabel="Go back"
  onPress={handleBack}
>
  <BackIcon />
</AccessiblePressable>
// AccessiblePressable enforces minHeight/minWidth AND hitSlop automatically
```

### Testing Approach

This is a foundation/platform story. Verification is through code inspection and manual accessibility testing:

1. `AccessibilityContext.tsx` exists and exports `AccessibilityProvider`, `useAccessibility`
2. `_layout.tsx` wraps tree with `<AccessibilityProvider>` inside `<SafeAreaProvider>`
3. `AccessiblePressable.tsx` has `minHeight: 44, minWidth: 44` in its styles
4. `ThemedText.tsx` always sets `allowFontScaling={true}` — grep for `allowFontScaling={false}` should return zero results
5. `useAccessibility()` throws when called outside provider (test with try/catch in Expo Go)
6. In iOS Simulator: Settings → Accessibility → Motion → Reduce Motion ON → verify context updates
7. In iOS Simulator: Settings → Accessibility → Larger Text → ×2.0 → verify `ThemedText` scales correctly, no clipping

No unit tests are mandated for this story. Future stories with stateful components will add tests.

### Previous Story Learnings Applied

From Story 1.3:
- Mobile project is at `mobile/` from repo root — all file paths relative to that
- `strict: true` in `tsconfig.json` is active — no `any` types, full return type annotations
- NativeWind is installed and configured — `global.css` is imported in `_layout.tsx`; preserve this import
- `app/(auth)/` and `app/(tabs)/` directories exist for future routing
- Expo SDK 55 is the target — use APIs available in SDK 55

From Story 1.4 (database story):
- Docker is in Windows containers mode on dev machine — run `docker compose` commands from PowerShell/CMD, not WSL
- No new env vars in this story — `.env.example` does not need updating (project-context rule 12 ✅)

### What NOT to Build in This Story

| Item | Story |
|---|---|
| `Button.tsx` component (primary, secondary, ghost, danger) | Story 2.1 (registration screen) |
| `Input.tsx` component (text field, multiline) | Story 2.1 |
| `ChatBubble.tsx` component | Story 5.1 |
| `RevealMoment.tsx` component (UX-DR3) | Story 6.4 |
| `RevealPrompt.tsx` component (UX-DR4) | Story 6.1 |
| `RevealCountdown.tsx` component (UX-DR5) | Story 6.1 |
| `EmptyMatchState.tsx` component (UX-DR7) | Story 4.5 |
| `ReportButton.tsx` component (UX-DR11) | Story 8.2 |
| Any screen implementation | Respective feature stories |
| DM Sans font loading (`expo-font` setup) | Story 2.1 (first screen that needs it) |

---

## Tasks / Subtasks

- [x] Task 1: Verify pre-existing state (AC: 1, 2)
  - [x] Read `mobile/constants/theme.ts` — confirm all 7 required color tokens exist: `colors.background.primary` (#1A1814), `colors.text.primary` (#F2EDE6), `colors.text.secondary` (#9E9790), `colors.accent.primary` (#C8833A), `colors.safety` (#4A9E8A), `colors.danger` (#D94F4F), `colors.text.muted` (#635D57)
  - [x] Confirm `colors.background.primary` is `#1A1814` (the "inactive" token maps to `colors.text.muted` #635D57)
  - [x] Confirm no light mode toggle exists in the file

- [x] Task 2: Create `contexts/AccessibilityContext.tsx` (AC: 3, 4)
  - [x] Create `mobile/contexts/` directory
  - [x] Implement `AccessibilityContextValue` type: `{ reduceMotion: boolean; fontScale: number; isScreenReaderEnabled: boolean }`
  - [x] Implement `AccessibilityProvider` component using `AccessibilityInfo.isReduceMotionEnabled()`, `AccessibilityInfo.isScreenReaderEnabled()`, and `useWindowDimensions().fontScale`
  - [x] Subscribe to `AccessibilityInfo.addEventListener('reduceMotionChanged', ...)` and `'screenReaderChanged'` — unsubscribe on unmount
  - [x] Default `reduceMotion` to `true` while loading (accessibility-first)
  - [x] Export `AccessibilityProvider`, `useAccessibility()` hook (throws outside provider), and `AccessibilityContext`
  - [x] Add a code comment showing the reduce-motion pattern for future animated components

- [x] Task 3: Update `app/_layout.tsx` (AC: 3)
  - [x] Import `AccessibilityProvider` from `../contexts/AccessibilityContext`
  - [x] Wrap `<Stack>` with `<AccessibilityProvider>` inside `<SafeAreaProvider>`
  - [x] Remove the placeholder comment "AccessibilityContext comes in Story 1.5"
  - [x] Verify `import "../global.css"` is still present (NativeWind requirement from Story 1.3)

- [x] Task 4: Create `components/shared/AccessiblePressable.tsx` (AC: 5, 6)
  - [x] Create component wrapping React Native `Pressable`
  - [x] Enforce `minHeight: touchTarget.minSize, minWidth: touchTarget.minSize` from theme
  - [x] Apply `hitSlop={touchTarget.hitSlop}` as default (overrideable via prop)
  - [x] Make `accessibilityRole` and `accessibilityLabel` required props (TypeScript-level enforcement)
  - [x] Accept optional `accessibilityHint` and `accessibilityState` props
  - [x] Delete `mobile/components/shared/.gitkeep`

- [x] Task 5: Create `components/shared/ThemedText.tsx` (AC: 7)
  - [x] Create component wrapping React Native `Text`
  - [x] Always set `allowFontScaling={true}` — hardcoded, never a prop
  - [x] Default color: `colors.text.primary`, default font: `typography.family.primary`
  - [x] Accept `variant` prop mapping to `typography.size.*` values
  - [x] Accept `color` prop for override (use `colors.*` values from theme)
  - [x] Accept `style` prop for additional overrides
  - [x] Add a comment: "allowFontScaling is always true — WCAG 2.1 AA requirement (project-context rule 15)"

- [x] Task 6: Create hook files (AC: 3)
  - [x] Create `mobile/hooks/useAccessibility.ts` — re-exports `useAccessibility` from `contexts/AccessibilityContext`
  - [x] Create `mobile/hooks/useResponsiveLayout.ts` — implements breakpoints from `useWindowDimensions()`: compact (<375), regular (375–427), expanded (≥428)
  - [x] Delete `mobile/hooks/.gitkeep`

- [x] Task 7: Verify no regressions
  - [x] Confirm `npx expo start` from `mobile/` still starts without errors
  - [x] Confirm TypeScript compiles with no errors: `npx tsc --noEmit` from `mobile/`
  - [x] grep for `allowFontScaling={false}` across `mobile/` — must return 0 results
  - [x] grep for `AccessibilityInfo` outside `contexts/AccessibilityContext.tsx` — must return 0 results in non-context files

---

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- TypeScript compile: `tsc --noEmit` — 0 errors after npm install in mobile/
- grep `allowFontScaling={false}` — 0 results ✅
- grep `AccessibilityInfo` outside contexts — only in `constants/theme.ts` (comment only) and `contexts/AccessibilityContext.tsx` ✅

### Completion Notes List

- AC 1 & 2: Pre-existing — `mobile/constants/theme.ts` already contained all 7 color tokens and WCAG contrast ratios from Story 1.3.
- AC 3: `AccessibilityContext.tsx` created with `AccessibilityProvider`, `useAccessibility` hook (throws outside provider), and `AccessibilityContext` export. Default `reduceMotion: true` (accessibility-first). Listens for `reduceMotionChanged` and `screenReaderChanged` system events.
- AC 4: `reduceMotion` defaults to `true` until OS confirms otherwise. Reduce motion usage pattern documented in context file comments for future animated components.
- AC 5: `AccessiblePressable.tsx` enforces `minHeight: 44, minWidth: 44` (from `touchTarget.minSize`) and `hitSlop` from theme.
- AC 6: `accessibilityRole` and `accessibilityLabel` are required props on `AccessiblePressable` — TypeScript compile-time enforcement.
- AC 7: `ThemedText.tsx` always sets `allowFontScaling={true}` — hardcoded, not a prop. WCAG comment included.
- `_layout.tsx` updated: `AccessibilityProvider` wraps `<Stack>` inside `<SafeAreaProvider>`; placeholder comment removed; `global.css` import preserved.
- `.gitkeep` files deleted from `mobile/components/shared/` and `mobile/hooks/`.
- `useResponsiveLayout.ts` created with compact/regular/expanded breakpoints via `useWindowDimensions()`.
- `useAccessibility.ts` re-exports `useAccessibility` from contexts/ for import ergonomics.
- Post-review fixes applied: `useResponsiveLayout.ts` now treats widths 375–427 as `regular`; `AccessiblePressable.tsx` preserves Pressable callback-style support; `AccessibilityContext.tsx` guards initial native accessibility reads and preserves safe defaults on failure.

### File List

- `mobile/contexts/AccessibilityContext.tsx` — NEW
- `mobile/app/_layout.tsx` — MODIFIED (added AccessibilityProvider, removed placeholder comment)
- `mobile/components/shared/AccessiblePressable.tsx` — NEW
- `mobile/components/shared/ThemedText.tsx` — NEW
- `mobile/components/shared/.gitkeep` — DELETED
- `mobile/hooks/useAccessibility.ts` — NEW
- `mobile/hooks/useResponsiveLayout.ts` — NEW
- `mobile/hooks/.gitkeep` — DELETED
- `README.md` — MODIFIED (added mobile/ to Repository Layout; added "Run the Mobile App" section with setup and accessibility patterns)
- `docs/tech-preferences.md` — MODIFIED (added `contexts/` and `components/shared/` to mobile directory tree in section 1)
- `mobile/README.md` — MODIFIED (updated Project Structure with contexts/, components/shared/; added Accessibility section)

## Change Log

- 2026-03-20: Story 1.5 created — mobile design system foundation. theme.ts confirmed already complete from Story 1.3; deliverables scoped to AccessibilityContext, _layout.tsx update, AccessiblePressable, ThemedText, and foundation hooks.
- 2026-03-20: Story 1.5 implemented — AccessibilityContext, AccessibilityProvider, AccessiblePressable, ThemedText, useAccessibility, useResponsiveLayout created; _layout.tsx updated; TypeScript strict mode passes with 0 errors. README.md and docs/tech-preferences.md updated per project-context rule 16 (architecture change).
- 2026-03-20: Story 1.5 reviewed and completed — documentation aligned to final responsive breakpoint behavior (regular 375–427, expanded ≥428), review fixes applied, and sprint status moved to done.
