---
stepsCompleted: [1, 2, 3, 4, 5, 6]
inputDocuments:
  - '_bmad-output/brainstorming/brainstorming-session-2026-03-11-blinder.md'
date: 2026-03-11
author: Piotr.palej
---

# Product Brief: Blinder

## Executive Summary

Blinder is a slow dating platform built on a radical conviction: that genuine human connection begins with personality, not appearance. In a world where dating apps optimize for dopamine-driven swipe loops and social-media-grade photo profiles, Blinder inverts the model entirely — removing the visual layer until both people choose to reveal themselves, having already connected as people.

Blinder's primary users are the overlooked majority: people who believe they have 0% chance in the current "Instagram-filtered, photo-decided" dating landscape. The dropout cohort — those who never joined an app, or quit in the first weeks because the experience was humiliating. Blinder doesn't try to be a better Tinder. It exists to make the Tinder model obsolete.

---

## Core Vision

### Problem Statement

Modern dating apps are designed to profit from loneliness, not solve it. By leading with photos and engineering swipe mechanics around dopamine reward loops, they filter matches almost entirely on appearance — a standard shaped by heavily curated, unrealistic social media imagery. "Ugly" is deeply subjective, yet the photo-first model applies it as an absolute filter before any conversation can begin. This eliminates the majority of users before they're ever given a chance to be known.

The damage is not just personal. Loneliness is a societal crisis. Birth rates are falling. The proportion of adults — particularly men over 30 — without romantic partners is at historic highs. Dating technology, which should be helping people connect, is actively making this worse by rewarding performance over personality and cycling users back into the app rather than toward lasting connection.

### Problem Impact

- Widespread dating anxiety, particularly among young people measuring themselves against impossible social media beauty standards
- The majority of potential users — those who don't fit the narrow definition of conventionally attractive — self-exclude before they're ever given a chance to be known
- The commodification of romantic connection: apps are incentivized to keep users returning to the market, not to help them leave it
- The erosion of depth in human contact — quick dopamine hits replacing the slow build of real intimacy
- A measurable societal cost: declining birth rates, rising loneliness statistics, and a growing cohort of people who feel fundamentally unlovable by design
- Women experience a different but equally damaging problem: volume fatigue and objectification — hundreds of low-effort messages from strangers who only saw a face

### Why Existing Solutions Fall Short

Every major dating app — Tinder, Hinge, Bumble — is built on the same philosophical foundation: present yourself as a product and let others judge you by your packaging. Even apps that claim depth still lead with photos and optimize for engagement, not outcomes. The business model depends on users cycling back — which means the incentive is structurally opposed to solving the actual problem.

Women are not winning the current system either — they're just more visibly present in it. They're exhausted by volume, objectification, and being treated as a menu item. The "satisfied user base" of major dating apps is a myth for both genders, just differently experienced.

The dropout cohort — the people who left or never joined — is invisible in engagement metrics. The industry optimizes only for users who stayed, which means it has been continuously refining a product for the conventionally attractive and socially confident, while the majority quietly exits and nobody measures the churn.

### Proposed Solution

Blinder removes the visual layer from matching and early connection entirely. Users are matched by values and personality — no browsing, no swiping. They enter an intimate chat environment with a friction-minimised onboarding (quick quiz, photo upload held privately, preferences, then immediately into conversation). Visual reveal is consensual, mutual, and earned — unlocked only after both parties opt in, with a minimum message exchange threshold. Neither person can trigger the reveal unilaterally.

The location system allows users to search within a chosen radius or expand further — local by default, global by choice. At launch, female onboarding is invite-only, and marketing leads explicitly with the female value proposition: relief from the volume and objectification of current apps.

Safety is architectural, not policy-based. Every image is scanned before delivery (SafeSearch/Azure Content Moderator). CSAM detection via PhotoDNA/NCMEC hash-matching is mandatory from day one. Text moderation flags harassment and grooming patterns automatically. The mutual opt-in reveal system structurally prevents unsolicited explicit images — no photo can reach someone who didn't explicitly ask for it.

### Key Differentiators

