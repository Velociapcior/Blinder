# Story 2-1-A: Shared Mobile Component Library

Status: done

## Story

As a developer,
I want a complete shared component library with all MVP components implemented or placeholder-scaffolded,
So that every future screen uses consistent, accessible, theme-compliant components rather than inline primitives.

## Context

This is an intra-epic insertion story created between 2-1 (Email/Password Registration) and 2-2 (Login). Its purpose is to retrofit the `register.tsx` screen built in Story 2-1 with proper shared components, and scaffold every other MVP component so future stories can never define UI inline in screen files.

**Rule 18 (project-context.md):** After this story, inline `TextInput`, `Pressable`-as-button, local loading animations, or ad-hoc error displays inside screen files are **prohibited**. All future stories must check `mobile/components/` first.

## Acceptance Criteria

1. `mobile/components/shared/` contains `Button.tsx`, `TextField.tsx`, `RadioChipGroup.tsx`, `Toggle.tsx`, `LoadingIndicator.tsx`, `ErrorBanner.tsx` (fully implemented) plus `Card.tsx`, `Modal.tsx`, `EmptyState.tsx`, `SkeletonLoader.tsx`, `StatusBadge.tsx`, `ConsentBadge.tsx` (placeholder stubs) — alongside existing `AccessiblePressable.tsx` and `ThemedText.tsx`.

2. `Button.tsx` supports `variant` prop (`primary | secondary | ghost | danger`); accepts `isLoading` that renders `LoadingIndicator`; uses theme tokens exclusively; enforces `minHeight: 44`; requires `accessibilityLabel`.

3. `TextField.tsx` wraps React Native `TextInput` with a `label` prop, `error` prop, `secureTextEntry` support, themed border/background; `accessibilityLabel` derived from label; always sets `allowFontScaling={true}`.

4. `RadioChipGroup.tsx` renders a horizontal row of selectable chips; selected chip uses `colors.accent.primary` border and `colors.background.surface` fill; `accessibilityRole="radiogroup"` on container, `accessibilityRole="radio"` on each chip.

5. `Toggle.tsx` wraps React Native `Switch` with a `label` prop; uses `colors.safety` as active track color; `accessibilityLabel` required.

6. `LoadingIndicator.tsx` renders three animated pulsing dots using theme colors; when `reduceMotion === true` (from `useAccessibility`), renders static dots with no animation.

7. `ErrorBanner.tsx` renders only when `error` prop is non-null; uses `colors.danger` text and `colors.background.surface` background; sets `accessibilityLiveRegion="polite"`.

8. Placeholder components in their subdirectories: `chat/` (ChatBubble, ChatInput, ConversationRow), `match/` (RevealMoment, RevealPrompt, RevealCountdown, EmptyMatchState, RevealProgress), `moderation/` (ReportButton), `onboarding/` (QuizCard, ProgressStepper) — each exports a typed props interface, renders a visible stub using `ThemedText` with the component name, and includes a `// TODO: implement in Story X.X` comment.

9. `register.tsx` is refactored: raw `TextInput` → `TextField`; submit and back buttons → `Button`; gender chips → `RadioChipGroup`; `Switch` → `Toggle`; local `LoadingDots` function removed → `LoadingIndicator`; error `ThemedText` → `ErrorBanner`; no inline component definitions remain.

10. `docs/component-library.md` exists and lists every component with file path, props summary, implementation status, and a usage example. _(Already created — verify it matches final implementation.)_

11. `docs/project-context.md` Rule 18 is present. _(Already inserted — no change needed unless wording requires update.)_

12. `tsc --noEmit` from `mobile/` exits with zero TypeScript errors.

---

## Dev Notes

### Existing Foundation — Do Not Reinvent

Before writing any code, review these files:

| File | What it provides |
|---|---|
| `mobile/components/shared/AccessiblePressable.tsx` | Pressable wrapper with `minHeight: 44`, `minWidth: 44`, hitSlop, enforced `accessibilityRole` + `accessibilityLabel`. **Button must use this as its base.** |
| `mobile/components/shared/ThemedText.tsx` | Text wrapper that always sets `allowFontScaling={true}`. All text inside components must use `ThemedText`, never raw `Text`. |
| `mobile/constants/theme.ts` | All design tokens: `colors`, `typography`, `spacing`, `radii`, `motion`, `touchTarget`. No hardcoded values anywhere. |
| `mobile/contexts/AccessibilityContext.tsx` | Provides `{ reduceMotion, fontScale, isScreenReaderEnabled }`. |
| `mobile/hooks/useAccessibility.ts` | Re-exports `useAccessibility` — import from here, not from contexts directly. |
| `mobile/app/(auth)/register.tsx` | The screen to refactor. Study its current form state, layout, and footer pattern. |

