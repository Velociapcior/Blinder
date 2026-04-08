---
stepsCompleted: ['step-01-init', 'step-02-discovery', 'step-02b-vision', 'step-02c-executive-summary', 'step-03-success', 'step-04-journeys', 'step-05-domain', 'step-06-innovation', 'step-07-project-type', 'step-08-scoping', 'step-09-functional', 'step-10-nonfunctional', 'step-11-polish', 'step-12-complete']
inputDocuments:
  - '_bmad-output/brainstorming/brainstorming-session-2026-04-03-blinder.md'
  - '_bmad-output/planning-artifacts/product-brief-Blinder-distillate.md'
  - '_bmad-output/planning-artifacts/product-brief-Blinder.md'
date: '2026-04-03'
workflowType: 'prd'
briefCount: 2
researchCount: 0
brainstormingCount: 1
projectDocsCount: 0
classification:
  projectType: 'Mobile App (cross-platform consumer)'
  domain: 'Social / Dating (General)'
  complexity: 'high'
  projectContext: 'greenfield'
---

# Product Requirements Document - Blinder

**Author:** Piotr.palej
**Date:** 2026-04-03

## Executive Summary

Blinder is a conversation-first dating platform designed for users who are filtered out too early by photo-first mechanics. The product's core job is to guarantee a fair first chance through dialogue before appearance enters the decision loop. It targets users who are exhausted by rejection-heavy, status-driven dating experiences and want lower-pressure, meaningful interaction.

The product solves a deeper need than "better matching": users want to be heard before they are judged. To deliver that, Blinder enforces a blind conversation phase, a structured decision gate, and dignified outcomes that reduce blame and emotional volatility. The intended result is higher-quality interaction, stronger trust, and a calmer path to connection.

Blinder launches as a new greenfield product with Poland as the initial validation market. Execution focus is to prove loop quality and user growth without drifting into engagement-bait mechanics that undermine trust.

### What Makes This Special

Blinder does not add kindness features to existing swipe logic; it replaces the logic. Users are not eliminated by visual standards before they can speak. During the core phase, conversation quality is the only signal that matters, which restores user agency and shifts outcomes toward controllable behavior rather than static appearance traits.

The differentiator is structural, not cosmetic: blind-first interaction, private/simultaneous decisioning, and anonymized non-mutual endings. Together, these mechanics create a dignity-preserving system that is difficult to replicate by layering features on top of mainstream dating app patterns.

Core insight: people do not just want more matches; they want a legitimate chance to connect before instant visual filtering decides their value.

## Project Classification

- Project Type: Mobile app (cross-platform consumer)
- Domain: Social / Dating (General)
- Complexity: High
- Project Context: Greenfield

## Success Criteria

### User Success

- Users experience a fair first chance: a high share of new users reach at least one blind conversation without photo exposure.
- Users feel emotionally safer than on mainstream apps: post-conversation sentiment reports indicate reduced feelings of rejection pressure and objectification.
- Users find conversations meaningful: a majority of blind conversations pass a minimum depth threshold (message volume + duration + reciprocal participation).
- Users experience agency: decision-gate completion rates remain high, with low abandonment caused by unclear flow or anxiety-inducing UX.
- Users understand the product promise quickly: first-session comprehension of "conversation before appearance" is validated through onboarding completion and behavior consistency.

### Business Success

- Poland launch achieves sustained growth in active user base across initial dense cohorts (not just installs).
- Match liquidity is healthy in launch pockets: users receive viable matches within expected cycle windows.
- Retention proves trust-based product-market fit: returning users correlate with conversation quality and safety perception, not notification pressure.
- Reveal-rate trend improves over baseline cohorts, indicating better compatibility and conversation outcomes over time.
- Monetization validates fairness thesis: premium conversion occurs through capacity demand (more concurrent conversations), without harming free-user loop quality.

### Technical Success

- Decision-gate state machine is deterministic and auditable across all branches (mutual reveal, one continue, any abandon, timeout).
- Dual-trigger orchestration is reliable: no race-condition outcomes when time floor and message threshold events collide.
- Photo safety pipeline enforces pre-persistence moderation with high processing reliability and low unsafe leakage.
- Notification system stays signal-only: only meaningful events fire, with low false/duplicate notification rate.
- Platform reliability supports trust-sensitive usage: strong message delivery integrity, low critical error rate, and resilient recovery from app restarts/network interruptions.