- **Personality-first matching**: No swiping. Values-driven algorithmic matching removes the browse-and-judge loop entirely
- **Equal mutual opt-in reveal**: Photos unlock only when both parties consent, after a minimum message threshold — a structural consent architecture, not a policy
- **Dual value proposition**: Men get a chance to be judged on who they are; women get relief from volume fatigue and objectification. One product, two genuine entry points
- **Flat playing field**: No boosts, no super likes, no pay-to-win. The only currency is personality
- **Safety by design**: Explicit content is structurally impossible to send unsolicited — by product mechanic, not moderation policy
- **Location flexibility**: Local-first with expandable radius — connection without geographic constraint
- **Slow dating as brand identity**: Explicitly anti-swipe, anti-hookup, anti-dopamine-loop
- **Built for the dropout cohort**: The first dating platform designed for people who believe they have no chance in the current system — the normal people

---

## Target Users

### Primary Users

#### Persona 1 — "The Invisible Man"
**Representative Profile:** Marek, 28, Kraków. Works a standard office job — IT support, logistics, administration. Comfortable but not flashy. Has real depth: reads, has opinions, makes his friends laugh. Not conventionally attractive by the narrow standards Instagram and Tinder reward.

**His experience of the problem:** Tried the apps. Spent an evening putting his best photos together, wrote a bio he was actually proud of. Got almost no matches. The few he got never replied. He quietly concluded the problem was him — his face, his body, his lack of status signals. He deleted the app and told himself dating apps just "aren't for people like me." He hasn't tried again. He's not depressed about it — he's just accepted a quiet background assumption that he is not competitive in the modern dating market.

**What Blinder does for him:** Removes the filter that eliminated him before he could open his mouth. His personality — the thing he actually has to offer — becomes the first and primary signal. He doesn't need to win a beauty contest to get a conversation. He just needs to be interesting. For the first time, the game is fair.

**His success moment:** The first time a conversation goes deep — someone laughs at something he said, asks a follow-up, is genuinely curious about him. Before he's shown anyone his face. He realizes: *I can do this.*

---

#### Persona 2 — "The Burned-Out Woman"
**Representative Profile:** Kasia, 26, Warsaw. Works in marketing, socially active, genuinely attractive — but exhausted. She was on Tinder for two years. Her inbox was full every day. Ninety percent of it was "hey," unsolicited comments about her appearance, or men who clearly only looked at her photo. The conversations that did start rarely went anywhere real. She felt like a product being browsed, not a person being met. She deleted the app eight months ago. She doesn't miss it.

**Her experience of the problem:** The volume problem is noise masking a signal problem. She could get matches — but she couldn't find *quality*. Every interaction started from the same place: her appearance was the entry ticket, and most men who swiped right had nothing else to say once they got past it. The objectification wasn't always overt. It was structural — built into the mechanics of how every app works.

**What Blinder does for her:** Eliminates the dynamic entirely. No one reaches her based on her photo. Conversations start because there's actual compatibility signalled by the matching algorithm. The message-minimum before reveal means anyone who's talking to her has already invested enough to say something real. She's not being browsed. She's being spoken to.

**Her success moment:** A conversation that feels like meeting someone at a dinner party — natural, curious, reciprocal — before either of them knows what the other looks like. She thinks: *this is what it was supposed to feel like.*

---

#### Persona 3 — "The Woman Who Never Tried"
**Representative Profile:** Natalia, 23, Gdańsk. University graduate, just started work. Follows Instagram, is aware of what "the standard" looks like, and has quietly decided she doesn't meet it. Not clinically insecure — just living with a constant low-level assumption that the dating app world is for people who look a certain way, and she doesn't. She has never downloaded a dating app. She tells herself she'll "meet someone naturally." She hasn't yet.

**Her experience of the problem:** Self-exclusion before first use. She never needed a bad experience to be deterred — the ambient cultural message of every dating app ("your photo is your value") was enough. She is the invisible dropout that no app's analytics shows, because she never created an account to churn.

**What Blinder does for her:** The marketing message reaches her directly — *"built for people who are interesting, not just photogenic"* — and she feels, for the first time, like a platform is speaking to her specifically. The no-photo-first mechanic removes the barrier that kept her out. She signs up not despite being nervous, but because the product's design explicitly tells her she belongs here.

**Her success moment:** Getting through onboarding without being asked to compete visually. Realising she's already in a conversation with someone, and it's going well, and nobody has judged her on her appearance yet.