### Critical Android Gotcha — StyleSheet.flatten is Mandatory

**From project memory:** Android's `Pressable` silently drops `backgroundColor` and `borderColor` from nested style arrays when the `style` prop receives a function. This affects `Button`, `RadioChipGroup`, and any component that conditionally sets border/background.

**Required pattern for all conditional styles:**

```tsx
// ✅ CORRECT — always use StyleSheet.flatten
style={StyleSheet.flatten([styles.base, isSelected && styles.selected])}

// ❌ WRONG — Android drops backgroundColor/borderColor from array inside function
style={(state) => [styles.base, state.pressed && styles.pressed]}
```

`AccessiblePressable` already uses this pattern internally. If you build `Button` on top of `AccessiblePressable`, the outer style you pass to it must also use `StyleSheet.flatten`.

### Component Implementation Specifications

#### `Button.tsx`

```
Location: mobile/components/shared/Button.tsx

Props interface:
  variant: 'primary' | 'secondary' | 'ghost' | 'danger'
  onPress: () => void
  accessibilityLabel: string          // required — no default
  children?: React.ReactNode          // optional label override; variant text used if omitted
  isLoading?: boolean                 // default false — renders <LoadingIndicator /> when true
  disabled?: boolean                  // default false — opacity 0.4, onPress blocked
  style?: StyleProp<ViewStyle>

Implementation:
- Build on AccessiblePressable (accessibilityRole="button")
- Enforce minHeight via AccessiblePressable's baseStyle (already 44)
- When isLoading is true: render <LoadingIndicator size="sm" />, set disabled=true
- Pass style as StyleSheet.flatten([variantStyle, disabled && styles.disabled, style])

Variant styles (use theme tokens only):
  primary:   backgroundColor: colors.accent.primary, text: colors.text.primary
  secondary: backgroundColor: colors.background.surface, borderWidth: 1.5, borderColor: colors.accent.primary, text: colors.text.primary
  ghost:     backgroundColor: 'transparent', text: colors.text.secondary
  danger:    backgroundColor: colors.danger, text: colors.text.primary
```

#### `TextField.tsx`

```
Location: mobile/components/shared/TextField.tsx

Props interface:
  label: string
  value: string
  onChangeText: (text: string) => void
  error?: string | null
  secureTextEntry?: boolean
  keyboardType?: KeyboardTypeOptions
  autoComplete?: TextInputProps['autoComplete']
  placeholder?: string
  style?: StyleProp<ViewStyle>

Implementation:
- Container: View with label ThemedText above, TextInput below, ErrorBanner below input if error is non-null
- TextInput: allowFontScaling={true} MUST be set explicitly (TextInput does not inherit from ThemedText)
- accessibilityLabel={label} on TextInput
- Themed styles: backgroundColor: colors.background.input, color: colors.text.primary,
  borderRadius: radii.sm (8), paddingHorizontal: spacing[4], paddingVertical: spacing[3],
  fontSize: typography.size.bodyLg, minHeight: 44
- When error is non-null: add borderColor: colors.danger, borderWidth: 1.5 to input
- Do NOT render ErrorBanner inside TextField if it adds redundant visual noise — ErrorBanner is for screen-level errors. Use a simple error ThemedText below the input for field-level errors.

NOTE: The register.tsx screen uses a single screen-level ErrorBanner for all validation errors.
TextField does not need its own error display — the error prop is for future use (e.g. server field errors).
For now, pass error={null} from register.tsx and use a separate <ErrorBanner /> at screen level.
```

#### `RadioChipGroup.tsx`

```
Location: mobile/components/shared/RadioChipGroup.tsx

Props interface:
  options: Array<{ label: string; value: string | number }>
  value: string | number | null
  onChange: (value: string | number) => void
  style?: StyleProp<ViewStyle>

Implementation:
- Container View: flexDirection: 'row', gap: spacing[3], accessibilityRole="radiogroup"
- Each chip: AccessiblePressable with accessibilityRole="radio", accessibilityState={{ selected: value === opt.value }}
- Chip style — MUST use StyleSheet.flatten (Android gotcha):
    unselected: backgroundColor: colors.background.input, borderColor: colors.text.muted, borderWidth: 1.5
    selected:   backgroundColor: colors.background.surface, borderColor: colors.accent.primary, borderWidth: 1.5
  Pass as: style={StyleSheet.flatten([styles.chip, value === opt.value && styles.chipSelected])}
- Chip inner: ThemedText variant="labelMd"
- minHeight: 44 on each chip (enforced by AccessiblePressable)
```

