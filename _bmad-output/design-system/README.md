# Blinder Design System

Blinder is a **conversation-first blind dating app**. Photos are hidden during the
initial conversation phase and only revealed after mutual intent. The core promise
is *dignity before desirability*: users are evaluated through interaction quality
before visual filtering. This is not a better swipe feed — it is a different
first-impression protocol.

**Audience:** Polish users tired of swipe-first dynamics, rejection loops, and
status-heavy UX. Launch is Poland-first, phone-only (iOS + Android).

**Emotional north star:** satisfaction → happiness-at-reveal → hope. Anti-urgency,
anti-rejection, anti-dopamine-loop.

---

## CONTENT FUNDAMENTALS

### Voice

The app sounds like **a thoughtful friend, not a system**. Calm authority (Zürich
Airport) meets warm humanity. It never pressures, never blames, and never performs
positivity it hasn't earned. It is short, specific, and forward-looking.

### Tone spectrum by moment

| Moment | Tone |
|---|---|
| Onboarding | Intrigue + quiet confidence ("this is different, you belong here") |
| Waiting | Calm anticipation, anchored in time |
| First message | Courageous warmth — acknowledge the vulnerability of going first |
| Active conversation | Receding — let the two humans talk |
| Decision gate | Grounded, explanatory, equal-weight |
| Resolution wait | Charged but unhurried — "your answer is with them" |
| Mutual reveal | Warm joy, shared achievement, unhurried |
| Non-mutual ending | Gentle acceptance — "this conversation has ended" |
| Re-entry after ending | Renewed hope, non-manufactured |

### Words to never use

| Avoid | Use instead |
|---|---|
| "Rejected", "declined", "didn't match" | "This conversation has ended" |
| "They chose not to reveal" | Omit entirely — never attribute |
| "Match expired" | "This conversation has ended" |
| "No matches yet" | "Your match arrives daily" |
| "Error" / "Failed" alone | Always pair with human explanation + next action |
| Exclamation marks in endings | Full stop or no punctuation |
| Streak / counter / score language | — |
| Countdown / urgency / scarcity language | — |

### Copy rules

- **One sentence, just-in-time** for any novel mechanic. Never front-load rules.
- **Non-attributing** in every ending. The user never knows who chose what.
- **Forward-looking** — every ending screen points to the next possibility.
- **Sentence case** for headings. No ALL CAPS except button letter-spacing.
- Gate self-explanation: *"You've had a real conversation — now you can decide
  what happens next."*
- Private decision framing: *"Your choice is private. You'll find out together."*
- Expiry / timeout: *"This conversation has ended."* — never "they didn't respond."

### Content density

Generous. Line height 1.6–1.65 on body copy. No character counters on the message
input. Single column on every screen. "Less is more" — if an element doesn't serve
the active user task, it is not visible.

---

## VISUAL FOUNDATIONS

### Palette — Warm Dusk

Warm neutrals as base; one muted primary (plum) and one ceremonial accent (amber)
reserved for the reveal. **No high-saturation colours anywhere in trust-critical
flows.** Tokens are defined in [`colors_and_type.css`](./colors_and_type.css).

| Token | Hex | Role |
|---|---|---|
| `--bg-base` | `#FBF5EE` | App background, all screens |
| `--bg-surface` | `#EDE3D8` | Cards, incoming bubbles, input fields |
| `--bg-elevated` | `#F5EDE2` | Modals, gate card background |
| `--primary` | `#8B4E6E` | Primary actions, outgoing bubbles, send button |
| `--primary-light` | `#B87A98` | Pressed states, secondary emphasis |
| `--reveal` | `#D4A85A` | **Reserved exclusively for the reveal ceremony** |
| `--accent` | `#C4825A` | Starter cards, links, secondary CTAs |
| `--text-primary` | `#2C1C1A` | All primary text |
| `--text-secondary` | `#7A5A52` | Timestamps, subtitles |
| `--text-muted` | `#A08878` | Placeholders, hints |
| `--border` | `#DDD0C4` | Dividers, input borders |
| `--error` | `#B85050` | Inline error states only |
| `--offline` | `#9A9090` | Offline state |
| `--bg-dark` | `#2C1C1A` | Gate overlay backdrop, offline full-screen |

Semantic rule: `--reveal` appears **nowhere** except the Reveal button at the gate
and the mutual-reveal ceremony. Its distinctiveness is part of the product.

### Typography — Lato

Lato: Light (300), Regular (400), Bold (700), Black (900). Loaded from Google Fonts
via `colors_and_type.css`.

| Token | Weight | Size | Line height | Usage |
|---|---|---|---|---|
| `text-display` | 900 | 32px | 1.2 | Reveal ceremony heading only |
| `text-h1` | 700 | 24px | 1.3 | Screen titles, onboarding headings |
| `text-h2` | 700 | 20px | 1.35 | Section headings, gate title |
| `text-h3` | 700 | 17px | 1.4 | Subsection labels, chat header name |
| `text-body` | 400 | 15px | 1.65 | Message bubbles, body copy |
| `text-body-sm` | 400 | 13px | 1.6 | Starter prompts, secondary |
| `text-caption` | 300 | 11px | 1.5 | Timestamps, status |
| `text-button` | 700 | 14px | 1 | Button labels (+0.04em tracking) |

