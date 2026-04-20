---
stepsCompleted: ['step-01-validate-prerequisites', 'step-02-design-epics', 'step-03-create-stories', 'step-04-final-validation']
separationRule: 'backend-and-frontend-stories-are-always-separate'
inputDocuments:
  - '_bmad-output/planning-artifacts/prd.md'
  - '_bmad-output/planning-artifacts/architecture.md'
  - '_bmad-output/planning-artifacts/ux-design-specification.md'
---

# Blinder - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for Blinder, decomposing the requirements from the PRD, UX Design, and Architecture requirements into implementable stories.

## Design System Alignment

All visual, copy, component, motion, and iconography decisions in this epic breakdown are bound to the packaged Blinder design system. Any AC that references a colour, token, component, piece of copy, or interaction pattern inherits from these sources \u2014 if a story AC disagrees with the design system, the design system wins:

- [`README.md`](./README.md) \u2014 written spec (voice, tone, tokens, copy rules, components)
- [`SKILL.md`](./SKILL.md) \u2014 core invariants (`--reveal` exclusivity, equal-weight gate, non-attributing endings, one primary per screen)
- [`colors_and_type.css`](./colors_and_type.css) \u2014 CSS variables that are the packaged token source of truth
- [`ui_kits/Blinder/index.html`](./ui_kits/Blinder/index.html) \u2014 live component showcase
- [`ux-design-specification.md`](./ux-design-specification.md) \u2014 full UX specification (journeys, patterns, vocabulary)

Key invariants reinforced across every story:
- **`--reveal` (amber `#D4A85A`)** appears nowhere except the Reveal gate option and the mutual-reveal ceremony.
- **Equal-weight gate options** \u2014 Reveal / Continue / Abandon share size, radius, and hit area; no visual default.
- **Non-attributing endings** \u2014 \"This conversation has ended.\" Never \"they didn't respond\" / \"match expired\" / \"declined\".
- **Confirmation dialogs only for irreversible destructive actions** (account deletion, block). Never for gate decisions, message sends, or reports.
- **No toasts for emotionally significant outcomes** \u2014 reveal uses `RevealTransition`, endings use `OutcomeScreen`, reports use a dedicated confirmation screen.
- **One primary action per screen.** Single column. Icon-light. Motion-wise: near-instant for utility, slow and emphasized for emotional moments.


## Requirements Inventory

### Functional Requirements

**Account, Identity, and Consent**
- FR1: A user can create an account and sign in to access Blinder.
- FR2: A user can delete their account from within the app.
- FR3: A user can review and accept product policies required for use.
- FR4: A user can confirm legal-age eligibility before entering dating flows.
- FR5: A user can grant or deny required permissions during feature use.
- FR6: A user can update profile basics required for matching participation.

**Onboarding and Matching Inputs**
- FR7: A user can complete an onboarding quiz that captures conversation-relevant traits.
- FR8: The system can generate conversation starter prompts from onboarding inputs.
- FR9: A user can provide location data for location-based matching.
- FR10: A user can upload profile photos for post-reveal profile use.
- FR11: A user can proceed through onboarding input flows only after required inputs are completed.
- FR12: A user can re-enter onboarding input flows to update match-relevant information.

**Match Lifecycle and Conversation Start**
- FR13: A user can receive a new match opportunity in the app.
- FR14: A user can accept or pass on a presented match opportunity.
- FR15: The system can start a conversation window only after both matched users send an initial message.
- FR16: A user can send and receive messages during the blind phase.
- FR17: A user can view conversation context and starter prompts while in blind chat.
- FR18: A user can continue active trust-critical conversation contexts within configured limits.

**Decision Gate and Outcome Handling**
- FR19: The system can trigger a decision gate based on time-floor conditions.
- FR20: The system can trigger a decision gate based on message-threshold conditions.
- FR21: A user can choose Reveal, Continue, or Abandon at the decision gate.
- FR22: The system can resolve decision outcomes using private, simultaneous decision handling.
- FR23: The system can transition both users to revealed state when reveal is mutual.
- FR24: The system can keep outcomes anonymized when conversations end without mutual reveal.
- FR25: A user can receive a clear conversation end state without counterpart-blame attribution.
- FR26: The system can expire inactive match or conversation states and release users back to matching eligibility.
- FR46: A user can view why the decision gate was triggered for a conversation.
- FR50: A user can view neutral status messaging when a match or conversation expires.

**Safety, Moderation, and Abuse Handling**
- FR27: The system can evaluate uploaded photos before they become available in product flows.
- FR28: The system can prevent disallowed media from entering user-visible flows.
- FR29: The system can route uncertain media cases for manual review.
- FR30: A user can report harmful or uncomfortable behavior encountered in conversation.
- FR31: A user can block another user from further interaction.
- FR32: The system can maintain auditable records for safety-critical and state-transition events.
- FR33: Internal trust-and-safety operators can review moderation decisions and apply corrective actions.
- FR34: A user can receive remediation or next-step guidance when media is rejected.

**Notifications and Engagement Model**
- FR35: A user can receive notifications for meaningful state transitions only.
- FR36: The system can notify users about new match availability.
- FR37: The system can notify users about partner replies.
- FR38: The system can notify users when a decision gate is reached.
- FR39: The system can avoid non-essential engagement notifications unrelated to trust-critical flow events.

**Monetization and Access Control**
- FR48: A user can subscribe to higher concurrent conversation capacity through premium access.
- FR49: The system can enforce configured concurrent conversation limits by subscription tier.

**Operations, Support, and Product Governance**
- FR40: Internal support staff can inspect conversation state timelines for issue investigation.
- FR41: Internal support staff can communicate user-facing issue outcomes without exposing counterpart-sensitive decision details.
- FR42: Product operators can monitor core quality indicators tied to matching and conversation outcomes.
- FR43: Product operators can monitor safety and moderation indicators for risk escalation.
- FR44: Internal teams can review cohort-level behavior signals to inform scope and quality decisions.
- FR45: The system can restrict core matching and conversation actions when connectivity is unavailable and present a clear blocked state.
- FR51: Internal support staff can view decision-gate trigger and resolution history for issue diagnosis.
- FR52: Product operators can configure market-specific policy controls for feature availability.
- FR53: The system can collect a lightweight post-outcome fairness or safety pulse from users.

**Permissions and Transparency**
- FR47: A user can view contextual rationale before granting precise location permission.

### NonFunctional Requirements

**Performance**
- NFR1: 95% of core in-app navigation actions complete in <= 2.0 seconds under normal network conditions.
- NFR2: 95% of message send acknowledgments are returned in <= 2.5 seconds under normal network conditions.
- NFR3: 95% of decision-gate action submissions (Reveal/Continue/Abandon) receive definitive server outcome in <= 3.0 seconds under normal network conditions.
- NFR4: Match availability checks complete in <= 2.0 seconds for 95% of requests.

**Security & Privacy**
- NFR5: All user data in transit is encrypted using TLS 1.2+.
- NFR6: Sensitive user data at rest is encrypted using AES-256 or equivalent in persistent storage, validated by security audit prior to production launch.
- NFR7: Pre-reveal media visibility is technically blocked across all user-facing interfaces and APIs until mutual reveal state is achieved.
- NFR8: Access to moderation, support, and operator tools is role-based and auditable.
- NFR9: GDPR baseline obligations are supported, including consent recordability, data export, account deletion, and retention policy enforcement.
- NFR10: Age-gate enforcement prevents users failing legal-age checks from entering dating flows.

**Reliability & Availability**
- NFR11: Monthly availability target for core user flows (auth, matching, messaging, gate decisions) is >= 99.5%.
- NFR12: Decision-gate resolution is exactly-once from the user perspective; duplicate client submissions cannot create conflicting outcomes.
- NFR13: System failures in non-core subsystems must not compromise integrity of conversation state transitions.
- NFR14: When connectivity is unavailable, the app must block core write actions and display a clear user-facing blocked state within <= 2 seconds.

**Scalability**
- NFR15: Architecture supports at least 10x growth from launch baseline without redesign of core domain model.
- NFR16: Under 5x launch baseline concurrency, p95 latency for core flows degrades by no more than 20% versus baseline.
- NFR17: Cohort/city rollout controls support phased expansion without full-system reconfiguration.

**Accessibility**
- NFR18: Mobile UI for core flows meets WCAG 2.1 AA-equivalent criteria applicable to native app experiences.
- NFR19: All trust-critical actions (report, block, decision choices, account deletion) are operable with assistive technologies on iOS and Android.
- NFR20: Critical status messages (offline blocked state, conversation ended, decision required) are presented in plain language and announced accessibly.

**Integration & Interoperability**
- NFR21: Push delivery integration supports APNS and FCM with retry handling and failure observability.
- NFR22: Media moderation integration supports automated decision plus manual-review fallback path with traceable decision state.
- NFR23: Integration failures (push or moderation provider outage) must fail safely and preserve trust-critical state consistency.

**Observability & Auditability**
- NFR24: All trust-critical state transitions are logged with correlation identifiers to support diagnostics and support investigation.
- NFR25: Moderation and safety decisions are audit-logged with actor, timestamp, and outcome.
- NFR26: Product quality guardrail metrics (conversation depth, gate completion, reveal trend, fairness/safety pulse) are available at cohort level no later than T+1 day.
- NFR27: Support tooling can retrieve complete decision-gate trigger and resolution history for a reported conversation issue.

**UX Tone**
- NFR28: The system shall use no urgency-inducing UI elements (countdown timers, pressure language, engagement counters, streak indicators) in trust-critical flows.

### Additional Requirements

**Starter Template (Epic 1, Story 1)**
- Mobile app must be initialized using Tamagui Expo Router starter: `yarn create tamagui@latest --template expo-router`
- Starter must be pruned of demo structure and reorganized into domain-first modules (onboarding, waiting, match-entry, conversation, gate, reveal, ending, settings)

**Backend Initialization**
- Backend initialized separately as ASP.NET Core 10 Web API project (not part of mobile starter)
- Three separate backend applications: `Blinder.IdentityServer`, `Blinder.Api`, `Blinder.AdminPanel`
- IdentityServer uses OpenIddict 7.4.0 as self-hosted OAuth2/OIDC server
- Api uses EF Core 10 with Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1
- AdminPanel uses Razor Pages on .NET 10, authenticates via OIDC against IdentityServer

**Database Architecture**
- Single PostgreSQL 18 database with logical schema separation: `identity.*` (owned by IdentityServer) and `app.*` (owned by Api)
- Separate EF Core migration sets per application ownership boundary
- Database constraints for invariants: foreign keys, uniqueness, check constraints, row-version concurrency on trust-critical aggregates

**API Design**
- RESTful JSON APIs as primary integration style
- Built-in ASP.NET Core OpenAPI 3.1 generation for API documentation
- RFC 9457 Problem Details error responses consistently across API surface
- Rate limiting on auth, onboarding, messaging, moderation, and upload endpoints
- ASP.NET Core SignalR for trust-critical real-time conversation updates (not sole source of truth)

**Authentication & Identity**
- Local first-party accounts + Google, Apple, Facebook federation via IdentityServer
- Bearer-token authorization for mobile; OIDC sign-in for AdminPanel
- Admin endpoints isolated via dedicated scopes, roles, and authorization policies
- MFA/passkey design must remain open even if not delivered in MVP

**Infrastructure & Deployment**
- Docker Compose initial production deployment on Hetzner VPS
- Traefik as reverse proxy/ingress (only container exposing ports 80/443)
- Containers: `traefik`, `identityserver`, `api`, `adminpanel`, `postgres`, `minio`
- All app services must expose health checks
- ASP.NET Core Data Protection keys persisted outside ephemeral container filesystems
- Identity signing and encryption keys persisted and shared appropriately
- Images built in CI, pushed to registry; never built on production VPS

**MinIO Object Storage**
- Self-hosted MinIO for photo storage
- Photo moderation pipeline integrated: automated scan + manual review fallback

**Architecture Tests**
- Architecture boundary enforcement tests in `backend/tests/Blinder.ArchitectureTests/`
- Cross-app integration tests in `backend/tests/Blinder.IntegrationTests/`

**Server-Authoritative State Machine**
- All conversation-state transitions (start, gate trigger, reveal/continue/abandon, expiry) are server-authoritative
- Idempotent event handling to prevent race-condition trust breaks
- Full audit trail for all safety and state-transition events