#### `Toggle.tsx`

```
Location: mobile/components/shared/Toggle.tsx

Props interface:
  label: string
  value: boolean
  onValueChange: (value: boolean) => void
  accessibilityLabel: string          // required — must be explicit, cannot derive from label safely
  style?: StyleProp<ViewStyle>

Implementation:
- Container View: flexDirection: 'row', alignItems: 'center', gap: spacing[3]
- Switch: trackColor={{ false: colors.background.input, true: colors.safety }}
          thumbColor={colors.text.primary}
          accessibilityLabel={accessibilityLabel}
- ThemedText variant="bodySm" for label, color: colors.text.secondary, flex: 1
```

#### `LoadingIndicator.tsx`

```
Location: mobile/components/shared/LoadingIndicator.tsx

Props interface:
  size?: 'sm' | 'md'    // default 'md'. sm = 6px dots (for Button), md = 8px dots
  color?: string         // default colors.text.primary

Implementation:
- Import { useAccessibility } from '../../hooks/useAccessibility'
- const { reduceMotion } = useAccessibility()
- Render three dots side by side, gap: 6
- When reduceMotion is false: Animated.loop with Animated.sequence stagger (delay 0, 200, 400ms)
  Use Animated.Value opacity (0.3 → 1 → 0.3), duration: motion.standard (300ms)
  useNativeDriver: true
- When reduceMotion is true: render static dots (no Animated.View, just View) at opacity 1
- Cleanup: stop animations in useEffect return
- Dot style: borderRadius: size/2, backgroundColor: color
  sm: width 6, height 6
  md: width 8, height 8
```

#### `ErrorBanner.tsx`

```
Location: mobile/components/shared/ErrorBanner.tsx

Props interface:
  error: string | null    // renders nothing if null

Implementation:
- If error is null, return null (do not render empty View)
- View: backgroundColor: colors.background.surface, borderRadius: radii.sm,
        paddingHorizontal: spacing[4], paddingVertical: spacing[3]
- ThemedText: color: colors.danger, variant: "bodySm"
- accessibilityLiveRegion="polite" on the outer View — screen readers announce on change without stealing focus
```

### Placeholder Component Pattern

Each placeholder file follows this exact structure (adjust component name, props, and target story):

```tsx
// TODO: implement in Story 5-1
import React from 'react';
import { View } from 'react-native';
import { ThemedText } from '../shared/ThemedText';

type ChatBubbleProps = {
  message: string;
  isMine: boolean;
  timestamp: string;
};

export function ChatBubble(_props: ChatBubbleProps): React.JSX.Element {
  return (
    <View>
      <ThemedText variant="bodySm">ChatBubble</ThemedText>
    </View>
  );
}
```

Rules for placeholders:
- Export a named function (not default)
- Prefix unused props param with `_` to satisfy TypeScript `noUnusedParameters`
- Use only `ThemedText` for visible content — no raw `Text`
- Props interface types must compile with `strict: true`
- Never import from theme or hooks unless the type requires it

### register.tsx Refactor

The `register.tsx` refactor is the integration test of Story 2-1-A. After refactoring:

**Imports to ADD:**
```tsx
import { Button } from '../../components/shared/Button';
import { TextField } from '../../components/shared/TextField';
import { RadioChipGroup } from '../../components/shared/RadioChipGroup';
import { Toggle } from '../../components/shared/Toggle';
import { LoadingIndicator } from '../../components/shared/LoadingIndicator';
import { ErrorBanner } from '../../components/shared/ErrorBanner';
```

**Imports to REMOVE (no longer needed after refactor):**
```tsx
// Remove from React import: useEffect (only used by LoadingDots)
// Remove from react-native: TextInput, Switch, Animated
import { TextInput, Switch, Animated } from 'react-native'; // REMOVE THESE THREE
// Keep: View, StyleSheet, ScrollView, KeyboardAvoidingView, Platform
// Keep: useReducer, useState, useRef from react (useRef still used for isSubmitting.current)
```

