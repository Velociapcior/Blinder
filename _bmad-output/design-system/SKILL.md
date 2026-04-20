# Blinder Design System — Skill

Use this skill when designing anything for **Blinder**, a conversation-first blind
dating app. The system is built to feel calm, equal-weight, and anti-urgent —
the opposite of a swipe feed. Follow these rules before touching a screen.

## Setup

1. Link the tokens in every HTML file:
   ```html
   <link rel="stylesheet" href="colors_and_type.css" />
   ```
   (Adjust the relative path.) This loads Lato from Google Fonts and defines
   every colour, type, spacing, radius, shadow, and motion variable.

2. For React prototypes, include the pinned React + Babel tags from the main
   design guidelines, then import the components you need from
   `ui_kits/Blinder/components/`. Components attach themselves to `window`, so
   subsequent `<script type="text/babel">` blocks can use them by name.

3. Order component scripts by dependency: `Icons` and `Button` first, then
   `BlindAvatar`, `ConversationBubble`, `StarterCard`, `GateOptionCard`,
   `RevealPortrait`, `ResolutionWait`, `Chrome`, `Screens`.

## Core invariants

- **`--reveal` (amber `#D4A85A`) appears nowhere except the Reveal button at
  the gate and the mutual-reveal ceremony.** Its distinctiveness is the point.
- **No high-saturation colours in trust-critical flows.** Warm neutrals +
  muted plum only.
- **Equal weight at the gate.** Reveal / Continue / Abandon share identical
  size, radius, and hit area. No default highlighted. No animation draws the
  eye toward one option.
- **Non-attributing endings.** Never reveal who chose what. Use *"This
  conversation has ended."* — never "they didn't respond" / "they declined."
- **One primary action per screen.** Secondary is a pair; tertiary is a link.
- **Single column everywhere.** No multi-column layouts on phone.
- **Icon-light.** Line icons only in: top-bar, send button, offline state,
  outcome screens. Never icon-only buttons in trust-critical flows.

## Voice & copy rules

- Sentence case headings. No ALL CAPS except button letter-spacing.
- One-sentence, just-in-time explanation for any novel mechanic.
- No countdowns, counters, streaks, scarcity, urgency, or dopamine language.
- No exclamation marks in endings.
- Forward-looking: every ending points to the next possibility.
- Gate self-explanation: *"You've had a real conversation — now you can
  decide what happens next."*
- Private-decision framing: *"Your choice is private. You'll find out
  together."*

## Motion rules

- **Two speeds.** Utility is near-instant (120–180ms, `--ease-standard`).
  Emotional moments are slow and emphasized (`--dur-gate` 420ms,
  `--dur-reveal` 1600–2400ms, `--ease-emphasize`).
- `prefers-reduced-motion`: all emotional animations collapse to a 200ms
  cross-fade.
- The resolution-wait component is a 2s pulse, not a spinner. Do not
  replace it with a loader.

## When in doubt

If a new element would introduce saturation, urgency, attribution, or
hierarchy anxiety — cut it. Whitespace, generous line-height, and the
restraint of the palette do the work that shadows and colour do in other
dating products.

## Entry points

- [`index.html`](./ui_kits/Blinder/index.html) — live showcase of every
  component, token, and screen.
- [`colors_and_type.css`](./colors_and_type.css) — all tokens.
- [`README.md`](./README.md) — full written spec.
- [`blinder-clickable-prototype.html`](./blinder-clickable-prototype.html) —
  original interactive prototype.