### UX Design Requirements

**Design System Foundation**
- UX-DR1: Design token architecture must be locked and reviewed before any screen implementation begins — hard gate. Token categories: colour (Warm Dusk palette), typography (Lato), spacing (8px base unit), border radius, shadow, motion curves.
- UX-DR2: Typography system implementation — Lato font (Light/Regular/Bold/Black) with all scale tokens: `text.display`, `text.h1`, `text.h2`, `text.h3`, `text.body`, `text.body.sm`, `text.caption`, `text.button`.
- UX-DR3: Spacing and layout foundation — 8px base unit, all tokens: `space.xs` (4px) through `space.2xl` (48px), border radius tokens `radius.sm` through `radius.full`.
- UX-DR4: Motion vocabulary spec — define what animates, at what speed, with what easing, distinguish in-screen animations vs. navigation-level transitions. Reveal ceremony and gate transition must be prototyped and felt before token lock.
- UX-DR5: Color token `color.reveal` reserved exclusively for the reveal ceremony and Reveal button at the gate — must not appear anywhere else in the app.

**Custom Components — Phase 1 (Trust-Critical Core)**
- UX-DR6: `BlindAvatar` component — visual representation of unseen person, states: `default/loading/error`, variants: `sm/lg`. Must not look like a broken image; warm, dignified, human-feeling. With aria-label.
- UX-DR7: `ConversationBubble` component — message display unit, states: `sent/received/sending/failed`, variants: `them` (surface, left-aligned) / `me` (primary colour, right-aligned). Timestamps announced on focus; failed state includes retry action.
- UX-DR8: `GateOptionCard` component — full-width equal-weight option card for decision gate. States: `default/pressed/submitted`. Variants: `reveal` (amber `color.reveal`), `continue` (surface), `abandon` (surface, muted text). No default highlighted — product requirement. Role="radio" within group.
- UX-DR9: `RevealTransition` component — multi-stage reveal ceremony animation: `waiting → animating → revealed`. Slow, warm, unhurried. Falls back to cross-fade at `prefers-reduced-motion`. Conversation history visible throughout.
- UX-DR10: `ResolutionWait` component — holding state between gate submission and outcome. Not a spinner, not a progress bar. States: `waiting/resolving`. Announces "Waiting for the other person's choice" on mount.
- UX-DR11: `OutcomeScreen` component — post-gate outcome delivery. Variants: `reveal` (hands off to RevealTransition) and `acceptance` (neutral closure). Acceptance anatomy: neutral icon + calm headline + compassionate body + "Find my next match" CTA + "Take a break" link. Never blame-attributing.
- UX-DR12: `OfflineBlocker` component — full-screen overlay appearing within ≤2 seconds (NFR14). States: `offline/reconnecting/restored`. Calm, not alarming; no retry button; automatic reconnection. Blocks all interactive elements behind it.

**Custom Components — Phase 2 (Full Journey)**
- UX-DR13: `StarterCard` component — tappable conversation prompt, states: `default/pressed/selected` (auto-populates input). Open-ended, warm, non-judgmental content. Full tap target, role="button".
- UX-DR14: `WaitingState` component — home screen with temporal anchor, states: `no_match/match_arriving/match_ready`. "Your match arrives daily" — never a blank screen or spinner.
- UX-DR15: `MatchEntryCard` component — first impression of match between WaitingState and StarterScreen. Anatomy: `BlindAvatar` (lg) + "Someone is waiting to talk with you" + single CTA. States: `default/entering` (animated arrival).
- UX-DR16: `ProfileAvatar` component — top-right header tap target, always visible, entry to ProfileSheet. States: `default/pending_moderation/unread_indicator`. Must be visually interactive — not passive. Sufficient touch target.
- UX-DR17: `ProfileSheet` component — slide-up panel for own profile/settings/account management. Sections: Profile preview → Edit profile → Notification settings → Support → Account deletion. Focus trapped within sheet while open.
- UX-DR18: `ProfileEditForm` component — edit traits, photo, location preference. Photo re-upload triggers moderation immediately. States: `viewing/editing/saving/moderation_pending`.
- UX-DR19: `RevealedProfileView` component — post-reveal read-only profile. Anatomy: full photo + name + trait chips + "Keep talking" CTA + "Read your conversation" link. Conversation history link must be prominent.

**Navigation & Information Architecture**
- UX-DR20: Single-focus navigation — no persistent bottom tab bar at any point. Profile/settings accessible via top-right avatar tap only. Navigation chrome disappears during active conversation (full-bleed).
- UX-DR21: Navigation state machine must be implemented: Onboarding → Waiting → Match Entry → Starter Screen → Conversation → Gate → Resolution Wait → Outcome → Waiting.
- UX-DR22: Full-screen decision gate (D4) — gate takes over entirely, explains itself in one calm sentence with trigger reason. Three options at equal visual weight.
- UX-DR23: Starter Forward first-message entry (D5) — 2-3 prominent starter cards before first send; transitions to conversation after first send.
- UX-DR24: Reveal ceremony screen (D6) — animated, warm, full-screen. Conversation history visible. Earned and celebratory moment.
- UX-DR25: Non-mutual ending screen (D6) — neutral copy, no blame, immediate forward path. Copy rules: "This conversation has ended" never "They chose not to reveal" or "match expired".
- UX-DR26: All trust-critical flows are linear/forward-only — no back navigation during gate, resolution wait, or outcome screens. Deep-link entry resolves to correct state without disrupting in-progress flows.

**Onboarding UX**
- UX-DR27: Onboarding quiz — one question per screen, progress bar (no percentage label), full-width tappable option cards.
- UX-DR28: Contextual permission prompts — location permission shown with rationale before system prompt; notification permission deferred until match is received, not bulk-shown at onboarding.
- UX-DR29: Photo moderation pending state — warm photo placeholder with "Your photo is being reviewed" copy, not a blank or error state.

**Interaction & Feedback Patterns**
- UX-DR30: No toasts for emotionally significant outcomes — mutual reveal uses RevealTransition, endings use OutcomeScreen, report uses dedicated confirmation screen.
- UX-DR31: Skeleton screens for content loading states — no full-screen spinners.
- UX-DR32: Report/block accessible from conversation header (⋯ menu) at all times — never more than one tap away.
- UX-DR33: Copy vocabulary enforcement — defined words to never use: "Rejected", "declined", "didn't match", "They chose not to reveal", "Match expired", "No matches yet". Use neutral forward-looking alternatives.
- UX-DR34: Equal-weight gate options — no visual default, all three options (Reveal/Continue/Abandon) same size/radius/weight. Color differentiation for Reveal only (amber `color.reveal`), not to imply hierarchy.

