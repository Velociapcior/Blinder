---
stepsCompleted: [step-01-init, step-02-discovery, step-02b-vision, step-02c-executive-summary, step-03-success, step-04-journeys, step-05-domain, step-06-innovation, step-07-project-type, step-08-scoping, step-09-functional, step-10-nonfunctional, step-11-polish]
inputDocuments:
  - '_bmad-output/planning-artifacts/product-brief-Blinder-2026-03-11.md'
  - '_bmad-output/brainstorming/brainstorming-session-2026-03-11-blinder.md'
workflowType: 'prd'
date: 2026-03-11
author: Piotr.palej
classification:
  projectType: mobile_app
  domain: consumer_social_dating
  complexity: medium
  projectContext: greenfield
---

# Product Requirements Document - Blinder

**Author:** Piotr.palej
**Date:** 2026-03-11

## Executive Summary

Blinder is a slow dating movement and platform built on a single conviction: genuine human connection begins with personality, not appearance. The product removes the visual layer from matching and early interaction entirely — users are matched by values and personality, enter a real-time chat environment, and choose together when they are ready to reveal themselves. The photo reveal is mutual, consensual, and structurally enforced: neither party can trigger it unilaterally, and a minimum message exchange must occur first.

The primary audience is the dropout cohort — people who left existing dating apps or never joined because the photo-first, swipe-driven model eliminated them before a single conversation could begin. This cohort is the silent majority: not measured by industry analytics, not served by any existing product, and not looking for a better Tinder. They are looking for a platform that treats personality as the currency of connection.

Women are served by a distinct but equally genuine value proposition: relief from volume fatigue and objectification. No one reaches them based on their photo. Conversations begin because of algorithmic compatibility. The mechanics structurally eliminate unsolicited explicit content.

**Tagline:** *Blinder — be known for who you are.*
**Brand statement:** *Being yourself is enough. Be known for it.*