---

### Secondary Users

None identified at this stage. No meaningful admin, oversight, or influencer user group required for initial product scope.

---

### User Journey

#### Discovery
- **Men**: Word of mouth from friends who share the frustration ("there's an app where you talk first and photos come later"), social content that frames Blinder as the anti-Tinder, targeted ads speaking to the feeling of being invisible on current apps.
- **Women (burned-out)**: Explicit female-focused marketing: *"No 'hey.' No unsolicited photos. By design."* Reaches them where they are — likely Instagram, TikTok — with messaging that names their specific exhaustion.
- **Women (never tried)**: Same female marketing channel. The message *"you don't have to compete with your photo"* intercepts them before they'd ever self-select into the existing market.
- **At launch**: Female users onboard via invite-only link from existing female members. This signals curation and safety before they've seen a single feature.

#### Geographic Scope
Poland-first launch, with radius-based matching. Users set their search distance — local by default (city/region), expandable by choice. This contains the cold-start problem to a manageable geography while proving the model before any expansion.

#### Onboarding
Intentionally fast and frictionless:
1. Short values/personality quiz (interests, what you're looking for, life priorities)
2. Photo upload — stored privately, never shown until mutual opt-in
3. Basic preferences (age range, search radius)
4. Immediately dropped into a first match conversation

No profile browsing. No swiping. You arrive, and someone is already there to talk to. The first 90 seconds must communicate: *you belong here.*

#### Core Usage
- Max 3 active chats at any time — scarcity creates intentionality
- Guided conversation prompts available if stuck
- Reveal unlock available only after both parties opt in AND a minimum message threshold is reached — the exact threshold to be validated during product development
- Location radius adjustable at any time

#### The Reveal Moment
Both users independently choose "I'm ready to share my photo." Once both have opted in and the message minimum is met, photos are exchanged simultaneously. This is the emotional centrepiece of the product — it must feel like a moment, not a button click. Design and copy around this interaction are critical.

#### Long-term Retention / Success
A user who has formed a genuine connection and chosen to reveal — regardless of outcome — has had an experience no other dating app offers. Even if that specific match doesn't develop further, they have felt what it's like to be valued for who they are. That experience is what drives word of mouth, return engagement, and ultimately the cultural shift Blinder is building toward.

---

## Success Metrics

### What Success Means for Blinder

Blinder's philosophy of success is deliberately humble: the product is a facilitator, not the hero. When a user forms a connection, that is *their* achievement. Blinder's job is to remove the barriers that would have stopped them from ever trying. Success metrics therefore focus on trust, courageous behaviour, and emotional safety — not on vanity numbers.

The north star: *at least one person who uses Blinder believes they are no longer lonely.* Everything else is a proxy for that.

---

### User Success Metrics

**Primary Trust Indicator — Reveal Rate**
The number of times a user initiates or completes a mutual reveal is the single most meaningful signal that they trust the platform and trust themselves. Tracked as two sub-metrics to separate trust from match quality:

- **Overall reveal rate**: % of active users who complete at least one mutual reveal within 60 days — target >40%
- **Depth-qualified reveal rate**: % of users who exchanged >20 messages and then completed a reveal — target >65%. If this number is high but overall reveal is low, the problem is match quality, not platform trust. If both are low, the product has a trust problem.

**Reveal Abandonment Rate**
Reveals initiated by one party but never confirmed by the other. High abandonment signals either match quality issues or cold feet at the critical moment. Tracked as a distinct event from reveal completion to enable targeted product learning.

**Return After Reveal**
A user who returned to the app after a reveal did not lead to a continued connection is proof of product resilience. They trusted the experience enough to try again.

- Target: >50% of users who complete a reveal without match continuation remain active 30 days later

**Conversation Depth**
Message count per conversation as a proxy for genuine engagement.

- Target: Median conversation length >20 messages before reveal or end of chat

**Onboarding Completion**
- Target: >70% of users who begin onboarding complete it through to first match

---

### Business Objectives

**Launch Phase (Poland, Months 1–3): Establish User Base**
- Build sufficient user density in priority cities (Warsaw, Kraków, Wrocław, Gdańsk)
- Achieve and maintain gender ratio of 45–55% (neither gender below 40% of total)
- Female users onboard via invite-only at launch to ensure quality and safety perception
- Track **Cost Per Activated User by gender** from day one — acquiring women is structurally more expensive; unit economics must be understood before scaling

**Months 3–12: Retention and Monetisation**
- MAU growth (month-on-month positive) as primary growth indicator
- 30-day retention rate: target >35%
- Freemium conversion rate: target >8% of active users convert to premium within 90 days of registration

**Failure Threshold**
MAU decline for two consecutive months, or new user acquisition dropping below replacement rate, triggers strategic review. Churn without return is the primary danger signal.

---

### Key Performance Indicators

| KPI | Target | Why It Matters |
|-----|--------|----------------|
| Overall reveal rate (≥1 reveal within 60 days) | >40% | Primary trust signal |
| Depth-qualified reveal rate (>20 msgs → reveal) | >65% | Separates trust from match quality |
| Reveal abandonment rate | Tracked, minimise | Product learning metric |
| Return rate post-reveal (no continuation) | >50% at 30 days | Emotional resilience of product |
| Gender ratio | 40–60% any gender | Marketplace health |
| Onboarding completion | >70% | Entry point quality |
| 30-day retention | >35% | Core product health |
| Median messages per conversation | >20 | Conversation depth / match quality |
| Freemium → premium conversion | >8% within 90 days | Business viability |
| MAU growth (month-on-month) | Positive | Sustainable growth signal |
| Cost Per Activated User by gender | Tracked from day one | Unit economics health |

---

### Monetisation Model

**Free Tier:** 1 new match per day, maximum 3 active conversations simultaneously.

**Premium Tier:** Increased daily matches, higher active conversation limit. Premium users also gain the ability to lower their personal message threshold for reveal readiness — they can signal readiness to reveal earlier, but the other party's independent consent remains mandatory regardless of tier. Blinder never creates pressure to reveal.

**Premium Conversion Funnel (4 touchpoints):**
1. **Anticipatory** — premium features introduced during onboarding ("here's what premium unlocks")
2. **Trial activation** — 7-day free premium trial activated on registration, with explicit in-app notification: *"We've unlocked a free week of premium for you — no card needed, no commitment. Just explore."*
3. **Expiry warning** — notification 2–3 days before trial ends: *"Your free premium week ends in 3 days"*
4. **Reactive prompt** — when free tier limit is hit, upgrade prompt framed as expansion ("unlock more connections"), never as removal

> ⚠️ **Design Decision Pending:** Whether premium users can reduce the reveal threshold unilaterally (on their side only) or whether it reduces the shared threshold requires careful UX design to ensure it never creates implicit pressure on non-premium users. To be resolved in PRD/UX design phase.

**Philosophical guardrail:** No boosts. No super likes. No pay-to-win mechanics that give premium users priority placement or visibility over free users. Premium buys volume and speed — never advantage over other people.

---

### Compliance Prerequisite

**GDPR — Launch Blocker, Not Backlog Item**
Dating app data falls under GDPR special category data (relationship preferences, appearance data, potential inference of sexual orientation). A documented GDPR compliance posture — including consent flows, data retention limits, right to erasure, and a Data Protection Officer or equivalent — is required before the first real user is onboarded.

Analytics data stays within the main EU-region PostgreSQL database — no third-party analytics services, fully GDPR-compliant by default.

---

## MVP Scope

### Core Features (v1 — 12-Month Target)

**Platform**
- Mobile app (iOS + Android via React Native) and web app at launch
- Mobile-first delivery — web app may follow 6-8 weeks after mobile if needed
- Poland-only geography with location-radius matching

**Onboarding**
- Values and personality quiz (interests, life priorities, what user is looking for)
- Photo upload — stored privately, never visible until mutual reveal
- Basic preferences: age range, search radius
- Invite-only female onboarding mechanism (invite link from existing female member)
- 7-day free premium trial activated on registration with explicit in-app notification

**Matching Algorithm**
- Rules-based, values-driven matching using weighted compatibility scoring from quiz data — not ML. Designed to be replaceable as data grows.
- Primary path: values-algorithm match above a defined compatibility threshold
- Fallback path: if no primary match found (pool too small or no threshold met), fall back to demographic matching (age range + location)
- Users see "We're finding your best match" — algorithm path not exposed
- Transparent community message: matching improves as more people join
- No browsing, no swiping — users receive curated matches only
- Exact "no match found" threshold to be defined as a conscious product decision in the PRD phase

**Chat**
- Real-time messaging built in-house using **Microsoft SignalR and WebSockets**
- Full control over data residency — aligns with GDPR / EU data requirements
- Maximum 3 active conversations simultaneously (free tier)
- Message count tracked per conversation (reveal threshold logic foundation)

**Reveal System**
- Mutual opt-in reveal: both parties must independently choose to share
- Minimum message threshold required before reveal is unlocked (exact number to be determined in UX/PRD phase)
- Photos exchanged simultaneously upon mutual confirmation
- Neither party can trigger reveal unilaterally — premium or free

**Safety Architecture**
- Image scanning at upload: Google Vision SafeSearch or Azure Content Moderator
- CSAM hash-matching: PhotoDNA / NCMEC integration (mandatory, launch blocker — legal agreements with NCMEC to be initiated immediately, not at month 11)
- Text moderation: automated flagging of harassment and explicit solicitation
- One-tap in-app reporting with immediate queue acknowledgement
- Temporary reveal suspension for reported users pending review

**Premium Tier**
- Increased daily matches
- Higher active conversation limit
- Reduced personal reveal readiness threshold (own side only — other party consent always required)
- Conversion funnel: onboarding intro → 7-day trial → expiry warning at day 5 → limit-reached prompt
- No boosts, no super likes, no pay-to-win placement advantages

**Analytics & Compliance**
- Reveal initiation, confirmation, and abandonment tracked as distinct DB-stored events
- Gender ratio dashboard with near-real-time visibility
- GDPR compliance posture fully documented before first user onboarded

---

### Out of Scope for MVP

| Feature | Rationale | Target |
|---------|-----------|--------|
| Guided conversation prompts | Valuable, not blocking core experience | v2 |
| Interest-based chat rooms | Adds complexity, not essential to core promise | Post-MVP |
| Real-time translation / international matching | Poland-only launch makes this irrelevant | Post-MVP |
| Anti-match mechanic (opt-in, outside stated type) | Best introduced after trust established | v2 |
| Post-reveal conversation quality rating | Valuable at scale, not critical at launch | v2 |
| ML-based matching | No training data at launch; rules-based ships first | Post-MVP |
| Advanced user match analytics | Nice-to-have, not core to problem | v2 |

---

### MVP Success Criteria

The MVP is validated when all of the following are present after 90 days of live operation:

1. **Gender ratio sustained** at 40–60% — two-sided marketplace is viable
2. **>40% of active users complete at least one mutual reveal** — core mechanic is trusted
3. **>35% 30-day retention** — users return, not just try once
4. **>50% return rate post-reveal without continuation** — emotional safety is real
5. **MAU growing month-on-month** — acquisition and word-of-mouth are working
6. **Zero critical safety incidents** — CSAM detection, content blocking, and moderation pipeline functioning as designed

**Scale/pivot decision point:** MAU decline for two consecutive months OR gender ratio below 40% for either gender triggers strategic review before further investment.

---

### Future Vision (v2 and Beyond)

**v2 Feature Candidates (post-MVP validation)**
- Guided conversation prompts — structured topic suggestions when conversation stalls
- Anti-match mechanic — opt-in challenge to stated preference bias
- Post-reveal conversation quality rating — accountability layer for interaction quality
- ML-based matching refinement using real match outcome data
- Advanced onboarding personalisation based on early user behaviour

**Longer-Term Expansion (12–24 months, contingent on MVP success)**
- Geographic expansion — Central/Eastern Europe first, then broader EU
- Real-time translation for cross-language matching (major TAM expansion)
- Interest-based discovery rooms — themed anonymous group spaces before 1-on-1
- Partnership model for mental health / relationship coaching integrations
- Potential B2B angle: workplace or community-based slow dating cohorts

**The long game**
Blinder's ultimate ambition is not to be a bigger dating app — it is to become the cultural reference point for a different way of meeting people. Every roadmap decision should be evaluated against that mission: does this feature make genuine connection more likely, or does it make the app more addictive? Those are not the same thing, and Blinder should never confuse them.
