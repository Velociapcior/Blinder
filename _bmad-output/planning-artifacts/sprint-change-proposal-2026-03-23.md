# Sprint Change Proposal — Shared Mobile Component Library
**Date:** 2026-03-23
**Author:** Piotr.palej
**Workflow:** Correct Course
**Status:** Approved — 2026-03-23

---

## Section 1: Issue Summary

### Problem Statement

Story 2-1 (Email/Password Registration) was implemented with inline UI primitives rather than reusable shared components. `register.tsx` defines a `LoadingDots` animation locally, uses raw `TextInput` three times with duplicated inline styles, constructs buttons by manually styling `AccessiblePressable`, and implements a gender chip selector and labeled toggle entirely within the screen file.

This pattern, if allowed to continue, will produce significant duplication across all future screens — login (2-2), social sign-in (2-3, 2-4), invite landing (2-5), onboarding quiz (3-1), preferences (3-5), chat (Epic 5), and reveal flows (Epic 6).

Story 1-5 explicitly deferred `Button.tsx` and `Input.tsx` to Story 2-1, but Story 2-1 built these patterns inline rather than as proper shared components.

### Discovery Context

Identified by PM during review of `register.tsx` after Story 2-1 completion. Intervening now, before Story 2-2 (Login) begins, avoids retrofitting across 20+ future screens.

### Evidence

- `mobile/app/(auth)/register.tsx` line 21: `function LoadingDots()` — defined inline, not in `components/shared/`
- `register.tsx` lines 209–249: three bare `TextInput` blocks with identical `styles.input` — no component wrapper
- `register.tsx` lines 291–306: submit button assembled from `AccessiblePressable` + manual style — no `Button` component
- `register.tsx` lines 255–274: gender chip selector built ad-hoc — no `RadioChipGroup` component
- `mobile/components/chat/`, `match/`, `moderation/`, `onboarding/` — all contain only `.gitkeep`
- UX Spec Phase 2 component list (already specified): Button, Input, Card, ChatBubble, ChatInput, StatusIndicator, RevealProgress, ConsentIndicator, EmptyState, Modal — none exist

---

## Section 2: Impact Analysis

### Epic Impact

| Epic | Impact |
|---|---|
| Epic 1 (Foundation) | No stories affected. Story 1-5 (Design System) remains complete. |
| Epic 2 (Auth) | **Directly affected.** New story 2-1-A inserted before 2-2. Story 2-2 and beyond must wait until 2-1-A is done. |
| Epic 3 (Onboarding) | Benefits — quiz cards, photo upload, preferences screens will have proper components ready. |
| Epics 4–9 | All benefit. Chat bubbles, reveal components, report button, empty states are all pre-scaffolded as placeholders. |

### Story Impact

| Story | Change |
|---|---|
| 2-1 (Registration) | `register.tsx` refactored to use extracted components — in scope for Story 2-1-A |
| 2-2 (Login) | Blocked until 2-1-A is done. No story changes needed. |
| All future stories | Must reference `docs/component-library.md` and Rule 18 before building any UI element. |

### Artifact Conflicts

| Artifact | Change Required |
|---|---|
| `_bmad-output/planning-artifacts/epics.md` | Add Story 2-1-A after Story 2-1 |
| `_bmad-output/implementation-artifacts/sprint-status.yaml` | Add `2-1-a-shared-mobile-component-library: ready-for-dev` entry |
| `docs/project-context.md` | Add Rule 18 (component library enforcement) |
| `docs/component-library.md` | New file — living component registry |

### Technical Impact

- `register.tsx` will be modified (refactor only, no behaviour change)
- No API changes, no backend changes, no migration changes
- No new dependencies required — all components use existing React Native primitives + theme tokens

---

## Section 3: Recommended Approach

**Selected: Option 1 — Direct Adjustment**

Insert Story 2-1-A into Epic 2 between the completed Story 2-1 and the backlog Story 2-2. This story:

1. Creates the full MVP shared component library in `mobile/components/`
2. Refactors `register.tsx` to use the new components
3. Establishes `docs/component-library.md` as the living component registry
4. Adds Rule 18 to `docs/project-context.md` making the library mandatory for all future stories

**Rationale:**
- Zero rollback required — Story 2-1 backend work is correct and untouched
- Zero MVP scope change — components are implementation detail, not new features
- Effort: Medium (1 sprint story worth of work)
- Risk: Low — purely additive, no behaviour changes
- Timeline impact: Story 2-2 delayed by one story cycle
- Long-term benefit: prevents ~20 screens worth of duplication debt

---

## Section 4: Detailed Change Proposals

---

### Change 4.1 — New Story 2-1-A in epics.md

**Story: 2-1-A: Shared Mobile Component Library**

```
As a developer,
I want a complete shared component library with all MVP components implemented or
placeholder-scaffolded,
So that every future screen uses consistent, accessible, theme-compliant components
rather than inline primitives.
```

**Acceptance Criteria:**

1. Given `mobile/components/shared/` is reviewed
   When the component inventory is checked
   Then `Button.tsx`, `TextField.tsx`, `RadioChipGroup.tsx`, `Toggle.tsx`, `LoadingIndicator.tsx`, `ErrorBanner.tsx`, `Card.tsx`, `Modal.tsx`, `EmptyState.tsx`, `SkeletonLoader.tsx`, `StatusBadge.tsx`, `ConsentBadge.tsx` all exist alongside the existing `AccessiblePressable.tsx` and `ThemedText.tsx`