### Measurable Outcomes

- Time-to-first-conversation: median time from onboarding completion to first active conversation.
- Conversation depth index: % of conversations reaching defined depth threshold.
- Decision-gate completion rate: % of started conversations that reach a completed decision state.
- Mutual reveal rate: directional trend over cohorts (primary v1 quality signal).
- Emotional safety score: periodic in-app pulse metric after conversation outcomes.
- 30-day retention by cohort: segmented by users who reached conversation depth threshold vs those who did not.
- Liquidity SLA: % of active users receiving a match opportunity within target window.
- Moderation efficacy: % photo uploads successfully scanned before availability + incident leakage rate.

## Product Scope

### MVP - Minimum Viable Product

- Blind conversation loop with hidden photos before reveal.
- Onboarding quiz that generates conversation starters.
- Async daily matching with soft-start conversation activation.
- Dual-trigger decision gate with private/simultaneous choices.
- Outcome anonymization for non-mutual endings.
- Upload-time image moderation pipeline.
- Minimal notification policy (meaningful events only).
- Conversation-capped freemium model.
- Core instrumentation for reveal trend, depth, retention, and safety sentiment.

### Growth Features (Post-MVP)

- Matching optimization beyond baseline (complementarity model tuning).
- Blind conversation archive for successful reveals.
- Optional cooldown flow after conversation endings.
- More advanced cohort-based trust and quality analytics.
- Expansion tooling for new geographies after Poland loop stability.

### Vision (Future)

- Category-defining calm dating platform centered on dignity-before-desirability.
- Best-in-class trust architecture where safety is preventative, not reactive.
- Highly adaptive matching quality engine optimized for conversation outcomes.
- Scalable multi-market rollout while preserving fairness, low-pressure UX, and anti-addiction principles.

## User Journeys

### Journey 1: Primary User Success Path - "A Fair First Chance That Feels Real"

**Persona:** Marek, 29, thoughtful but often filtered out on photo-first apps.

**Opening Scene:**
Marek joins Blinder because he wants to be heard before judged. He completes onboarding and gets his first daily match.

#### Variant 1A: Fast Chemistry (Early Gate Unlock)
- Both users engage quickly and reciprocally.
- Message threshold is reached before the time floor.
- Early decision gate appears; both privately choose.
- If mutual reveal occurs, both transition to revealed profile context with continuity of conversation.
- Emotional outcome: excitement with low social-performance anxiety.

#### Variant 1B: Slow-Burn Connection (Time-Floor Gate)
- Conversation is steady but not high-volume.
- Time floor triggers the gate instead of message count.
- Decision happens with enough context for both sides to choose intentionally.
- Emotional outcome: calm confidence, less rushed judgment.

#### Variant 1C: Post-Reveal Continuation
- Mutual reveal occurs.
- Conversation history from blind phase remains visible as shared context.
- Users decide whether to continue in-app or move toward date planning.
- Emotional outcome: trust reinforced by "we connected before visuals."

**Critical failure points / recovery**
- User confusion about why gate appeared: clear gate trigger explanation.
- Match stalls before gate: neutral inactivity handling and graceful expiry messaging.
- App state mismatch around reveal: deterministic server resolution + clear client sync states.

### Journey 2: Primary User Edge Cases - "Staying Safe, Dignified, and in Control"

**Persona:** Ania, 27, safety-sensitive, emotionally intentional, low tolerance for manipulative dynamics.

**Opening Scene:**
Ania wants meaningful conversation but needs strong control over emotional risk.

#### Variant 2A: Uneven Effort
- Partner gives low-effort replies; conversation quality drops.
- Ania chooses to continue briefly, then abandons.
- Outcome is anonymized; no blame assignment.
- Emotional outcome: disengagement without humiliation.

#### Variant 2B: Safety Discomfort
- Conversation tone becomes uncomfortable.
- Ania exits quickly; system prevents photo-phase exposure.
- Report/block tools remain available without forcing high-friction steps.
- Emotional outcome: safety restored, trust preserved.

#### Variant 2C: Ghosting / Timeout Ambiguity
- Partner stops responding near gate threshold.
- System resolves with neutral expiration/end messaging.
- Ania sees a clear status outcome, not a personal rejection signal.
- Emotional outcome: disappointment without self-worth penalty.

