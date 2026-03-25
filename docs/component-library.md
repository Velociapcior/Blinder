# Blinder Mobile Component Library

This document is the authoritative registry of all mobile UI components.

**Rule (project-context Rule 18):** Before building any UI element, check this file first. If the component exists — use it. If it doesn't — create it in the correct subdirectory, add it here, then use it.

---

## How to Use This Registry

| Column | Meaning |
|---|---|
| **Component** | Import name |
| **Path** | File path relative to `mobile/` |
| **Status** | `ready` = fully implemented; `placeholder` = stub only, full implementation in target story |
| **Key Props** | Essential props at a glance |
| **Implemented in** | Story that delivered the full implementation |

---

## `components/shared/` — General Purpose

| Component | Path | Status | Key Props | Implemented in |
|---|---|---|---|---|
| `AccessiblePressable` | `components/shared/AccessiblePressable.tsx` | ready | `accessibilityRole` (required), `accessibilityLabel` (required), `onPress`, `style`, `disabled` | Story 1-5 |
| `ThemedText` | `components/shared/ThemedText.tsx` | ready | `variant` (displayXl…labelSm), `color`, `style` | Story 1-5 |
| `Button` | `components/shared/Button.tsx` | ready | `variant` (primary\|secondary\|ghost\|danger), `onPress`, `accessibilityLabel` (required), `isLoading`, `disabled`, `style` | Story 2-1-A |
| `TextField` | `components/shared/TextField.tsx` | ready | `label`, `value`, `onChangeText`, `error`, `secureTextEntry`, `keyboardType`, `autoComplete`, `placeholder` | Story 2-1-A |
| `RadioChipGroup` | `components/shared/RadioChipGroup.tsx` | ready | `options` ({label, value}[]), `value`, `onChange` | Story 2-1-A |
| `Toggle` | `components/shared/Toggle.tsx` | ready | `label`, `value`, `onValueChange`, `accessibilityLabel` (required) | Story 2-1-A |
| `LoadingIndicator` | `components/shared/LoadingIndicator.tsx` | ready | `size` (sm\|md), `color` | Story 2-1-A |
| `ErrorBanner` | `components/shared/ErrorBanner.tsx` | ready | `error` (string \| null) | Story 2-1-A |
| `Card` | `components/shared/Card.tsx` | placeholder | `children`, `style` | Story 4-5 |
| `Modal` | `components/shared/Modal.tsx` | placeholder | `visible`, `onDismiss`, `children`, `title` | Story 6-2 |
| `EmptyState` | `components/shared/EmptyState.tsx` | placeholder | `title`, `body`, `action?` | Story 4-5 |
| `SkeletonLoader` | `components/shared/SkeletonLoader.tsx` | placeholder | `width`, `height`, `borderRadius` | Story 5-2 |
| `StatusBadge` | `components/shared/StatusBadge.tsx` | placeholder | `label` (e.g. "active recently") | Story 5-1 |
| `ConsentBadge` | `components/shared/ConsentBadge.tsx` | placeholder | (no props — static indicator) | Story 5-1 |

---

## `components/chat/` — Chat-Specific

| Component | Path | Status | Key Props | Implemented in |
|---|---|---|---|---|
| `ChatBubble` | `components/chat/ChatBubble.tsx` | placeholder | `message`, `isMine`, `timestamp` | Story 5-1 |
| `ChatInput` | `components/chat/ChatInput.tsx` | placeholder | `value`, `onChangeText`, `onSend`, `disabled` | Story 5-1 |
| `ConversationRow` | `components/chat/ConversationRow.tsx` | placeholder | `name`, `lastMessage`, `unreadCount`, `onPress` | Story 5-2 |

---

## `components/match/` — Matching & Reveal

| Component | Path | Status | Key Props | Implemented in |
|---|---|---|---|---|
| `RevealMoment` | `components/match/RevealMoment.tsx` | placeholder | `photoUrl`, `onContinue`, `onDecline` | Story 6-4 |
| `RevealPrompt` | `components/match/RevealPrompt.tsx` | placeholder | `onRevealPress` | Story 6-1 |
| `RevealCountdown` | `components/match/RevealCountdown.tsx` | placeholder | (no props — pure waiting state) | Story 6-1 |
| `EmptyMatchState` | `components/match/EmptyMatchState.tsx` | placeholder | (no props — brand-voice copy, no spinner) | Story 4-5 |
| `RevealProgress` | `components/match/RevealProgress.tsx` | placeholder | `current`, `threshold` | Story 6-1 |

---

## `components/moderation/` — Safety & Reporting

| Component | Path | Status | Key Props | Implemented in |
|---|---|---|---|---|
| `ReportButton` | `components/moderation/ReportButton.tsx` | placeholder | `conversationId`, `onReported` | Story 8-2 |

---

## `components/onboarding/` — Onboarding Flow

| Component | Path | Status | Key Props | Implemented in |
|---|---|---|---|---|
| `QuizCard` | `components/onboarding/QuizCard.tsx` | placeholder | `question`, `options`, `onAnswer` | Story 3-1 |
| `ProgressStepper` | `components/onboarding/ProgressStepper.tsx` | placeholder | `current`, `total` | Story 3-1 |

---

## Adding a New Component

1. Create the file in the correct subdirectory
2. Add a typed props interface (`type XxxProps = { ... }`)
3. Use only theme tokens from `constants/theme.ts` — no hardcoded colours, sizes, or spacing
4. Add `accessibilityRole`, `accessibilityLabel` on all interactive elements
5. Add a row to this table
6. If it is a placeholder, add `// TODO: implement in Story X.X` at the top of the file

---

*Last updated: Story 2-1-A (2026-03-23)*