2. Given `Button.tsx` is implemented
   When rendered with `variant` prop
   Then it supports `primary`, `secondary`, `ghost`, `danger` variants; accepts `isLoading` prop that renders `LoadingIndicator`; uses theme tokens exclusively; enforces 44×44 touch target; requires `accessibilityLabel`

3. Given `TextField.tsx` is implemented
   When rendered
   Then it wraps React Native `TextInput` with a `label` prop, `error` prop, `secureTextEntry` support, themed border/background, and `accessibilityLabel` derived from the label

4. Given `RadioChipGroup.tsx` is implemented
   When rendered
   Then it renders a horizontal row of selectable chips; selected chip uses `colors.accent.primary` border and `colors.background.surface` fill; unselected chip uses `colors.text.muted` border; `accessibilityRole="radiogroup"` on container, `accessibilityRole="radio"` on each chip

5. Given `Toggle.tsx` is implemented
   When rendered
   Then it wraps React Native `Switch` with a `label` prop; uses `colors.safety` as active track color; uses `colors.background.input` as inactive track color

6. Given `LoadingIndicator.tsx` is implemented
   When rendered
   Then it produces three animated pulsing dots using theme colors; respects `reduceMotion` from `AccessibilityContext` (static dots when true)

7. Given `ErrorBanner.tsx` is implemented
   When rendered with an `error` string prop
   Then it displays a themed error block using `colors.danger` text and `colors.background.surface` background with `accessibilityLiveRegion="polite"`

8. Given placeholder components are implemented (`Card`, `Modal`, `EmptyState`, `SkeletonLoader`, `StatusBadge`, `ConsentBadge` in `shared/`; `ChatBubble`, `ChatInput`, `ConversationRow` in `chat/`; `RevealMoment`, `RevealPrompt`, `RevealCountdown`, `EmptyMatchState`, `RevealProgress` in `match/`; `ReportButton` in `moderation/`; `QuizCard`, `ProgressStepper` in `onboarding/`)
   When each placeholder is reviewed
   Then each exports a typed props interface, renders a minimal visible stub using `ThemedText` with the component name, uses theme tokens, and includes an `// TODO: implement in Story X.X` comment citing the target story

9. Given `register.tsx` is refactored
   When the file is reviewed
   Then: raw `TextInput` replaced by `TextField`; submit and back buttons replaced by `Button`; gender selector replaced by `RadioChipGroup`; over-18 switch replaced by `Toggle`; `LoadingDots` local function removed; error display replaced by `ErrorBanner`; no inline component definitions remain

10. Given `docs/component-library.md` is created
    When reviewed
    Then it lists every component with: file path, props summary, which story fully implements it, and a usage example

11. Given `docs/project-context.md` is updated
    When Rule 18 is reviewed
    Then it states: developers must check `mobile/components/` before building any UI element; inline component definitions in screen files are prohibited; new components must be added to `docs/component-library.md`

12. Given `tsc --noEmit` is run from `mobile/`
    When compilation completes
    Then zero TypeScript errors

---

### Change 4.2 — epics.md insertion point

```
OLD (after Story 2.1 Acceptance Criteria):
### Story 2.2: User Login, JWT Tokens and Logout
...

NEW (insert between 2.1 and 2.2):
### Story 2-1-A: Shared Mobile Component Library
[Full story text as above]

### Story 2.2: User Login, JWT Tokens and Logout
...
```

**Rationale:** Story 2-2 will immediately need `Button` and `TextField` for the login form. This must be in place first.

---

### Change 4.3 — sprint-status.yaml

```yaml
OLD:
  epic-2: in-progress
  2-1-email-password-user-registration: done
  2-2-user-login-jwt-tokens-and-logout: backlog

NEW:
  epic-2: in-progress
  2-1-email-password-user-registration: done
  2-1-a-shared-mobile-component-library: ready-for-dev
  2-2-user-login-jwt-tokens-and-logout: backlog
```

**Rationale:** Story is immediately ready for dev — no blockers, no unknowns.

---

### Change 4.4 — docs/project-context.md Rule 18

```
OLD:
17. Identity auth logic is single-source: ...

NEW:
17. Identity auth logic is single-source: ...

18. Never define UI components inline in screen files. Before building any UI element,
    check mobile/components/ first — if a matching component exists, use it. If it
    does not exist, create it in the correct subdirectory (shared/, chat/, match/,
    moderation/, or onboarding/) with a typed props interface, theme tokens, and
    accessibility attributes, then register it in docs/component-library.md. Inline
    TextInput, Pressable-as-button, local loading animations, or ad-hoc error
    displays inside screen files are prohibited after Story 2-1-A.
```

---

### Change 4.5 — docs/component-library.md (new file)

New living registry created alongside the story — pre-populated with all components from Story 2-1-A. Each entry updated by the story that fully implements it.

---

## Section 5: Implementation Handoff

**Change scope: Minor** — direct implementation by dev agent.

| Role | Responsibility |
|---|---|
| Dev Agent | Implement Story 2-1-A in full per acceptance criteria |
| SM (next story creation) | Use `bmad-create-story` only after 2-1-A is `done` |
| All future dev agents | Check `docs/component-library.md` before building UI; follow Rule 18 |

**Success criteria:**
- `tsc --noEmit` passes with zero errors after 2-1-A
- `register.tsx` contains no inline component definitions
- `docs/component-library.md` exists and is complete
- Rule 18 is present in `docs/project-context.md`
- All placeholder components exist in their correct subdirectories
- Story 2-2 dev agent can import `Button` and `TextField` without creating them

**Dependencies:** None — Story 2-1-A has no blockers. Can begin immediately.

---

*Sprint Change Proposal generated by Correct Course workflow — 2026-03-23*
