---
stepsCompleted: [step-01-validate-prerequisites, step-02-design-epics, step-03-create-stories, step-04-final-validation]
inputDocuments:
  - '_bmad-output/planning-artifacts/prd.md'
  - '_bmad-output/planning-artifacts/architecture.md'
  - '_bmad-output/planning-artifacts/ux-design-specification.md'
---

# Blinder - Epic Breakdown

## Overview

This document provides the complete epic and story breakdown for Blinder, decomposing the requirements from the PRD, UX Design, and Architecture into implementable stories.

## Requirements Inventory

### Functional Requirements

FR1: A new user can register with email/password
FR2: A new user can register using social login (Apple, Google, or Facebook)
FR3: A user can log in and log out of their account
FR4: A user can delete their account, which triggers permanent erasure of all personal data (photos, chat history, profile data)
FR5: A user must declare they are 18 or older during registration
FR6: A user can update their preferences (age range, search radius)
FR7: A female user can only complete registration via a valid invite link from an existing female member
FR8: A new user can complete a values and personality quiz covering interests, life priorities, and relationship intent
FR9: A new user can upload a profile photo, which is stored privately and never displayed until mutual reveal is confirmed
FR10: A new user is placed into a first match conversation immediately upon completing onboarding
FR11: A registered user receives a 7-day free premium trial automatically activated on registration
FR12: The system matches users using a rules-based, values-weighted compatibility algorithm derived from quiz responses
FR13: When no match meets the compatibility threshold, the system falls back to demographic matching (age range + location)
FR14: A user receives curated matches only — no browsing or searching of other user profiles is available
FR15: A user can set a search radius to control geographic matching scope
FR16: The system generates and tracks unique invite links associated with individual female user accounts
FR17: A matched user can send and receive real-time text messages within a conversation
FR18: A user can have a maximum of 3 active conversations simultaneously (free tier)
FR19: The system tracks the total message count per conversation
FR20: A user can view their active conversations and their respective message counts
FR21: A user receives push notifications for new messages, new matches, and reveal-related events
FR22: A user can express readiness to reveal their photo to their match
FR23: The reveal option is only available after the conversation has reached the minimum message threshold
FR24: A user's photo is only delivered to their match after both parties have independently expressed reveal readiness
FR25: Both users receive each other's photos simultaneously upon mutual reveal confirmation
FR26: Neither user can view the other's photo before mutual reveal confirmation, regardless of subscription tier
FR27: A premium user can lower their personal reveal readiness threshold (their side only — the other party's independent consent remains mandatory)
FR28: A user can subscribe to a premium tier via in-app purchase (Apple In-App Purchase / Google Play Billing)
FR29: A premium user has access to an increased daily match allowance and higher active conversation limit
FR30: A user receives an in-app notification when their free premium trial is approaching expiry
FR31: A user receives an in-app prompt to upgrade when they reach their free tier conversation or match limit
FR32: All user-uploaded images are scanned for explicit content before being stored or delivered
FR33: All user-uploaded images are scanned against CSAM hash databases before being stored or delivered
FR34: The system automatically flags text messages containing harassment patterns or explicit solicitation
FR35: A user can report a conversation or message with a single tap, selecting a report category
FR36: A reporting user receives an immediate acknowledgement that their report has been received
FR37: A reporting user receives a follow-up notification when their report has been reviewed and actioned
FR38: A moderator can view flagged reports including conversation content and automated screening signals
FR39: A moderator can apply a warning, reveal suspension, or account ban to a reported user
FR40: A reported user's reveal capability is suspended pending moderation review upon report submission
FR41: The system records reveal initiation, mutual reveal confirmation, and reveal abandonment as distinct trackable events
FR42: An operator can view a near-real-time gender ratio dashboard
FR43: A user can request export of their personal data (GDPR right of access)
FR44: A user's data deletion request results in permanent erasure of all personal data within a defined retention window (GDPR right to erasure)
FR45: The system records a timestamped audit log entry for every moderation action taken

**Total: 45 FRs**

### NonFunctional Requirements

NFR1: Chat message delivery (send → receive) completes within 500ms under normal network conditions
NFR2: App launch to first interactive screen completes within 3 seconds on a mid-range device
NFR3: Image upload and content scanning pipeline completes within 10 seconds before confirming upload success
NFR4: Reveal photo delivery completes within 3 seconds of mutual confirmation
NFR5: Matching algorithm produces a match result within 30 seconds of onboarding completion
NFR6: API response time for all non-media endpoints is under 300ms at the 95th percentile for Poland-region users
NFR7: All personal data is encrypted at rest using AES-256 or equivalent
NFR8: All data in transit uses TLS 1.2 or higher
NFR9: Profile photos served only via authenticated, time-limited signed URLs — no public URL access permitted
NFR10: Authentication tokens expire after 30 days of inactivity
NFR11: CSAM scanning occurs server-side before image storage — no CSAM material may be persisted
NFR12: Moderation audit logs retained for minimum 2 years and are tamper-evident
NFR13: GDPR special category data stored only in EU-region infrastructure
NFR14: Payment processing must not store raw card data — PCI DSS compliance delegated to payment provider
NFR15: Backend must support 10,000 concurrent users in Poland-first launch geography
NFR16: Matching system must sustain throughput to 100,000 registered users without architectural changes
NFR17: Real-time chat infrastructure must support horizontal scaling beyond initial launch targets
NFR18: Content scanning pipeline must process images within SLA at up to 1,000 concurrent uploads
NFR19: Core user flows meet WCAG 2.1 AA compliance
NFR20: All interactive elements have accessible labels compatible with VoiceOver (iOS) and TalkBack (Android)
NFR21: App supports dynamic text sizing without breaking core UI layouts
NFR22: Colour contrast ratios meet WCAG AA minimums
NFR23: Core services target 99.5% uptime measured monthly
NFR24: Undelivered messages queued and delivered when connectivity restored, with no message loss
NFR25: Content scanning pipeline must have fallback on third-party API failure — images not accepted if scanning cannot be confirmed
NFR26: CSAM detection pipeline failure must trigger immediate alert to operations team
NFR27: Apple IAP and Google Play Billing must process subscription state changes within 60 seconds
NFR28: Push notification delivery must achieve >95% delivery rate within 60 seconds for time-sensitive events
NFR29: Reveal event tracking (initiation, confirmation, abandonment) must be recorded with no data loss — events are DB writes within the same transaction as the reveal state change
NFR30: NCMEC/PhotoDNA integration must be tested with known hash sets prior to launch

**Total: 30 NFRs**

### Additional Requirements

Architecture-derived requirements that directly impact epic and story creation:

- **ARCH-1 (Greenfield scaffolding):** Backend: `dotnet new sln -n Blinder && dotnet new webapi -n Blinder.Api --use-controllers --framework net10.0`; Mobile: `npx create-expo-app@latest --template default@sdk-55`. Epic 1 Story 1 must be project scaffolding.
- **ARCH-2 (Docker-first):** All development and production execution runs through Docker Compose. `Dockerfile` (multi-stage SDK→runtime), `docker-compose.yml`, `docker-compose.override.yml`, `nginx/nginx.conf`, `.env.example` must be in the first commit. Nothing runs on host OS.
- **ARCH-3 (Nginx WebSocket headers):** `/hubs/` location block in `nginx/nginx.conf` MUST include `proxy_http_version 1.1`, `Upgrade $http_upgrade`, `Connection "upgrade"`, `proxy_read_timeout 3600s` — omitting these causes SignalR to silently fall back to long polling, breaking NFR1.
- **ARCH-4 (EF Core migrations via SQL script):** `Database.MigrateAsync()` on startup is explicitly prohibited. Migrations applied via `dotnet ef migrations script --idempotent` (SDK stage only) → committed as `migrations/latest.sql` → applied via `docker compose exec -T db psql` on deploy.
- **ARCH-5 (PostGIS from day one):** `CREATE EXTENSION postgis` in first migration. Required for geographic radius matching (FR15). Never haversine on raw column coordinates.
- **ARCH-6 (Custom ApplicationUser):** `ApplicationUser : IdentityUser` from the first story — never add custom fields directly to `IdentityUser`. Required fields: gender, quiz refs, invite FK.
- **ARCH-7 (expo-secure-store mandatory):** All JWT token storage on mobile uses `expo-secure-store` (maps to iOS Keychain / Android Keystore). `AsyncStorage` is explicitly prohibited for auth tokens.
- **ARCH-8 (Mapperly for all DTO mapping):** No manual `new DTO { Property = entity.Property }` construction. All entity↔DTO conversions via Mapperly `[Mapper]` partial classes in `Mappings/`.
- **ARCH-9 (FluentValidation for all input):** Every endpoint accepting a request body or complex query params has an `AbstractValidator<TRequest>` in `Validators/`. No inline validation in controllers.
- **ARCH-10 (RFC 7807 Problem Details):** `AddProblemDetails()` + `UseExceptionHandler()` in `Program.cs` required from the first story. All 4xx/5xx errors return Problem Details shape. `AppErrors.cs` is the single source of truth for problem type URIs.
- **ARCH-11 (Synchronous content scan before storage):** All images scanned via `ContentScanningClient` before upload is confirmed. Must hard-fail (reject image) if scan API is unavailable — no pass-through on timeout.
- **ARCH-12 (Coravel match generation idempotency):** `MatchGenerationJob` must be idempotent and triggered at startup (in addition to cron schedule) to prevent empty match screen after VPS restart.
- **ARCH-13 (SignalR real-time reveal broadcast):** When BOTH `reveal_ready` flags are set, a `RevealStateUpdated` SignalR hub event must be broadcast to both active clients immediately — this is what drives the simultaneous UX reveal moment. Not addressed in architecture narrative; must be a dedicated story in the Reveal epic.
- **ARCH-26 (Reveal message threshold — product decision):** Initial value = **100 messages**. Stored as admin-configurable `AppSettings` record (key: `reveal_message_threshold`, value: `100`). Premium users can lower their own personal threshold via `FR27` (other party's threshold and consent always independent). This value must be seeded in the first migration that creates the `AppSettings` table.
- **ARCH-14 (Admin Nginx IP allowlist):** `/admin` path protected by Nginx IP allowlist in `nginx/nginx.conf` in addition to application-level Razor Pages cookie auth. Must be implemented before admin UI is deployed.
- **ARCH-15 (IAP webhook JWT verification, OWASP A01):** `SubscriptionController` must verify Apple RS256 (JWKS) and Google JWT signatures on all IAP webhooks. `SkipWebhookVerification` flag valid only in `appsettings.Testing.json`. Startup assertion must enforce this is off in production. Spike story required before any subscription implementation.
- **ARCH-16 (Social login not scaffold-covered):** Apple, Google, Facebook login uses `ExternalLoginAsync` wiring in `SocialLoginHandler.cs` — not covered by default Identity scaffolding templates. Explicit dedicated stories are required.
- **ARCH-17 (Hetzner Object Storage ForcePathStyle):** S3 client requires `ForcePathStyle = true` for Hetzner Object Storage non-standard endpoint. Spike story required before photo upload story.
- **ARCH-18 (Push tokens via native token):** Mobile client calls `getDevicePushTokenAsync()` (raw native token, NOT `getExpoPushTokenAsync()`). Stale token cleanup via `SendPushNotificationJob` on FCM/APNs error response.
- **ARCH-19 (Account deletion cascade):** FR4 deletion must purge photos, chat history, profile, and analytics references. Exception: moderation audit logs retained 2 years regardless. Deletion orchestrator required.
- **ARCH-20 (LUKS disk encryption at VPS provisioning):** Non-code deployment checklist item. Must be setup before first user. Tracked as a deployment task in Epic 1.
- **ARCH-21 (NCMEC/PhotoDNA legal workstream):** Non-code parallel workstream. Legal agreements must be initiated at project start. Developer tracking item, not a code story.
- **ARCH-22 (Serilog structured logging):** Replaces default .NET logger. Rolling file sink. `ILogger<T>` injection only. No `Console.WriteLine`. No PII in structured log properties.
- **ARCH-23 (DateTimeOffset throughout):** `DateTime` is prohibited in C# code. All datetimes `DateTimeOffset` (C#) / `timestamptz` (PostgreSQL) / ISO 8601 (API responses). Enforced in every story with date fields.
- **ARCH-24 (EAS Build for mobile store delivery):** `eas.json` committed from first commit. EAS Build used for App Store / Play Store submissions. Not for backend deployment.
- **ARCH-25 (TypeScript strict mode):** `strict: true` in `tsconfig.json` set before writing the first component. Retrofitting strict mode is costly.

### UX Design Requirements

UX-specification-derived requirements for implementation:

- **UX-DR1 (Design token system):** Implement Blinder colour palette, typography scale (DM Sans), and spacing tokens in `constants/theme.ts` using NativeWind. Dark mode only at MVP. Key colours: bg `#1A1814`, primary text `#F2EDE6`, amber accent `#C8833A`, safety teal `#4A9E8A`, danger `#D94F4F`.
- **UX-DR2 (Amber bookend):** Amber accent `#C8833A` must appear at two moments: onboarding entry (within first 3 seconds of app open) and reveal trigger screen. Visual through-line felt subconsciously.
- **UX-DR3 (RevealMoment component):** `RevealMoment.tsx` — full-screen, no UI chrome, 600–800ms motion treatment for simultaneous photo delivery. 1–2 seconds of space before any interactive element appears. Reduce-motion fallback: opacity fade only. This is the most design-invested single component.
- **UX-DR4 (RevealPrompt component):** `RevealPrompt.tsx` — inline, quiet reveal affordance appearing in chat when message threshold is reached. NOT a push notification, NOT a modal interrupt. Copy: "The reveal is available when you're both ready."
- **UX-DR5 (RevealCountdown component — waiting state):** `RevealCountdown.tsx` — shown after user sets reveal_ready flag. Copy: "You've indicated you're ready. Waiting for them." No timer. No countdown. No indication to other party. Feel: shared patience, not rejection.
- **UX-DR6 (Post-reveal continuation gate):** Both users independently asked "Would you like to keep talking?" after reveal. Asymmetric outcome hidden from both parties. Neutral framing for all outcomes.
- **UX-DR7 (EmptyMatchState component):** `EmptyMatchState.tsx` — brand-voice copy, never generic spinner. Copy: "You have not been forgotten. Your match is being found." Explicitly specified as AC in first match story — never a polish item to add later.
- **UX-DR8 (Warm processing screen):** Post-quiz / match assignment processing screen uses warm brand copy. Never a generic spinner. Treated with same design investment as primary screens.
- **UX-DR9 (Consent gate pattern):** All consent gates (photo upload, reveal trigger, post-reveal continuation) have two visually EQUAL-weight options ("ready" / "not yet"). No dark patterns nudging toward one choice. Never irreversible in a single tap.
- **UX-DR10 (Invite landing screen):** `invite-landing.tsx` — structural safety promise surfaced BEFORE any data entry. Inviter's name visible. Copy highlights structural impossibility of unsolicited photos. Invite context preserved through registration flow via deep link.
- **UX-DR11 (Report always one tap):** `ReportButton.tsx` always accessible from the chat header. Never buried in settings. Defined once in `components/moderation/` — never reimplemented inline.
- **UX-DR12 (AccessibilityContext at root):** `AccessibilityContext` providing `{ reduceMotion, fontScale, isScreenReaderEnabled }` at app root. All animated components and scaling decisions read from this context.
- **UX-DR13 (Reduce motion):** All animations respect `AccessibilityInfo.isReduceMotionEnabled()`. When enabled: all transitions snap to 0ms or substitute opacity fade. `RevealMoment.tsx` uses fade-only variant.
- **UX-DR14 (Touch targets 44×44px):** All interactive elements have minimum `minHeight: 44, minWidth: 44` touch target. `hitSlop={{ top: 8, bottom: 8, left: 8, right: 8 }}` on icon-only controls (back button, report icon).
- **UX-DR15 (Accessibility attributes):** Every interactive element declares `accessibilityRole`, `accessibilityLabel`, `accessibilityHint` (where non-obvious), and `accessibilityState`. Per-component requirements documented in UX spec.
- **UX-DR16 (Dynamic text sizing):** All text uses `allowFontScaling={true}`. Layouts tested at system text ×1.0, ×1.3, ×2.0. No fixed-height text containers. No truncation at ×2.0.
- **UX-DR17 (Responsive layout hook):** `useWindowDimensions()` hook stores `compact` (<375px), `regular` (375–427px), `expanded` (≥428px) in context. Applied to onboarding and reveal modal.
- **UX-DR18 (WhatsApp-conventional chat UI):** Chat interface must feel completely familiar — no novel interaction patterns in the message layer. The conversation is the novel experience; the interface must disappear.
- **UX-DR19 (Photo upload positioning):** Photo upload step AFTER quiz in onboarding flow. Low-pressure copy: "No one sees this until you both choose." Positioned when user is already invested from quiz completion.
- **UX-DR20 (SignalR connection lifecycle in service):** `signalrService.ts` manages connection as singleton. Components NEVER directly manage connection state. Mobile components subscribe via hooks only (`useConversation`, `useRevealState`).
- **UX-DR21 (AsyncState<T> shape):** All async hooks return `{ data: T | null; error: string | null; isLoading: boolean }`. Never raw `try/catch` in components. Error strings from `constants/errors.ts` only.
- **UX-DR22 (No spinner for brand moments):** Empty states, match waiting state, and post-reveal holding state use brand-voice copy — never a `<ActivityIndicator>`. Skeleton screens use NativeWind pulsing background with reduce-motion static fallback.

**Total: 22 UX Design Requirements**

### FR Coverage Map

FR1:  Epic 2 — Email/password registration
FR2:  Epic 2 — Social login (Apple, Google, Facebook)
FR3:  Epic 2 — Login and logout
FR4:  Epic 2 — Account deletion with full data purge
FR5:  Epic 2 — 18+ age declaration at registration
FR6:  Epic 3 — Update preferences (age range, search radius)
FR7:  Epic 2 — Female invite-only registration
FR8:  Epic 3 — Values and personality quiz
FR9:  Epic 3 — Private photo upload (content-scanned)
FR10: Epic 3 — First match conversation on onboarding completion
FR11: Epic 3 — 7-day premium trial activation
FR12: Epic 4 — Values-weighted compatibility matching
FR13: Epic 4 — Demographic fallback matching
FR14: Epic 4 — Curated matches only (no browse)
FR15: Epic 4 — Search radius preference
FR16: Epic 4 — Invite link generation and tracking
FR17: Epic 5 — Real-time text messaging
FR18: Epic 5 — Max 3 active conversations (free tier)
FR19: Epic 5 — Message count tracking per conversation
FR20: Epic 5 — View active conversations and message counts
FR21: Epic 5 — Push notifications for all key events
FR22: Epic 6 — Express reveal readiness
FR23: Epic 6 — Reveal option gated by message threshold (100)
FR24: Epic 6 — Photo delivered only after both parties opt in
FR25: Epic 6 — Simultaneous photo exchange on mutual confirm
FR26: Epic 6 — No photo visible before mutual reveal
FR27: Epic 6 — Premium lowers personal reveal threshold
FR28: Epic 7 — Subscribe via Apple IAP / Google Play Billing
FR29: Epic 7 — Premium: increased limits
FR30: Epic 7 — Trial expiry notification
FR31: Epic 7 — Free tier limit upgrade prompt
FR32: Epic 3 — Image scan at upload (explicit content)
FR33: Epic 3 — CSAM scan at upload
FR34: Epic 8 — Automated text flagging for harassment
FR35: Epic 8 — One-tap reporting with category
FR36: Epic 8 — Immediate report acknowledgement
FR37: Epic 8 — Follow-up notification after moderation action
FR38: Epic 8 — Moderator views flagged reports
FR39: Epic 8 — Moderator applies warning/suspension/ban
FR40: Epic 8 — Reported user reveal capability suspended
FR41: Epic 9 — Reveal event tracking (DB-stored: initiation, confirmation, abandonment)
FR42: Epic 9 — Gender ratio dashboard
FR43: Epic 9 — GDPR personal data export
FR44: Epic 9 — GDPR data deletion (right to erasure)
FR45: Epic 9 — Tamper-evident moderation audit log

## Epic List

### Epic 1: Project Foundation & Developer Environment
Developers can run the full Blinder stack locally via Docker Compose from a single command, with a scaffolded ASP.NET Core backend, Expo mobile project, Nginx reverse proxy with SignalR WebSocket headers, and all environment conventions established. This foundation enables every subsequent epic.
**FRs covered:** None directly (ARCH-1 through ARCH-4, ARCH-22, ARCH-24, ARCH-25 — greenfield scaffolding)

### Epic 2: User Registration & Authentication
A user can create an account (email/password or social login via Apple, Google, or Facebook), log in, log out, and permanently delete their account with full data purge. Female users can only register via a valid invite link from an existing female member. All authentication flows (email/password, social login) are built on a centralized OAuth2/OIDC token endpoint.
**FRs covered:** FR1, FR2, FR3, FR4, FR5, FR7
**Architecture:** ARCH-16 requires OAuth2 foundation story (2-0) before social login stories (2-3, 2-4)

### Epic 3: Onboarding & Profile Setup
A new user can complete the values and personality quiz, upload a private profile photo (scanned for explicit content and CSAM before storage), set age range and radius preferences, and receive their 7-day premium trial — landing immediately in their first match conversation upon completion.
**FRs covered:** FR6, FR8, FR9, FR10, FR11, FR32, FR33

### Epic 4: Matching Engine
The system matches users using the rules-based values-weighted compatibility algorithm (with demographic fallback when pool is insufficient), users receive curated matches only with no browsing, geographic radius controls match scope, and invite links are generated and tracked per female account.
**FRs covered:** FR12, FR13, FR14, FR15, FR16

### Epic 5: Real-Time Chat
A matched user can send and receive real-time text messages via SignalR, view their active conversation list with message counts (max 3 active on free tier), and receive push notifications for new messages, new matches, and reveal-related events.
**FRs covered:** FR17, FR18, FR19, FR20, FR21

### Epic 6: Mutual Photo Reveal
A user can express reveal readiness after reaching the 100-message threshold (admin-configurable), and when both parties independently opt in, both photos are delivered simultaneously via a real-time SignalR broadcast — with the full emotional UX arc (RevealMoment, RevealPrompt, waiting state, continuation gate). Premium users can lower their personal threshold while preserving the other party's independent consent.
**FRs covered:** FR22, FR23, FR24, FR25, FR26, FR27

### Epic 7: Subscriptions & Premium
A user can subscribe to the premium tier via Apple In-App Purchase or Google Play Billing (with full JWT webhook signature verification), access increased match and conversation limits, receive trial expiry notifications, and see upgrade prompts when free tier limits are reached.
**FRs covered:** FR28, FR29, FR30, FR31

### Epic 8: Safety & Content Moderation
A user can report a conversation with one tap and receive immediate acknowledgement and follow-up notification. Text messages are automatically flagged for harassment patterns. A moderator can review flagged reports with automated screening signals and apply warnings, reveal suspensions, or permanent bans — with reported users' reveal capability automatically suspended on submission.
**FRs covered:** FR34, FR35, FR36, FR37, FR38, FR39, FR40

### Epic 9: Analytics, Compliance & GDPR
An operator can view a near-real-time gender ratio dashboard. Users can request personal data export and data deletion (GDPR rights). All moderation actions are recorded in tamper-evident audit logs retained for 2 years. Reveal initiation, confirmation, and abandonment are tracked as distinct DB-stored events.
**FRs covered:** FR41, FR42, FR43, FR44, FR45

---

## Epic 1: Project Foundation & Developer Environment

Developers can run the full Blinder stack locally via Docker Compose from a single command, with a scaffolded ASP.NET Core backend, Expo mobile project, Nginx reverse proxy with SignalR WebSocket headers, and all environment conventions established. This foundation enables every subsequent epic.

### Story 1.1: Scaffold Backend Project

As a developer,
I want a fully scaffolded ASP.NET Core .NET 10 backend project committed to source control,
So that every subsequent story has a working, convention-compliant backend to build on.

**Acceptance Criteria:**

**Given** the repository is cloned fresh
**When** the developer runs `dotnet build` inside `backend/`
**Then** the solution builds with zero warnings or errors

**Given** the backend project is scaffolded
**When** the project structure is inspected
**Then** `backend/Blinder.sln`, `backend/Blinder.Api/`, and `backend/Blinder.Tests/` exist with the following packages added: `Microsoft.AspNetCore.SignalR`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.AspNetCore.RazorPages`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Riok.Mapperly`, `FluentValidation.AspNetCore`, `Serilog.AspNetCore`, `Serilog.Sinks.File`, `FirebaseAdmin`, `dotAPNS`, `AWSSDK.S3`, `Coravel`, `MailKit`

**Given** the project is scaffolded
**When** `Program.cs` is reviewed
**Then** `AddProblemDetails()` and `UseExceptionHandler()` are configured, Serilog is the logging provider (no default `Console.WriteLine`), and `AddValidatorsFromAssemblyContaining<Program>()` is registered

**Given** `Blinder.Api/` is created
**When** the directory structure is verified
**Then** the following directories exist: `Controllers/`, `Hubs/`, `Pages/Admin/`, `Models/`, `DTOs/`, `Services/`, `Repositories/`, `Mappings/`, `Validators/`, `BackgroundJobs/`, `Errors/AppErrors.cs`, `Migrations/`, `Infrastructure/Data/`, `Infrastructure/Auth/`, `Infrastructure/Storage/`, `Infrastructure/Scanning/`, `Infrastructure/Email/`, `Infrastructure/Middleware/`

**Given** the project is scaffolded
**When** `Errors/AppErrors.cs` is reviewed
**Then** it exists as the single source of truth for RFC 7807 problem type URIs — no error strings defined inline in any controller

---

### Story 1.2: Docker Compose Stack with Nginx and WebSocket Support

As a developer,
I want the full backend stack running via `docker compose up` with Nginx correctly configured for SignalR,
So that development and production environments are identical from the first commit and real-time chat will work at all scales.

**Acceptance Criteria:**

**Given** the repository is cloned fresh and `.env` is populated from `.env.example`
**When** the developer runs `docker compose up`
**Then** three containers start successfully: `api` (ASP.NET Core), `db` (postgis/postgis), `nginx` (reverse proxy)

**Given** the Nginx container is running
**When** a WebSocket upgrade request is sent to `/hubs/chat`
**Then** Nginx forwards it with `proxy_http_version 1.1`, `Upgrade $http_upgrade`, `Connection "upgrade"`, and `proxy_read_timeout 3600s` headers — SignalR does NOT fall back to long polling

**Given** the Docker setup is committed
**When** `docker-compose.yml` is reviewed
**Then** all services use `restart: unless-stopped`, the PostgreSQL volume is a named volume `db-data` (never anonymous), and no service runs directly on the host OS

**Given** the project is set up
**When** `.env.example` is reviewed
**Then** it documents every required environment variable with placeholder values, is committed to source control, and `.env` itself is in `.gitignore`

**Given** the multi-stage `Dockerfile` is reviewed
**When** the image is built
**Then** the production image is based on the ASP.NET runtime image (not SDK), `dotnet-ef` tools are NOT present in the production image, and the image builds without errors

**Given** `docker-compose.override.yml` exists
**When** a developer runs `docker compose run --rm api dotnet ef migrations add Test`
**Then** the EF CLI tools succeed because the override targets the SDK build stage — the production image is unaffected

**Given** `nginx/nginx.conf` is reviewed
**When** the `/admin` location block is inspected
**Then** an IP allowlist (`allow` / `deny all`) is present, separate from application-level cookie authentication

---

### Story 1.3: Scaffold Mobile Project with Strict TypeScript and NativeWind

As a developer,
I want a fully scaffolded Expo SDK 55 React Native project with strict TypeScript, NativeWind styling, and EAS Build configured,
So that every mobile story has a convention-compliant base with no retrofitting of type safety or styling later.

**Acceptance Criteria:**

**Given** the repository is cloned fresh
**When** the developer runs `npx expo start` from `mobile/`
**Then** the Expo dev server starts without errors on both iOS and Android simulators

**Given** the mobile project is scaffolded
**When** `tsconfig.json` is reviewed
**Then** `"strict": true` is set — TypeScript strict mode is active before the first component is written

**Given** the mobile project is scaffolded
**When** the `mobile/` directory structure is inspected
**Then** the following directories and files exist: `app/(auth)/`, `app/(tabs)/`, `components/`, `hooks/`, `services/apiClient.ts`, `services/signalrService.ts`, `services/storageService.ts`, `constants/errors.ts`, `constants/theme.ts`, `types/api/index.ts`, `types/signalr/index.ts`, `utils/dateFormat.ts`

**Given** the mobile project is scaffolded
**When** the installed packages are reviewed
**Then** `expo-router`, `expo-notifications`, `expo-image-picker`, `expo-secure-store`, `@microsoft/signalr`, `nativewind` are all installed

**Given** `services/storageService.ts` is implemented
**When** a JWT token is stored and retrieved
**Then** `expo-secure-store` is used exclusively — `AsyncStorage` is not imported anywhere in `storageService.ts`

**Given** `eas.json` is reviewed
**When** it is verified it is committed to the repository
**Then** it exists with at minimum `development`, `preview`, and `production` build profiles configured

**Given** an async hook is implemented
**When** the return type is inspected
**Then** it returns `AsyncState<T>` = `{ data: T | null; error: string | null; isLoading: boolean }` — never raw `try/catch` in components

---

### Story 1.4: Database Setup with PostGIS and EF Core Migrations Pipeline

As a developer,
I want EF Core configured with PostGIS, snake_case naming, and a proven idempotent migration deployment pipeline,
So that all subsequent stories can add their own tables without risk of production incidents from auto-applied migrations.

**Acceptance Criteria:**

**Given** the database container is running
**When** the first migration is applied via `docker compose exec -T db psql -U $POSTGRES_USER -d $POSTGRES_DB < migrations/latest.sql`
**Then** the `__EFMigrationsHistory` table is created and the migration is recorded as applied

**Given** `AppDbContext.cs` is reviewed
**When** the `OnModelCreating` configuration is inspected
**Then** `UseSnakeCaseNamingConvention()` is called, `UseNetTopologySuite()` is called (PostGIS), and no column or table uses PascalCase naming in the generated migration SQL

**Given** the first migration is generated
**When** the migration SQL is reviewed
**Then** `CREATE EXTENSION IF NOT EXISTS postgis;` appears before any table creation statement

**Given** `Program.cs` is reviewed
**When** the application startup sequence is inspected
**Then** `Database.MigrateAsync()` is NOT called anywhere at startup — auto-migration on startup is explicitly absent

**Given** a new migration is added via `docker compose run --rm api dotnet ef migrations add <Name>`
**When** `migrations/latest.sql` is regenerated via `dotnet ef migrations script --idempotent`
**Then** the `--idempotent` flag produces a script that checks `__EFMigrationsHistory` before applying — safe to re-run on every deploy

---

### Story 1.5: Mobile Design System Foundation and Accessibility Context

As a user,
I want the app to use Blinder's dark, warm visual identity from the very first screen, with full accessibility support built in from the start,
So that every subsequent screen is consistent, accessible, and never requires design retrofitting.

**Acceptance Criteria:**

**Given** `constants/theme.ts` is implemented
**When** the colour tokens are reviewed
**Then** the following tokens exist: background `#1A1814`, primary text `#F2EDE6`, secondary text `#9E9790`, amber accent `#C8833A`, safety teal `#4A9E8A`, danger red `#D94F4F`, and inactive `#635D57` — dark mode only at MVP; no light mode toggle

**Given** a screen renders
**When** WCAG AA contrast is checked between all text/background pairs
**Then** amber on background = 4.82:1 ✅, primary text on background = 13.4:1 ✅, secondary text on background = 5.1:1 ✅

**Given** `AccessibilityContext` is implemented at app root
**When** any component accesses the context
**Then** `{ reduceMotion: boolean, fontScale: number, isScreenReaderEnabled: boolean }` are available — no per-component `AccessibilityInfo` calls needed

**Given** `reduceMotion` is `true` in `AccessibilityContext`
**When** any animated component renders
**Then** all transitions snap to 0ms duration or use opacity-fade-only — no motion-based transitions fire

**Given** any interactive element renders
**When** its touch target dimensions are measured
**Then** minimum `minHeight: 44, minWidth: 44` logical pixels are applied; icon-only controls have `hitSlop={{ top: 8, bottom: 8, left: 8, right: 8 }}`

**Given** any interactive element renders
**When** VoiceOver or TalkBack inspects it
**Then** `accessibilityRole`, `accessibilityLabel`, and `accessibilityHint` (where non-obvious) are present on every element

**Given** the app is running with system font size at ×2.0
**When** any screen is rendered
**Then** `allowFontScaling={true}` is set on all `Text` components, no text is truncated, and no fixed-height text containers clip content

---

## Epic 2: User Registration & Authentication

A user can create an account (email/password or social login), log in, log out, and permanently delete their account with full data purge. Female users can only register via a valid invite link.

### Story 2.0: OAuth2/OIDC Authentication Foundation

As a developer,
I want a complete OAuth2/OIDC token endpoint and grant handler infrastructure,
So that all authentication flows (email/password, social login, future integrations) are built on a single, secure, standards-compliant foundation.

**Acceptance Criteria:**

1. OAuth2 token endpoint operational: `POST /api/auth/oauth/token` returns `{ access_token, refresh_token, expires_in, token_type: "Bearer" }` (RFC 6749 compliant)
2. Resource Owner Password Credentials (ROPC) grant implemented for email/password auth with 30-day access token, 90-day refresh token
3. Authorization Code grant framework implemented (extensible for Stories 2-3/2-4 social login providers)
4. Refresh token lifecycle: one-time use, hashed storage in database, new token pair issued on valid refresh
5. Token revocation endpoint: `POST /api/auth/oauth/revoke` marks tokens invalid, prevents replay
6. Mobile token storage contract: both access + refresh tokens stored via `expo-secure-store`
7. JWT structure standardized: claims include `sub`, `iss`, `aud`, `exp`, `iat`, `auth_time`
8. Rate limiting on token endpoint: 5 failed attempts per IP per minute → 429 Too Many Requests
9. JwtBearer middleware checks revocation status before accepting token
10. All acceptance criteria thoroughly tested (unit, integration, security, mobile)

**Dev Notes:** This story establishes zero user-facing features. It is pure infrastructure enabling Stories 2-1 (registration), 2-2 (login/logout), 2-3 (Apple), 2-4 (Google/Facebook). All token issuance routes through this single OAuth2 endpoint.

**Estimated Effort:** 5-6 days (medium, foundational)

---

### Story 2.1: Email/Password User Registration

**Updated (March 25, 2026):** This story was redesigned after Story 2-0 (OAuth2 Foundation) was approved. The key change: registration no longer issues JWT tokens. Token issuance is delegated to the OAuth2 token endpoint, centralizing authentication logic and enabling email verification before token issuance (future enhancement).

As a new user,
I want to register with my email address and password,
So that I can create a Blinder account and then authenticate via the OAuth2 token endpoint.

**Acceptance Criteria:**

**Given** a `POST /api/auth/register` request with valid email, password, and `over18Declaration: true`
**When** the request is processed
**Then** a new `ApplicationUser` is created (extending `IdentityUser` with gender, quiz refs, invite FK fields), a 201 response is returned with the user's ID and email — no JWT token is issued at registration; token issuance delegated to OAuth2 token endpoint (Story 2-0)

**Given** Identity scaffolded registration exists in Razor Pages for web/admin
**When** mobile registration is implemented
**Then** mobile uses native UI and API only; the API path delegates to the same Identity-backed registration rules (single source of truth), not a second divergent ruleset

**Given** `ApplicationUser` is created in `Models/ApplicationUser.cs`
**When** the class definition is reviewed
**Then** it inherits from `IdentityUser`, includes `Gender` (enum: Male/Female/NonBinary), `QuizCompletedAt` (DateTimeOffset?), `InviteLinkId` (FK, nullable), `IsOnboardingComplete` (bool), and uses `DateTimeOffset` for all date fields — `DateTime` is never used

**Given** a registration request is received
**When** FluentValidation runs `RegisterRequestValidator`
**Then** email format is validated, password minimum requirements are enforced, `over18Declaration` must be `true` — missing any field returns Problem Details 400

**Given** a duplicate email is submitted
**When** Identity attempts to create the user
**Then** a 409 Conflict Problem Details response is returned — the error type URI comes from `AppErrors.cs`

**Given** a Mapperly `UserMapper` is implemented
**When** `ApplicationUser` is mapped to `UserResponse` DTO
**Then** no manual `new UserResponse { ... }` construction exists — all mapping goes through `[Mapper]` partial class

**Given** a new user registers
**When** their `over18Declaration` field is reviewed
**Then** it is stored as `age_declaration_accepted` (snake_case) in the `AspNetUsers` table with a `timestamptz` timestamp

---

### Story 2-1-A: Shared Mobile Component Library

As a developer,
I want a complete shared component library with all MVP components implemented or placeholder-scaffolded,
So that every future screen uses consistent, accessible, theme-compliant components rather than inline primitives.

**Acceptance Criteria:**

**Given** `mobile/components/shared/` is reviewed
**When** the component inventory is checked
**Then** `Button.tsx`, `TextField.tsx`, `RadioChipGroup.tsx`, `Toggle.tsx`, `LoadingIndicator.tsx`, `ErrorBanner.tsx`, `Card.tsx`, `Modal.tsx`, `EmptyState.tsx`, `SkeletonLoader.tsx`, `StatusBadge.tsx`, `ConsentBadge.tsx` all exist alongside the existing `AccessiblePressable.tsx` and `ThemedText.tsx`

**Given** `Button.tsx` is implemented
**When** rendered with `variant` prop
**Then** it supports `primary`, `secondary`, `ghost`, `danger` variants; accepts `isLoading` prop that renders `LoadingIndicator`; uses theme tokens exclusively; enforces `minHeight: 44`; requires `accessibilityLabel`

**Given** `TextField.tsx` is implemented
**When** rendered
**Then** it wraps React Native `TextInput` with a `label` prop, `error` prop, `secureTextEntry` support, themed border/background, and `accessibilityLabel` derived from label; always sets `allowFontScaling={true}` (WCAG 2.1 AA)

**Given** `RadioChipGroup.tsx` is implemented
**When** rendered
**Then** it renders a horizontal row of selectable chips; selected chip uses `colors.accent.primary` border and `colors.background.surface` fill; `accessibilityRole="radiogroup"` on container, `accessibilityRole="radio"` on each chip

**Given** `Toggle.tsx` is implemented
**When** rendered
**Then** it wraps React Native `Switch` with a `label` prop; uses `colors.safety` as active track color; `accessibilityLabel` is required

**Given** `LoadingIndicator.tsx` is implemented
**When** rendered
**Then** it produces three animated pulsing dots using theme colors; respects `reduceMotion` from `AccessibilityContext` — static dots when `reduceMotion === true`

**Given** `ErrorBanner.tsx` is implemented
**When** rendered with an `error` string prop
**Then** it displays a themed error block using `colors.danger` text and `colors.background.surface` background with `accessibilityLiveRegion="polite"`

**Given** placeholder components are implemented in their subdirectories (`Card`, `Modal`, `EmptyState`, `SkeletonLoader`, `StatusBadge`, `ConsentBadge` in `shared/`; `ChatBubble`, `ChatInput`, `ConversationRow` in `chat/`; `RevealMoment`, `RevealPrompt`, `RevealCountdown`, `EmptyMatchState`, `RevealProgress` in `match/`; `ReportButton` in `moderation/`; `QuizCard`, `ProgressStepper` in `onboarding/`)
**When** each placeholder is reviewed
**Then** each exports a typed props interface, renders a minimal visible stub using `ThemedText` with the component name, uses only theme tokens, and includes a `// TODO: implement in Story X.X` comment citing the target story

**Given** `register.tsx` is refactored
**When** the file is reviewed
**Then** raw `TextInput` is replaced by `TextField`; submit and back buttons are replaced by `Button`; gender selector is replaced by `RadioChipGroup`; over-18 switch is replaced by `Toggle`; the local `LoadingDots` function is removed (replaced by `LoadingIndicator`); error display is replaced by `ErrorBanner`; no inline component definitions remain

**Given** `docs/component-library.md` is created
**When** reviewed
**Then** it lists every component from this story with file path, props summary, implementation status (`ready` or `placeholder — Story X.X`), and a usage example

**Given** `docs/project-context.md` is updated with Rule 18
**When** reviewed
**Then** it states that developers must check `mobile/components/` before building any UI element; inline component definitions in screen files are prohibited; new components must be added to `docs/component-library.md`

**Given** `tsc --noEmit` is run from `mobile/`
**When** compilation completes
**Then** zero TypeScript errors

---

### Story 2.2: User Login, JWT Tokens, and Logout

**Updated (March 25, 2026):** This story was redesigned after Story 2-0 (OAuth2 Foundation) was approved. Major changes: (1) Login delegates token issuance to OAuth2TokenService (ROPC grant), (2) Adds refresh token lifecycle management, (3) Logout revokes tokens at token endpoint (not just client-side), (4) Mobile implements automatic token refresh with 5-minute buffer.

As a registered user,
I want to log in with my credentials, receive secure access and refresh tokens, automatically refresh my tokens when needed, and log out with server-side revocation,
So that I can access Blinder securely with persistent sessions and know my tokens are invalidated on logout.

**Acceptance Criteria:**

**Given** a `POST /api/auth/login` request with valid email and password
**When** credentials are verified
**Then** OAuth2TokenService (ROPC grant) issues both access token (30-day) and refresh token (90-day); response returns `{ access_token, refresh_token, expires_in, token_type: "Bearer" }`; mobile stores both via `expo-secure-store` (mapped to iOS Keychain / Android Keystore)

**Given** login is supported by both web/admin and mobile channels
**When** authentication logic is reviewed
**Then** scaffolded Identity login flow and API login endpoint delegate to OAuth2TokenService; mobile does not consume scaffolded Razor pages directly

**Given** the mobile client receives tokens
**When** `storageService.ts` persists both access + refresh tokens
**Then** `SecureStore.setItemAsync` is called twice — `AsyncStorage` is never used for token storage; token expiry timestamps extracted for 5-minute pre-refresh logic

**Given** a mobile app detects access token expires within 5 minutes
**When** `authService.refreshAccessToken()` is called automatically
**Then** `POST /api/auth/oauth/token` with `grant_type=refresh_token` is invoked; new access + refresh token pair issued; old tokens invalidated (one-time use)

**Given** a `POST /api/auth/logout` request is made with `Authorization: Bearer <access_token>`
**When** the logout succeeds
**Then** both access token and refresh token are revoked at server via `POST /api/auth/oauth/revoke`; subsequent API requests with either token return 401; client clears tokens from `expo-secure-store`

**Given** an API request is made with an expired, invalid, or revoked token
**When** the JwtBearer middleware processes the request
**Then** a 401 Unauthorized Problem Details response is returned — no stack trace is exposed

---

### Story 2.3: Apple Sign In

**Updated (March 25, 2026):** This story is simplified by Story 2-0 (OAuth2 Foundation). Social login now uses the shared OAuth2 token endpoint with provider-specific validators plugging in.

As a new or returning iOS user,
I want to register or log in using Apple Sign In,
So that I can use Blinder without creating a separate password.

**Acceptance Criteria:**

**Given** a `POST /api/auth/oauth/token` request with `grant_type=authorization_code`, `provider: "Apple"`, and a valid Apple identity token
**When** the authorization code grant handler validates the token
**Then** the Apple identity token is verified against `https://appleid.apple.com/auth/keys` (JWKS), the `iss` claim equals `"https://appleid.apple.com"`, and the `bundle_id` claim matches the app; authorization code valid for 10 minutes, one-time use

**Given** authorization code is valid
**When** client exchanges code via `POST /api/auth/oauth/token` (same endpoint as Story 2-2 refresh)
**Then** OAuth2TokenService issues access token (30-day) + refresh token (90-day); response format identical to email/password login (RFC 6749 compliant)

**Given** Apple token validation succeeds for a new user
**When** account is created via `ExternalLoginAsync`
**Then** a new `ApplicationUser` is created; user directed to onboarding quiz (onboarding not skipped)

**Given** Apple token validation succeeds for an existing user
**When** the authorization code exchange completes
**Then** tokens are issued; no new `ApplicationUser` created

**Given** a social login completes
**When** the user profile is inspected
**Then** no profile photo is imported from Apple — social login is for authentication only

---

### Story 2.4: Google and Facebook Social Login

**Updated (March 25, 2026):** This story is simplified by Story 2-0 (OAuth2 Foundation). Social login now uses the shared OAuth2 token endpoint with provider-specific validators plugging in.

As a new or returning user,
I want to register or log in using Google or Facebook,
So that I can use Blinder without creating a separate password.

**Acceptance Criteria:**

**Given** a `POST /api/auth/oauth/token` request with `grant_type=authorization_code`, `provider: "Google"` or `"Facebook"`, and a valid provider identity token
**When** the authorization code grant handler validates the token
**Then** the provider token is verified via provider JWKS endpoint (Google) or token validation endpoint (Facebook); authorization code valid for 10 minutes, one-time use

**Given** authorization code is valid
**When** client exchanges code via `POST /api/auth/oauth/token` (shared endpoint with Story 2-3)
**Then** OAuth2TokenService issues access token (30-day) + refresh token (90-day); response format identical to email/password + Apple

**Given** any social login provider is used
**When** account creation occurs
**Then** user is directed to onboarding quiz — social login does not bypass onboarding; no social graph data, no profile photos are imported

**Given** a social login provider returns an email already registered via a different provider
**When** the account lookup runs
**Then** accounts are linked via Identity's external login system — not duplicated

---

### Story 2.5: Female Invite-Only Registration and Invite Landing Screen

As a female user invited by a Blinder member,
I want to register via an invite link,
So that I can join Blinder through a trusted channel that communicates structural safety before I invest any time.

**Acceptance Criteria:**

**Given** a female user taps an invite deep link
**When** `invite-landing.tsx` renders
**Then** the inviter's name is visible, the structural safety promise ("Structurally impossible to receive unsolicited photos — by design") is displayed BEFORE any data entry fields, and the invite token is preserved in navigation context throughout the registration flow

**Given** a female user submits a registration request
**When** the invite token is validated server-side
**Then** a `GET /api/invites/{token}/validate` call confirms the token is valid, unused, and a `POST /api/auth/register` with the `inviteToken` field proceeds; the `InviteLink.used_by` FK is set to the new user's ID on completion

**Given** an invalid or already-used invite token is submitted
**When** registration is attempted
**Then** a 422 Unprocessable Entity Problem Details response is returned — the user sees a clear in-app error message from `constants/errors.ts`

**Given** a female user completes registration without an invite token
**When** the request reaches `RegisterRequestValidator`
**Then** validation fails with a specific error indicating an invite link is required for female registration

**Given** `InviteLink` model is created
**When** the migration is reviewed
**Then** table `invite_links` contains `id`, `token` (unique index), `generated_by_user_id` (FK to `users`), `used_by_user_id` (FK to `users`, nullable), `created_at` (timestamptz) — all snake_case

---

### Story 2.6: Account Deletion with Full Data Purge Cascade

As a user,
I want to permanently delete my account and all associated data,
So that I can exercise my right to erasure and have confidence my data is fully removed.

**Acceptance Criteria:**

**Given** a `DELETE /api/account` request from an authenticated user
**When** the deletion orchestrator executes
**Then** the following data is permanently deleted: profile photo from object storage, all messages in all conversations, all conversation records, the user's `UserProfile`, quiz answers, invite links generated by this user, subscription records, device tokens, report records (as reporter), and the `ApplicationUser` record itself

**Given** a user has been involved in moderation actions
**When** the deletion cascade runs
**Then** `ModerationAction` records where this user is the subject are retained for the minimum 2-year audit period — they are NOT deleted; the `user_id` reference is anonymised but the action record itself persists

**Given** account deletion completes
**When** the response is returned
**Then** HTTP 204 No Content is returned; the user's JWT becomes invalid; attempting to use the deleted account's token returns 401

**Given** the deletion request is received
**When** the photo purge runs
**Then** `AWSSDK.S3` `DeleteObjectAsync` is called for the user's photo object key — the stored photo is permanently removed from Hetzner Object Storage

**Given** a user has active conversations at deletion time
**When** deletion completes
**Then** the matched user's conversation is closed/archived and they receive an in-app notification that the conversation has ended — they are not left with an orphaned open conversation

---

## Epic 3: Onboarding & Profile Setup

A new user completes the values and personality quiz, uploads a private profile photo (content-scanned before storage), sets preferences, and receives their 7-day premium trial — landing in their first match conversation upon completion.

### Story 3.1: Values and Personality Quiz

As a new user completing onboarding,
I want to answer a short values and personality quiz,
So that the algorithm has the data it needs to find me a genuinely compatible match.

**Acceptance Criteria:**

**Given** a new authenticated user opens `app/(auth)/onboarding/quiz.tsx`
**When** the quiz screen renders
**Then** 4–5 questions appear covering interests, life priorities, and relationship intent; the total interaction takes under 3 minutes; no question asks the user to rate their own attractiveness

**Given** the user completes all quiz questions and submits
**When** `POST /api/onboarding/quiz` is called
**Then** quiz answers are stored in `UserProfile.quiz_answers` (JSONB column, PostgreSQL); `UserProfile` is created if it does not exist; `ApplicationUser.QuizCompletedAt` is set

**Given** `UserProfile` model is created
**When** the migration is reviewed
**Then** table `user_profiles` contains: `id`, `user_id` (FK → `users`, unique), `quiz_answers` (jsonb), `photo_object_key` (text, nullable), `radius_km` (int, default 50), `min_age` (int), `max_age` (int), `created_at` (timestamptz), `updated_at` (timestamptz)

**Given** a quiz submission fails FluentValidation
**When** the error response is returned
**Then** HTTP 400 Problem Details is returned with per-field error details — no quiz answer is persisted on validation failure

**Given** the user navigates back during the quiz
**When** they return to the quiz screen
**Then** previously answered questions retain their answers — progress is not lost

---

### Story 3.2: Private Profile Photo Upload

As a new user completing onboarding,
I want to upload a profile photo that is stored privately and never shown until I choose to reveal it,
So that I can participate in Blinder without the anxiety of being judged on my appearance immediately.

**Acceptance Criteria:**

**Given** `app/(auth)/onboarding/photo.tsx` renders
**When** the photo upload step appears
**Then** it appears AFTER the quiz step (the user is already invested); copy reads "No one sees this until you both choose." — low-pressure framing; the photo upload UI does NOT display the photo to other users

**Given** the user selects a photo from the device library or camera via `expo-image-picker`
**When** the photo is uploaded via `POST /api/onboarding/photo`
**Then** the raw photo bytes are received by the backend BEFORE any storage write; the content scanning pipeline executes synchronously before any S3 write is confirmed

**Given** the Hetzner Object Storage S3 client is configured
**When** `S3ClientFactory.cs` creates the client
**Then** `ForcePathStyle = true` is set — this is validated during the spike before this story is implemented

**Given** the photo is successfully scanned and stored
**When** the object key is persisted
**Then** `UserProfile.photo_object_key` is set to `photos/{guid}` — the key does NOT contain the user's ID or any PII; the bucket has no public ACL

**Given** the photo upload response is returned to the mobile client
**When** the confirmation is received
**Then** no public URL is returned — clients never receive a direct S3 URL for a profile photo

---

### Story 3.3: Explicit Content Scanning Gate at Photo Upload

As a platform operator,
I want all uploaded photos to be scanned for explicit content before storage,
So that harmful content never enters the system and the scanning pipeline fails safe (hard-reject on API outage).

**Acceptance Criteria:**

**Given** a photo upload request arrives at `POST /api/onboarding/photo`
**When** `ContentScanningClient.ScanAsync` executes
**Then** the scan completes within 10 seconds (NFR3); the photo is NOT written to S3 until a clean scan result is confirmed

**Given** the content scan detects explicit content
**When** the result is returned
**Then** the upload is rejected with HTTP 422 Problem Details; no bytes are written to S3; the user receives a clear in-app error message

**Given** the content scanning API (Google Vision SafeSearch or Azure Content Moderator) is unavailable
**When** `ContentScanningClient` receives a timeout or connection error
**Then** the photo is rejected (hard-fail policy) — HTTP 503 is returned; nothing is stored; `ContentScanningClient` is in `Infrastructure/Scanning/` (NOT in `Services/`)

**Given** up to 1,000 concurrent photo uploads occur
**When** the scanning pipeline is under load
**Then** each upload still completes scan + storage within the 10-second SLA (NFR18)

**Given** `ContentScanningClient` succeeds
**When** the scan result is logged
**Then** the scan outcome is recorded via `ILogger<T>` with structured properties — no PII (no user IDs or photo content) in log properties

---

### Story 3.4: CSAM Hash Scanning Integration Slot

As a platform operator,
I want all uploaded photos to be checked against CSAM hash databases before storage,
So that the platform meets its legal obligations and no CSAM material is ever persisted.

**Acceptance Criteria:**

**Given** a photo upload request arrives
**When** the CSAM scan check executes
**Then** it runs server-side BEFORE any S3 write — CSAM material is never persisted (NFR11)

**Given** the CSAM scan detects a hash match
**When** the result is returned
**Then** the upload is rejected and the detection event is logged to the operations alert channel; a Coravel-queued `IInvocable` notifies the operations team immediately (NFR26)

**Given** the NCMEC PhotoDNA legal agreement is not yet in place
**When** the scanning pipeline runs in the interim
**Then** the CSAM scan slot is implemented as a configurable `ICsamScanningProvider` interface with a `StubCsamScanningProvider` that logs a warning and passes — the real PhotoDNA provider is injected when the legal agreement is signed

**Given** the CSAM integration is not yet active (stub mode)
**When** the operations team reviews the configuration
**Then** a prominent startup warning is logged: "CSAM scanning is running in STUB mode — real PhotoDNA integration not active. NCMEC agreement required before launch."

**Given** the CSAM scan fails (API outage, not a detection)
**When** the pipeline error handling runs
**Then** an immediate alert is sent to the operations team (NFR26); the photo upload is rejected (hard-fail); silent failure is not acceptable

---

### Story 3.5: Preferences Setup (Age Range and Search Radius)

As a user,
I want to set my age range preference and search radius during onboarding,
So that matches are geographically and demographically relevant to me.

**Acceptance Criteria:**

**Given** `app/(auth)/onboarding/preferences.tsx` renders
**When** the user sets preferences
**Then** minimum and maximum age range sliders and a search radius slider (in km) are presented; defaults are pre-populated (age ±5 years, 50km radius)

**Given** a `POST /api/onboarding/preferences` request is submitted
**When** the preferences are validated
**Then** `min_age` ≥ 18, `max_age` ≤ 100, `max_age` > `min_age`, `radius_km` between 1 and 500 — violations return HTTP 400 Problem Details

**Given** valid preferences are submitted
**When** they are persisted
**Then** `UserProfile.min_age`, `UserProfile.max_age`, and `UserProfile.radius_km` are updated; `updated_at` is set to the current UTC `DateTimeOffset`

**Given** a user wants to update preferences post-onboarding
**When** `PUT /api/account/preferences` is called
**Then** the same validation rules apply and `UserProfile` is updated — FR6 fulfilled

---

### Story 3.6: 7-Day Premium Trial Activation on Registration

As a new user,
I want a 7-day premium trial activated automatically when I register,
So that I experience premium features immediately and understand what I'd be paying for.

**Acceptance Criteria:**

**Given** a user completes registration (any method)
**When** their account is created
**Then** a `Subscription` record is created with `plan: "trial"`, `status: "active"`, `trial_expires_at: now + 7 days` (UTC DateTimeOffset)

**Given** `Subscription` model is created
**When** the migration is reviewed
**Then** table `subscriptions` contains: `id`, `user_id` (FK → `users`, unique), `plan` (text: trial/free/premium), `status` (text: active/expired/cancelled), `trial_expires_at` (timestamptz, nullable), `platform` (text: Apple/Google, nullable), `created_at` (timestamptz)

**Given** a user opens the app after registration
**When** the app shell renders
**Then** a visible in-app notification states "Your 7-day premium trial is active" — explicit notification as per FR11

**Given** the `TrialExpiryNotificationJob` runs (scheduled daily via Coravel)
**When** a user's trial expires in 2 days
**Then** an in-app push notification is sent: "Your premium trial expires in 2 days" — day-5 warning as per FR30

---

### Story 3.7: First Match Conversation on Onboarding Completion

As a new user who has completed the quiz, uploaded a photo, and set preferences,
I want to land on an active conversation with my first match immediately after onboarding,
So that I never face an empty home screen and the product delivers on its first promise.

**Acceptance Criteria:**

**Given** a user completes all onboarding steps (quiz, photo, preferences)
**When** `POST /api/onboarding/complete` is called
**Then** `ApplicationUser.IsOnboardingComplete` is set to `true`; `MatchGenerationJob` is triggered immediately (not waiting for the next cron tick)

**Given** `MatchGenerationJob` finds a compatible match
**When** the match is created
**Then** a `Match` record is persisted, a `Conversation` is created for the match, and a `MatchAssigned` SignalR event is broadcast to both clients; the mobile client navigates to the conversation

**Given** `MatchGenerationJob` does NOT find a match immediately
**When** the home screen renders
**Then** `EmptyMatchState.tsx` is displayed with brand-voice copy: "You have not been forgotten. Your match is being found." — never a generic spinner or blank screen

**Given** `EmptyMatchState.tsx` is implemented
**When** a match is later assigned
**Then** a push notification arrives and the app navigates directly to the new conversation — the empty state is replaced immediately

**Given** the app server restarts (VPS reboot, deployment)
**When** `MatchGenerationJob` initialises at startup
**Then** an idempotent startup check runs immediately: any onboarding-complete users without an active match are queued for match generation — no user stares at an empty screen indefinitely due to a restart

**Given** `Match` and `Conversation` models are created
**When** migrations are reviewed
**Then** `matches` table contains: `id`, `user_a_id`, `user_b_id`, `compatibility_score` (float), `matched_at` (timestamptz), `is_active` (bool); `conversations` table contains: `id`, `match_id` (FK), `message_count` (int default 0), `status` (text: active/archived/reported), `created_at` (timestamptz)

---

## Epic 4: Matching Engine

The system matches users using a rules-based values-weighted compatibility algorithm (with demographic fallback), delivers curated matches only — no browsing — and tracks invite links per female account.

### Story 4.1: Values-Weighted Compatibility Matching Algorithm

As the matching system,
I want to score compatibility between users based on their quiz answers,
So that matches are based on genuine values alignment rather than demographics alone.

**Acceptance Criteria:**

**Given** two users with completed `quiz_answers` in their `UserProfile`
**When** `MatchService.ComputeCompatibilityScore` runs
**Then** a deterministic float score between 0.0 and 1.0 is returned based on weighted comparison of quiz answer values — the algorithm is rules-based (not ML)

**Given** the compatibility threshold is read by `MatchService`
**When** the threshold lookup executes
**Then** the value is read from `AppSettings` (key: `compatibility_threshold`) — it is NOT hardcoded; the threshold is admin-configurable via the Razor Pages admin UI

**Given** `AppSettings` model is created
**When** the migration is reviewed
**Then** table `app_settings` contains: `key` (text, primary key), `value` (text), `updated_at` (timestamptz); seeded with `compatibility_threshold = "0.6"` and `reveal_message_threshold = "100"` in the migration

**Given** `MatchGenerationJob` runs
**When** an eligible user needs a match
**Then** the job queries users of the preferred gender within the radius (PostGIS spatial query), computes compatibility scores, and selects the highest-scoring user above the threshold; if multiple users qualify, the highest scorer is selected

**Given** the match is created
**When** `Match` record is persisted
**Then** `compatibility_score` is stored; `matched_at` is a UTC `DateTimeOffset`; both `user_a_id` and `user_b_id` are set

---

### Story 4.2: Demographic Fallback Matching

As a user in a small geographic area,
I want to receive a match even when no algorithmically compatible user is available,
So that I don't wait indefinitely in an empty state because the pool is too small.

**Acceptance Criteria:**

**Given** `MatchGenerationJob` finds no users above the compatibility threshold
**When** the fallback logic runs
**Then** demographic matching activates: users within the radius and within the age range preference are candidates; the closest-age user of the preferred gender is selected

**Given** demographic fallback is used
**When** the match is created
**Then** `Match.compatibility_score` is set to `0.0` (indicating fallback) to distinguish from algorithm matches; the user-facing experience is identical — no indication of fallback is shown to the user

**Given** no users exist in the demographic fallback pool either
**When** the job completes
**Then** the user remains on `EmptyMatchState.tsx`; a message is logged at `Information` level: "No match candidates found for user {UserId} in radius {RadiusKm}" — no PII in the log properties

---

### Story 4.3: Match Generation Background Job with Idempotent Startup Check

As a platform operator,
I want match generation to run on a schedule AND immediately at startup,
So that users who signed up or were waiting before a VPS restart immediately get a match without waiting for the next cron tick.

**Acceptance Criteria:**

**Given** the API container starts
**When** the application startup sequence completes
**Then** `MatchGenerationJob` executes immediately (before accepting HTTP traffic) as an idempotent check — all onboarding-complete users without an active match are queued for match generation

**Given** `MatchGenerationJob` runs (startup or scheduled)
**When** a user already has an active match
**Then** the job does NOT create a duplicate match — idempotency check: `WHERE is_active = true AND (user_a_id = {userId} OR user_b_id = {userId})` returns a result → skip

**Given** Coravel is configured in `Program.cs`
**When** the job schedule is reviewed
**Then** `MatchGenerationJob` is scheduled to run every N minutes (configurable via `app_settings`) in addition to the startup check

**Given** the VPS restarts during active operations
**When** `MatchGenerationJob` runs at startup
**Then** any user who was waiting for their first match before the restart receives a match assignment within 30 seconds of the API container being ready (NFR5)

---

### Story 4.4: Geographic Radius Matching with PostGIS

As a user,
I want matches to respect my search radius preference,
So that I only receive matches with people who are realistically meetable.

**Acceptance Criteria:**

**Given** a user sets their `radius_km` preference
**When** their location is stored
**Then** `UserProfile` stores coordinates as a `Point` (PostGIS geometry type) using `NetTopologySuite` — geographic coordinates are stored as `GEOGRAPHY(POINT, 4326)` in the migration

**Given** `MatchService` queries for candidates
**When** the spatial query executes
**Then** a PostGIS `ST_DWithin` query filters candidates by the user's `radius_km` — no haversine calculation on raw lat/lng columns; a spatial index is present on the coordinates column

**Given** a user has not provided location
**When** the matching job runs
**Then** geographic filtering is skipped and demographic fallback applies; the user sees a prompt to enable location services to improve match quality

**Given** location data is stored
**When** the `UserProfile` is inspected
**Then** location is stored as coarse coordinates (not real-time tracking) — location is only updated when the user explicitly updates their radius preference, not on every app open

---

### Story 4.5: Curated Match Display — No Browse Interface

As a user,
I want to see my single curated match without any ability to browse other profiles,
So that I experience Blinder's no-browse philosophy from day one.

**Acceptance Criteria:**

**Given** a user has an active match
**When** `app/(tabs)/match/index.tsx` renders
**Then** the current match is displayed with minimal information (first name, quiz-derived conversation prompt); there is NO grid of profiles, NO swipe cards, NO "see more people" button, NO search or filter UI anywhere in the app

**Given** `GET /api/matches/current` is called
**When** the response is returned
**Then** the response contains ONLY the current active match's anonymous info (no photo, no full profile); `UserProfile.photo_object_key` is never included in this response — not even as a signed URL

**Given** `EmptyMatchState.tsx` renders
**When** no active match exists
**Then** the component displays brand-voice copy "You have not been forgotten. Your match is being found." with the amber `#C8833A` accent visible — never a generic loading spinner for this state (UX-DR22)

---

### Story 4.6: Invite Link Generation and Tracking

As a female user,
I want to generate invite links to share with friends,
So that I can bring trusted people into the community and Blinder can maintain gender balance.

**Acceptance Criteria:**

**Given** a female authenticated user calls `POST /api/invites`
**When** the invite is created
**Then** a new `InviteLink` record is created with a unique cryptographically random token; the `generated_by_user_id` FK is set to the requesting user

**Given** an invite link is generated
**When** the response is returned
**Then** the response includes the full deep-link URL (`blinder://invite/{token}`) for sharing; the token itself is a minimum 32-character URL-safe random string

**Given** a male user calls `POST /api/invites`
**When** the request is processed
**Then** HTTP 403 Forbidden is returned — only female users can generate invite links

**Given** an invite link is used during female registration
**When** registration completes
**Then** `InviteLink.used_by_user_id` is updated to the new user's ID and `used_at` timestamptz is recorded — the link cannot be reused

**Given** the admin gender ratio dashboard is viewed
**When** invite usage data is queried
**Then** invite lineage (who invited whom) is traceable via `InviteLink.generated_by_user_id` and `used_by_user_id` FKs

---

## Epic 5: Real-Time Chat

A matched user can send and receive real-time text messages via SignalR, view their conversation list with message counts, and receive push notifications for all key events.

### Story 5.1: SignalR Chat Hub and Real-Time Messaging

As a matched user,
I want to send and receive messages in real time,
So that conversations feel natural and immediate.

**Acceptance Criteria:**

**Given** two matched users both have the mobile app open
**When** one user sends a message via `POST /api/conversations/{id}/messages`
**Then** the message is persisted to the `messages` table AND a `ReceiveMessage` SignalR event is broadcast to the other user's connected client within 500ms (NFR1)

**Given** `Hubs/ChatHub.cs` is implemented
**When** the hub method is reviewed
**Then** the SignalR hub lives at `/hubs/chat`; hub method names are PascalCase matching exactly on client: `ReceiveMessage`, `RevealStateUpdated`, `MatchAssigned`

**Given** `signalrService.ts` is implemented on the mobile client
**When** the connection is inspected
**Then** it is a singleton — never instantiated per-component; connection lifecycle (start, stop, reconnect) is managed entirely within `signalrService.ts`; components subscribe via `useConversation` hook only

**Given** the chat UI renders in `app/(tabs)/conversations/[id].tsx`
**When** the interface is inspected
**Then** it uses a WhatsApp-conventional layout (sender right, receiver left, timestamps below bubbles); there is NO profile photo visible anywhere in the chat interface — the conversation is photo-free until reveal

**Given** `Message` model is created
**When** the migration is reviewed
**Then** table `messages` contains: `id`, `conversation_id` (FK), `sender_id` (FK → `users`), `body` (text), `sent_at` (timestamptz); index on `conversation_id`

**Given** the backend sends a message
**When** `Message` is mapped to `MessageResponse` DTO
**Then** all mapping goes through `ConversationMapper` (Mapperly `[Mapper]` partial class) — no manual `new MessageResponse { ... }` construction

---

### Story 5.2: Conversation List with Message Counts

As a user,
I want to see all my active conversations with their message counts,
So that I can track conversation depth and stay aware of my progress toward the reveal threshold.

**Acceptance Criteria:**

**Given** an authenticated user calls `GET /api/conversations`
**When** the response is returned
**Then** all active conversations are listed with: conversation ID, matched user's first name, `message_count`, `last_message_preview` (first 50 chars), `last_message_at`; conversations are sorted by most recent activity

**Given** `app/(tabs)/conversations/index.tsx` renders
**When** conversations are displayed
**Then** each conversation card shows the matched user's name and message count; the reveal threshold progress is subtly indicated (e.g., "42 / 100 messages") — no urgency framing, presented as progress not countdown

**Given** a new message is sent in any conversation
**When** `Conversation.message_count` is updated
**Then** the increment is atomic (SQL `UPDATE conversations SET message_count = message_count + 1`) — no race condition from concurrent messages; the conversation list refreshes via SignalR push or on next poll

---

### Story 5.3: Active Conversation Limit Enforcement (Free Tier)

As a free-tier user,
I want the app to enforce the 3-conversation limit,
So that I understand the freemium model and see a clear upgrade path when I need more.

**Acceptance Criteria:**

**Given** a free-tier user has 3 active conversations
**When** the matching job attempts to assign a new match
**Then** the new match is NOT created; the user does NOT receive a new conversation; the existing 3 remain active

**Given** a free-tier user reaches their conversation limit
**When** they open the match screen
**Then** an in-app upgrade prompt is displayed (FR31): "You've reached your free limit of 3 conversations. Upgrade to continue."

**Given** a premium user exists
**When** the active conversation limit is checked
**Then** the premium limit (defined in `app_settings` as `premium_max_conversations`, default 10) applies instead of the free limit of 3

**Given** a free-tier user's conversation ends (both parties move on post-reveal)
**When** the conversation status changes to `archived`
**Then** the active conversation count decrements and the user is eligible for a new match

---

### Story 5.4: Push Notifications for Chat Events

As a user,
I want push notifications for new messages, new matches, and reveal events,
So that I stay engaged with my conversations even when the app is in the background.

**Acceptance Criteria:**

**Given** a user logs in or re-authenticates on a mobile device
**When** the app calls `POST /api/account/device-token`
**Then** the device's raw native token (from `getDevicePushTokenAsync()`, NOT `getExpoPushTokenAsync()`) is stored in `DeviceTokens` with `user_id`, `token`, `platform` (Android/iOS), `created_at`

**Given** a new message arrives in a conversation
**When** the message is persisted
**Then** `SendPushNotificationJob` is queued via Coravel with the recipient's device tokens; FCM (Android via `FirebaseAdmin`) or APNs (iOS via `dotAPNS`) delivers the notification within 60 seconds (NFR28)

**Given** `SendPushNotificationJob` receives an FCM `registration-token-not-registered` error
**When** the error is handled
**Then** the stale token is deleted from `DeviceTokens` immediately — the table never accumulates dead tokens

**Given** the following events occur
**When** a user is not actively viewing the relevant screen
**Then** push notifications are sent: new match assigned, incoming message, match has opted into reveal, mutual reveal confirmed, premium trial expiring (day 5)

**Given** `DeviceToken` model is created
**When** the migration is reviewed
**Then** table `device_tokens` contains: `id`, `user_id` (FK, indexed), `token` (text, unique), `platform` (text: Android/iOS), `created_at` (timestamptz)

---

## Epic 6: Mutual Photo Reveal

A user can express reveal readiness after reaching the 100-message threshold, and when both parties independently opt in, both photos are delivered simultaneously via a real-time SignalR broadcast — with the full emotional UX arc.

### Story 6.1: Reveal Readiness Flag and Message Threshold Gate

As a user in a conversation,
I want to see the reveal option appear quietly after 100 messages,
So that I can choose to share my photo when I feel genuinely ready — not pressured by a countdown.

**Acceptance Criteria:**

**Given** `RevealState` model is created
**When** the migration is reviewed
**Then** table `reveal_states` contains: `id`, `conversation_id` (FK, unique), `user_a_reveal_ready` (bool, default false), `user_b_reveal_ready` (bool, default false), `user_a_ready_at` (timestamptz, nullable), `user_b_ready_at` (timestamptz, nullable); `RevealState` is created alongside each `Conversation`

**Given** `app_settings` is seeded
**When** the `reveal_message_threshold` key is checked
**Then** value is `"100"` — admin-configurable; this is the default initial value decided by the product owner

**Given** a conversation's `message_count` reaches the threshold value from `app_settings`
**When** `GET /api/conversations/{id}/reveal-state` is called
**Then** `isThresholdMet: true` is returned; `RevealPrompt.tsx` renders in the chat interface as a quiet, non-intrusive affordance — NOT a push notification, NOT a modal interrupt

**Given** `RevealPrompt.tsx` renders
**When** the component is inspected
**Then** copy reads "The reveal is available when you're both ready."; "Not yet" option is visually equal weight to "I'm ready" — no dark pattern nudging toward readiness; the component appears below the message input, not as an overlay

**Given** a conversation has fewer than `reveal_message_threshold` messages
**When** `GET /api/conversations/{id}/reveal-state` is called
**Then** `isThresholdMet: false` is returned; `RevealPrompt.tsx` does NOT render — no indication of threshold proximity is shown

---

### Story 6.2: Mutual Reveal Confirmation and Signed Photo URL Delivery

As a user who has expressed reveal readiness,
I want my photo to only be delivered when my match has also independently opted in,
So that no photo is ever seen without full bilateral consent.

**Acceptance Criteria:**

**Given** a user taps "I'm ready" in `RevealPrompt.tsx`
**When** `POST /api/conversations/{id}/reveal-ready` is called
**Then** the calling user's `reveal_ready` flag (`user_a_reveal_ready` or `user_b_reveal_ready`) is set to `true` and `*_ready_at` timestamptz is recorded; the OTHER user's flag is NOT changed and NOT visible to the first user

**Given** only one user has set their flag
**When** either user calls `GET /api/conversations/{id}/reveal-state`
**Then** the response only shows the calling user's OWN readiness flag — the other party's flag is NEVER exposed until both are true

**Given** both users have set their `reveal_ready` flags to `true`
**When** `RevealService.GetRevealPhotoUrls` is called
**Then** time-limited signed URLs (1-hour expiry) for BOTH parties' photos are generated via `S3ClientFactory`; URL generation is gated: if either flag is `false`, signed URLs are NOT generated and a 403 is returned

**Given** signed URLs are generated
**When** the photo is retrieved by the mobile client
**Then** the URL expires after 1 hour; the client must request a fresh signed URL on each reveal screen open; no public S3 URL is ever returned

---

### Story 6.3: Real-Time Simultaneous Reveal Broadcast via SignalR

As both users in a conversation,
I want to receive the reveal moment simultaneously when we both opt in,
So that the emotional experience is a shared discovery rather than an asynchronous notification check.

**Acceptance Criteria:**

**Given** the second user sets their `reveal_ready` flag (completing mutual readiness)
**When** `RevealService` detects both flags are `true`
**Then** a `RevealStateUpdated` event is broadcast via `IHubContext<ChatHub>` to BOTH connected clients simultaneously — not just the second user

**Given** `RevealStateUpdated` is received by the mobile client
**When** `useRevealState` hook processes the event
**Then** the client immediately navigates to `RevealMoment.tsx` without requiring a manual app open or page refresh

**Given** one or both users are NOT currently connected to SignalR (app backgrounded)
**When** mutual reveal is confirmed
**Then** a push notification is sent to the backgrounded user(s) via `SendPushNotificationJob`: "Mutual reveal confirmed — open Blinder to see your match"; on app open, the reveal state is fetched and `RevealMoment.tsx` renders

**Given** the `RevealStateUpdated` SignalR event payload is defined in `types/signalr/index.ts`
**When** the type is reviewed
**Then** it contains `conversationId`, `revealReady: true`, and does NOT include photo URLs (those are fetched via REST after receiving the event)

---

### Story 6.4: Reveal Moment — Full UX Experience

As a user experiencing a mutual reveal,
I want the reveal to feel like a meaningful, emotionally appropriate moment,
So that the product delivers on its core promise: a shared discovery that honours the courage it took to get here.

**Acceptance Criteria:**

**Given** `RevealMoment.tsx` receives both signed photo URLs
**When** the component mounts
**Then** the screen is full-screen with no UI chrome (no back button, no tab bar, no navigation header); background is `#0F0D0B`; amber `#C8833A` accent is visible at the reveal moment (UX-DR2 amber bookend)

**Given** `RevealMoment.tsx` plays the reveal animation
**When** `reduceMotion` is `false`
**Then** photos appear with a 600–800ms motion treatment; after the animation completes, 1–2 seconds of space pass before any interactive element appears; copy reads "[Name] — there they are."

**Given** `reduceMotion` is `true` (from `AccessibilityContext`)
**When** the reveal animation runs
**Then** photos appear via opacity fade only — no motion-based transition fires

**Given** the continuation gate renders after the reveal
**When** both users are asked "Would you like to keep talking?"
**Then** each user answers independently; "Yes" and "Move on" are both present; the asymmetric outcome (one yes, one no) is NEVER shown to either party — both see only "This chapter is complete. Ready to begin another?"

**Given** `RevealCountdown.tsx` renders while one user is waiting (flag set, other not yet)
**When** the component is inspected
**Then** copy reads "You've indicated you're ready. Waiting for them."; there is NO countdown timer, NO read receipt indicator, NO signal to the other party that this user is ready

**Given** the consent gate pattern is applied
**When** "I'm ready" and "Not yet" buttons are rendered at any consent decision point
**Then** both options have visually equal weight (same size, same prominence) — no dark pattern and the "Not yet" option is never styled as secondary/grey/dismissive

---

### Story 6.5: Premium Reveal Threshold Customization

As a premium user,
I want to lower my personal reveal readiness threshold,
So that I can express readiness at fewer messages — while my match's independent consent remains mandatory.

**Acceptance Criteria:**

**Given** a premium user's subscription status is active
**When** `RevealService.GetEffectiveThreshold` is called for this user
**Then** the premium threshold (from `app_settings` key `premium_reveal_threshold`, default `"50"`) is used for THIS user's side only — the other party's threshold is always the standard `reveal_message_threshold` regardless of the premium user's setting

**Given** a premium user reaches their lower threshold (e.g., 50 messages)
**When** the reveal affordance check runs
**Then** `RevealPrompt.tsx` becomes available for the premium user; the other user still sees no reveal option until THEY reach THEIR own threshold (100 messages by default)

**Given** the other party is NOT premium and has NOT reached their threshold
**When** the premium user sets their `reveal_ready` flag
**Then** mutual reveal does NOT trigger — both flags must be `true` regardless of threshold; the premium user sees `RevealCountdown.tsx` (waiting state)

---

## Epic 7: Subscriptions & Premium

A user can subscribe via Apple IAP or Google Play Billing, access premium features, receive trial expiry notifications, and see upgrade prompts at free-tier limits.

### Story 7.1: IAP Webhook JWT Verification Spike

As a developer,
I want to implement and verify JWT signature verification for both Apple and Google IAP webhooks before writing any subscription business logic,
So that fake webhook payloads cannot be used to fraudulently unlock premium features (OWASP A01).

**Acceptance Criteria:**

**Given** an Apple AppStore Server Notification arrives at `POST /api/subscriptions/apple-webhook`
**When** the controller processes the request
**Then** the signed JWT payload is verified against Apple's JWKS endpoint (`https://appleid.apple.com/auth/keys`) using `Microsoft.IdentityModel.Tokens`; `iss` claim equals `"https://appleid.apple.com"`; `bundle_id` claim matches the app's bundle ID

**Given** Apple JWKS keys are fetched
**When** subsequent webhook requests arrive
**Then** JWKS keys are cached with a 24-hour TTL — the JWKS endpoint is NOT called per-request

**Given** a Google Play Real-Time Developer Notification arrives at `POST /api/subscriptions/google-webhook`
**When** the controller processes the request
**Then** the JWT in the `Authorization: Bearer` header is verified against Google's JWKS endpoint; `iss` and `aud` claims are validated against the expected service account and package name

**Given** `appsettings.Testing.json` is reviewed
**When** `Subscriptions:SkipWebhookVerification` is present
**Then** it is `true` only in the test environment; production `appsettings.json` does NOT contain this key

**Given** the API starts in production
**When** the startup assertions run
**Then** if `Subscriptions:SkipWebhookVerification` is `true` AND the environment is not `Testing`, startup fails with a clear error: "IAP webhook verification bypass is not permitted in production"

---

### Story 7.2: Apple In-App Purchase Subscription Integration

As a premium iOS subscriber,
I want to purchase a Blinder premium subscription via Apple In-App Purchase,
So that my subscription is processed through the App Store with full Apple billing compliance.

**Acceptance Criteria:**

**Given** an Apple subscription purchase completes on the mobile client (StoreKit)
**When** a `SUBSCRIBED` AppStore Server Notification arrives at the verified webhook endpoint
**Then** `SubscriptionService` updates the user's `Subscription` record: `plan: "premium"`, `status: "active"`, `platform: "Apple"`

**Given** an Apple subscription renews
**When** a `DID_RENEW` notification arrives
**Then** the `Subscription` record's `status` remains `active`; expiry date is updated

**Given** an Apple subscription is cancelled or expires
**When** the corresponding notification arrives
**Then** `Subscription.status` is set to `expired`; the user's premium features are downgraded to free tier limits on their next API call

**Given** raw webhook processing occurs
**When** `Subscription` data is persisted
**Then** no raw card data is ever stored — all payment data handling is delegated to Apple (PCI DSS compliance, NFR14)

---

### Story 7.3: Google Play Billing Subscription Integration

As a premium Android subscriber,
I want to purchase a Blinder premium subscription via Google Play Billing,
So that my subscription is processed through Google Play with full billing compliance.

**Acceptance Criteria:**

**Given** a Google Play subscription purchase completes on the mobile client
**When** a verified Google Play Real-Time Developer Notification arrives
**Then** `SubscriptionService` updates the user's `Subscription` record: `plan: "premium"`, `status: "active"`, `platform: "Google"`

**Given** a Google Play subscription renewal or cancellation event arrives
**When** the webhook is processed
**Then** `Subscription.status` is updated accordingly within 60 seconds of the platform event (NFR27)

**Given** both Apple and Google webhooks can update the same user's subscription
**When** the `SubscriptionService` processes either
**Then** the `platform` field correctly identifies the billing source; no cross-platform subscription conflict occurs

---

### Story 7.4: Premium Feature Gating and Increased Limits

As a premium user,
I want my premium subscription to unlock increased match and conversation limits,
So that I can have more simultaneous conversations and matches than the free tier allows.

**Acceptance Criteria:**

**Given** a user's `SubscriptionService.IsPremium` returns `true`
**When** active conversation limit is checked
**Then** the premium limit (from `app_settings` key `premium_max_conversations`, default 10) is applied — not the free tier limit of 3

**Given** a user's subscription expires
**When** `SubscriptionService.IsPremium` returns `false`
**Then** the user's active conversation count is compared to the free limit (3); if they exceed it, existing conversations are NOT deleted — they are preserved but no NEW conversations are created until the count drops to ≤3

**Given** a premium check is performed on any feature-gated endpoint
**When** subscription state is read
**Then** it reads from the `subscriptions` table in real time — it is NOT cached in memory in a way that persists stale state across subscription changes

---

### Story 7.5: Trial Expiry Notifications

As a user whose premium trial is ending,
I want to receive a notification before my trial expires,
So that I can decide whether to subscribe before losing premium access.

**Acceptance Criteria:**

**Given** `TrialExpiryNotificationJob` runs daily via Coravel
**When** the job executes
**Then** all users with `subscriptions.plan = "trial"` AND `trial_expires_at` between now and now + 48 hours are identified; a push notification is sent: "Your premium trial expires in 2 days — upgrade to keep your benefits"

**Given** a user's trial has expired
**When** `TrialExpiryNotificationJob` runs
**Then** `Subscription.status` is set to `expired` and `plan` to `free`; the user's next API request reflects free-tier limits

**Given** a push notification is sent by `TrialExpiryNotificationJob`
**When** the job fires
**Then** it uses `SendPushNotificationJob` via Coravel fire-and-forget — it does NOT await the push response synchronously

---

### Story 7.6: Free Tier Limit Upgrade Prompts

As a free-tier user who has hit their limits,
I want to see an in-app upgrade prompt when I reach my conversation or match limit,
So that I understand my options and can easily upgrade.

**Acceptance Criteria:**

**Given** a free-tier user has 3 active conversations and a new match would be assigned
**When** the match assignment is blocked
**Then** an in-app notification is queued: "You've reached your free limit of 3 conversations. Upgrade to Blinder Premium for more."

**Given** the upgrade prompt appears
**When** the user taps it
**Then** they are directed to the subscription purchase screen (Apple StoreKit / Google Play Billing); the web subscription path is NOT promoted from within the iOS app (Apple anti-steering compliance)

**Given** a free-tier user's active conversation count drops below 3 (post-reveal, conversation archived)
**When** the next match assignment runs
**Then** the new match is assigned without requiring any user action — no stale blocked state

---

## Epic 8: Safety & Content Moderation

A user can report a conversation with one tap and receive immediate acknowledgement and follow-up. Text messages are auto-flagged for harassment. A moderator can review reports, apply actions, and reports immediately suspend reveal capability.

### Story 8.1: Automated Text Message Flagging

As a platform operator,
I want the system to automatically flag messages containing harassment patterns or explicit solicitation,
So that moderators are alerted to potential violations without relying solely on user reports.

**Acceptance Criteria:**

**Given** a message is submitted via `POST /api/conversations/{id}/messages`
**When** the message body is evaluated
**Then** a text flagging check runs against a configurable pattern list (stored in `app_settings` as a JSON array of regex patterns); flagged messages are still delivered to the recipient but a `Report` record is created automatically with `reason: "auto_flagged"` and the triggering pattern logged

**Given** an auto-flagged message creates a `Report`
**When** the moderator queue is reviewed
**Then** the auto-flagged report appears in the queue alongside user-submitted reports, clearly labelled as "Auto-flagged"

**Given** the flagging pattern list is updated via the admin settings UI
**When** a new regex pattern is saved to `app_settings`
**Then** new messages are evaluated against the updated pattern list within one application restart — no code deployment required

---

### Story 8.2: One-Tap Reporting with Immediate Acknowledgement

As a user who encounters harmful behaviour,
I want to report it with a single tap and receive immediate acknowledgement,
So that I feel heard and safe without friction between the decision to report and the action.

**Acceptance Criteria:**

**Given** `ReportButton.tsx` renders in `app/(tabs)/conversations/[id].tsx`
**When** the chat header is inspected
**Then** the report icon is always visible in the header — never buried in a settings menu; `accessibilityRole="button"`, `accessibilityLabel="Report this conversation"` are present; defined once in `components/moderation/ReportButton.tsx` — NOT reimplemented inline

**Given** the user taps the report button
**When** the report type selection sheet opens
**Then** categories are: Explicit content / Harassment / Impersonation / Other; optional context text field is present but NOT required — the submit button is enabled without it

**Given** the user submits a report
**When** `POST /api/moderation/reports` is called
**Then** a `Report` record is created with `reporter_id`, `reported_user_id`, `conversation_id`, `reason`, `created_at`; the reported user's `RevealState` flags are set to suspended (both flags reset, conversation's `reveal_suspended: true`); an immediate 200 response with acknowledgement copy is returned

**Given** the acknowledgement screen renders
**When** the response is displayed
**Then** copy reads: "We've received your report. You are safe here."; a `BlockConfirmation.tsx` modal offers the option to block the reported user — defined once in `components/moderation/BlockConfirmation.tsx`

**Given** the `Report` model is created
**When** the migration is reviewed
**Then** table `reports` contains: `id`, `reporter_id` (FK → `users`), `reported_user_id` (FK → `users`), `conversation_id` (FK), `reason` (text), `context` (text, nullable), `status` (text: pending/reviewed/actioned), `created_at` (timestamptz)

---

### Story 8.3: Reveal Suspension on Report Submission

As a reporting user,
I want the reported user's reveal capability to be immediately suspended when I submit a report,
So that a potentially harmful user cannot proceed to a reveal while their report is under review.

**Acceptance Criteria:**

**Given** a report is submitted via `POST /api/moderation/reports`
**When** the report is persisted
**Then** `RevealState.reveal_suspended` is set to `true` for the conversation; `RevealService.GetRevealPhotoUrls` for this conversation returns 403 until the suspension is lifted

**Given** `RevealState` is updated to add `reveal_suspended`
**When** the migration is reviewed
**Then** `reveal_states.reveal_suspended` (bool, default false) column exists; existing rows default to `false`

**Given** a moderator reviews the report and finds no violation
**When** the moderation action is applied
**Then** `reveal_suspended` is set back to `false` for the conversation; both users can continue to the reveal if they were both ready

---

### Story 8.4: Moderator Reports Queue and Admin Review Interface

As a moderator,
I want to see a queue of pending reports with automated screening signals,
So that I can efficiently review and action cases with full context.

**Acceptance Criteria:**

**Given** a moderator is authenticated on the Razor Pages admin (`/admin/reports`)
**When** the reports queue page loads
**Then** all `Report` records with `status: "pending"` are listed with: reporter info (anonymised), reported user info, report reason, conversation message excerpt, auto-scan signals (text flagging matches, if any), `created_at`

**Given** the moderator opens a specific report
**When** the report detail page renders
**Then** the full conversation transcript is displayed; automated screening signals are shown (text flag patterns matched); the reported user's account age and prior moderation history is visible

**Given** the admin reports page exists
**When** `/admin/reports` is accessed without a valid admin cookie session
**Then** a redirect to the admin login page occurs — Razor Pages cookie auth is enforced; the Nginx IP allowlist also blocks access from non-allowlisted IPs

---

### Story 8.5: Moderator Actions — Warn, Reveal Suspend, and Ban

As a moderator,
I want to apply proportional moderation actions to reported users,
So that violations are addressed consistently and with full audit trail.

**Acceptance Criteria:**

**Given** a moderator reviews a report and clicks "Warn"
**When** `POST /admin/moderation/actions` is called
**Then** a `ModerationAction` record is created with `action_type: "warning"`, `moderator_id`, `report_id`, `notes`, `actioned_at`; the reported user receives an in-app notification: "Your account has received a warning for [reason]"; `Report.status` is updated to `"actioned"`

**Given** a moderator applies a reveal suspension
**When** the action is saved
**Then** `ModerationAction` is created with `action_type: "reveal_suspended"`; the user's reveal capability is suspended across ALL their conversations (not just the reported one) for the suspension period

**Given** a moderator applies a permanent ban
**When** the action is saved
**Then** `ModerationAction` is created with `action_type: "banned"`; `ApplicationUser.LockoutEnd` is set to `DateTimeOffset.MaxValue` (permanent lockout via ASP.NET Core Identity); all active conversations for the banned user are archived; the banned user's JWT becomes invalid on next request

**Given** `ModerationAction` model is created
**When** the migration is reviewed
**Then** table `moderation_actions` contains: `id`, `report_id` (FK), `moderator_id` (FK → `users`), `action_type` (text), `notes` (text, nullable), `actioned_at` (timestamptz); all rows are APPEND-ONLY — no updates or deletes permitted on this table

---

### Story 8.6: Follow-Up Notification to Reporting User

As a user who submitted a report,
I want to receive a follow-up notification after the moderator has reviewed my report,
So that I know my report was taken seriously and actioned.

**Acceptance Criteria:**

**Given** a moderator actions a report (any action type)
**When** the moderation action is saved
**Then** `SendModeratorAlertJob` (renamed for reporter notification context: `SendReportFollowUpJob`) is queued via Coravel to notify the reporter

**Given** `SendReportFollowUpJob` executes
**When** the notification is sent
**Then** the reporting user receives an in-app push notification: "We've reviewed your report and taken action. Thank you for helping keep Blinder safe."; no detail about WHAT action was taken is disclosed to the reporter

**Given** the report resolution notification is sent
**When** the `Report` record is reviewed
**Then** `Report.status` is `"actioned"` and `actioned_at` timestamptz is set

---

## Epic 9: Analytics, Compliance & GDPR

An operator can view a near-real-time gender ratio dashboard. Users can request data export and deletion. All moderation actions are in tamper-evident audit logs. Reveal initiation, confirmation, and abandonment are tracked as distinct DB-stored events.

### Story 9.1: Gender Ratio Dashboard

As a platform operator,
I want a near-real-time gender ratio dashboard,
So that I can immediately intervene if the gender balance falls outside the 40–60% safe zone before it degrades user experience.

**Acceptance Criteria:**

**Given** an authenticated admin accesses `/admin/dashboard`
**When** the gender ratio panel renders
**Then** it shows: total registered users, female count, male count, non-binary count, female % and male % with a visual indicator; data is at most 5 minutes stale

**Given** the female percentage drops below 40% or male percentage drops below 40%
**When** the dashboard renders
**Then** a prominent warning indicator is displayed: "⚠️ Gender ratio outside safe zone — immediate action required"

**Given** invite link usage is queried
**When** the dashboard shows invite funnel data
**Then** invite send count, invite acceptance rate, and invite conversion (accepted → registered) are shown — enabling supply-side control via invite management

---

### Story 9.2: Tamper-Evident Moderation Audit Log

As a legal compliance officer,
I want all moderation actions to be recorded in a tamper-evident log,
So that Blinder can demonstrate legal defensibility for every moderation decision for a minimum of 2 years.

**Acceptance Criteria:**

**Given** any moderation action is applied (warn, suspend, ban)
**When** `ModerationAction` is written
**Then** the record is append-only: no UPDATE or DELETE SQL operations are permitted on `moderation_actions`; EF Core is configured with `ToTable` + no-update/delete conventions for this entity

**Given** an account deletion (`DELETE /api/account`) runs
**When** the purge cascade executes
**Then** `ModerationAction` records where the deleted user is the subject are NOT deleted — they are retained for 2 years from `actioned_at` (NFR12); `reported_user_id` FK is nullified / anonymised but the action record persists

**Given** the admin audit log page renders at `/admin/moderation/audit-log`
**When** an admin views the log
**Then** all moderation actions are listed with: `actioned_at` (timestamptz), `action_type`, moderator identifier (hashed), `report_id`, `notes` — the log is read-only in the UI; no edit or delete controls exist

**Given** the 2-year retention window passes for an action record
**When** a scheduled cleanup job runs
**Then** `ModerationAction` records older than 2 years (based on `actioned_at`) are eligible for purge — a `ModerationLogRetentionJob` (Coravel, daily schedule) deletes only records beyond the retention window

---

### Story 9.3: GDPR Personal Data Export

As a user exercising my GDPR right of access,
I want to request and receive a complete export of all my personal data,
So that I can see exactly what Blinder holds about me.

**Acceptance Criteria:**

**Given** an authenticated user calls `POST /api/account/data-export`
**When** the request is received
**Then** an export job is queued; HTTP 202 Accepted is returned immediately with a message: "Your data export is being prepared. You'll receive a notification when it's ready."

**Given** the export job completes
**When** the export package is prepared
**Then** a ZIP file is created containing: `profile.json` (quiz answers, preferences, registration date), `messages.json` (all sent messages with timestamps), `matches.json` (match history), `subscription.json` (subscription history); NO photos are included in the export — a signed URL to download the photo is included instead

**Given** the export ZIP is generated
**When** the user is notified
**Then** a push notification arrives: "Your data export is ready. Download it in your profile settings." The download link is a signed URL valid for 24 hours.

**Given** a user is not authenticated
**When** `POST /api/account/data-export` is called
**Then** HTTP 401 Unauthorized is returned — data exports require authentication

---

### Story 9.4: GDPR Right to Erasure

As a user exercising my GDPR right to erasure,
I want requesting account deletion to permanently purge all my personal data within the required retention window,
So that Blinder fully complies with GDPR Article 17.

**Acceptance Criteria:**

**Given** an authenticated user calls `DELETE /api/account`
**When** the deletion orchestrator runs (Story 2.6)
**Then** all personal data (profile, quiz answers, messages, conversations, photos, device tokens, subscription records) is permanently deleted within the deletion request; `DeletedAt` is recorded for the erasure event in an anonymised compliance log

**Given** the deletion is complete
**When** a compliance audit is performed
**Then** a `DataErasureLog` record exists with: `erased_at` (timestamptz), `data_categories_erased` (JSON array), `retention_window_complied` (bool) — this record contains NO PII; it records the fact of erasure only

**Given** `DataErasureLog` model is created
**When** the migration is reviewed
**Then** table `data_erasure_log` contains: `id`, `erased_at` (timestamptz), `categories_erased` (jsonb), `moderation_records_retained` (bool), `created_at` (timestamptz)

**Given** a user deletes their account and then attempts to re-register with the same email
**When** the registration attempt is made
**Then** the registration is permitted — the deleted account's email is no longer in the system; no ghost record blocks re-registration

