---
name: blinder-design-system
description: |
  Enforce the Blinder Design System when implementing any UI, screen, or component.
  Use this skill when: implementing screens, styling components, choosing colors,
  writing copy, designing motion/animation, or reviewing frontend code for design
  compliance. Must be combined with blinder-rn-expo-screen for React Native work.
  Triggers: "design system", "color token", "typography", "motion", "component",
  "conversation screen", "gate screen", "reveal", "button", "avatar", "bubble",
  "styling", "UI", "visual", "copy", "microcopy", "animation", "frontend".
---

# Blinder Design System — Claude Code Enforcement

**Source of truth:** [`_bmad-output/design-system/`](_bmad-output/design-system/)

Read the full spec before implementing any UI:
- [`_bmad-output/design-system/README.md`](_bmad-output/design-system/README.md) — complete written spec
- [`_bmad-output/design-system/SKILL.md`](_bmad-output/design-system/SKILL.md) — invariants and rules
- [`_bmad-output/design-system/colors_and_type.css`](_bmad-output/design-system/colors_and_type.css) — all design tokens
- [`_bmad-output/design-system/ui_kits/Blinder/index.html`](_bmad-output/design-system/ui_kits/Blinder/index.html) — live component showcase

---

## Checklist: run before marking any UI task done

- [ ] All colours come from `colors_and_type.css` tokens — no hardcoded hex values
- [ ] `--reveal` (#D4A85A) appears **only** on the Reveal gate button and mutual-reveal ceremony
- [ ] Gate options (Reveal / Continue / Abandon) are equal size, radius, hit area — no visual default
- [ ] Endings use "This conversation has ended." — never "declined", "expired", "didn't respond"
- [ ] One primary action per screen max
- [ ] No confirmation dialogs on gate decisions, message sends, or reports
- [ ] No urgency copy: no countdowns, streaks, scarcity, exclamation marks in endings
- [ ] Post-report: use `OutcomeScreen` layout, not a toast
- [ ] Navigation target is `WaitingState` — not "Discover screen" (does not exist)
- [ ] Utility transitions 120–180ms; gate 420ms; reveal 1600–2400ms; pulse 2000ms loop
- [ ] `prefers-reduced-motion` falls back to 200ms cross-fade on all emotional animations
- [ ] Body copy line-height ≥ 1.6; buttons letter-spacing +0.04em

---

## Tamagui token mapping (React Native)

`mobile/blinder-app/tamagui.config.ts` **must match `colors_and_type.css` one-to-one.**  
`_bmad-output/design-system/ui_kits/Blinder/index.html` is the visual-diff reference for the Tamagui token showcase screen (Story 1.5).

| CSS token | Tamagui key |
|---|---|
| `--bg-base` | `background` |
| `--bg-surface` | `backgroundHover` |
| `--bg-elevated` | `backgroundFocus` |
| `--primary` | `color` (primary) |
| `--reveal` | `yellow10` (reserved) |
| `--accent` | `orange10` |
| `--text-primary` | `color` (default) |
| `--text-secondary` | `colorSubtitle` |
| `--border` | `borderColor` |
| `--error` | `red10` |

---

## Component reference

| Component | File |
|---|---|
| `Button` | [`ui_kits/Blinder/components/Button.jsx`](_bmad-output/design-system/ui_kits/Blinder/components/Button.jsx) |
| `BlindAvatar` | [`ui_kits/Blinder/components/BlindAvatar.jsx`](_bmad-output/design-system/ui_kits/Blinder/components/BlindAvatar.jsx) |
| `ConversationBubble` | [`ui_kits/Blinder/components/ConversationBubble.jsx`](_bmad-output/design-system/ui_kits/Blinder/components/ConversationBubble.jsx) |
| `StarterCard` | [`ui_kits/Blinder/components/StarterCard.jsx`](_bmad-output/design-system/ui_kits/Blinder/components/StarterCard.jsx) |
| `GateOptionCard` | [`ui_kits/Blinder/components/GateOptionCard.jsx`](_bmad-output/design-system/ui_kits/Blinder/components/GateOptionCard.jsx) |
| `RevealPortrait` | [`ui_kits/Blinder/components/RevealPortrait.jsx`](_bmad-output/design-system/ui_kits/Blinder/components/RevealPortrait.jsx) |
| `ResolutionWait` | [`ui_kits/Blinder/components/ResolutionWait.jsx`](_bmad-output/design-system/ui_kits/Blinder/components/ResolutionWait.jsx) |
| `Screens` (WaitingState, OutcomeScreen…) | [`ui_kits/Blinder/components/Screens.jsx`](_bmad-output/design-system/ui_kits/Blinder/components/Screens.jsx) |
| `Chrome` (TopBar, MessageInput…) | [`ui_kits/Blinder/components/Chrome.jsx`](_bmad-output/design-system/ui_kits/Blinder/components/Chrome.jsx) |
| `Icons` | [`ui_kits/Blinder/components/Icons.jsx`](_bmad-output/design-system/ui_kits/Blinder/components/Icons.jsx) |

---

## Preview screens

Open these in a browser to verify visual output matches the design:

| Screen | File |
|---|---|
| Conversation | [`preview/conversation.html`](_bmad-output/design-system/preview/conversation.html) |
| Decision gate | [`preview/gate.html`](_bmad-output/design-system/preview/gate.html) |
| Buttons | [`preview/buttons.html`](_bmad-output/design-system/preview/buttons.html) |
| Colours | [`preview/colors.html`](_bmad-output/design-system/preview/colors.html) |
| Typography | [`preview/type.html`](_bmad-output/design-system/preview/type.html) |
| Spacing | [`preview/spacing.html`](_bmad-output/design-system/preview/spacing.html) |
| Brand | [`preview/brand.html`](_bmad-output/design-system/preview/brand.html) |
