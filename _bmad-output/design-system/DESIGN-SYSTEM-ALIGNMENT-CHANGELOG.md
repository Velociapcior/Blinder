# Design System Alignment — Changelog

This document records the edits made to every project artefact to bring them into alignment with the packaged Blinder design system:

- [`README.md`](./README.md) — written spec
- [`SKILL.md`](./SKILL.md) — core invariants
- [`colors_and_type.css`](./colors_and_type.css) — packaged CSS tokens
- [`ui_kits/Blinder/`](./ui_kits/Blinder/) — live component showcase
- [`ux-design-specification.md`](./ux-design-specification.md) — full UX specification

The design system is the source of truth. These edits do not change intent or scope — they make each document explicitly subordinate to the packaged system, and correct a handful of places where acceptance criteria or prototype markup had drifted from the system's rules.

---

## 1. `product-brief-Blinder.md`

**Added** a *Design System Alignment* footer that names the five canonical artefacts (README, SKILL, colors_and_type.css, ui_kits/Blinder, ux-design-specification.md) and makes clear that every downstream UX, copy, visual, motion, and component decision inherits from them.

## 2. `product-brief-Blinder-distillate.md`

**Added** a *Design System Pointer* section with the same five artefacts and an inline restatement of the non-negotiable UX rules: `--reveal` reserved for reveal only, equal-weight gate, non-attributing endings, one primary per screen, no urgency language, confirmation dialogs only for irreversible destructive actions.

## 3. `prd.md`

**Added** a top-of-doc callout immediately under the title block that binds every user-facing requirement to the packaged design system and specifically ties NFR28 (no urgency UI) to the `--reveal`-exclusivity / equal-weight-gate / non-attributing-endings invariants documented in `SKILL.md`.

## 4. `architecture.md`

**Added** a top-of-doc *Design System Alignment* callout immediately under the preamble that names the five canonical artefacts and states that Tamagui tokens, `colors_and_type.css`, and the live showcase are all materializations of the same source and must stay in lockstep.

**Expanded** the *Styling Solution* section under the Tamagui Expo Router starter selection with an explicit rule that `mobile/blinder-app/tamagui.config.ts` and `colors_and_type.css` must match one-to-one, and that `ui_kits/Blinder/index.html` is the visual-diff reference for the Tamagui token showcase screen shipped by Story 1.5.

## 5. `ux-design-specification.md`

**Added** a *Design System Packaging* section immediately under the title block. The spec is the authoring source for the design system; the new section makes the packaged artefacts (README, SKILL, colors_and_type.css, ui_kits/Blinder, blinder-clickable-prototype.html) visible as implementation-ready outputs of this document, and states that the Tamagui token implementation specified downstream must match the packaged CSS variables one-to-one.

## 6. `epics.md`

### 6.1 New top-of-doc *Design System Alignment* section

Added immediately under *Overview*. Names the five canonical artefacts and restates the invariants every AC must respect:

- `--reveal` (amber `#D4A85A`) appears nowhere except the Reveal gate option and the mutual-reveal ceremony.
- Equal-weight gate options — Reveal / Continue / Abandon share size, radius, and hit area; no visual default.
- Non-attributing endings — "This conversation has ended." never "they didn't respond" / "match expired" / "declined".
- Confirmation dialogs only for irreversible destructive actions (account deletion, block). **Never for gate decisions, message sends, or reports.**
- No toasts for emotionally significant outcomes.
- One primary action per screen.

### 6.2 Story 1.5 — Warm Dusk Token System

**Added** acceptance criteria binding the Tamagui token values to `colors_and_type.css` one-to-one, requiring the dev-only token showcase screen to mirror the sections in `ui_kits/Blinder/index.html`, and requiring the Story 1.5 design review gate to diff the two showcases visually.

### 6.3 Story 6.5 — Decision Gate Screen (AC3 & AC4) **[behavioural change]**

**Removed** the "Are you sure?" confirmation dialog from AC4. A confirmation dialog on the gate decision contradicts:

- The UX spec's *Form Patterns* rule ("Confirmation screens only where irreversible"). Continue, Reveal, and Abandon are not account-destructive; Continue is explicitly reversible in effect.
- The design system's anti-urgency posture — a modal that asks "Are you sure?" injects exactly the friction and doubt the gate is designed to remove.