**Remove entire `LoadingDots` function** (lines 21–50 in current file).

**Replace in JSX:**
- `<TextInput ... />` (email, password, confirmPassword) → `<TextField label="Email" value={...} onChangeText={...} />`
- Gender `<View accessibilityRole="radiogroup">` with `AccessiblePressable` chips → `<RadioChipGroup options={GENDER_OPTIONS} value={form.gender} onChange={(v) => dispatch({ type: 'SET_GENDER', value: v as RegisterRequest['gender'] })} />`
- `<Switch ... />` + its `<ThemedText>` label → `<Toggle label="I confirm I am 18 years of age or older" value={form.over18Declaration} onValueChange={() => dispatch({ type: 'TOGGLE_OVER18' })} accessibilityLabel="I confirm I am 18 years of age or older" />`
- Error `<ThemedText variant="bodySm" style={styles.errorBanner}>` → `<ErrorBanner error={state.error} />`
- Submit `<AccessiblePressable>` in footer → `<Button variant="primary" onPress={handleRegister} accessibilityLabel="Register" isLoading={state.isLoading} disabled={state.isLoading} />`
- Back `<AccessiblePressable>` in success view → `<Button variant="ghost" onPress={() => router.back()} accessibilityLabel="Go back" />`

**Styles to REMOVE from `StyleSheet.create` after refactor:**
```
genderRow, genderChip, genderChipSelected, errorBanner, checkRow, checkLabel (partially)
```
These are now owned by the component library. Keep only layout-specific styles.

**GENDER_OPTIONS:** The `value` type may need casting. `RadioChipGroup` accepts `string | number`, so `onChange` callback may return `string | number`. Cast in the dispatch: `onChange={(v) => dispatch({ type: 'SET_GENDER', value: v as RegisterRequest['gender'] })}`.

### File Structure

```
mobile/
├── components/
│   ├── shared/
│   │   ├── AccessiblePressable.tsx     ✅ exists (Story 1-5)
│   │   ├── ThemedText.tsx              ✅ exists (Story 1-5)
│   │   ├── Button.tsx                  🆕 implement
│   │   ├── TextField.tsx               🆕 implement
│   │   ├── RadioChipGroup.tsx          🆕 implement
│   │   ├── Toggle.tsx                  🆕 implement
│   │   ├── LoadingIndicator.tsx        🆕 implement
│   │   ├── ErrorBanner.tsx             🆕 implement
│   │   ├── Card.tsx                    🆕 placeholder (Story 4-5)
│   │   ├── Modal.tsx                   🆕 placeholder (Story 6-2)
│   │   ├── EmptyState.tsx              🆕 placeholder (Story 4-5)
│   │   ├── SkeletonLoader.tsx          🆕 placeholder (Story 5-2)
│   │   ├── StatusBadge.tsx             🆕 placeholder (Story 5-1)
│   │   └── ConsentBadge.tsx            🆕 placeholder (Story 5-1)
│   ├── chat/
│   │   ├── ChatBubble.tsx              🆕 placeholder (Story 5-1)
│   │   ├── ChatInput.tsx               🆕 placeholder (Story 5-1)
│   │   └── ConversationRow.tsx         🆕 placeholder (Story 5-2)
│   ├── match/
│   │   ├── RevealMoment.tsx            🆕 placeholder (Story 6-4)
│   │   ├── RevealPrompt.tsx            🆕 placeholder (Story 6-1)
│   │   ├── RevealCountdown.tsx         🆕 placeholder (Story 6-1)
│   │   ├── EmptyMatchState.tsx         🆕 placeholder (Story 4-5)
│   │   └── RevealProgress.tsx          🆕 placeholder (Story 6-1)
│   ├── moderation/
│   │   └── ReportButton.tsx            🆕 placeholder (Story 8-2)
│   └── onboarding/
│       ├── QuizCard.tsx                🆕 placeholder (Story 3-1)
│       └── ProgressStepper.tsx         🆕 placeholder (Story 3-1)
├── app/(auth)/
│   └── register.tsx                    🔄 refactor
docs/
├── component-library.md                ✅ already created — verify matches
└── project-context.md                  ✅ Rule 18 already present — no change
```

### Pre-existing Docs Status

Both `docs/component-library.md` and Rule 18 in `docs/project-context.md` are **already created** (pre-populated from the epics planning). Verify the component-library.md table matches the final implementation (correct props, correct status). Update only if there are discrepancies.