**Critical failure points / recovery**
- User feels trapped in low-quality chats: easy abandon with dignified copy.
- User misreads neutral endings as hidden rejection score: explicit product-language principles.
- Repeated low-quality pairings: matching quality guardrail feedback loop.

### Journey 3: Trust & Safety Operations (Lean Stub)

**Actor:** Kasia (T&S operator)
Photo uploads are scanned before persistence; uncertain cases enter review queue; decisions are logged for auditability; users get clear remediation paths and appeal options.
**Capability implications:** moderation pipeline, reviewer console, appeals, policy audit logs, queue SLAs.

### Journey 4: Support Resolution (Lean Stub)

**Actor:** Tomek (support specialist)
When users report "unexpected conversation ending," support reconstructs event timelines, explains outcomes without violating anonymization rules, and escalates traceable defects.
**Capability implications:** support timeline tooling, redacted state explanations, incident escalation workflow.

### Journey 5: Product Quality Operations (Lean Stub)

**Actor:** Nina (product/ops analyst)
Weekly cohort reviews detect drift between growth and conversation quality; interventions adjust matching or UX before trust erosion.
**Capability implications:** guardrail dashboards, cohort segmentation, quality-alert thresholds, corrective-action playbook.

### Journey Requirements Summary

- Deep primary-user coverage for both success and failure branches.
- Explicit emotional-state design requirements at decision points.
- Deterministic gate/reveal state handling and conflict-safe orchestration.
- Dignified exits and neutral language system-wide.
- Minimum viable operational coverage for moderation, support, and quality governance without over-specifying post-MVP tooling.

## Domain-Specific Requirements

### Compliance & Regulatory

- GDPR-first MVP for Poland/EU: lawful basis, consent, export/delete rights, and retention policy.
- Age-gate enforcement to prevent minors entering adult dating flows.
- Clear policy acceptance for blind-phase rules, moderation boundaries, and anonymized outcomes.

### Technical Constraints

- Server-authoritative decision-gate state machine.
- Strict no-photo visibility before reveal across all API/client paths.
- Basic audit trail for safety and state-transition events.
- Keep architecture lean: first-party components preferred over vendor-heavy stacks.

### Integration Requirements

- Push notifications via direct platform rails only (APNS/FCM), no extra notification SaaS in MVP.
- Prefer self-hosted media storage and scanning pipeline for uploads.
- First-party telemetry storage for core product quality metrics.
- Internal admin/support tooling for MVP before external support platforms.

### Risk Mitigations

- Neutral ending language to prevent perceived rejection scoring.
- Manual-review fallback when automated scan confidence is low.
- Conflict-safe event handling to avoid race-condition trust breaks.
- Quality guardrails tracked with first-party metrics before growth acceleration.

## Innovation & Novel Patterns

### Detected Innovation Areas

- Interaction-order inversion:
  - Blinder flips the default dating sequence from look then talk to talk then reveal.
  - This changes who gets a first chance and what signal determines progress.
- Dignity-preserving decision architecture:
  - Private simultaneous decisioning reduces strategic signaling and social pressure.
  - Anonymized non-mutual outcomes remove explicit blame assignment.
- Safety-through-structure:
  - Harm reduction is embedded in flow design (no photo exchange during blind phase, controlled reveal transition), not just moderation policy.
- Anti-compulsion product stance:
  - Minimal-notification model and one-conversation focus challenge attention-maximizing norms.

### Market Context & Competitive Landscape

- Most mainstream apps optimize visual-first sorting and engagement loops; conversation quality is secondary.
- Blinder's differentiation is structural rather than branding-level:
  - Not a better swipe feed.
  - Not AI-coached chat.
  - A different first-impression protocol.
- Competitive risk:
  - Features can be copied; coherent dignity-first system behavior is harder to copy.
- Positioning implication:
  - Category language should emphasize fair first chance and conversation-before-judgment, not generic "better matching."

### Validation Approach

- Validate the core innovation through behavioral evidence:
  - Compare conversation depth and decision-gate completion versus baseline cohort expectations.
  - Track directional mutual reveal trend and emotional safety pulse after outcomes.
  - Measure first-session comprehension of product rule set (talk first, reveal later).