**Accessibility (Non-negotiable minimums)**
- UX-DR35: All interactive elements minimum 44×44px touch targets enforced via `minHeight`/`minWidth` tokens.
- UX-DR36: `color.text.primary` (#2C1C1A) on `color.bg.base` (#FBF5EE) meets WCAG AA contrast — verified in token design.
- UX-DR37: iOS Dynamic Type and Android font scale respected via Tamagui text primitives (no custom font size override).
- UX-DR38: All screens wrapped in safe area insets — required for notch/Dynamic Island/Android nav bar.

### FR Coverage Map

| FR | Epic | Domain |
|---|---|---|
| FR1 | Epic 2 | Account creation & sign-in |
| FR2 | Epic 2 | Account deletion |
| FR3 | Epic 2 | Policy acceptance |
| FR4 | Epic 2 | Age gate |
| FR5 | Epic 3 | Contextual permissions |
| FR6 | Epic 3 | Profile basics update |
| FR7 | Epic 3 | Onboarding quiz |
| FR8 | Epic 3 | Starter prompt generation |
| FR9 | Epic 3 | Location data |
| FR10 | Epic 3 | Photo upload |
| FR11 | Epic 3 | Required inputs gate |
| FR12 | Epic 3 | Re-enter onboarding |
| FR13 | Epic 4 | Receive match opportunity |
| FR14 | Epic 4 | Accept/pass match |
| FR15 | Epic 4 | Conversation window activation |
| FR16 | Epic 5 | Send/receive messages |
| FR17 | Epic 4 | Starter prompts in blind chat |
| FR18 | Epic 5 | Active conversation limits |
| FR19 | Epic 6 | Time-floor gate trigger |
| FR20 | Epic 6 | Message-threshold gate trigger |
| FR21 | Epic 6 | Reveal/Continue/Abandon choice |
| FR22 | Epic 6 | Private simultaneous decisions |
| FR23 | Epic 6 | Mutual reveal transition |
| FR24 | Epic 6 | Anonymized non-mutual endings |
| FR25 | Epic 6 | End state without blame |
| FR26 | Epic 6 | Inactive state expiry |
| FR27 | Epic 3 | Photo evaluation before availability |
| FR28 | Epic 3 | Prevent disallowed media |
| FR29 | Epic 3 | Manual review routing |
| FR30 | Epic 5 | Report behavior |
| FR31 | Epic 5 | Block user |
| FR32 | Epic 7 | Safety audit records |
| FR33 | Epic 7 | T&S operator review & corrective action |
| FR34 | Epic 3 | Media rejection remediation |
| FR35 | Epic 8 | Meaningful-events-only notifications |
| FR36 | Epic 8 | Notify new match |
| FR37 | Epic 8 | Notify partner reply |
| FR38 | Epic 8 | Notify gate reached |
| FR39 | Epic 8 | No non-essential notifications |
| FR40 | Epic 9 | Support staff timeline inspection |
| FR41 | Epic 9 | Support outcome communication (redacted) |
| FR42 | Epic 11 | Quality indicators monitoring |
| FR43 | Epic 11 | Safety/moderation indicators |
| FR44 | Epic 11 | Cohort-level behavior signals |
| FR45 | Epic 5 | Offline blocked state |
| FR46 | Epic 6 | Gate trigger reason visibility |
| FR47 | Epic 3 | Location permission rationale |
| FR48 | Epic 10 | Premium subscription |
| FR49 | Epic 10 | Conversation capacity enforcement |
| FR50 | Epic 6 | Neutral expiry messaging |
| FR51 | Epic 9 | Gate history for support |
| FR52 | Epic 9 | Market-specific policy controls |
| FR53 | Epic 11 | Post-outcome fairness/safety pulse |

## Epic List

### Epic 1: Project Foundation & Infrastructure Setup
Enable the development team to build and run the full Blinder system locally and in production from day one — backend scaffolding, database schema separation, Docker Compose stack, CI pipeline, and the frontend design system foundation.
**FRs covered:** None (infrastructure enabler — prerequisite for all epics)

### Epic 2: User Authentication & Identity
Users can create an account, sign in with email or social auth (Google, Apple, Facebook), accept policies, verify their age, and delete their account.
**FRs covered:** FR1, FR2, FR3, FR4

### Epic 3: Onboarding & Profile Setup
Users can complete the onboarding quiz, upload a photo through the moderation pipeline, grant location permission, and become fully match-ready; they can also update their profile later.
**FRs covered:** FR5, FR6, FR7, FR8, FR9, FR10, FR11, FR12, FR27, FR28, FR29, FR34, FR47

### Epic 4: Daily Matching & Conversation Entry
Users receive a daily match opportunity, decide to enter, and arrive at their first conversation with starter prompts that dissolve blank-page anxiety.
**FRs covered:** FR13, FR14, FR15, FR17

### Epic 5: Blind Conversation
Users can exchange messages in the blind phase with a distraction-free experience, report or block when needed, and the app handles connectivity loss gracefully.
**FRs covered:** FR16, FR18, FR30, FR31, FR45

### Epic 6: Decision Gate & Reveal
The system detects gate conditions, presents a pressure-free private decision, resolves simultaneous choices, and delivers dignified outcomes — reveal ceremony or anonymized ending.
**FRs covered:** FR19, FR20, FR21, FR22, FR23, FR24, FR25, FR26, FR46, FR50

### Epic 7: Safety Operations & Moderation Admin
Trust-and-safety operators can review flagged media, apply corrective actions, and access a complete audit trail — keeping the platform safe from within the AdminPanel.
**FRs covered:** FR32, FR33

### Epic 8: Push Notifications
Users receive push notifications exclusively for meaningful state transitions (new match, partner reply, gate reached) via direct APNS/FCM, with privacy-preserving payloads and no engagement-bait.
**FRs covered:** FR35, FR36, FR37, FR38, FR39

### Epic 9: Admin, Support & Operations Panel
Internal support staff can investigate conversation issues and communicate outcomes without exposing sensitive decision details; operators can configure market-specific controls.
**FRs covered:** FR40, FR41, FR51, FR52

### Epic 10: Monetization & Conversation Capacity
Users can subscribe to premium access for higher concurrent conversation capacity; the system enforces tier limits fairly.
**FRs covered:** FR48, FR49

### Epic 11: Product Quality Telemetry & Safety Pulse
Product operators can monitor core quality and safety metrics at cohort level; users can share a lightweight fairness pulse after outcomes.
**FRs covered:** FR42, FR43, FR44, FR53

---

## Epic 1: Project Foundation & Infrastructure Setup

Enable the development team to build and run the full Blinder system locally and in production from day one — backend scaffolding, database schema separation, Docker Compose stack, CI pipeline, and the frontend design system foundation.

### Story 1.1: Scaffold ASP.NET Core 10 Three-Project Backend Solution

As a developer,
I want a scaffolded backend solution with IdentityServer, Api, and AdminPanel projects,
So that all backend work has a consistent, boundary-enforcing structure from day one.

**Acceptance Criteria:**

**Given** the backend source directory exists under `backend/src/`
**When** the solution is initialized
**Then** it contains `Blinder.IdentityServer`, `Blinder.Api`, `Blinder.AdminPanel`, `Blinder.SharedKernel`, and `Blinder.Contracts` projects targeting .NET 10
**And** each project compiles cleanly with `dotnet build`
**And** `Blinder.SharedKernel` contains no business logic — only foundational primitives (result types, base exceptions, correlation ID helpers)
**And** `Blinder.Contracts` contains only intentionally shared cross-process contracts
**And** test projects exist under `backend/tests/` for each app (unit + `Blinder.IntegrationTests` + `Blinder.ArchitectureTests`)
**And** all projects have health check endpoints registered (returns 200 on `/health`)
**And** `appsettings.json` and `appsettings.Development.json` are present per app with no secrets in tracked config files

---

### Story 1.2: Docker Compose Production Stack with Traefik, PostgreSQL, and MinIO

As a developer,
I want a Docker Compose stack that runs the full Blinder infrastructure locally and in production,
So that all services are consistently deployable and network-isolated.

**Acceptance Criteria:**

**Given** `docker-compose.yml` exists at the project root
**When** `docker compose up -d` is run
**Then** containers start for: `traefik`, `identityserver`, `api`, `adminpanel`, `postgres` (PostgreSQL 18), `minio`
**And** only Traefik exposes public ports 80 and 443; all other services communicate on an internal Docker network
**And** host-based routing separates `api.<domain>`, `auth.<domain>`, `admin.<domain>`
**And** PostgreSQL and MinIO use persistent named volumes
**And** ASP.NET Core Data Protection keys are persisted to a volume outside the container filesystem
**And** identity signing and encryption keys are persisted and mounted at runtime
**And** all app services (`identityserver`, `api`, `adminpanel`) are stateless containers
**And** a `deploy/hetzner/` directory contains environment-specific compose override and config examples
**And** `docker compose down` followed by `docker compose up -d` restores all services without data loss

---

### Story 1.3: PostgreSQL Schema Separation and EF Core Migrations Pipeline

As a developer,
I want a single PostgreSQL database with enforced `identity.*` and `app.*` schema ownership and separate EF Core migration sets,
So that data boundaries are structurally enforced from the first migration.

**Acceptance Criteria:**

**Given** the PostgreSQL container is running
**When** EF Core migrations are applied
**Then** `identity.*` schema exists and is owned exclusively by `Blinder.IdentityServer`
**And** `app.*` schema exists and is owned exclusively by `Blinder.Api`
**And** separate DB roles with schema-scoped permissions are provisioned (IdentityServer role cannot write to `app.*`; Api role cannot write to `identity.*`)
**And** `Blinder.IdentityServer` has its own `Migrations/` folder that only targets `identity.*`
**And** `Blinder.Api` has its own `Migrations/` folder that only targets `app.*`
**And** `dotnet ef migrations add` and `dotnet ef database update` work independently per project
**And** `Blinder.ArchitectureTests` contains a test that fails if any project references a migration outside its schema boundary

---

### Story 1.4: Scaffold Mobile App with Tamagui Expo Router Starter

As a developer,
I want a scaffolded Expo mobile app using the Tamagui Expo Router starter pruned into a domain-first module structure,
So that all mobile work starts from the correct navigation model and design system foundation.

**Acceptance Criteria:**

**Given** the mobile source directory exists under `mobile/blinder-app/`
**When** initialized with `yarn create tamagui@latest --template expo-router`
**Then** the app runs on iOS and Android via Expo Go and development build without errors
**And** all Tamagui demo/example structure is removed
**And** the app is reorganized into domain-first modules under `src/features/`: `onboarding/`, `waiting/`, `match-entry/`, `conversation/`, `decision-gate/`, `reveal/`, `ending/`, `settings/`
**And** `src/services/` contains `auth/` and `realtime/` stubs
**And** navigation uses Expo Router with a stack-based model — no bottom tab bar at any route
**And** TypeScript strict mode is enabled with zero type errors on `tsc --noEmit`
**And** Yarn 4.4.0+ is used as the package manager
**And** `expo doctor` reports no blocking issues

---

### Story 1.5: Design System Foundation — Warm Dusk Token System

As a developer,
I want the complete Blinder design token system implemented in Tamagui — colours, typography, spacing, border radius, and motion vocabulary,
So that all future screen work is built from tokens with zero hardcoded values.

**Acceptance Criteria:**

**Given** the Tamagui configuration file exists at `mobile/blinder-app/tamagui.config.ts`
**When** the token system is defined
**Then** all colour tokens from the Warm Dusk palette are implemented: `color.bg.base`, `color.bg.surface`, `color.bg.elevated`, `color.primary`, `color.primary.light`, `color.reveal`, `color.accent`, `color.text.primary`, `color.text.secondary`, `color.text.muted`, `color.border`, `color.error`, `color.offline`
**And** `color.reveal` is defined as a named semantic token with a code comment marking it exclusive to the reveal ceremony and Reveal gate button
**And** typography tokens cover all 8 scale levels (`text.display` through `text.button`) using Lato (300/400/700/900 weights)
**And** spacing tokens are defined: `space.xs` (4px) through `space.2xl` (48px)
**And** border radius tokens are defined: `radius.sm` (8px) through `radius.full` (9999px)
**And** a `motion.ts` file documents the motion vocabulary: what animates, at what duration, with what easing curve — distinguishing in-screen animations from navigation-level transitions
**And** `color.text.primary` (#2C1C1A) on `color.bg.base` (#FBF5EE) is verified to meet WCAG AA contrast (≥4.5:1), documented in a comment
**And** the Tamagui token values match the canonical packaged design system one-to-one — `colors_and_type.css` (CSS variables, loaded by HTML prototypes and the live component showcase at `ui_kits/Blinder/index.html`) is the cross-platform source of truth; any drift is resolved in favour of the packaged system
**And** a token showcase screen exists at a dev-only route rendering all tokens visually for design review, mirroring the sections in `ui_kits/Blinder/index.html` so the two showcases can be visually diffed
**And** zero hardcoded colour, font-size, or spacing values exist anywhere in the codebase (enforced by ESLint rule)
**And** this story has a design review gate — a human review of the token showcase screen alongside `ui_kits/Blinder/index.html` is required before any screen implementation stories begin in Epic 3+

---

## Epic 2: User Authentication & Identity

Users can create an account, sign in with email or social auth (Google, Apple, Facebook), accept policies, verify their age, and delete their account — all authentication flows are owned by IdentityServer via OAuth2/OIDC; `Blinder.Api` is a resource server only.

### Story 2.1: IdentityServer OAuth2/OIDC Foundation with OpenIddict

As a developer,
I want a functioning IdentityServer with OpenIddict 7.4.0 that is the sole authority for all authentication, registration, and token issuance,
So that the mobile app and AdminPanel never talk to `Blinder.Api` for identity operations — only to IdentityServer.

**Acceptance Criteria:**

**Given** `Blinder.IdentityServer` is running
**When** a mobile client initiates authentication
**Then** it uses Authorization Code Flow with PKCE against IdentityServer OIDC endpoints — `Blinder.Api` has no registration or login endpoints
**And** OpenIddict 7.4.0 is configured with standard OIDC discovery at `/.well-known/openid-configuration`
**And** the following endpoints are functional: `/connect/authorize`, `/connect/token`, `/connect/userinfo`, `/connect/endsession`, `/connect/introspect`, `/connect/revoke`
**And** a mobile client (`blinder-mobile`) and admin client (`blinder-admin`) are registered in OpenIddict with correct redirect URIs, scopes, and grant types
**And** the mobile client uses Authorization Code + PKCE only — no implicit or password grant
**And** `identity.*` schema tables are managed exclusively by IdentityServer EF Core migrations
**And** HTTPS (TLS 1.2+) is enforced on all IdentityServer endpoints
**And** refresh token rotation is enabled; revoked tokens are rejected on next use
**And** the IdentityServer health check endpoint returns 200

---

### Story 2.2: Local Account Registration and Sign-In via IdentityServer OIDC

As a user,
I want to register and sign in through the IdentityServer OIDC flow,
So that my credentials are managed entirely by the identity authority and never pass through the business API.

**Acceptance Criteria:**

**Given** IdentityServer exposes a registration endpoint under its own host (`auth.<domain>`)
**When** a user submits a valid email and password for registration
**Then** the account is created in `identity.*` by IdentityServer — `Blinder.Api` performs no writes to identity data
**And** password is stored as a salted hash using ASP.NET Core Identity hashing
**And** after registration the user is directed into the OIDC Authorization Code + PKCE flow to receive tokens — there is no separate login call to `Blinder.Api`

**Given** a user initiates sign-in
**When** they complete the OIDC Authorization Code + PKCE flow against IdentityServer
**Then** IdentityServer issues an access token, ID token, and refresh token
**And** the access token contains the user's subject ID and configured API scopes consumable by `Blinder.Api`
**And** failed authentication attempts return standard OIDC error responses with no credential detail leakage
**And** `Blinder.Api` validates incoming access tokens as a resource server — it does not issue or manage tokens itself

---

### Story 2.3: Social Federation via IdentityServer — Google, Apple, and Facebook

As a user,
I want to sign in with Google, Apple, or Facebook through IdentityServer,
So that external identity providers are federated through the single IdentityServer authority — not handled by the mobile app or Api directly.

**Acceptance Criteria:**

**Given** IdentityServer is configured as the federation hub with Google, Apple, and Facebook as external providers
**When** a user selects a social provider on the sign-in screen
**Then** the mobile app initiates an Authorization Code + PKCE flow against IdentityServer — not directly against Google/Apple/Facebook
**And** IdentityServer handles the external provider exchange, creates or links a local `identity.*` account, and returns its own access/ID/refresh tokens to the mobile client
**And** the mobile app only ever holds IdentityServer-issued tokens — external provider tokens never reach the mobile client or `Blinder.Api`
**And** Apple sign-in (SIWA) is configured with `name` and `email` scopes — required for App Store compliance on iOS (**launch blocker**)
**And** if an external email matches an existing local account, IdentityServer prompts to link rather than silently duplicating
**And** external login unlinking triggers an IdentityServer-owned command; `Blinder.Api` requests this via an explicit service call — it never mutates `identity.*` directly

---

### Story 2.4: Age Gate, Policy Acceptance, and GDPR Consent Records

As a user,
I want to confirm my age and accept product policies before entering dating flows,
So that the product meets legal and compliance requirements.

**Acceptance Criteria:**

**Given** a user has completed registration via IdentityServer OIDC
**When** they submit their date of birth for age verification
**Then** users below the legal age threshold are blocked from proceeding to dating flows (NFR10)
**And** the age gate result is stored in `identity.*` with timestamp

**Given** a user proceeds to policy acceptance
**When** they accept the blind-phase rules, moderation boundaries, and anonymized outcome policies
**Then** the acceptance event is recorded in `identity.*` with policy version, timestamp, and user ID (NFR9 — consent recordability)
**And** users who have not accepted the current policy version are blocked from matching flows on subsequent sessions
**And** a GET endpoint returns the current policy acceptance status for the authenticated user

---

### Story 2.5: Account Deletion — GDPR Right to Erasure

As a user,
I want to delete my account from within the app,
So that my data is removed in compliance with GDPR right to erasure.

**Acceptance Criteria:**

**Given** an authenticated user submits a DELETE account request
**When** the request is processed
**Then** the identity account is deleted per the configured retention policy
**And** all active sessions and refresh tokens for the user are revoked immediately via IdentityServer
**And** a deletion record is retained for legal audit purposes (deletion timestamp, pseudonymised user ID) per GDPR retention policy
**And** the deletion is irreversible — re-registration with the same email creates a new identity
**And** `Blinder.Api` triggers the deletion via an explicit IdentityServer-owned command — it does not mutate `identity.*` directly
**And** the API returns 204 on success; 409 if deletion is already in progress

---

### Story 2.6: Welcome, Registration, and Sign-In Screens (OIDC Flow)

As a new user,
I want to register or sign in through the IdentityServer OIDC flow from a welcoming entry screen,
So that authentication is handled securely without the app ever handling raw credentials beyond the OIDC handshake.

**Acceptance Criteria:**

**Given** the app is launched for the first time
**When** the welcome screen is shown
**Then** it presents the product value proposition and two CTAs: "Create account" and "Sign in"
**And** both CTAs initiate an Authorization Code + PKCE flow against IdentityServer — the app does not POST credentials to `Blinder.Api`
**And** the app uses `expo-auth-session` or equivalent to manage the OIDC authorization code exchange with IdentityServer
**And** on successful token receipt, the access token and refresh token are stored securely (Expo SecureStore / iOS Keychain / Android Keystore)
**And** all screens use only Warm Dusk design tokens — zero hardcoded values
**And** error responses from IdentityServer OIDC endpoints are translated into warm, non-technical user-facing copy

---

### Story 2.7: Social Sign-In Buttons via IdentityServer Federation

As a user,
I want to tap a social sign-in button and be authenticated through IdentityServer's federation,
So that the app always receives IdentityServer-issued tokens regardless of the external provider used.

**Acceptance Criteria:**

**Given** the registration or sign-in screen is shown
**When** the user taps a social provider button (Google, Apple, Facebook)
**Then** the app initiates an Authorization Code + PKCE flow against IdentityServer — not directly against Google, Apple, or Facebook
**And** IdentityServer handles the external provider exchange internally and returns its own tokens to the app
**And** Apple sign-in button on iOS uses Apple-required visual treatment (Sign in with Apple guidelines) — App Store **required** capability and launch blocker
**And** on successful flow completion the app receives and stores IdentityServer-issued tokens (not external provider tokens)
**And** if the OIDC flow is cancelled or fails the user is returned to the sign-in screen with a non-alarming error message

---

### Story 2.8: Age Gate and Policy Acceptance Screens

As a new user,
I want to confirm my age and accept product policies,
So that I understand the blind-phase rules before entering dating flows.

**Acceptance Criteria:**

**Given** a newly registered user proceeds past account creation
**When** the age gate screen is shown
**Then** the user can enter their date of birth
**And** users below the legal age threshold see a clear, respectful blocked state explaining they cannot proceed
**And** users above the threshold proceed to policy acceptance

**Given** the policy acceptance screen is shown
**When** the user reviews and accepts
**Then** the acceptance is submitted to the backend and confirmed before the user proceeds
**And** policy copy is readable with correct font scale and line height from the token system
**And** the "Accept" CTA is a primary button; no dark patterns are present

---

### Story 2.9: Account Deletion Flow (Frontend)

As a user,
I want to delete my account from within the app,
So that I have a clear, GDPR-compliant path to remove my data.

**Acceptance Criteria:**

**Given** the user navigates to account deletion within ProfileSheet settings
**When** they initiate deletion
**Then** a Dialog confirmation screen is shown with consequences clearly stated (irreversible, data removed)
**And** the confirmation requires an explicit tap on a destructive-styled button ("Delete my account") — not the default action
**And** on confirmed deletion all local session state is cleared and the user is returned to the welcome screen
**And** the flow is reachable in no more than 3 taps from the profile settings entry point (App Store compliance requirement)

---

## Epic 3: Onboarding & Profile Setup

Users can complete the onboarding quiz, upload a photo through the moderation pipeline, grant location permission, and become fully match-ready. They can also update their profile later.

### Story 3.1: Onboarding Quiz API and Conversation Starter Prompt Generation

As a user,
I want to submit my onboarding quiz answers and receive generated conversation starter prompts,
So that my first conversations have relevant, personalised icebreakers.

**Acceptance Criteria:**

**Given** an authenticated user POSTs their quiz answers to `/api/v1/onboarding/quiz`
**When** all required questions are answered
**Then** answers are persisted in `app.*` linked to the user's subject ID
**And** the system generates 2–3 conversation starter prompts from the quiz answers and stores them against the user profile
**And** prompts are retrievable via GET `/api/v1/onboarding/prompts`
**And** a user who has not completed the quiz cannot advance to the matching-eligible state (FR11)
**And** a user can resubmit quiz answers to update their prompts (FR12); updated prompts take effect from the next match cycle
**And** the endpoint returns 400 with Problem Details if required questions are missing

---

### Story 3.2: Location Data Ingestion

As a user,
I want to provide my location so the matching service can find nearby matches,
So that I receive geographically relevant daily matches.

**Acceptance Criteria:**

**Given** an authenticated user POSTs location coordinates to `/api/v1/onboarding/location`
**When** valid coordinates are provided
**Then** the location is stored in `app.*` linked to the user's subject ID
**And** the user is not blocked from completing onboarding if they decline location permission — but location-based matching requires it
**And** location can be updated at any time via the same endpoint (FR12)
**And** stored coordinates are never exposed to other users at any point

---

### Story 3.3: Photo Upload and Automated Moderation Pipeline

As a user,
I want to upload a profile photo that is evaluated before it enters any product flow,
So that only safe, appropriate photos become available for post-reveal display.

**Acceptance Criteria:**

**Given** an authenticated user POSTs a photo to `/api/v1/onboarding/photo`
**When** the upload is received
**Then** the photo is stored in MinIO in a private, non-public bucket under the user's ID
**And** the moderation pipeline is triggered immediately — the photo is not marked available until moderation completes (FR27, NFR7)
**And** the automated scan returns one of three outcomes: `approved`, `rejected`, `needs_review`
**And** `approved` photos are marked available in post-reveal context only — never accessible during the blind phase (NFR7)
**And** `rejected` photos trigger a remediation response with clear guidance (FR34); the user is notified and must re-upload
**And** `needs_review` photos are queued for manual review (FR29); the user receives a pending status
**And** if the moderation provider is unavailable the photo defaults to `needs_review` — never silently approved (NFR23)
**And** the upload endpoint enforces rate limiting

---

### Story 3.4: Manual Moderation Review Queue

As a trust-and-safety operator,
I want to review photos flagged for manual review and approve or reject them,
So that uncertain cases are resolved by a human before photos enter the product.

**Acceptance Criteria:**

**Given** a photo has been flagged with `needs_review` status
**When** an authenticated T&S operator accesses `/api/v1/admin/moderation/queue`
**Then** pending photos are returned with metadata (upload timestamp, user ID, scan confidence score)
**And** the operator can submit an `approved` or `rejected` decision via POST `/api/v1/admin/moderation/{photoId}/decision`
**And** the decision is audit-logged with operator ID, timestamp, and outcome (NFR25)
**And** on approval the photo becomes available in the post-reveal context; on rejection the user is notified with remediation guidance (FR34)
**And** the endpoint is accessible only to accounts with the `moderation` role (NFR8)

---

### Story 3.5: Profile Update API

As a user,
I want to update my profile basics — quiz answers, photo, location — after initial onboarding,
So that my match-relevant information stays current.

**Acceptance Criteria:**

**Given** an authenticated user submits a PATCH to `/api/v1/profile`
**When** valid fields are provided
**Then** quiz answers and generated prompts are updated in `app.*`
**And** a new photo upload triggers the full moderation pipeline again — the previous approved photo remains active until the new one is approved
**And** location coordinates are updated immediately
**And** incomplete updates with missing required fields return 400 with Problem Details
**And** GET `/api/v1/profile` returns the current profile state including moderation status of the active photo

---

### Story 3.6: Onboarding Quiz Flow (Frontend)

As a new user,
I want to complete the onboarding quiz one question at a time,
So that I can share conversation-relevant traits without feeling overwhelmed.

**Acceptance Criteria:**

**Given** the user reaches the onboarding quiz after age gate and policy acceptance
**When** the quiz screen is shown
**Then** exactly one question is displayed per screen with full-width tappable option cards (UX-DR27)
**And** a progress bar is shown at the top with no percentage label
**And** the user cannot advance without selecting an answer (FR11)
**And** tapping an option card visually confirms selection and enables a "Continue" CTA
**And** the final quiz screen submits answers to the backend and transitions to photo upload on success
**And** all screens use only Warm Dusk tokens — zero hardcoded values

---

### Story 3.7: Photo Upload, Moderation Pending, and Remediation Screens (Frontend)

As a new user,
I want to upload my profile photo and receive clear feedback on its moderation status,
So that I understand what happens to my photo and what to do if it is not accepted.

**Acceptance Criteria:**

**Given** the user reaches the photo upload screen
**When** they select and upload a photo
**Then** the photo is sent to the backend and a moderation pending state is shown immediately
**And** the pending state shows a warm photo placeholder with copy "Your photo is being reviewed" — never a blank or error state (UX-DR29)
**And** on `approved` the user is advanced to the next onboarding step
**And** on `rejected` a remediation screen is shown with clear, non-alarming guidance and a re-upload CTA (FR34)
**And** on `needs_review` the pending state persists with a calm holding message; the user is not blocked from completing remaining onboarding steps
**And** all screens use only Warm Dusk tokens

---

### Story 3.8: Contextual Location and Notification Permission Flows (Frontend)

As a new user,
I want to grant location and notification permissions at the right moment with a clear explanation,
So that I understand why each permission is needed before the system prompt appears.

**Acceptance Criteria:**

**Given** the user has completed photo upload
**When** the location permission screen is shown
**Then** a contextual rationale screen is displayed first explaining why location is needed for matching — before the OS system prompt (FR47, UX-DR28)
**And** the CTA "Enable location" triggers the OS prompt; "Not now" allows the user to proceed without granting it
**And** notification permission is not requested during onboarding — it is deferred until after the user receives their first match (UX-DR28)
**And** permission prompts are always shown individually, never in bulk
**And** all screens use only Warm Dusk tokens

---

## Epic 4: Daily Matching & Conversation Entry

Users receive a daily match opportunity, decide to enter, and arrive at their first conversation with starter prompts that dissolve blank-page anxiety.

### Story 4.1: Matching Service and Daily Match Delivery

As a user,
I want to receive a new match opportunity once per daily cycle,
So that I have a focused, quality-first matching experience rather than an overwhelming feed.

**Acceptance Criteria:**

**Given** a match-eligible user (onboarding complete, photo approved, location provided)
**When** the daily match cycle runs
**Then** a match is generated and delivered to the user stored in `app.*` with status `pending_entry`
**And** GET `/api/v1/matches/current` returns the current pending match (or empty if none)
**And** match availability check completes in ≤2 seconds for 95% of requests (NFR4)
**And** a user has at most one active match at a time within the free tier
**And** the matching logic uses location proximity as a base filter
**And** a notification trigger event is emitted when the match is created (consumed by Epic 8)
**And** if no eligible counterpart is available the response returns an empty match state with a `next_cycle_at` timestamp — never an error

---

### Story 4.2: Match Accept / Pass and Conversation Window Activation

As a user,
I want to accept or pass on a presented match, and for the conversation to open only once both users have sent a first message,
So that both sides enter the conversation willingly and actively.

**Acceptance Criteria:**

**Given** a user has a match in `pending_entry` status
**When** they POST to `/api/v1/matches/{matchId}/accept`
**Then** the match transitions to `accepted` on their side
**And** when the counterpart also accepts the match transitions to `conversation_pending`

**Given** both users have accepted the match
**When** both users have each sent their first message (FR15)
**Then** the conversation window transitions to `active` and the conversation clock and message counter begin
**And** if only one user sends a first message the conversation remains in `conversation_pending` until the counterpart also sends
**And** a user who passes via POST `/api/v1/matches/{matchId}/pass` has the match closed; the counterpart is returned to matching eligibility without blame attribution
**And** all state transitions are server-authoritative and audit-logged (NFR24)

---

### Story 4.3: Starter Prompts Retrieval for Conversation Entry

As a user,
I want to retrieve my personalised conversation starter prompts when entering a new conversation,
So that I have relevant icebreakers ready at the moment I need them most.

**Acceptance Criteria:**

**Given** a user has an active or pending conversation
**When** they GET `/api/v1/conversations/{conversationId}/starters`
**Then** 2–3 starter prompts generated from the user's onboarding quiz are returned (FR17)
**And** prompts are specific to the authenticated user — the counterpart's prompts are never exposed
**And** if quiz data is unavailable a set of safe generic prompts is returned — never an empty response
**And** the endpoint returns 403 if the requesting user is not a participant in the conversation

---

### Story 4.4: Waiting State Screen (Frontend)

As a user,
I want to see a calm, purposeful screen when I have no active conversation,
So that waiting feels like calm anticipation rather than an empty void.

**Acceptance Criteria:**

**Given** the user has no active match or conversation
**When** the home screen is shown
**Then** the `WaitingState` component is displayed with a temporal anchor ("Your match arrives daily") — never a blank screen or spinner (UX-DR14)
**And** the screen shows one of three states: `no_match`, `match_arriving`, `match_ready`
**And** no competing UI elements are present — single-focus screen with no tab bar (UX-DR20)
**And** the profile avatar is visible in the top-right corner as a visually interactive tap target
**And** all elements use only Warm Dusk tokens

---

### Story 4.5: Match Entry Screen (Frontend)

As a user,
I want to see a warm, dedicated screen when a match is available,
So that the moment of entering a conversation feels like an invitation, not a notification.

**Acceptance Criteria:**

**Given** a match is available for the user
**When** the match entry screen is shown
**Then** the `MatchEntryCard` component is displayed with `BlindAvatar` (lg variant), "Someone is waiting to talk with you", and a single primary CTA "Start conversation" (UX-DR15)
**And** the card has an animated arrival state (`entering`)
**And** the notification permission prompt is shown contextually on this screen if not yet granted (UX-DR28)
**And** the user can pass on the match via a low-prominence text link — not a competing button
**And** tapping "Start conversation" navigates to the Starter Screen
**And** all elements use only Warm Dusk tokens

---

### Story 4.6: Starter Screen and First Message Send (Frontend)

As a user,
I want to see personalised conversation starter prompts before sending my first message,
So that blank-page anxiety is dissolved and my first send feels natural and low-stakes.

**Acceptance Criteria:**

**Given** the user has entered a new conversation
**When** the Starter Screen is shown
**Then** 2–3 `StarterCard` components are displayed with prompts from the backend (UX-DR13)
**And** the `BlindAvatar` (sm) and a placeholder name are shown in the conversation header
**And** tapping a StarterCard auto-populates the message input and focuses it with the prompt text (StarterCard `selected` state)
**And** the user can also type their own message freely
**And** sending the first message transitions the screen to the active `ConversationScreen`
**And** the Starter Screen is shown only before the first message is sent — subsequent opens go directly to the conversation
**And** all elements use only Warm Dusk tokens


---

## Epic 5: Blind Conversation

Users can exchange messages in the blind phase with a distraction-free experience, report or block when needed, and the app handles connectivity loss gracefully.

### Story 5.1: Messaging API — Send, Receive, and Conversation History

As a user,
I want to send and receive messages in the blind phase with reliable delivery,
So that my conversation feels responsive and nothing I send is silently lost.

**Acceptance Criteria:**

**Given** an authenticated user with an active conversation
**When** they POST a message to `/api/v1/conversations/{conversationId}/messages`
**Then** the message is persisted in `app.*` with sender ID, timestamp, and conversation ID
**And** 95% of message send acknowledgements are returned in ≤2.5 seconds (NFR2)
**And** the endpoint is idempotent — submitting the same client-generated message ID twice returns the existing message, not a duplicate (NFR12)
**And** GET `/api/v1/conversations/{conversationId}/messages` returns paginated message history for participants only
**And** the endpoint returns 403 if the requesting user is not a participant
**And** messages are stored in `app.*` — media attachments are out of scope for the blind phase
**And** all message events are audit-logged with correlation IDs (NFR24)

---

### Story 5.2: SignalR Real-Time Conversation Updates

As a user,
I want to receive new messages in near-real-time without polling,
So that the conversation feels live and responsive.

**Acceptance Criteria:**

**Given** a user is connected to the SignalR hub at `/hubs/conversation`
**When** the counterpart sends a message
**Then** the SignalR hub pushes a `MessageReceived` event to the recipient's connection
**And** SignalR is not the sole source of truth — on reconnect or restart GET `/api/v1/conversations/{conversationId}/messages` returns the complete authoritative history
**And** a notification trigger event is emitted on new message (consumed by Epic 8 for push when recipient is offline)
**And** the SignalR hub requires a valid bearer token; unauthenticated connections are rejected
**And** hub connection failures degrade gracefully — the client falls back to polling without data loss

---

### Story 5.3: Conversation Limits Enforcement

As the system,
I want to enforce the maximum number of active concurrent conversations per user tier,
So that free-tier users are limited as configured and premium users get expanded capacity.

**Acceptance Criteria:**

**Given** a user attempts to accept a new match
**When** they already have the maximum allowed active conversations for their tier
**Then** the accept endpoint returns 409 with a Problem Details response indicating the capacity limit
**And** the limit threshold is configurable per tier in `app.*` settings (free tier default: 1 active conversation)
**And** a completed, expired, or abandoned conversation releases the slot immediately
**And** premium tier limits are enforced using subscription status stored in `app.*` (populated by Epic 10)

---

### Story 5.4: Report and Block Endpoints

As a user,
I want to report harmful behaviour or block another user,
So that I can protect myself quickly without high friction.

**Acceptance Criteria:**

**Given** an authenticated user POSTs to `/api/v1/conversations/{conversationId}/report`
**When** the request is processed
**Then** a report record is created in `app.*` with reporter ID, reported user ID, conversation ID, reason, and timestamp
**And** the report is audit-logged (NFR24, NFR25)
**And** the reported user is not notified of the report

**Given** an authenticated user POSTs to `/api/v1/users/{userId}/block`
**When** the request is processed
**Then** the block record is stored in `app.*`
**And** any active conversation between the users is terminated immediately with a neutral closure state
**And** the blocked user cannot be matched with the blocking user again
**And** both endpoints enforce rate limiting to prevent abuse

---

### Story 5.5: Blind Conversation Screen (Frontend)

As a user,
I want a clean, distraction-free conversation screen focused entirely on the message exchange,
So that I can be fully absorbed in the conversation without UI competing for attention.

**Acceptance Criteria:**

**Given** the user has an active conversation
**When** the Conversation Screen is shown
**Then** the `ConversationBubble` component renders sent messages (primary colour, right-aligned) and received messages (surface colour, left-aligned) with timestamps (UX-DR7)
**And** the `BlindAvatar` (sm) is shown in the conversation header — warm and dignified, never resembling a broken image (UX-DR6)
**And** navigation chrome is hidden — full-bleed conversation screen (UX-DR20)
**And** the message input is single-line, expands to 4 lines max, and the send button activates only when content is present (opacity change, not disabled state)
**And** optimistic UI is used — sent bubbles appear immediately in `sending` state, confirmed on server acknowledgement, shown in `failed` state with retry action if delivery fails
**And** the screen recovers correctly after app restart — message history loads from the API
**And** no typing indicators are shown; all elements use only Warm Dusk tokens

---

### Story 5.6: Report and Block UI (Frontend)

As a user,
I want to report or block someone from the conversation header in one tap,
So that I can exit an unsafe or unwanted interaction quickly and without friction.

**Acceptance Criteria:**

**Given** the user is on the Conversation Screen
**When** they tap the ⋯ menu in the conversation header
**Then** "Report" and "Block" options are accessible — never more than one tap from the conversation (UX-DR32)
**And** tapping "Block" shows a Dialog confirmation before executing
**And** tapping "Report" goes to a brief reason selection screen
**And** after confirming block the conversation ends immediately and the user sees a neutral closure screen
**And** after submitting a report a dedicated confirmation screen is shown ("Our team will review this") — not a toast (UX-DR30)
**And** both actions navigate the user to the Waiting Screen
**And** copy never assigns blame or implies the user did something wrong (UX-DR33)

---

### Story 5.7: Offline Blocked State (Frontend)

As a user,
I want the app to clearly communicate when I am offline and block write actions,
So that I never lose a message or submit an action that cannot be completed.

**Acceptance Criteria:**

**Given** the user's device loses connectivity
**When** the connection is lost
**Then** the `OfflineBlocker` component appears as a full-screen overlay within ≤2 seconds (NFR14, UX-DR12)
**And** the overlay is calm and non-alarming — no retry button, reconnection is automatic
**And** all write actions (send message, gate decision, match accept) are blocked while offline
**And** when connectivity is restored the overlay fades out (`restored` state) and the app resumes without data loss
**And** the offline state is communicated in plain language accessible to screen readers (NFR20)

---

## Epic 6: Decision Gate & Reveal

**Goal:** Implement the dual-trigger decision gate, private simultaneous decisioning, reveal ceremony, and all outcome branches — the core differentiating mechanic of Blinder.

**FRs covered:** FR19, FR20, FR21, FR22, FR23, FR24, FR25, FR26, FR46, FR50

---

### Story 6.1 — Backend: Decision Gate State Machine — Dual-Trigger Detection

**As a** backend system,
**I want** to detect when a conversation crosses the dual-trigger threshold (time-floor AND message-count),
**so that** the gate is unlocked at the right moment without relying on client signals.

**Acceptance Criteria:**

- AC1: A background service or domain event evaluates each active conversation against both triggers: time-floor (configurable, default 24h from first message) AND message-threshold (configurable, default 10 messages).
- AC2: The gate unlocks only when both conditions are simultaneously met — neither condition alone is sufficient.
- AC3: Gate unlock is idempotent — if evaluated multiple times, the conversation transitions to `gate_open` state exactly once (NFR12).
- AC4: The trigger reason (time_floor_met, message_threshold_met, both) is stored on the conversation record and retrievable via GET `/api/conversations/{id}`.
- AC5: A GET `/api/conversations/{id}` response includes `gateState` (locked | open | submitted | resolved) and `triggerReason` (null when locked; populated on unlock).
- AC6: Unit tests cover: both conditions met → unlock; only time met → no unlock; only message count met → no unlock; repeated evaluation → no double-unlock.
- AC7: Integration test: seed conversation with exact threshold values → verify single state transition with correct triggerReason.

**Technical Notes:**
- FR19 (time-floor trigger), FR20 (message-count trigger), FR46 (configurable thresholds).
- Gate state lives in `app.conversations` schema, owned by Blinder.Api.
- No client-side trigger — server-side only evaluation.

---

### Story 6.2 — Backend: Private Simultaneous Decision Submission and Resolution

**As a** user,
**I want** to submit my gate decision privately,
**so that** my choice is never revealed to the other person unless the outcome is mutual reveal.

**Acceptance Criteria:**

- AC1: `POST /api/conversations/{id}/gate-decision` accepts `{ decision: "reveal" | "continue" | "abandon" }` and requires the conversation to be in `gate_open` state; returns 409 if already submitted by this user.
- AC2: Decisions are stored privately — neither user can query the other's decision before resolution.
- AC3: Resolution triggers when both users have submitted decisions; the full branch matrix is enforced:
  - both `reveal` → `mutual_reveal`
  - one `reveal` + one `continue` → `continued`
  - both `continue` → `continued`
  - either `abandon` → `ended_anonymized`
  - timeout (configurable, default 72h after gate open) → `expired`
- AC4: Resolution is atomic and idempotent (NFR12 exactly-once gate action). Concurrent submissions from both users do not produce double-resolution.
- AC5: GET `/api/conversations/{id}` returns `gateState: "resolved"` and `outcome` (mutual_reveal | continued | ended_anonymized | expired) after resolution; no decision details of the other party are ever exposed.
- AC6: A SignalR `GateOutcome` event is broadcast to both participants on resolution with `{ outcome, conversationId }`.
- AC7: Unit tests cover all 5 outcome branches. Integration test: two concurrent decision submissions → single resolution with correct outcome.

**Technical Notes:**
- FR21 (private decisioning), FR22 (simultaneous resolution), FR24 (non-mutual handling), FR25 (abandon → anonymized), FR50 (outcome states).
- NFR3: gate action complete in ≤3s end-to-end.
- NFR12: exactly-once resolution — use optimistic concurrency or DB transaction with row lock.

---

### Story 6.3 — Backend: Mutual Reveal — Profile Access and Conversation History Persistence

**As a** system,
**I want** to unlock full profile access (including photos) only on mutual reveal outcome,
**so that** photos are never accessible before both parties consent (NFR7).

**Acceptance Criteria:**

- AC1: On `mutual_reveal` outcome, server grants read access to both users' photos stored in MinIO private bucket — access is scoped to the matched pair only.
- AC2: Photo URLs returned from GET `/api/users/{id}/profile` (when requesting mutual match) are signed MinIO pre-signed URLs with configurable TTL (default 1h); URL generation fails with 403 if conversation is not in `mutual_reveal` state.
- AC3: Full conversation history remains accessible after reveal — no message pruning on state transition.
- AC4: GET `/api/conversations/{id}/history` returns all messages regardless of gate state (messages are not deleted on resolution).
- AC5: Attempting to access another user's photo without `mutual_reveal` state returns 403 with RFC 9457 Problem Details body.
- AC6: Integration tests: mutual_reveal state → photo URL generation succeeds; non-mutual state → 403; conversation history intact post-reveal.

**Technical Notes:**
- FR23 (mutual reveal profile access).
- NFR7: zero pre-reveal photo access — enforced server-side, not relying on client.
- MinIO private bucket; no public URLs ever generated for user photos.

---

### Story 6.4 — Backend: Inactive Conversation Expiry and Neutral State Cleanup

**As a** system,
**I want** to expire conversations that stall at the gate without resolution,
**so that** stale gate-open conversations are cleaned up gracefully without blaming either party.

**Acceptance Criteria:**

- AC1: A background job scans conversations in `gate_open` state older than the configurable expiry threshold (default 72h after gate open) and transitions them to `expired`.
- AC2: Expiry is neutral — no information about whether either party submitted a decision is exposed to either user.
- AC3: Expired conversations surface the copy "This conversation has ended" (neutral, non-judgmental) when fetched via GET `/api/conversations/{id}`.
- AC4: Expiry job is idempotent — running multiple times on the same conversation does not change state after first expiry.
- AC5: A SignalR `ConversationExpired` event is broadcast to active participants when expiry fires.
- AC6: Integration test: seed gate_open conversation past threshold → verify expired state + neutral copy + SignalR event emitted.

**Technical Notes:**
- FR26 (inactive expiry), FR50 (outcome states).
- Copy string "This conversation has ended" is the canonical neutral ending text — do not vary wording.

---

### Story 6.5 — Frontend: Full-Screen Decision Gate

**As a** user,
**I want** a full-screen decision gate UI when the conversation gate opens,
**so that** I can make my choice deliberately without feeling rushed.

**Acceptance Criteria:**

- AC1: When `gateState === "open"`, the ConversationScreen surfaces the full-screen gate overlay using `GateOptionCard` components for each of the three choices: Reveal, Continue, Abandon.
- AC2: All three `GateOptionCard` components are equal weight — same size, same visual prominence. No option is visually preferred over another (NFR28 — no urgency UI).
- AC3: The Reveal option uses `color.reveal` (`#D4A85A`) token exclusively. Continue and Abandon use standard Warm Dusk tokens. No countdown timer, progress indicator, or urgency copy is shown (NFR28).
- AC4: Selecting an option submits immediately — **no confirmation dialog**. The design system reserves confirmation dialogs for irreversible destructive actions only (account deletion, block); the gate decision is private, reversible in effect (Continue keeps the conversation), and must feel calm and unforced. The pressed option transitions to a brief `submitted` state for all three cards (UX-DR8) while the request is in flight.
- AC5: On submission, the app calls `POST /api/conversations/{id}/gate-decision` and transitions to the ResolutionWait screen (Story 6.6).
- AC6: If the user has already submitted (API returns 409), the screen shows the ResolutionWait screen directly without re-prompting.
- AC7: TypeScript strict mode; all props typed; no `any`.

**Technical Notes:**
- FR21 (private decisioning), FR46 (gate UX).
- `GateOptionCard` component from UX spec.
- NFR28: no urgency UI — no countdown, no "decide before X" copy.
- `color.reveal` (`#D4A85A`) must not be used for any purpose other than the Reveal option.

---

### Story 6.6 — Frontend: Resolution Wait Screen

**As a** user,
**I want** to see a waiting state after submitting my gate decision,
**so that** I understand the system is waiting for the other person without anxiety-inducing UI.

**Acceptance Criteria:**

- AC1: After decision submission, the app navigates to the ResolutionWait screen using the `ResolutionWait` component from the UX spec.
- AC2: The screen shows a calm, non-anxious waiting state — no countdown, no "they haven't decided yet" copy, no urgency language (NFR28).
- AC3: The app subscribes to the SignalR `GateOutcome` event; on receipt, it navigates to the appropriate screen:
  - `mutual_reveal` → Reveal Ceremony screen (Story 6.7)
  - `continued` → ConversationScreen (normal chat resumed)
  - `ended_anonymized` → Outcome screen (Story 6.8)
  - `expired` → Outcome screen (Story 6.8)
- AC4: Fallback polling: if SignalR is disconnected, the app polls GET `/api/conversations/{id}` every 15s to check for resolution. On reconnect, polling stops.
- AC5: The `OfflineBlocker` overlay is shown if network is unavailable (NFR14 — offline state detected within ≤2s).
- AC6: TypeScript strict mode; all props typed; no `any`.

**Technical Notes:**
- FR22 (simultaneous resolution UX).
- `ResolutionWait` component from UX spec.
- SignalR primary; REST polling fallback pattern consistent with Epic 3.

---

### Story 6.7 — Frontend: Reveal Ceremony Screen

**As a** user,
**I want** a meaningful reveal animation when a mutual reveal outcome occurs,
**so that** seeing the other person's photo for the first time feels special and intentional.

**Acceptance Criteria:**

- AC1: On `mutual_reveal` outcome, the app navigates to the Reveal Ceremony screen using the `RevealTransition` component cycling through states: `waiting → animating → revealed`.
- AC2: The `RevealTransition` animation uses `color.reveal` (`#D4A85A`) as the accent color. The animation respects `prefers-reduced-motion` — if set, the transition skips animation and jumps directly to `revealed` state.
- AC3: The `revealed` state renders the `RevealedProfileView` component showing the other person's full profile (photo + name + details).
- AC4: The screen includes a prominent CTA to view the full conversation history (GET `/api/conversations/{id}/history`).
- AC5: A back gesture or hardware back button on this screen is disabled — users cannot navigate back to the waiting screen.
- AC6: TypeScript strict mode; all props typed; no `any`.

**Technical Notes:**
- FR23 (reveal ceremony UX).
- `RevealTransition` and `RevealedProfileView` components from UX spec.
- `color.reveal` exclusive to reveal; must not appear in any other context on this screen.
- `prefers-reduced-motion` via React Native `AccessibilityInfo.isReduceMotionEnabled()`.

---

### Story 6.8 — Frontend: Non-Mutual Ending and Outcome Screen

**As a** user,
**I want** a respectful, neutral outcome screen when the gate does not result in mutual reveal,
**so that** a non-mutual ending feels dignified and not shame-inducing.

**Acceptance Criteria:**

- AC1: For `ended_anonymized` or `expired` outcomes (non-reveal terminal endings), the app navigates to the `OutcomeScreen` component using the acceptance variant styling.
- AC2: The screen uses only neutral, non-blaming vocabulary — per UX vocabulary rules, no copy that implies rejection, failure, or suggests who chose what. Copy example: "This conversation has reached its conclusion."
- AC3: No back navigation is possible from the OutcomeScreen — the back gesture and hardware back button are both disabled.
- AC4: For `continued` outcome, the OutcomeScreen is NOT shown — the app returns directly to ConversationScreen (normal chat resumes). This screen is only for terminal endings.
- AC5: The OutcomeScreen provides a forward CTA ("Find my next match") that returns to the Waiting screen (`WaitingState`) plus a "Take a break" text-link — per UX-DR11 anatomy. Copy is non-attributing and forward-looking; never "they didn't respond", "match expired", or "declined".
- AC6: TypeScript strict mode; all props typed; no `any`.

**Technical Notes:**
- FR24 (non-mutual handling), FR25 (abandon → anonymized), FR50 (outcome states).
- `OutcomeScreen` component from UX spec.
- Vocabulary rules: no "rejected", "unmatched", "ghosted" — neutral language only.
- No `color.reveal` on this screen.

---

## Epic 7: Safety Operations & Moderation Admin

**Goal:** Give users the ability to report and block other users, and give admins the tools to action moderation reports — keeping Blinder safe while preserving user dignity.

**FRs covered:** FR32, FR33

---

### Story 7.1 — Backend: User Report and Block API

**As a** user,
**I want** to report or block another user,
**so that** I can protect myself from harmful interactions.

**Acceptance Criteria:**

- AC1: `POST /api/users/{id}/report` accepts `{ reason: string, details?: string }` and creates a moderation report record in `app.*` schema; returns 201 with report ID.
- AC2: `POST /api/users/{id}/block` creates a block record; blocked users can no longer send messages to the blocking user and cannot appear in the blocker's Discover feed.
- AC3: Both endpoints are idempotent on repeated submission (duplicate report creates new record; duplicate block returns 200 without creating duplicate).
- AC4: A GET `/api/moderation/reports` admin endpoint (requires admin role claim from IdentityServer) returns paginated list of open reports with `{ reportId, reporterId, reportedUserId, reason, details, status, createdAt }`.
- AC5: `PATCH /api/moderation/reports/{id}` accepts `{ status: "reviewed" | "actioned" | "dismissed", adminNotes?: string }` and updates the report.
- AC6: Rate limiting: max 5 reports per user per hour (NFR rate limiting).
- AC7: Unit tests cover: report creation, block creation, duplicate block idempotency, admin list pagination. Integration test: report → admin list → patch status.

**Technical Notes:**
- FR32 (report/block), FR33 (moderation admin tools).
- Admin role claim must originate from IdentityServer — Blinder.Api validates the claim from the JWT; no admin auth logic in Api itself.
- `app.moderation_reports` and `app.user_blocks` tables in `app.*` schema.

---

### Story 7.2 — Backend: Content Moderation — Message Flagging

**As a** system,
**I want** to automatically flag messages that match predefined patterns,
**so that** moderators have signal without requiring a user report.

**Acceptance Criteria:**

- AC1: Incoming messages (POST `/api/conversations/{id}/messages`) are evaluated against a configurable blocklist (stored in DB, admin-manageable); matched messages are stored with `flagged: true` and a flag reason.
- AC2: Flagged messages are still delivered (not blocked) but appear in the admin moderation queue.
- AC3: A GET `/api/moderation/flagged-messages` admin endpoint returns paginated flagged messages with `{ messageId, conversationId, senderId, content, flagReason, flaggedAt }`.
- AC4: `PATCH /api/moderation/flagged-messages/{id}` accepts `{ action: "clear" | "warn_user" | "suspend_user" }` and applies the action.
- AC5: Blocklist entries are manageable via `POST/DELETE /api/moderation/blocklist` (admin only).
- AC6: Unit tests cover: message flagging on match, clean message passes through, admin list, patch action.

**Technical Notes:**
- FR33 (moderation tools).
- Blocklist evaluation is synchronous on message ingestion — keep it fast (simple string/regex match, not ML).
- `app.message_flags` and `app.moderation_blocklist` tables.

---

### Story 7.3 — Frontend: Report and Block User Flow

**As a** user,
**I want** to report or block another user from within the app,
**so that** I can take protective action without leaving the conversation.

**Acceptance Criteria:**

- AC1: A "Report / Block" option is accessible from the conversation header menu (three-dot menu or equivalent) on the ConversationScreen.
- AC2: Selecting "Report" opens a bottom sheet with a list of reason options (harassment, inappropriate content, spam, other) plus an optional free-text details field.
- AC3: On submission, the app calls `POST /api/users/{id}/report` and navigates to a dedicated **ReportConfirmationScreen** (never a toast — UX-DR30 prohibits toasts for emotionally significant outcomes). The screen uses `OutcomeScreen` layout with calm headline ("Thank you for letting us know"), short compassionate body, and a single forward CTA back to the Waiting screen.
- AC4: Selecting "Block" shows a confirmation dialog ("Block this person? They won't be able to contact you.") before calling `POST /api/users/{id}/block`.
- AC5: After a successful block, the conversation is removed from the active conversations list and the user is navigated to the Waiting screen (`WaitingState`).
- AC6: All Warm Dusk tokens used (see `colors_and_type.css` + `ui_kits/Blinder/` for the canonical packaged design system); no `color.reveal` on this flow. TypeScript strict mode; all props typed; no `any`.

**Technical Notes:**
- FR32 (user-facing report/block).
- Uses standard bottom sheet and toast patterns established in Epic 3/4.

---

## Epic 8: Push Notifications

**Goal:** Deliver timely, meaningful push notifications for messages, gate events, and match updates — without being intrusive or waking users for low-priority events.

**FRs covered:** FR35, FR36, FR37, FR38, FR39

---

### Story 8.1 — Backend: Push Notification Service and Device Token Registration

**As a** system,
**I want** to store and manage push notification device tokens per user,
**so that** I can deliver notifications to the correct device.

**Acceptance Criteria:**

- AC1: `POST /api/users/me/push-tokens` accepts `{ token: string, platform: "ios" | "android" }` and upserts the device token for the authenticated user; returns 200.
- AC2: `DELETE /api/users/me/push-tokens/{token}` removes a specific token (e.g., on logout).
- AC3: A notification dispatch service (internal, not exposed externally) accepts `{ userId, type, payload }` and looks up the user's active tokens, then dispatches to APNs (iOS) or FCM (Android) accordingly.
- AC4: Dispatch failures (invalid token, expired token) are handled gracefully — invalid tokens are automatically removed from the DB; transient failures are retried up to 3 times with exponential backoff.
- AC5: Unit tests cover: token upsert, token deletion, dispatch with valid token, dispatch with expired token (auto-removal), retry logic.

**Technical Notes:**
- FR35 (push notification infrastructure).
- `app.push_tokens` table. APNs and FCM credentials stored as environment config (never hardcoded).
- Notification dispatch is fire-and-forget from the API perspective — does not block the originating request.

---

### Story 8.2 — Backend: Notification Triggers — Messages, Gate, and Match Events

**As a** system,
**I want** to send push notifications when key events occur,
**so that** users are informed without needing to keep the app open.

**Acceptance Criteria:**

- AC1: New message received (while recipient is not active in the conversation) → send "New message" notification with conversation ID in payload.
- AC2: Gate unlocks (`gate_open` transition) → send "Your conversation has reached a milestone" notification to both participants.
- AC3: Gate resolution → send outcome-appropriate notification:
  - `mutual_reveal` → "It's a match! See who you've been talking to."
  - `continued` → "Your conversation continues."
  - `ended_anonymized` / `expired` → "This conversation has ended." (neutral copy, no blame)
- AC4: New match opportunity available (user has capacity) → send "Someone is waiting to talk with you" notification, matching the `MatchEntryCard` copy for continuity (UX-DR15).
- AC5: All notification copy follows the vocabulary rules (no "rejected", "unmatched", urgency language).
- AC6: Notifications are not sent if the user has the app in the foreground and is actively viewing the relevant screen (presence-aware suppression via SignalR connection state).
- AC7: Unit tests cover all 5 trigger types. Integration test: message sent → notification dispatched.

**Technical Notes:**
- FR36 (message notifications), FR37 (gate notifications), FR38 (match notifications), FR39 (discover notifications).
- Presence-aware suppression: if the user has an active SignalR connection scoped to the relevant conversation, suppress the push notification.

---

### Story 8.3 — Backend: User Notification Preferences

**As a** user,
**I want** to control which notifications I receive,
**so that** I can manage my attention without being overwhelmed.

**Acceptance Criteria:**

- AC1: `GET /api/users/me/notification-preferences` returns current preferences: `{ messages: bool, gateEvents: bool, matchEvents: bool, discoverEvents: bool }`.
- AC2: `PATCH /api/users/me/notification-preferences` accepts partial update and persists preferences.
- AC3: Notification dispatch respects user preferences — if `gateEvents: false`, no gate-related push is sent regardless of event type.
- AC4: Default preferences on account creation: all `true`.
- AC5: Unit tests cover: get preferences, patch partial, dispatch respects disabled preference.

**Technical Notes:**
- FR35 (notification preferences).
- `app.user_notification_preferences` table.

---

### Story 8.4 — Frontend: Push Token Registration and Notification Permission

**As a** user,
**I want** the app to request notification permission at the right moment,
**so that** I can opt in without being prompted too early.

**Acceptance Criteria:**

- AC1: The app requests notification permission (via Expo `Notifications.requestPermissionsAsync()`) after the user completes onboarding (first successful profile save), not at app launch.
- AC2: If permission is granted, the app retrieves the Expo push token and calls `POST /api/users/me/push-tokens` to register it.
- AC3: If permission is denied, the app gracefully degrades — no error shown; the user can still use the app without notifications.
- AC4: On logout, the app calls `DELETE /api/users/me/push-tokens/{token}` to deregister the current device token.
- AC5: Token registration is retried once on network failure; subsequent failure is silently swallowed (non-critical path).
- AC6: TypeScript strict mode; all props typed; no `any`.

**Technical Notes:**
- FR35 (device token registration).
- Expo `expo-notifications` package. Uses Expo push token (not raw APNs/FCM token) for simplicity.

---

### Story 8.5 — Frontend: Notification Deep-Link Routing

**As a** user,
**I want** tapping a notification to take me directly to the relevant screen,
**so that** I don't have to navigate manually after being notified.

**Acceptance Criteria:**

- AC1: Incoming notifications carry a `type` and `conversationId` (or `screenTarget`) payload.
- AC2: Tapping a "New message" notification navigates to `ConversationScreen` for that conversation ID.
- AC3: Tapping a gate or match notification navigates to the appropriate screen (ResolutionWait, Reveal Ceremony, or Outcome screen) based on current conversation state fetched on tap.
- AC4: Tapping a new-match notification navigates to the Waiting screen (`WaitingState`) in its `match_ready` state, which then advances to the `MatchEntryCard` per the navigation state machine (UX-DR21).
- AC5: If the app is cold-started from a notification, the navigation state is correctly initialized (deep link handled on startup via Expo Router).
- AC6: If the target conversation no longer exists (e.g., deleted), the app navigates to the Waiting screen (`WaitingState`) with no error crash.
- AC7: TypeScript strict mode; all props typed; no `any`.

**Technical Notes:**
- FR36–FR39 (notification deep-linking).
- Expo Router handles deep links via notification response listener (`Notifications.addNotificationResponseReceivedListener`).
- Navigation state machine must handle cold-start and foreground-tap cases.

---

## Epic 9: Admin, Support & Operations Panel

**Goal:** Give internal admins a web-based panel to manage users, review content, handle support actions, and oversee platform operations — built on Blinder.AdminPanel.

**FRs covered:** FR40, FR41, FR51, FR52

---

### Story 9.1 — Backend: Admin User Management API

**As an** admin,
**I want** API endpoints to view, search, suspend, and delete user accounts,
**so that** I can manage the user base without direct DB access.

**Acceptance Criteria:**

- AC1: `GET /api/admin/users` returns paginated user list with search by email/displayName; requires admin role claim from IdentityServer JWT.
- AC2: `GET /api/admin/users/{id}` returns full user profile including account status (`active | suspended | deleted`), registration date, last active, report count.
- AC3: `PATCH /api/admin/users/{id}` accepts `{ status: "suspended" | "active" }` with optional `reason` and applies the change; suspended users receive 403 on all authenticated endpoints.
- AC4: `DELETE /api/admin/users/{id}` soft-deletes the account — anonymizes PII fields, retains record for audit; returns 204.
- AC5: All admin actions are written to an `app.admin_audit_log` table with `{ adminId, action, targetId, reason, timestamp }`.
- AC6: Unit tests cover: list/search, get, patch status, soft-delete, audit log entry. Integration test: suspend user → user gets 403 on next API call.

**Technical Notes:**
- FR40 (user management), FR51 (account deletion/suspension).
- Admin role claim from IdentityServer — Blinder.Api never issues admin tokens itself.
- Soft-delete: anonymize `email`, `displayName`, `photos`; retain `userId` and timestamps for audit integrity.

---

### Story 9.2 — Backend: Admin Platform Configuration API

**As an** admin,
**I want** API endpoints to manage platform-level configuration at runtime,
**so that** I can tune thresholds and feature flags without redeployment.

**Acceptance Criteria:**

- AC1: `GET /api/admin/config` returns current platform config: `{ gateLockDuration: int (hours), messageThreshold: int, gateTimeoutDuration: int (hours), conversationExpiryDuration: int (hours), maxActiveConversations: int }`.
- AC2: `PATCH /api/admin/config` accepts partial update and persists changes; new values take effect on next evaluation cycle (no restart required).
- AC3: Config changes are written to `app.admin_audit_log`.
- AC4: Invalid values (e.g., negative durations, zero thresholds) return 422 with RFC 9457 Problem Details.
- AC5: Unit tests cover: get config, valid patch, invalid patch → 422, audit log entry.

**Technical Notes:**
- FR52 (platform config), FR46 (configurable thresholds).
- Config stored in `app.platform_config` table (single-row keyed config). Application reads config on each evaluation cycle, not cached indefinitely.

---

### Story 9.3 — Backend: Admin Support — Conversation and Message Inspection

**As an** admin,
**I want** to inspect any conversation and its messages,
**so that** I can investigate reports and support tickets.

**Acceptance Criteria:**

- AC1: `GET /api/admin/conversations` returns paginated list with filters: `{ userId?, status?, reportedOnly? }`.
- AC2: `GET /api/admin/conversations/{id}` returns full conversation metadata including both participant IDs, gate state, outcome, and message count.
- AC3: `GET /api/admin/conversations/{id}/messages` returns full message history (admin bypasses gate state restrictions).
- AC4: Admin message inspection access is logged to `app.admin_audit_log`.
- AC5: Unit tests cover: list with filters, get conversation, get messages, audit log.

**Technical Notes:**
- FR41 (support tooling), FR33 (moderation).
- Admin endpoints are in Blinder.Api (not AdminPanel) — AdminPanel is the UI that calls these APIs.

---

### Story 9.4 — Frontend (AdminPanel): User Management UI

**As an** admin,
**I want** a web UI to search, view, suspend, and delete users,
**so that** I can perform user management without using API tools directly.

**Acceptance Criteria:**

- AC1: AdminPanel is a Razor Pages or Blazor Server web app within `Blinder.AdminPanel` project; accessible only via Traefik after authenticating with IdentityServer (admin role required).
- AC2: A Users page shows a searchable/paginated table: columns = displayName, email, status, registrationDate, reportCount.
- AC3: Clicking a user opens a detail view with full profile, status change controls (Suspend / Reactivate), and a Delete (soft) button with confirmation dialog.
- AC4: Suspension and deletion actions call the corresponding Blinder.Api admin endpoints and refresh the view on success.
- AC5: All admin actions show a confirmation dialog before executing.
- AC6: Unauthorized access (non-admin token) redirects to IdentityServer login.

**Technical Notes:**
- FR40, FR51.
- AdminPanel authenticates via IdentityServer Authorization Code flow (server-side, no PKCE needed for server web app).
- No custom CSS framework required — use Bootstrap or ASP.NET default styling; this is an internal tool.

---

### Story 9.5 — Frontend (AdminPanel): Moderation Queue and Platform Config UI

**As an** admin,
**I want** a web UI to review the moderation queue and adjust platform configuration,
**so that** I can keep the platform safe and tune behavior at runtime.

**Acceptance Criteria:**

- AC1: A Moderation page shows two tabs: "Reports" (user-submitted reports) and "Flagged Messages" (auto-flagged).
- AC2: Each report row shows: reporter, reported user, reason, status; clicking opens detail with action buttons (Reviewed / Actioned / Dismissed).
- AC3: Each flagged message row shows: sender, conversation, content preview, flag reason; clicking opens detail with action buttons (Clear / Warn User / Suspend User).
- AC4: A Platform Config page shows current config values as editable fields; a Save button calls `PATCH /api/admin/config` and shows success/error feedback.
- AC5: Blocklist management: a simple list of blocked terms with add/remove controls calling `POST/DELETE /api/moderation/blocklist`.
- AC6: Unauthorized access redirects to IdentityServer login.

**Technical Notes:**
- FR33, FR52.
- Shares the same AdminPanel auth session as Story 9.4.

---

## Epic 10: Monetization & Conversation Capacity

**Goal:** Implement the freemium conversation slot model — free users get a limited number of active conversations; premium users unlock expanded capacity — with in-app purchase flows on both platforms.

**FRs covered:** FR48, FR49

---

### Story 10.1 — Backend: Conversation Capacity Enforcement

**As a** system,
**I want** to enforce per-user active conversation limits based on their tier,
**so that** free users are capped and premium users get expanded capacity.

**Acceptance Criteria:**

- AC1: When a new match opportunity is about to be assigned to a user, the system checks the user's current active conversation count against their tier limit.
- AC2: Free tier limit is configurable via platform config (default: 3 active conversations). Premium tier limit is configurable (default: unlimited / 999).
- AC3: If the user is at their limit, no new match is assigned; the user is not notified of missed matches (no urgency signal — NFR28).
- AC4: `GET /api/users/me/capacity` returns `{ tier: "free" | "premium", activeConversations: int, limit: int }`.
- AC5: Unit tests cover: free user at limit → no new assignment; free user under limit → assignment proceeds; premium user → always proceeds.

**Technical Notes:**
- FR48 (conversation slot limits).
- Tier determination based on a `tier` field on the user record in `app.users`. Premium is set server-side after purchase verification — never trusted from the client.

---

### Story 10.2 — Backend: In-App Purchase Verification and Premium Upgrade

**As a** user,
**I want** my in-app purchase to be verified server-side and my account upgraded,
**so that** premium access is securely granted.

**Acceptance Criteria:**

- AC1: `POST /api/users/me/purchase/verify` accepts `{ platform: "ios" | "android", receiptData: string, productId: string }` and validates the receipt with Apple App Store / Google Play server-side verification APIs.
- AC2: On successful verification, the user's `tier` is updated to `premium` and a purchase record is written to `app.purchases` with `{ userId, platform, productId, transactionId, verifiedAt }`.
- AC3: On failed verification (invalid receipt, already used, expired), returns 422 with RFC 9457 Problem Details; tier is not changed.
- AC4: Purchase verification is idempotent — re-submitting the same `transactionId` returns 200 without creating a duplicate record or double-upgrading.
- AC5: `GET /api/users/me/subscription` returns `{ tier, premiumSince?, expiresAt? }`.
- AC6: Unit tests cover: valid receipt → upgrade; invalid receipt → 422; duplicate transactionId → idempotent. Integration test (mocked store API): full verify flow.

**Technical Notes:**
- FR49 (in-app purchase).
- Apple and Google store API credentials in environment config. Never trust client-reported purchase status — always verify server-side.
- Initial launch may be one-time purchase (lifetime premium) rather than subscription — productId-based, configurable.

---

### Story 10.3 — Frontend: Conversation Capacity Indicator and Upgrade Prompt

**As a** user,
**I want** to see how many conversation slots I have left and be offered an upgrade when I'm at the limit,
**so that** I understand the freemium model without feeling trapped.

**Acceptance Criteria:**

- AC1: The Waiting screen (`WaitingState`) displays the user's current capacity usage (e.g., "2 of 3 conversations active") fetched from `GET /api/users/me/capacity`, rendered as a neutral `Pill` component — not a progress bar, not a counter that reads as a gamified score.
- AC2: When the user reaches their limit, the Waiting screen shows a non-blocking upgrade prompt (not a modal wall) — a banner or card offering premium upgrade, visually subordinate to the waiting state itself.
- AC3: The upgrade prompt uses neutral, value-positive language ("Unlock more conversations") — no scarcity or urgency language.
- AC4: Tapping the upgrade prompt opens the Upgrade screen (Story 10.4).
- AC5: Premium users see no capacity indicator or upgrade prompt — the feature is invisible to them.
- AC6: TypeScript strict mode; all props typed; no `any`. All Warm Dusk tokens.

**Technical Notes:**
- FR48 (capacity display), FR49 (upgrade entry point).
- Capacity data is fetched on Waiting screen focus (not cached across navigation).

---

### Story 10.4 — Frontend: In-App Purchase Flow

**As a** user,
**I want** to purchase premium access through the native store,
**so that** I can unlock more conversations without leaving the app.

**Acceptance Criteria:**

- AC1: The Upgrade screen displays the premium product with price (fetched from the native store via `expo-iap` or `react-native-iap`), feature list, and a single purchase CTA.
- AC2: Tapping the CTA triggers the native store purchase sheet (Apple Sheet / Google Play billing).
- AC3: On successful native purchase, the app calls `POST /api/users/me/purchase/verify` with the receipt; on server confirmation, the UI updates to show premium status and the capacity indicator disappears from Discover.
- AC4: On purchase failure or cancellation, the app shows a neutral dismissible error ("Purchase was not completed") and returns to the Upgrade screen. No state change occurs.
- AC5: Restore Purchases button is present (required for App Store compliance) — calls the native restore flow and re-verifies with the server.
- AC6: TypeScript strict mode; all props typed; no `any`. All Warm Dusk tokens.

**Technical Notes:**
- FR49 (IAP flow).
- App Store requires a restore purchases mechanism — failure to include it will cause App Store rejection.
- `expo-iap` preferred for Expo SDK 55 compatibility; fallback to `react-native-iap` if not compatible.

---

## Epic 11: Product Quality Telemetry & Safety Pulse

**Goal:** Instrument the platform with the metrics, error tracking, and safety signal monitoring needed to operate Blinder confidently in production — covering both technical health and product quality signals.

**FRs covered:** FR42, FR43, FR44, FR53

---

### Story 11.1 — Backend: Structured Logging and Error Tracking

**As a** system,
**I want** all backend services to emit structured logs and report unhandled errors to a centralized sink,
**so that** on-call engineers can diagnose issues quickly.

**Acceptance Criteria:**

- AC1: All three backend projects (Blinder.IdentityServer, Blinder.Api, Blinder.AdminPanel) use structured logging via Serilog with JSON output format.
- AC2: Log entries include: `timestamp`, `level`, `service` (identityserver | api | adminpanel), `traceId`, `userId` (when available, never PII like email), `message`, `exception` (if applicable).
- AC3: Unhandled exceptions are caught by a global middleware and logged at `Error` level with full stack trace; the response returns RFC 9457 Problem Details with a `traceId` reference.
- AC4: Log sink is configurable via environment variable (`LOG_SINK`: stdout | seq | file); default is stdout for Docker compatibility.
- AC5: No PII (email, display name, photo URLs) appears in log output — only opaque IDs.
- AC6: Unit test: global exception middleware returns RFC 9457 with traceId; log output contains traceId but not PII.

**Technical Notes:**
- FR42 (error tracking), FR44 (structured logging).
- `traceId` correlates across IdentityServer and Api using `Activity.Current.TraceId` (W3C trace context).
- Seq is the preferred local dev sink (runs in Docker Compose); stdout in production.

---

### Story 11.2 — Backend: Product Metrics and Key Event Instrumentation

**As a** product team,
**I want** key product events emitted as structured metrics,
**so that** we can track activation, engagement, and reveal rates without manual SQL queries.

**Acceptance Criteria:**

- AC1: The following events are emitted as structured log entries (metric-tagged) on occurrence:
  - `user.registered` — new user completes registration
  - `user.onboarded` — user completes profile (photo + bio + preferences)
  - `conversation.started` — first message sent in a new conversation
  - `gate.opened` — dual-trigger threshold crossed
  - `gate.decision.submitted` — user submits gate decision
  - `gate.resolved` — gate resolution fires (with `outcome` field)
  - `reveal.completed` — mutual reveal outcome viewed
  - `user.reported` — user report submitted
  - `user.blocked` — user block submitted
- AC2: Each event entry includes `{ eventType, userId (opaque), timestamp, properties }` — no PII in properties.
- AC3: Events are emitted in addition to (not instead of) normal request processing — fire-and-forget, non-blocking.
- AC4: Unit tests cover: each event type is emitted on the corresponding action.

**Technical Notes:**
- FR43 (product metrics).
- Events are structured log entries, not a separate metrics pipeline — keeps infrastructure simple at launch. Can be extracted to a proper event bus post-launch.
- `outcome` field on `gate.resolved` = mutual_reveal | continued | ended_anonymized | expired.

---

### Story 11.3 — Backend: Safety Pulse Monitoring — Moderation Signal Aggregation

**As an** operations team,
**I want** a dashboard-ready endpoint that surfaces safety signal aggregates,
**so that** we can detect spikes in harmful behavior without building a separate analytics system.

**Acceptance Criteria:**

- AC1: `GET /api/admin/safety-pulse` (admin only) returns rolling 24h aggregates: `{ reportCount, flaggedMessageCount, suspendedUserCount, blockedPairCount, topReportReasons: [{ reason, count }] }`.
- AC2: Aggregates are computed from live DB data (no separate cache required at launch); query must complete in ≤2s.
- AC3: A `since` query parameter (ISO 8601 datetime) allows custom time window queries.
- AC4: Unit test: endpoint returns correctly shaped response. Integration test: seed report data → verify aggregate counts.

**Technical Notes:**
- FR53 (safety pulse).
- Admin role claim required from IdentityServer JWT.
- Keep aggregation queries simple (COUNT + GROUP BY) — no materialized views at launch.

---

### Story 11.4 — Backend: Health Check Endpoints

**As an** infrastructure operator,
**I want** health check endpoints on all backend services,
**so that** Docker Compose and Traefik can route traffic only to healthy instances.

**Acceptance Criteria:**

- AC1: All three backend projects expose `GET /health` returning `{ status: "healthy" | "degraded" | "unhealthy", checks: [{ name, status, duration }] }`.
- AC2: Health checks include: database connectivity (PostgreSQL ping), MinIO connectivity (Blinder.Api only), IdentityServer discovery document reachability (Blinder.Api only).
- AC3: `GET /health/live` returns 200 if the process is running (liveness); `GET /health/ready` returns 200 only when all checks pass (readiness).
- AC4: Traefik health check configuration uses `/health/ready`; Docker Compose `healthcheck` uses `/health/live`.
- AC5: Unit test: healthy state returns 200; degraded DB returns 503 on `/health/ready`.

**Technical Notes:**
- FR44 (operational health).
- ASP.NET Core built-in `IHealthCheck` infrastructure. No external dependencies for health check implementation.

---

### Story 11.5 — Frontend: Client-Side Error Reporting

**As a** developer,
**I want** unhandled mobile errors reported to a server-side endpoint,
**so that** production crashes are visible without requiring a third-party crash SDK at launch.

**Acceptance Criteria:**

- AC1: A global React error boundary wraps the app root; caught errors call `POST /api/diagnostics/client-error` with `{ errorMessage, stackTrace, screenName, appVersion, platform }`.
- AC2: The error boundary renders a neutral fallback UI ("Something went wrong. Please restart the app.") — not a raw stack trace.
- AC3: `POST /api/diagnostics/client-error` is unauthenticated (errors may occur before login) and rate-limited to 10 reports per IP per minute to prevent abuse.
- AC4: Error payloads are logged via the structured logging pipeline (Story 11.1) — no separate storage table needed at launch.
- AC5: TypeScript strict mode; all props typed; no `any`.

**Technical Notes:**
- FR42 (client error tracking).
- No third-party crash SDK (Sentry, etc.) at launch — keeps infrastructure lean. The `/api/diagnostics/client-error` endpoint can be replaced by a proper SDK post-launch.
- `stackTrace` is truncated server-side to 4KB max before logging.