Principles: generous line-height for body, no italics in trust-critical flows,
default letter-spacing (0) for body and `+0.04em` for buttons and captions only.

### Spacing — 8-pt grid

| Token | Value | Usage |
|---|---|---|
| `--space-xs` | 4px | Icon gaps, tight inline |
| `--space-sm` | 8px | Bubble + timestamp, related elements |
| `--space-md` | 16px | Standard padding, between bubbles |
| `--space-lg` | 24px | Screen horizontal padding, section gaps |
| `--space-xl` | 32px | Between major sections |
| `--space-2xl` | 48px | Onboarding breathing room |

Screen horizontal padding is `--space-lg` (24px visually / 16px per spec token)
consistently — no screen-specific overrides. Minimum touch target 44×44px.

### Radii & elevation

| Token | Value | Usage |
|---|---|---|
| `--radius-sm` | 8px | Chips, tags |
| `--radius-md` | 14px | Input fields |
| `--radius-lg` | 18px | Conversation bubbles |
| `--radius-xl` | 20px | Cards, gate card, modals |
| `--radius-full` | 9999px | Avatars, send button, pill buttons |

**Elevation is minimal.** Shadows are used only for: the single primary CTA on a
screen (`--shadow-cta`), elevated modal/sheet surfaces (`--shadow-modal`), and
the reveal portrait (custom amber glow). No depth cues create hierarchy anxiety.

### Motion

**Motion vocabulary is the emotional vocabulary.**

| Use | Duration | Easing |
|---|---|---|
| Utility transitions (nav, back) | 120–180ms | `cubic-bezier(.2,.0,.0,1)` (standard) |
| In-screen feedback (press, send) | 150ms | ease-out |
| Gate appearance | 420ms | `cubic-bezier(.16,1,.3,1)` (emphasized decel) |
| Resolution wait pulse | 2000ms loop | ease-out (intentional, unhurried) |
| Reveal ceremony | 1600–2400ms, multi-stage | slow bezier, warm |
| `prefers-reduced-motion` | — | All emotional animations fall back to 200ms cross-fade |

Rule: **slow for emotional moments (reveal, gate, resolution wait), near-instant
for utility.** The gap between the two is part of the experience design.

### Iconography

Line icons, **1.75–2px stroke, rounded caps & joins**, 24px grid. The app is
deliberately **icon-light** — the conversation carries the product. Icons appear
only in: the top-bar (back, profile, more), send button, offline blocker, outcome
screens. No icon-only buttons in trust-critical flows — always paired with a
label.

### Layout

- Single column throughout — never multi-column on phone.
- Screen horizontal padding consistent at `--space-lg`.
- Safe-area insets respected everywhere (notch, Dynamic Island, Android nav bar).
- Conversation bubbles max-width 78% so they scale across phone sizes.
- No `position: absolute` for primary content — breaks at 1.3× font scale.

---

## COMPONENTS

See [`ui_kits/Blinder/index.html`](./ui_kits/Blinder/index.html) for a live
showcase of every custom component. Source components live in
[`ui_kits/Blinder/components/`](./ui_kits/Blinder/components/).

| Component | Purpose |
|---|---|
| `Button` | Primary / Secondary / Reveal / Destructive / Link hierarchy |
| `BlindAvatar` | Warm dignified placeholder during the blind phase |
| `ConversationBubble` | Message unit — sent/received/sending/failed |
| `StarterCard` | Tappable conversation prompt — dissolves blank-page anxiety |
| `GateOptionCard` | Equal-weight decision card — Reveal / Continue / Abandon |
| `RevealPortrait` | Full portrait with amber glow for the reveal ceremony |
| `ResolutionWait` | Warm pulse — "your answer is with them" |
| `WaitingState` | Home screen with temporal anchor |
| `MatchEntryCard` | "Someone is waiting to talk with you" |
| `OutcomeScreen` | Neutral closure for non-mutual endings |
| `OfflineBlocker` | Full-screen calm offline overlay (≤2s) |
| `ProfileAvatar` | Top-right entry to ProfileSheet |
| `TopBar` | Screen header — back / name / more |
| `MessageInput` | Single-line message composer |
| `Pill` | Temporal anchors and status tags |

---

## REFERENCE MATERIAL

- [`product-brief-Blinder-distillate.md`](./product-brief-Blinder-distillate.md) — product thesis
- [`ux-design-specification.md`](./ux-design-specification.md) — full UX spec
- [`prd.md`](./prd.md) — product requirements
- [`epics.md`](./epics.md) — epic breakdown
- [`blinder-clickable-prototype.html`](./blinder-clickable-prototype.html) — original HTML prototype

---

## CAVEATS

- **No logo or brand mark yet.** The name "Blinder" is rendered in Lato Black
  (900) at `--text-display` scale for all wordmark uses until a proper logo is
  designed.
- **Iconography is described but not packaged.** The app is intentionally icon-
  light; use any rounded, 1.75–2px stroke line-icon set (Phosphor "Regular",
  Lucide, Heroicons outline). All icons used in this kit are inline SVG.
- **No dark theme tokens yet** — post-MVP token swap per the UX spec.