- Run phased cohort tests in Poland launch pockets:
  - Early chemistry path (message-threshold unlock).
  - Slow-burn path (time-floor unlock).
  - Outcome clarity and dignity perception in non-mutual endings.
- Success criterion:
  - Users report feeling heard before judged, and behavior supports repeat use without pressure-heavy notification tactics.

### Risk Mitigation

- Risk: Users perceive innovation as friction versus convenience.
  - Mitigation: clear onboarding framing and transparent gate-state communication.
- Risk: Competitors imitate surface mechanics.
  - Mitigation: keep differentiation in system coherence and dignity language consistency across every flow.
- Risk: Quality drops under growth pressure.
  - Mitigation: enforce quality guardrails as release constraints, not optional dashboards.
- Risk: Novel flow causes confusion in edge cases.
  - Mitigation: deterministic server-side resolution plus clear client recovery messaging.

## Mobile App Specific Requirements

### Project-Type Overview

Blinder will be delivered as a cross-platform mobile app for iOS and Android. The product model is asynchronous conversation-first matching with stateful reveal logic, so mobile reliability, clear state transitions, and calm notification behavior are first-class requirements.

### Technical Architecture Considerations

- Use a shared cross-platform app codebase with platform-specific wrappers only where required by OS policy.
- Keep backend server-authoritative for all conversation-state transitions (start, gate trigger, reveal/continue/abandon, expiry).
- Prefer first-party services and self-hosted components for storage, telemetry, and ops tooling.
- Keep client architecture simple:
  - resilient session handling,
  - deterministic state refresh after reconnect/app relaunch,
  - strict guard against pre-reveal photo exposure.

### Platform Requirements

- Primary platforms: iOS and Android phone form factors.
- Same core feature parity across both platforms for trust-critical flows.
- Release path must support staged rollout by cohort/city.

### Device Permissions

- Required in MVP:
  - Notifications permission (for meaningful state transitions only).
  - Media/photo access for profile upload.
  - Precise location permission for location-based matching.
- Not required in MVP unless justified:
  - Contacts, microphone, camera live capture.
- Permission prompts must be contextual and delayed until feature use, not shown in bulk at onboarding.

### Offline Mode

- Offline support is out of scope for MVP.
- Product rule: no connectivity means no active app usage for core flows (matching, messaging, decision actions).
- UI should communicate this explicitly and block write actions until connection is restored.

### Push Strategy

- Use direct APNS/FCM rails only; no extra notification SaaS in MVP.
- Notifications are limited to meaningful events:
  - new match available,
  - partner reply,
  - decision gate reached.
- No engagement-bait notifications, streak nudges, or vanity alerts.
- Notification content must preserve privacy and avoid exposing counterpart-sensitive context on lock screens.

### Store Compliance

- App Store / Play Store compliance requires:
  - clear account deletion path,
  - age-appropriate gating for dating context,
  - moderation and abuse-reporting policy visibility,
  - privacy policy and data-handling disclosures aligned to GDPR commitments.
- Trust-and-safety claims in store listing must map to actual implemented controls.

### Implementation Considerations

- Build trust-critical state handling first (conversation lifecycle + decision gate) before growth tooling.
- Instrument first-party event telemetry for quality guardrails from day one.
- Keep support/admin functions internal for MVP and avoid external support platform dependencies.
- Treat push, moderation, and state consistency as release blockers, not post-launch hardening tasks.

## Project Scoping & Phased Development

### MVP Strategy & Philosophy

**MVP Approach:** Problem-solving MVP focused on proving the core thesis: conversation before appearance creates a fairer, higher-quality first interaction.

**MVP Principle:**
If a capability does not directly strengthen the trust-critical core loop, defer it.

**Core Loop to Prove:**
- Onboarding -> daily match -> blind conversation -> dual-trigger decision gate -> dignified outcome.

**Resource Requirements (Lean):**
- 1 mobile-focused full-stack engineer
- 1 backend engineer (state machine + matching + moderation flow)
- 0.5 product/UX for trust-critical copy and flow clarity
- 0.5 trust/safety + ops support
- Optional part-time analytics support

### MVP Feature Set (Phase 1)

**Core User Journeys Supported:**
- Primary success path (fast chemistry, slow-burn, post-reveal continuation)
- Primary edge path (uneven effort, safety discomfort, ghosting/timeout)
- Lean operational stubs:
  - Trust & Safety moderation
  - Support timeline investigation
  - Product quality monitoring

