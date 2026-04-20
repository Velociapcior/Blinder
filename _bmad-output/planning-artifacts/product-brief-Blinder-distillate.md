---
title: "Product Brief Distillate: Blinder"
type: llm-distillate
source: "product-brief-Blinder.md"
created: "2026-04-03T15:12:33.8181663+02:00"
purpose: "Token-efficient context for downstream PRD creation"
---

# Product Brief Distillate: Blinder

## Product Thesis
- Blinder is a conversation-first blind dating platform where profile photos are hidden during the initial conversation phase and revealed only after mutual intent.
- Core promise is dignity before desirability: users get evaluated through interaction quality before visual filtering.
- Strategic intent is to build a safer and calmer alternative to swipe-first apps by redesigning the behavior loop, not just adding features.
- Launch strategy is Poland-first for focused validation, then geographic scaling after loop quality and growth signals are stable.

## Problem and Market Signals
- Mainstream dating app mechanics create rapid visual filtering and high social-performance pressure; this excludes users who do not fit dominant appearance norms.
- Target users report fatigue with dopamine-loop products and rejection-heavy dynamics (explicit acceptance/rejection cues, metricized social comparison).
- Safety skepticism is a meaningful barrier in online dating; prevention-focused architecture is a differentiator versus report-only safety models.
- Relevant external context from Pew (2023) supports pain profile:
- Many users report insecurity or overwhelm in message dynamics.
- Women under 50 report high levels of unsolicited explicit content and unwanted follow-up contact.
- Users and non-users are divided on dating app safety.

## Target Users
- Primary segment at launch: people in Poland who are tired of or repulsed by current app norms (Tinder/Hinge/Instagram-style value systems).
- Psychographic emphasis: users seeking fairness, emotional safety, and authentic conversation over visual competition.
- Core adoption trigger: first experience of being understood in conversation before appearance disclosure.
- Secondary segment (post-launch): broader users seeking lower-noise, intention-led dating in additional markets.

## Core Experience Mechanics (MVP Signals)
- Psychologically informed onboarding quiz defines conversation-relevant profile signals and generates match-start prompts.
- Async daily matching model: one best-available match surfaces per cycle with "someone is waiting" style notification.
- Soft-start conversation: timer/window begins only after both users send first message.
- Dual-trigger decision gate:
- Time floor ensures minimum breathing room.
- Message threshold can unlock earlier decision for high-energy conversations.
- Decision options at gate: Reveal, Continue, Abandon.
- Simultaneous and private decisions reduce strategic signaling and emotional pressure.
- Outcome anonymization on non-mutual endings removes explicit rejection attribution.

## Emotional Safety Architecture
- No photo exchange in blind phase.
- Photo moderation at upload is mandatory safety control.
- Minimal notification policy: only meaningful state transitions (new match, reply, decision gate).
- No rejection scoreboards, streaks, social counters, or status metrics that function as personal worth indicators.
- UI/UX tone requirement is anti-urgency and calming; product should avoid pressure-inducing visual language.

## Business Model and Monetization Signals
- Freemium model is conversation-cap based, not access-to-people based.
- Free users receive core dignity/safety experience without second-class treatment.
- Premium expands concurrent active conversation capacity and optional profile media capacity.
- Monetization sequencing principle: do not optimize revenue before trust and loop quality are stable.

## Success Signals (Non-Numeric Commitments)
- Primary first-year objective is user growth in Poland.
- Engagement quality is co-equal guardrail to growth.
- Reveal rate is primary v1 quality signal, tracked directionally (no numeric public commitment yet).
- Additional quality signals:
- Meaningful conversation depth before ending.
- User-reported emotional safety.
- Retention linked to trust/fairness rather than notification compulsion.

## Scope Signals
- In-scope for v1:
- Blind conversation loop with private decision gate.
- Onboarding quiz + conversation starters.
- Async daily matching with mutual-start behavior.
- Outcome anonymization.
- Upload-time image safety filtering.
- Minimal-notification policy.
- Conversation-capped freemium.
- Explicitly out-of-scope for v1:
- Advanced complementarity algorithm optimization beyond baseline matching.
- AI mid-conversation coaching.
- Asymmetric gender mechanics.
- Exclusivity gatekeeping or pay-to-win ranking/visibility mechanics.
- Complex dynamic profile systems during blind phase.

## Rejected Ideas and Rationale
- Asymmetric gender mechanics rejected because mission requires equal treatment and shared dignity.
- AI in-conversation suggestions rejected because they reduce authenticity and may feel manipulative.
- Forced cooldown rejected as paternalistic; optional cooldown kept as possible post-MVP aid.
- Paid barrier to entry and exclusivity rejected because they conflict with inclusivity and fairness position.
- Engagement-bait mechanics (streaks, social counters, noisy notifications) rejected because they directly oppose anti-addiction thesis.

## Go-to-Market and Launch Risk Notes
- Launch in dense local pockets in Poland to reduce cold-start liquidity risk.
- Category-language messaging should focus on conversation-before-appearance and safety-by-architecture.
- Distribution hypotheses include community and creator channels that can credibly discuss dating fatigue and emotional safety.
- Key launch risks to manage in PRD:
- Liquidity risk in early cohorts.
- Trust gap if safety claims are not visible, specific, and enforceable.
- Product drift toward mainstream engagement patterns under growth pressure.
- Premature monetization pressure that harms perceived fairness.

## Requirements Hints for PRD
- Decision gate logic needs deterministic state handling for all branches (mutual reveal, one continue, any abandon, timeout).
- Conversation window requires dual-trigger orchestration and conflict-safe event resolution.
- Expiry and pass outcomes must preserve anonymity and dignity language in UX copy.
- Safety controls need upload pipeline checks before media persistence.
- Notification layer must be tightly constrained to approved event types only.
- Analytics should capture quality signals without exposing user-level rejection scoring.
- Matchmaker service should support baseline random/heuristic logic with clear extension path to complementarity model.

## Open Questions
- Which launch cities in Poland should be prioritized first for liquidity and channel efficiency?
- What minimum moderation and trust policy disclosures are needed at onboarding to support safety positioning?
- What exact copy and UX framing should be used at decision gate to maximize clarity without pressure?
- What operational policy governs repeat low-quality behavior while preserving non-punitive brand tone?
- When and how should optional post-conversation cooldown be introduced without harming momentum?

## Longer-Term Strategic Signals
- Potential moat is trust and behavior design consistency, not algorithm mystique alone.
- Complementarity matching informed by psychological expertise is a candidate second-order differentiator after baseline validation.
- Blind conversation archive and post-reveal narrative artifacts can deepen emotional attachment and brand distinctiveness post-MVP.

## Design System Pointer
- All visual, copy, motion, and component guidance that downstream artefacts depend on lives in the packaged Blinder design system:
- `README.md` \u2014 written spec; `SKILL.md` \u2014 core invariants; `colors_and_type.css` \u2014 tokens; `ui_kits/Blinder/` \u2014 live components; `ux-design-specification.md` \u2014 full UX spec.
- Non-negotiable UX rules downstream work must honour: `--reveal` (amber) reserved for reveal only; equal-weight gate options; non-attributing endings; one primary per screen; no urgency language; confirmation dialogs only for irreversible destructive actions (account deletion, block).