### Non-Negotiable Constraints

1. **No hardcoded values** — every color, size, spacing, radius comes from `constants/theme.ts`
2. **StyleSheet.flatten for conditional styles** — mandatory on Pressable-based components (Button, RadioChipGroup) due to Android behavior
3. **useAccessibility from hooks/**, not from contexts/ directly
4. **ThemedText everywhere** — no raw `<Text>` inside components
5. **allowFontScaling={true}** on all `TextInput` elements (TextInput is not a ThemedText subclass)
6. **Named exports** — all components export named functions, not default exports (consistency with AccessiblePressable, ThemedText)
7. **TypeScript strict** — `_` prefix for unused placeholder props, typed props interfaces, no `any`

### Task Checklist

- [x] Create `mobile/components/shared/Button.tsx`
- [x] Create `mobile/components/shared/TextField.tsx`
- [x] Create `mobile/components/shared/RadioChipGroup.tsx`
- [x] Create `mobile/components/shared/Toggle.tsx`
- [x] Create `mobile/components/shared/LoadingIndicator.tsx`
- [x] Create `mobile/components/shared/ErrorBanner.tsx`
- [x] Create placeholder stubs: Card, Modal, EmptyState, SkeletonLoader, StatusBadge, ConsentBadge in `shared/`
- [x] Create placeholder stubs: ChatBubble, ChatInput, ConversationRow in `chat/`
- [x] Create placeholder stubs: RevealMoment, RevealPrompt, RevealCountdown, EmptyMatchState, RevealProgress in `match/`
- [x] Create placeholder stub: ReportButton in `moderation/`
- [x] Create placeholder stubs: QuizCard, ProgressStepper in `onboarding/`
- [x] Refactor `mobile/app/(auth)/register.tsx` — replace all inline primitives
- [x] Verify `docs/component-library.md` matches final implementation
- [x] Run `tsc --noEmit` from `mobile/` — must be zero errors

---

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Completion Notes

All 6 fully-implemented shared components created (Button, TextField, RadioChipGroup, Toggle, LoadingIndicator, ErrorBanner). All 13 placeholder stubs created across shared/, chat/, match/, moderation/, onboarding/ subdirectories. register.tsx refactored to replace all inline primitives with shared components — removed LoadingDots function, TextInput, Switch, Animated imports, and inline chip/error styles. StyleSheet.flatten used on all Pressable-based conditional styles (Button, RadioChipGroup) per Android gotcha. LoadingIndicator respects reduceMotion from useAccessibility. tsc --noEmit exits with zero errors. docs/component-library.md verified accurate.

### File List

- mobile/components/shared/Button.tsx (new)
- mobile/components/shared/TextField.tsx (new)
- mobile/components/shared/RadioChipGroup.tsx (new)
- mobile/components/shared/Toggle.tsx (new)
- mobile/components/shared/LoadingIndicator.tsx (new)
- mobile/components/shared/ErrorBanner.tsx (new)
- mobile/components/shared/Card.tsx (new)
- mobile/components/shared/Modal.tsx (new)
- mobile/components/shared/EmptyState.tsx (new)
- mobile/components/shared/SkeletonLoader.tsx (new)
- mobile/components/shared/StatusBadge.tsx (new)
- mobile/components/shared/ConsentBadge.tsx (new)
- mobile/components/chat/ChatBubble.tsx (new)
- mobile/components/chat/ChatInput.tsx (new)
- mobile/components/chat/ConversationRow.tsx (new)
- mobile/components/match/RevealMoment.tsx (new)
- mobile/components/match/RevealPrompt.tsx (new)
- mobile/components/match/RevealCountdown.tsx (new)
- mobile/components/match/EmptyMatchState.tsx (new)
- mobile/components/match/RevealProgress.tsx (new)
- mobile/components/moderation/ReportButton.tsx (new)
- mobile/components/onboarding/QuizCard.tsx (new)
- mobile/components/onboarding/ProgressStepper.tsx (new)
- mobile/app/(auth)/register.tsx (modified)

### Change Log

- 2026-03-23: Implemented complete shared mobile component library (Story 2-1-A). Created 6 fully-implemented shared components and 17 typed placeholder stubs across all feature subdirectories. Refactored register.tsx to consume all shared components — no inline UI primitives remain in screen files.
- 2026-03-25: Story reviewed and accepted. Sprint status moved to done.