**Must-Have Capabilities:**
- Onboarding quiz and conversation starters
- Daily async match
- Blind conversation (strict no-photo pre-reveal)
- Dual-trigger decision gate (time floor + message threshold)
- Private simultaneous decisions (Reveal/Continue/Abandon)
- Outcome anonymization for non-mutual endings
- Location-based matching with precise location permission
- Upload-time photo moderation with manual fallback
- Direct APNS/FCM push only (meaningful events)
- Server-authoritative state machine + audit trail
- GDPR baseline + age gate + account deletion + policy acceptance
- Internal admin/support tools (minimal)

### Post-MVP Features

**Phase 2 (Growth):**
- Matching optimization (complementarity tuning)
- Blind conversation archive
- Optional cooldown flow
- Improved moderation tooling and ops automation
- Expanded cohort quality analytics

**Phase 3 (Expansion):**
- Expansion beyond Poland
- Advanced personalization/ranking refinement
- Richer post-reveal profile/media features
- Localization and market-specific growth capabilities

### Risk Mitigation Strategy

**Technical Risks:**
- Primary risk: trust breaks caused by inconsistent gate/reveal state.
- Mitigation: server-authoritative resolution, idempotent events, trust-critical defect release gates.

**Market Risks:**
- Primary risk: users perceive flow as slower than swipe-first alternatives.
- Mitigation: optimize time-to-first-conversation, onboarding clarity, and first-session fairness perception.

**Resource Risks:**
- Primary risk: overbuilding too early.
- Mitigation: keep operational tooling thin in MVP, enforce strict must-have scope gate, defer non-core enhancements.

**Operational UX Constraint (from your decision):**
- No connectivity, no app core flow.
- Must present clear, calm blocked-state messaging when offline so behavior is understandable and consistent.

## Functional Requirements

### Account, Identity, and Consent

- FR1: A user can create an account and sign in to access Blinder.
- FR2: A user can delete their account from within the app.
- FR3: A user can review and accept product policies required for use.
- FR4: A user can confirm legal-age eligibility before entering dating flows.
- FR5: A user can grant or deny required permissions during feature use.
- FR6: A user can update profile basics required for matching participation.

### Onboarding and Matching Inputs

- FR7: A user can complete an onboarding quiz that captures conversation-relevant traits.
- FR8: The system can generate conversation starter prompts from onboarding inputs.
- FR9: A user can provide location data for location-based matching.
- FR10: A user can upload profile photos for post-reveal profile use.
- FR11: A user can proceed through onboarding input flows only after required inputs are completed.
- FR12: A user can re-enter onboarding input flows to update match-relevant information.

### Match Lifecycle and Conversation Start

- FR13: A user can receive a new match opportunity in the app.
- FR14: A user can accept or pass on a presented match opportunity.
- FR15: The system can start a conversation window only after both matched users send an initial message.
- FR16: A user can send and receive messages during the blind phase.
- FR17: A user can view conversation context and starter prompts while in blind chat.
- FR18: A user can continue active trust-critical conversation contexts within configured limits.

### Decision Gate and Outcome Handling

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

### Safety, Moderation, and Abuse Handling

- FR27: The system can evaluate uploaded photos before they become available in product flows.
- FR28: The system can prevent disallowed media from entering user-visible flows.
- FR29: The system can route uncertain media cases for manual review.
- FR30: A user can report harmful or uncomfortable behavior encountered in conversation.
- FR31: A user can block another user from further interaction.
- FR32: The system can maintain auditable records for safety-critical and state-transition events.
- FR33: Internal trust-and-safety operators can review moderation decisions and apply corrective actions.
- FR34: A user can receive remediation or next-step guidance when media is rejected.

### Notifications and Engagement Model

- FR35: A user can receive notifications for meaningful state transitions only.
- FR36: The system can notify users about new match availability.
- FR37: The system can notify users about partner replies.
- FR38: The system can notify users when a decision gate is reached.
- FR39: The system can avoid non-essential engagement notifications unrelated to trust-critical flow events.

### Monetization and Access Control

- FR48: A user can subscribe to higher concurrent conversation capacity through premium access.
- FR49: The system can enforce configured concurrent conversation limits by subscription tier.