Blinder's launch strategy is Poland-first with radius-based geographic matching. Female onboarding is invite-only at launch to ensure safety perception and gender balance. The business model is freemium: free tier provides one match per day with three simultaneous active conversations; premium expands volume and lowers the user's personal reveal readiness threshold (while always preserving the other party's independent consent).

The north star metric is trust, not engagement: **>40% of active users complete at least one mutual reveal within 60 days.** Blinder measures success by the depth and courage of connection — not time spent in the app.

### What Makes This Special

Blinder's differentiator is not a feature — it is a structural consent architecture. The mutual opt-in reveal is built into the product mechanic, making unsolicited visual exposure impossible by design rather than policy. Paired with values-driven algorithmic matching and a strict no-swipe, no-browse interface, Blinder removes the two failure modes of every existing dating app simultaneously: the visual filter that eliminates people before they speak, and the volume and objectification dynamic that burns users out.

The core insight: the people the current industry has failed — those who quietly self-excluded because they believe the system is not for them — are not a niche segment. They are the majority. Blinder is the first platform built explicitly for them. Every product decision, every marketing message, every mechanic is designed to communicate: *you belong here.*

No boosts. No super likes. No pay-to-win mechanics. The only currency is personality.

### Project Classification

| Dimension | Value |
|---|---|
| **Project Type** | Mobile app (iOS + Android via React Native) with companion web app |
| **Domain** | Consumer social / dating |
| **Complexity** | Medium — GDPR special category data, mandatory CSAM detection, real-time chat infrastructure, two-sided marketplace cold-start |
| **Project Context** | Greenfield — Poland-first launch, no existing codebase |
| **Build Target** | Mobile-first (iOS + Android); web app deferred to Phase 2 (post-MVP) |

---

## Success Criteria

> **Brand North Star:** *At least one person who uses Blinder believes they are no longer lonely.* Everything else is a proxy for that. This is a brand principle, not a KPI — it is the moral test against which every product decision is evaluated.

### User Success

User success is measured through trust and courageous behaviour — not time spent in the app.

| Metric | Target | Rationale |
|---|---|---|
| Overall reveal rate (≥1 reveal within 60 days) | >40% | Primary trust signal — users trusted both the platform and themselves enough to be seen |
| Depth-qualified reveal rate (>20 messages → reveal) | >65% | Separates trust from match quality; high trust + low overall = match problem, not product problem |
| Reveal abandonment rate | Track and minimise | One party ready, other not — signals cold feet or match quality issues; product learning metric |
| Return rate post-reveal without continuation | >50% at 30 days | Emotional resilience — users came back after vulnerability; the product felt safe |
| Median messages per conversation | >20 | Conversation depth proxy; match quality signal |
| Onboarding completion | >70% | Entry point quality; users who started felt they belonged |

### Business Success

| Metric | Target | Rationale |
|---|---|---|
| Gender ratio | 40–60% (neither gender below 40%) | Two-sided marketplace health; below threshold triggers immediate action |
| 30-day retention | >35% | Core product health; users return, not just try once |
| MAU growth | Positive month-on-month | Acquisition and word-of-mouth are working |
| Freemium → premium conversion | >8% within 90 days of registration | Business viability signal |
| Cost Per Activated User (by gender) | Tracked from day one | Unit economics — acquiring women is structurally more expensive; must be understood before scaling |

**Failure threshold:** MAU decline for two consecutive months, or new user acquisition dropping below replacement rate, triggers strategic review. MAU decline + gender ratio below 40% for either gender = scale/pivot decision point.

### Technical Success

The following are hygiene baselines — hard floors, not targets. The product may not launch without them, and any breach post-launch is a P0 incident.

- **GDPR compliance posture fully documented** before first real user onboards (special category data: relationship preferences, appearance, potential inference of sexual orientation)
- **CSAM detection operational** via PhotoDNA/NCMEC hash-matching — legal agreements with NCMEC initiated immediately, not deferred
- **Content moderation pipeline live** — image scanning at upload, text flagging for harassment and explicit solicitation
- **Zero critical safety incidents** post-launch — structural consent architecture functioning as designed (no unsolicited explicit image delivery possible)
- **Real-time chat reliability** — SignalR/WebSocket infrastructure sustains message delivery under expected load for Poland-first geography

### Measurable Outcomes (90-Day MVP Validation)

The MVP is validated when **all** of the following are present after 90 days of live operation:

1. Gender ratio sustained at 40–60%
2. >40% of active users complete at least one mutual reveal
3. >35% 30-day retention
4. >50% return rate post-reveal without continuation
5. MAU growing month-on-month
6. Zero critical safety incidents

---

## Product Scope

### MVP — Minimum Viable Product

**Platform**
- Mobile app (iOS + Android via React Native), mobile-first delivery

**Onboarding**
- Values and personality quiz (interests, life priorities, what user is looking for)
- Photo upload stored privately — never visible until mutual reveal
- Basic preferences: age range, search radius
- Invite-only female onboarding via link from existing female member
- 7-day free premium trial activated on registration with explicit in-app notification

**Matching**
- Rules-based, values-driven weighted compatibility scoring from quiz data (not ML)
- Primary path: match above defined compatibility threshold
- Fallback path: demographic matching (age range + location) if pool too small
- No browsing, no swiping — curated matches only
- "No match found" threshold to be defined as a conscious product decision

**Chat**
- Real-time messaging via Microsoft SignalR and WebSockets
- Maximum 3 active conversations simultaneously (free tier)
- Message count tracked per conversation (reveal threshold foundation)

**Reveal System**
- Mutual opt-in reveal: both parties must independently choose to share
- Minimum message threshold required before reveal unlocks (exact number defined in UX phase)
- Photos exchanged simultaneously on mutual confirmation
- Unilateral reveal impossible — premium or free

**Safety Architecture**
- Image scanning at upload: Google Vision SafeSearch or Azure Content Moderator
- CSAM hash-matching: PhotoDNA / NCMEC integration (launch blocker)
- Text moderation: automated flagging of harassment and explicit solicitation
- One-tap in-app reporting with immediate queue acknowledgement
- Temporary reveal suspension for reported users pending review

**Premium Tier**
- Increased daily matches and active conversation limit
- Reduced personal reveal readiness threshold (own side only — other party consent always required)
- Conversion funnel: onboarding intro → 7-day trial → expiry warning at day 5 → limit-reached prompt
- No boosts, no super likes, no pay-to-win placement

**Analytics & Compliance**
- PostHog (self-hosted, EU data residency) from day one
- Reveal initiation, confirmation, and abandonment tracked as distinct events
- Gender ratio dashboard with near-real-time visibility
- GDPR compliance posture documented before first onboard

### Growth Features (Post-MVP)

| Feature | Rationale for Deferral |
|---|---|
| Guided conversation prompts | Valuable, not blocking core experience |
| Interest-based chat rooms | Adds complexity; not essential to core promise |
| Anti-match mechanic | Best introduced after trust established |
| Post-reveal conversation quality rating | Valuable at scale, not critical at launch |
| ML-based matching refinement | No training data at launch |
| Advanced user match analytics | Nice-to-have |
| Web app (React / PWA, mobile-optimised) | Mobile-first delivery validated first; web adds accessibility coverage and alternate acquisition channel |

### Vision (Future)

- Geographic expansion — Central/Eastern Europe first, then broader EU
- Real-time translation for cross-language matching (major TAM expansion)
- Interest-based discovery rooms — themed anonymous group spaces before 1-on-1
- ML-based matching using real match outcome data
- Partnership model for relationship coaching / mental health integrations
- Potential B2B angle: workplace or community-based slow dating cohorts

---

## User Journeys

### Journey 1: Marek — The Invisible Man (Primary Happy Path)

**Persona:** Marek, 28, Kraków. IT support. Reads widely, has real opinions, makes his friends laugh. Not conventionally attractive by the standards Tinder rewards. Tried the apps twice. Got almost no matches. Quietly concluded the problem was him.

**Opening Scene**
Marek sees a post shared by a friend — something like *"tried an app where you talk first and the photo comes later, it's actually different."* He's sceptical. He's heard "different" before. But the tagline catches him: *"Be known for who you are."* He downloads it on a Thursday evening, half-expecting to feel the same quiet rejection within twenty minutes.

**Rising Action**
Onboarding surprises him. No photo required upfront — he uploads one but is told immediately it won't be shown to anyone yet. The quiz takes maybe four minutes: what matters to him, what he's looking for, what he finds interesting. It doesn't ask him to rate his own attractiveness. When he finishes, the app tells him it's finding his best match. He goes to bed. In the morning, there's a conversation already open — someone matched to him, waiting. No swipe. No profile browse. Just a name and a first message prompt.

He types something real. She responds. The conversation goes somewhere unexpected — she asks him a follow-up question about something he said. He realises: *she's actually curious about what I think.*

**Climax**
After a week of messaging — somewhere past thirty messages — the reveal option appears. Both of them have been dancing around it. He taps "I'm ready." An hour later, she does too. Their photos appear simultaneously. He sees her. She sees him. It doesn't undo anything that came before. They keep talking.

**Resolution**
Marek's experience of dating apps has changed. Not because he got lucky — because the game was different. He was judged on something he actually has. Whether this specific connection continues doesn't determine his confidence. The *experience* already did that. He tells two friends about Blinder unprompted.

**Requirements revealed:** values quiz, private photo storage, algorithmic matching, real-time chat, message count tracking, mutual reveal mechanic, simultaneous photo exchange, push notifications.

---

### Journey 2: Kasia — The Burned-Out Woman (Primary Happy Path, Female Entry Point)

**Persona:** Kasia, 26, Warsaw. Marketing professional. Socially active, genuinely attractive, and completely exhausted by the inbox she accumulated on Tinder. Deleted it eight months ago. Doesn't miss it.

**Opening Scene**
Kasia receives an invite link from a colleague who joined Blinder two weeks earlier. The colleague mentions: *"It's invite-only for women. No unsolicited photos — structurally impossible, apparently."* That last phrase is what gets her. Not a policy. Not a promise. *Structurally impossible.* She follows the link.

**Rising Action**
The onboarding is noticeably different. No profile photo shown to matches. No browsing other users. She answers a short quiz, sets her preferences, and is dropped directly into a conversation — someone the algorithm judged compatible, not someone who swiped because of her photo. The first message isn't *"hey"*. It's a question prompted by something she said in her quiz. She finds herself answering it.

A few days in, she notices what's absent: no stream of low-effort openers, no comments about her appearance, no sense of being browsed. The conversation she's in has a texture she rarely experienced on Tinder — it feels like meeting someone at a dinner party, not being selected from a catalogue.

**Climax**
When the reveal option activates, she realises she's not nervous in the way she expected. She's curious. That's new. She opts in. He opts in. The photos arrive simultaneously. She smiles — not because of what she sees, but because of what it means: they got here together, on equal terms.

**Resolution**
Kasia tells her colleague: *"I actually enjoyed using a dating app. I didn't think that was possible."* She stays active on the platform. She refers two friends using her invite link.

**Requirements revealed:** invite-only female onboarding, invite link generation and tracking, max 3 active chats constraint (no volume problem), structural prevention of unsolicited photos, conversation-first UX, mutual reveal, post-reveal continuation flow.

---

### Journey 3: Natalia — The Woman Who Never Tried (Discovery & First-Use)

**Persona:** Natalia, 23, Gdańsk. Recent graduate, first job. Follows Instagram, is aware of "the standard," and has quietly decided she doesn't meet it. Has never downloaded a dating app. Tells herself she'll meet someone naturally.

**Opening Scene**
A Blinder ad finds Natalia on Instagram. It doesn't show beautiful people. It doesn't promise matches. It says: *"Built for people who are interesting, not just photogenic."* She stops scrolling. She reads it again. Something about it speaks directly to an assumption she's been carrying — that apps are for people who look a certain way, and she doesn't. She taps through.

**Rising Action**
The landing page reinforces the same message. She creates an account. The photo upload screen is deliberately low-pressure: *"Your photo is stored privately and only shared when you both choose to — not before."* She uploads one, small relief that it's going nowhere yet.

The quiz asks about her values and interests — things she actually has opinions on. It doesn't ask her to market herself. When it drops her into a first conversation, she feels a flutter of anxiety immediately followed by something else: *nobody has seen my face yet. This conversation starts from zero.* She types a message.

**Climax**
Three days in, Natalia is still talking to the same person. The conversation has gone deeper than she expected. At some point she thinks: *I've already told this person more about myself than I'd tell someone I'd been on two dates with.* That's the moment. Not the reveal — the realisation that she's already been brave, and it worked.

**Resolution**
Natalia doesn't think of herself the same way on the dating market anymore. The product didn't tell her she was attractive. It gave her a space where that wasn't the first question. She now actively recommends Blinder to a university flatmate.

**Requirements revealed:** targeted marketing landing page (values-first messaging), low-pressure photo upload copy, values-first quiz, immediate first-match drop on onboarding completion, no photo-first visibility.

---

### Journey 4: The Trust & Safety Moderator (Internal Operator — Manual MVP)

**Persona:** A small internal team member (at MVP: 1–2 people including the founding team) responsible for reviewing flagged content and taking moderation action.

**Opening Scene**
A user reports a conversation. They've tapped the one-tap in-app report button and selected a category (harassment). The report enters a queue with an immediate acknowledgement sent to the reporting user: *"We've received your report and are reviewing it."*

**Rising Action**
The moderator receives an alert (at MVP: email or lightweight internal notification). They review the flagged content: the conversation transcript and any images involved. Automated systems have already run — text flagging has highlighted specific messages, image scanning has checked any photos. The moderator sees automated signals plus raw content.

They make a decision: warning, temporary suspension of reveal privileges, or account ban. They execute the action via a basic admin interface (at MVP: minimal admin panel or direct action — no polished tooling required).

**Climax**
The action is applied. The reported user's reveal capability is suspended pending review. The reporting user receives a follow-up: *"We've reviewed your report and taken action."* No detail on what action — that's intentional. The moderation loop closes.

**Resolution**
The reporting user feels heard. The violating user faces consequences. The moderator has a log entry. At MVP, this process is deliberately manual and low-tech — the priority is that it *happens*, not that it scales. Tooling investment follows volume. Banned/suspended user enforcement is the resolution of this journey, not a separate flow.

**Requirements revealed:** one-tap in-app reporting with category selection, report queue with email/notification alerting, automated pre-screening (image scan + text flag) to assist manual review, reveal suspension capability, basic admin moderation actions (ban/warn/restore), user-facing report acknowledgement and follow-up notification.

---

### Journey Requirements Summary

| Capability Area | Driven By |
|---|---|
| Values quiz + personality onboarding | All primary journeys |
| Private photo storage (never shown until mutual reveal) | All primary journeys |
| Algorithmic matching (rules-based, no browse) | Marek, Natalia |
| Invite-only female onboarding + invite link system | Kasia |
| Real-time chat (max 3 active conversations) | All primary journeys |
| Message count tracking per conversation | Marek, Kasia |
| Mutual opt-in reveal with simultaneous photo exchange | Marek, Kasia |
| Push notifications | Marek |
| Marketing landing page (values-first messaging) | Natalia |
| One-tap in-app reporting with category selection | Moderator journey |
| Automated image scan + text flagging (pre-screening) | Moderator journey |
| Reveal suspension for reported users | Moderator journey |
| Basic admin moderation interface | Moderator journey |
| User-facing report acknowledgement + follow-up | Moderator journey |

---

## Domain-Specific Requirements

### Compliance & Regulatory

- **GDPR (mandatory, launch blocker)** — Dating app data qualifies as GDPR special category data: relationship preferences, appearance data, and potential inference of sexual orientation. Required before first real user onboards:
  - Documented consent flows for special category data collection
  - Data retention limits and automated deletion policy
  - Right to erasure (account deletion must purge all personal data including photos and chat history)
  - Data Protection Officer designation or equivalent (legal review required)
  - Privacy-compliant analytics stack — PostHog self-hosted, EU data residency
- **CSAM compliance (mandatory, launch blocker)** — Integration with PhotoDNA / NCMEC hash-matching. Legal agreements with NCMEC must be initiated at project start, not deferred. Any detected CSAM must be reported to the relevant authority per legal obligation.
- **Child protection / age verification** — Platform must enforce minimum age (18+). Age declaration at registration required at launch; further verification methods considered for post-MVP.

### Technical Constraints

- **Photo storage isolation** — User photos must be stored in a private, access-controlled store. Photos must never be served via public URL. Delivery only via authenticated, time-limited signed URLs after mutual reveal confirmation.
- **Content scanning pipeline** — All images scanned at upload before storage confirmation (Google Vision SafeSearch or Azure Content Moderator). Scan must block or quarantine before the image enters the system.
- **End-to-end data residency** — All user data (chat, photos, profile) stored in EU-region infrastructure to satisfy GDPR and user trust requirements for Poland-first launch.
- **Chat data retention policy** — Conversation history retention period must be defined and enforced. Account deletion must trigger full purge of chat, photos, and profile data. GDPR obligation, not optional.
- **Audit logging** — Moderation actions (reports, bans, suspensions) must be logged with timestamps and actor identity for legal defensibility.

### Risk Mitigations

| Risk | Mitigation |
|---|---|
| Unsolicited explicit image delivery | Structural — no photo delivery path exists until mutual opt-in confirmed; not policy-based |
| CSAM reaching the platform | PhotoDNA/NCMEC hash-matching at upload; legal reporting obligation met |
| Profile impersonation / fake accounts | Photo stored privately reduces incentive; moderation queue for reports; age declaration at onboarding |
| GDPR breach | EU data residency; documented retention limits; right to erasure implemented at account deletion |
| Gender ratio imbalance causing marketplace failure | Near-real-time gender ratio dashboard; invite-only female onboarding at launch as supply control |
| Cold-start pool too small for algorithm | Demographic fallback matching when compatibility threshold unmet; transparent "matching improves as more people join" message |

---

## Innovation & Novel Patterns

### Detected Innovation Areas

**1. Structural Consent Architecture (Core Innovation)**
Every existing dating platform treats content moderation as a policy layer — humans or algorithms reviewing content after it reaches users. Blinder's innovation is architectural: the product mechanic makes unsolicited explicit image delivery impossible *by construction*, not by enforcement. No photo can reach a user who did not independently opt in. This is a novel approach to a real and persistent consumer harm. It doesn't require moderation to prevent the most common harassment vector — it eliminates the delivery pathway entirely.

**2. Inversion of the Matching Funnel**
Every major dating app leads with visual filtering — browse, judge, swipe, then optionally converse. Blinder inverts this: algorithm matches, conversation begins, visual reveal is the *final* stage rather than the first. This is not a feature difference — it is a fundamentally different product philosophy that changes the user's relationship with the platform from day one. The browse-and-judge loop is not reduced; it is removed.

**3. The Dropout Cohort as Primary Target**
Every competitor in the dating app market optimises for active, retained users — which means they have been continuously refining a product for the people who stayed. Blinder is the first platform to treat the dropout cohort (self-excluded non-users) as the primary target market. This is a strategic innovation that opens a largely unaddressed total addressable market. The innovation is not technical — it is a market insight that reshapes the entire product and marketing strategy.

**4. Dual-Value Two-Sided Marketplace Design**
Most dating app marketplaces treat gender balance as a logistics problem. Blinder's design gives men and women *genuinely different but equally valid* value propositions for the same core mechanic. Men get a chance to be judged on personality; women get relief from volume and objectification. One product, two entry points, both architecturally supported.

### Market Context & Competitive Landscape

The dating app market is dominated by Tinder (swipe-first, photo-led), Hinge (photo-led with "designed to be deleted" positioning), and Bumble (photo-led with female-first messaging). All share the same philosophical foundation: visual presentation precedes conversation. Slow dating positioning exists at the margins (e.g., Thursday, Once), but none have structurally removed the photo from the matching flow.

Blinder's structural consent architecture and no-browse matching have no direct equivalent in the market. The closest analogues:
- **Hinge** — "designed to be deleted" brand alignment, but photo-first product
- **Thursday** — scarcity mechanic, but visual-first
- **Slowly** — conversation-first pen-pal app, but not a dating product

No current competitor combines: algorithmic values-matching + photo-withheld-until-mutual-reveal + no-browse interface + structural unsolicited photo prevention.

### Validation Approach

| Innovation | Validation Signal | Timing |
|---|---|---|
| Structural consent architecture resonates with women | Female registration rate + invite acceptance rate | Month 1 |
| Values-matched conversations outperform swipe conversations | Median message count vs. industry benchmark (~5 msgs avg) | Month 2–3 |
| Dropout cohort is a real and addressable market | Onboarding survey: "Have you used a dating app before?" + retention data | Month 1–3 |
| Dual value proposition drives word-of-mouth | Referral tracking by gender; organic vs. paid acquisition split | Month 3–6 |
| Reveal mechanic drives emotional differentiation | Return rate post-reveal + qualitative feedback | Month 2–3 |

### Risk Mitigation

| Innovation Risk | Mitigation |
|---|---|
| Users find no-browse frustrating — want to choose their own matches | Transparent "we match you" UX from day one; brand positioning primes expectation; early qualitative feedback loop |
| Algorithm match quality too low at small pool size | Demographic fallback matching; "matching improves as more people join" in-app messaging |
| Women don't convert from marketing interest to registration | Invite-only mechanism creates exclusivity signal; qualitative landing page testing pre-launch |
| Reveal mechanic creates anxiety rather than excitement | UX/copy investment at reveal moment; reveal abandonment rate tracked as P1 metric; iterate fast |

---

## Mobile App Specific Requirements

### Project-Type Overview

Blinder is a cross-platform mobile application built in React Native (iOS + Android). Mobile is the primary delivery surface — the product experience is designed for mobile-first interaction. A web companion is planned for Phase 2.

### Technical Architecture Considerations

- **Cross-platform:** React Native for iOS and Android from a single codebase. Native modules only where strictly required (e.g., push notification handling, camera access).
- **Web companion (Phase 2):** React Native Web or a separate React/Next.js web app — architecture decision deferred. Must share core business logic when built.
- **Real-time infrastructure:** Microsoft SignalR over WebSockets for chat. Mobile client must handle connection lifecycle (background/foreground transitions, reconnection on poor connectivity).
- **EU data residency:** All backend infrastructure in EU-region. No data transiting non-EU cloud regions for personal data.

### Platform Requirements

| Platform | Minimum OS Version | Store Target |
|---|---|---|
| iOS | iOS 16+ | Apple App Store |
| Android | Android 10 (API 29)+ | Google Play Store |
| Web *(Phase 2)* | Modern browsers (Chrome, Firefox, Safari, Edge) | N/A |

### Authentication

**At MVP:** Email/password registration + social login:
- **Apple Sign In** — mandatory for iOS when any social login is offered (App Store requirement)
- **Google Sign In**
- **Facebook Login** — covers the Meta ecosystem (Instagram login not available via official third-party OAuth and is dropped)

All social login paths must still collect age consent and complete the onboarding quiz before entering the app. Social login is for authentication only — no profile photo import, no social graph access.

### Push Notifications

Required notification types at MVP:

| Event | Notification Type |
|---|---|
| New match assigned | Push + in-app |
| Incoming message | Push + in-app |
| Match has opted into reveal | Push + in-app |
| Mutual reveal confirmed | Push + in-app |
| Premium trial expiring (day 5) | Push + in-app |
| Free tier limit reached | In-app only |
| Report acknowledgement | In-app only |

Push infrastructure: **Firebase Cloud Messaging (FCM)** for Android and **Apple Push Notification Service (APNs)** for iOS, via a unified backend abstraction (e.g., OneSignal or direct FCM/APNs integration).

### Device Features

| Feature | Usage | Required at MVP |
|---|---|---|
| Camera / Photo library | Profile photo upload | Yes |
| Location services | Search radius preference (coarse, not real-time tracking) | Yes |
| Push notifications | Match and message alerts | Yes |
| Biometric authentication | Face ID / fingerprint app lock | No (v2) |

### Store Compliance

**Content Rating:**
- Apple App Store: **17+** (dating, mature/suggestive themes)
- Google Play: **Mature 17+** (dating apps category)
- Both stores require accurate content rating declaration — misrepresentation risks removal.

**Age Gating:**
- 18+ minimum enforced at registration via age declaration
- Store content ratings apply on top of in-app enforcement

**In-App Purchases / Subscription Billing:**

| Channel | Approach | Notes |
|---|---|---|
| In-app (iOS) | Apple In-App Purchase (StoreKit) | Mandatory for digital subscriptions on iOS — Apple takes 15–30% |
| In-app (Android) | Google Play Billing | Mandatory for digital subscriptions on Android — Google takes 15–30% |
| Web | Direct payment (Stripe or equivalent) | Full revenue retained; cannot be promoted from within iOS app per Apple anti-steering rules |

**Apple anti-steering compliance:** The in-app UI must not contain links or copy directing users to the website for cheaper subscription pricing. Web subscription is available as a channel but promoted only through external marketing — not from within the iOS app.

### Offline Behaviour

At MVP: **No offline mode.** The core experience (matching, chat, reveal) is inherently real-time and requires connectivity. The app must degrade gracefully — show cached conversation list on poor connectivity, queue outbound messages for retry, display a connectivity status indicator.

### Implementation Considerations

- **Deep linking:** Required for invite links (female onboarding invite flow), push notification tap-through, and web-to-app handoff
- **App versioning:** Semantic versioning; forced update mechanism for breaking backend changes
- **Accessibility:** WCAG 2.1 AA target for core flows (onboarding, chat, reveal) — aligned with inclusive brand positioning
- **Localisation:** Polish (pl-PL) as primary locale at launch; English (en) as fallback. i18n architecture required from day one to support future geographic expansion.

---

## Project Scoping & Phased Development

### MVP Strategy & Philosophy

**MVP Approach:** Experience MVP — the product's value is the complete emotional arc: match → conversation → reveal. This arc cannot be further reduced without undermining the core thesis. Every MVP feature exists to protect the integrity of that arc.

**Primary delivery surface:** Mobile app (iOS + Android via React Native). This is where the target audience lives and where word-of-mouth operates.

### MVP Feature Set (Phase 1)

**Core User Journeys Supported:**
- Marek: values quiz → matching → chat → mutual reveal
- Kasia: invite link → onboarding → chat → mutual reveal
- Natalia: marketing discovery → onboarding → first conversation
- Moderator: report receipt → review → action → user notification

**Must-Have Capabilities:**
- Mobile app (iOS + Android, React Native)
- Email/password + social login (Apple, Google, Facebook)
- Values and personality quiz onboarding
- Private photo storage (never visible until mutual reveal)
- Invite-only female onboarding via invite link
- 7-day free premium trial on registration
- Rules-based algorithmic matching (values-weighted + demographic fallback)
- Real-time chat (SignalR/WebSockets, max 3 active conversations free tier)
- Message count tracking per conversation
- Mutual opt-in reveal with simultaneous photo exchange
- Push notifications (FCM/APNs)
- Premium tier (increased limits, reduced personal reveal threshold)
- Image scanning at upload (Google Vision SafeSearch or Azure Content Moderator)
- CSAM hash-matching (PhotoDNA/NCMEC — launch blocker)
- Text moderation (automated flagging)
- One-tap in-app reporting + basic admin moderation interface
- PostHog analytics (self-hosted, EU data residency)
- Gender ratio dashboard
- GDPR compliance posture documented before first onboard
- Polish (pl-PL) localisation + English fallback
- WCAG 2.1 AA accessibility for core flows

### Phase 2 (Post-MVP)

- Web app (React / PWA, mobile-browser optimised)
- Guided conversation prompts
- Anti-match mechanic (opt-in challenge to preference bias)
- Post-reveal conversation quality rating
- ML-based matching refinement (once training data exists)
- Biometric app lock (Face ID / fingerprint)
- Advanced user match analytics

### Phase 3 (Expansion)

- Geographic expansion (Central/Eastern Europe → broader EU)
- Real-time translation for cross-language matching
- Interest-based discovery rooms
- Partnership model (relationship coaching / mental health)
- Potential B2B angle (workplace / community cohorts)

### Risk Mitigation Strategy

**Technical Risks:** Real-time chat infrastructure and safety pipeline are the highest-complexity MVP components. Mitigated by technology selection (SignalR, proven content scanning APIs) and early technical spike recommended before committing to implementation timeline.

**Market Risks:** Gender ratio cold-start is the primary marketplace risk. Invite-only female onboarding is the structural mitigation; a pre-launch waitlist strategy for women is recommended to ensure supply before male demand is opened.

**Compliance Risks:** NCMEC legal agreements and GDPR compliance posture are launch blockers with non-zero lead time. Both must be initiated at project start — not when development is nearly complete.

---

## Functional Requirements

### User Account Management

- **FR1:** A new user can register with email/password
- **FR2:** A new user can register using social login (Apple, Google, or Facebook)
- **FR3:** A user can log in and log out of their account
- **FR4:** A user can delete their account, which triggers permanent erasure of all personal data (photos, chat history, profile data)
- **FR5:** A user must declare they are 18 or older during registration
- **FR6:** A user can update their preferences (age range, search radius)
- **FR7:** A female user can only complete registration via a valid invite link from an existing female member

### Onboarding & Profile

- **FR8:** A new user can complete a values and personality quiz covering interests, life priorities, and relationship intent
- **FR9:** A new user can upload a profile photo, which is stored privately and never displayed until mutual reveal is confirmed
- **FR10:** A new user is placed into a first match conversation immediately upon completing onboarding
- **FR11:** A registered user receives a 7-day free premium trial automatically activated on registration

### Matching

- **FR12:** The system matches users using a rules-based, values-weighted compatibility algorithm derived from quiz responses
- **FR13:** When no match meets the compatibility threshold, the system falls back to demographic matching (age range + location)
- **FR14:** A user receives curated matches only — no browsing or searching of other user profiles is available
- **FR15:** A user can set a search radius to control geographic matching scope
- **FR16:** The system generates and tracks unique invite links associated with individual female user accounts

### Chat & Conversation

- **FR17:** A matched user can send and receive real-time text messages within a conversation
- **FR18:** A user can have a maximum of 3 active conversations simultaneously (free tier)
- **FR19:** The system tracks the total message count per conversation
- **FR20:** A user can view their active conversations and their respective message counts
- **FR21:** A user receives push notifications for new messages, new matches, and reveal-related events

### Reveal System

- **FR22:** A user can express readiness to reveal their photo to their match
- **FR23:** The reveal option is only available after the conversation has reached the minimum message threshold
- **FR24:** A user's photo is only delivered to their match after both parties have independently expressed reveal readiness
- **FR25:** Both users receive each other's photos simultaneously upon mutual reveal confirmation
- **FR26:** Neither user can view the other's photo before mutual reveal confirmation, regardless of subscription tier
- **FR27:** A premium user can lower their personal reveal readiness threshold (their side only — the other party's independent consent remains mandatory)

### Subscription & Premium

- **FR28:** A user can subscribe to a premium tier via in-app purchase (Apple In-App Purchase / Google Play Billing)
- **FR29:** A premium user has access to an increased daily match allowance and higher active conversation limit
- **FR30:** A user receives an in-app notification when their free premium trial is approaching expiry
- **FR31:** A user receives an in-app prompt to upgrade when they reach their free tier conversation or match limit

### Safety & Content Moderation

- **FR32:** All user-uploaded images are scanned for explicit content before being stored or delivered
- **FR33:** All user-uploaded images are scanned against CSAM hash databases before being stored or delivered
- **FR34:** The system automatically flags text messages containing harassment patterns or explicit solicitation
- **FR35:** A user can report a conversation or message with a single tap, selecting a report category
- **FR36:** A reporting user receives an immediate acknowledgement that their report has been received
- **FR37:** A reporting user receives a follow-up notification when their report has been reviewed and actioned
- **FR38:** A moderator can view flagged reports including conversation content and automated screening signals
- **FR39:** A moderator can apply a warning, reveal suspension, or account ban to a reported user
- **FR40:** A reported user's reveal capability is suspended pending moderation review upon report submission

### Analytics & Compliance

- **FR41:** The system records reveal initiation, mutual reveal confirmation, and reveal abandonment as distinct trackable events
- **FR42:** An operator can view a near-real-time gender ratio dashboard
- **FR43:** A user can request export of their personal data (GDPR right of access)
- **FR44:** A user's data deletion request results in permanent erasure of all personal data within a defined retention window (GDPR right to erasure)
- **FR45:** The system records a timestamped audit log entry for every moderation action taken

---

## Non-Functional Requirements

### Performance

- **NFR1:** Chat message delivery (send → receive) completes within 500ms under normal network conditions
- **NFR2:** App launch to first interactive screen completes within 3 seconds on a mid-range device (e.g., equivalent to iPhone 12 / Android with 4GB RAM)
- **NFR3:** Image upload and content scanning pipeline completes within 10 seconds before confirming upload success to the user
- **NFR4:** Reveal photo delivery (both photos exchanged simultaneously) completes within 3 seconds of mutual confirmation
- **NFR5:** Matching algorithm produces a match result within 30 seconds of onboarding completion
- **NFR6:** API response time for all non-media endpoints is under 300ms at the 95th percentile for Poland-region users

### Security

- **NFR7:** All personal data (profile, chat history, photos) is encrypted at rest using AES-256 or equivalent
- **NFR8:** All data in transit uses TLS 1.2 or higher
- **NFR9:** Profile photos are stored in a private, access-controlled object store and served only via authenticated, time-limited signed URLs — no public URL access permitted
- **NFR10:** Authentication tokens expire and require re-authentication after 30 days of inactivity
- **NFR11:** All CSAM scanning occurs server-side before image storage — no CSAM material may be persisted at any point
- **NFR12:** Moderation audit logs are retained for a minimum of 2 years and are tamper-evident
- **NFR13:** GDPR special category data (relationship preferences, appearance data) is stored only in EU-region infrastructure
- **NFR14:** Payment processing must not store raw card data — all payment handling delegated to Apple/Google billing or Stripe (PCI DSS compliance delegated to the payment provider)

### Scalability

- **NFR15:** The backend infrastructure must support 10,000 concurrent users in the Poland-first launch geography without performance degradation beyond NFR6 thresholds
- **NFR16:** The matching system must sustain match generation throughput as user pool grows to 100,000 registered users without requiring architectural changes
- **NFR17:** The real-time chat infrastructure must support horizontal scaling to accommodate MAU growth beyond initial launch targets
- **NFR18:** The content scanning pipeline must process images within SLA (NFR3) at up to 1,000 concurrent uploads

### Accessibility

- **NFR19:** Core user flows (onboarding, chat, reveal) meet WCAG 2.1 AA compliance
- **NFR20:** All interactive elements have accessible labels compatible with VoiceOver (iOS) and TalkBack (Android)
- **NFR21:** The app supports dynamic text sizing (iOS Dynamic Type / Android font scaling) without breaking core UI layouts
- **NFR22:** Colour contrast ratios meet WCAG AA minimums (4.5:1 for normal text, 3:1 for large text)

### Reliability

- **NFR23:** Core services (chat, matching, reveal) target 99.5% uptime measured monthly
- **NFR24:** Undelivered messages (due to connectivity loss) are queued and delivered when connectivity is restored, with no message loss
- **NFR25:** The content scanning pipeline must have a fallback behaviour on third-party API failure — images must not be accepted into the system if scanning cannot be confirmed
- **NFR26:** The CSAM detection pipeline failure must trigger an immediate alert to the operations team — silent failure is not acceptable

### Integration

- **NFR27:** Apple In-App Purchase and Google Play Billing integrations must process subscription state changes (purchase, renewal, cancellation) within 60 seconds of the platform event
- **NFR28:** Push notification delivery (FCM/APNs) must achieve >95% delivery rate for time-sensitive events (new message, reveal confirmation) within 60 seconds
- **NFR29:** PostHog event tracking must capture all defined analytics events with <1% event loss rate
- **NFR30:** NCMEC/PhotoDNA integration must be tested with known hash sets prior to launch — detection accuracy cannot be validated in production for the first time