**Replaced** with: the chosen option submits immediately, with no confirmation dialog; the pressed card enters a brief `submitted` state (UX-DR8) for all three options while the request is in flight. AC3 also now explicitly reiterates no countdown timer, no progress indicator, and no urgency copy (NFR28).

### 6.4 Story 5.x — Report & Block (AC3, AC5, AC6)

**Removed** the "confirmation toast" on report submission. UX-DR30 prohibits toasts for emotionally significant outcomes. **Replaced** with a dedicated *ReportConfirmationScreen* built on the `OutcomeScreen` layout, with calm headline ("Thank you for letting us know"), short compassionate body, and a single forward CTA back to the Waiting screen.

**Updated** the post-block navigation target from "Discover screen" (which does not exist in the design system) to the Waiting screen (`WaitingState`) — the home of the navigation state machine per UX-DR21.

**Updated** the tokens AC to reference `colors_and_type.css` and `ui_kits/Blinder/` as the canonical packaged design system.

### 6.5 Story on Outcome / Ending — AC5

**Updated** the CTA copy and destination to match the UX spec's `OutcomeScreen` anatomy (UX-DR11): a forward CTA ("Find my next match") returning to the Waiting screen (`WaitingState`) plus a "Take a break" text-link. Added an explicit reminder that copy is non-attributing ("they didn't respond" / "match expired" / "declined" are prohibited).

### 6.6 Notifications AC4, AC5, AC6

**Replaced** every "Discover screen" destination with the Waiting screen (`WaitingState`). **Rewrote** the new-match notification copy to "Someone is waiting to talk with you" so it matches the `MatchEntryCard` copy for continuity (UX-DR15) rather than the older "Someone wants to talk" phrasing.

### 6.7 Capacity Management (AC1, AC2)

**Replaced** "Discover screen" with the Waiting screen (`WaitingState`). **Tightened** AC1 to specify the capacity indicator is rendered as a neutral `Pill` component — not a progress bar, not a gamified counter. AC2 now explicitly requires the upgrade prompt to be visually subordinate to the waiting state itself.

### 6.8 Match Opportunity vocabulary

**Replaced** "starter card" with "new match opportunity" / "match" in the capacity story ACs (1 and 3) and in the notifications AC4 (matching the `MatchEntryCard` component naming), and added the NFR28 reference on the "no urgency signal" clause.

### 6.9 "Discover screen" → Waiting screen

Global replacement across all stories. The design system has no "Discover screen"; the home state is `WaitingState` per UX-DR14 / UX-DR20 / UX-DR21.

## 7. `blinder-clickable-prototype.html`

**Switched** the prototype to consume the packaged tokens — added `<link href="colors_and_type.css" rel="stylesheet" />` and deleted the inline `:root` block that duplicated (and could drift from) those values. Kept a tiny `:root` block containing only prototype-scoped aliases (`--shadow` → `var(--shadow-modal)`) with a comment telling editors to change the packaged CSS, not this file.

**Tokenised hardcoded values:**

| Before | After |
|---|---|
| `color: #B85050` on Confirm deletion button | `color: var(--error)` |
| Inline `color: #e7d7c5` on offline overlay copy | `color: var(--on-dark)` |
| `.offline { color: #f6eee5 }` stylesheet rule | `color: var(--on-dark)` |
| Inline avatar gradient `linear-gradient(135deg,#d18f5f,#8b4e6e)` | `linear-gradient(135deg, var(--accent), var(--primary))` |
| `.gate-card.reveal { background: #f2dfb7; border-color: #e5c27f }` | `background: color-mix(in oklab, var(--reveal) 28%, var(--bg-elevated)); border-color: color-mix(in oklab, var(--reveal) 55%, var(--border));` — with a comment noting this is the only screen allowed to use `--reveal` |

The ambient page gradient at the top of the file (the warm radial background behind the phone frame) was left as hardcoded hex values because it is a prototype-only backdrop, not a product surface. If this file is ever reused inside a real app shell, those values should come from the design system.

---

## Items deliberately left as-is

- **Account deletion's "Are you sure?" screen** in the prototype. This is the one place in the product where a confirmation is explicitly required by the design system ("Confirmation screens only where irreversible — account deletion, block"). Kept.
- **The Blinder product brief and distillate** did not need content rewrites. Both were already consistent with the system; they received pointer sections only.
- **The `#16100f` phone-shell background** in the prototype. Not a product surface — it is the visualization of a device bezel around the phone screen.