### Operations, Support, and Product Governance

- FR40: Internal support staff can inspect conversation state timelines for issue investigation.
- FR41: Internal support staff can communicate user-facing issue outcomes without exposing counterpart-sensitive decision details.
- FR42: Product operators can monitor core quality indicators tied to matching and conversation outcomes.
- FR43: Product operators can monitor safety and moderation indicators for risk escalation.
- FR44: Internal teams can review cohort-level behavior signals to inform scope and quality decisions.
- FR45: The system can restrict core matching and conversation actions when connectivity is unavailable and present a clear blocked state.
- FR51: Internal support staff can view decision-gate trigger and resolution history for issue diagnosis.
- FR52: Product operators can configure market-specific policy controls for feature availability.
- FR53: The system can collect a lightweight post-outcome fairness or safety pulse from users.

### Permissions and Transparency

- FR47: A user can view contextual rationale before granting precise location permission.

## Non-Functional Requirements

### Performance

- NFR1: 95% of core in-app navigation actions complete in <= 2.0 seconds under normal network conditions.
- NFR2: 95% of message send acknowledgments are returned in <= 2.5 seconds under normal network conditions.
- NFR3: 95% of decision-gate action submissions (Reveal/Continue/Abandon) receive definitive server outcome in <= 3.0 seconds under normal network conditions.
- NFR4: Match availability checks complete in <= 2.0 seconds for 95% of requests.

### Security & Privacy

- NFR5: All user data in transit is encrypted using TLS 1.2+.
- NFR6: Sensitive user data at rest is encrypted using AES-256 or equivalent in persistent storage, validated by security audit prior to production launch.
- NFR7: Pre-reveal media visibility is technically blocked across all user-facing interfaces and APIs until mutual reveal state is achieved.
- NFR8: Access to moderation, support, and operator tools is role-based and auditable.
- NFR9: GDPR baseline obligations are supported, including consent recordability, data export, account deletion, and retention policy enforcement.
- NFR10: Age-gate enforcement prevents users failing legal-age checks from entering dating flows.

### Reliability & Availability

- NFR11: Monthly availability target for core user flows (auth, matching, messaging, gate decisions) is >= 99.5%.
- NFR12: Decision-gate resolution is exactly-once from the user perspective; duplicate client submissions cannot create conflicting outcomes.
- NFR13: System failures in non-core subsystems must not compromise integrity of conversation state transitions.
- NFR14: When connectivity is unavailable, the app must block core write actions and display a clear user-facing blocked state within <= 2 seconds.

### Scalability

- NFR15: Architecture supports at least 10x growth from launch baseline without redesign of core domain model.
- NFR16: Under 5x launch baseline concurrency, p95 latency for core flows degrades by no more than 20% versus baseline.
- NFR17: Cohort/city rollout controls support phased expansion without full-system reconfiguration.

### Accessibility

- NFR18: Mobile UI for core flows meets WCAG 2.1 AA-equivalent criteria applicable to native app experiences (contrast, readable text scaling, focus order, labels).
- NFR19: All trust-critical actions (report, block, decision choices, account deletion) are operable with assistive technologies supported by iOS and Android.
- NFR20: Critical status messages (offline blocked state, conversation ended, decision required) are presented in plain language and announced accessibly.

### Integration & Interoperability

- NFR21: Push delivery integration supports APNS and FCM with retry handling and failure observability.
- NFR22: Media moderation integration supports automated decision plus manual-review fallback path with traceable decision state.
- NFR23: Integration failures (push or moderation provider outage) must fail safely and preserve trust-critical state consistency.

### Observability & Auditability

- NFR24: All trust-critical state transitions are logged with correlation identifiers to support diagnostics and support investigation.
- NFR25: Moderation and safety decisions are audit-logged with actor, timestamp, and outcome.
- NFR26: Product quality guardrail metrics (conversation depth, gate completion, reveal trend, fairness/safety pulse) are available at cohort level no later than T+1 day.
- NFR27: Support tooling can retrieve complete decision-gate trigger and resolution history for a reported conversation issue.

### UX Tone

- NFR28: The system shall use no urgency-inducing UI elements (countdown timers, pressure language, engagement counters, streak indicators) in trust-critical flows, as verified by design review against the approved UX tone guidelines.
