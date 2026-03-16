---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
workflowType: 'architecture'
lastStep: 8
status: 'complete'
completedAt: '2026-03-12'
inputDocuments:
  - '_bmad-output/planning-artifacts/prd.md'
  - '_bmad-output/planning-artifacts/ux-design-specification.md'
  - '_bmad-output/planning-artifacts/product-brief-Blinder-2026-03-11.md'
project_name: 'Blinder'
user_name: 'Piotr.palej'
date: '2026-03-11'
---

# Architecture Decision Document

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

---

## Project Context Analysis

### Requirements Overview

**Functional Requirements (45 total):**

Organized across 8 domains:

- **Account Management (FR1–7):** Registration via email/password and social login (Apple, Google, Facebook); account deletion with full data purge; 18+ age declaration; invite-only female registration via validated invite links
- **Onboarding & Profile (FR8–11):** Values and personality quiz; private photo storage (never visible until mutual reveal); immediate first-match drop on onboarding completion; automatic 7-day premium trial
- **Matching (FR12–16):** Rules-based values-weighted compatibility algorithm; demographic fallback when pool insufficient; no browsing or searching — curated matches only; geographic radius control; invite link generation and tracking
- **Chat & Conversation (FR17–21):** Real-time text messaging (SignalR/WebSockets); max 3 active conversations (free tier); message count tracking per conversation; push notifications for all key events
- **Reveal System (FR22–27):** Async flag-per-user opt-in reveal; minimum message threshold gate; photo unlocks when both flags are set (user sees photo on next app open); no unilateral reveal path; premium lowers personal threshold only (other party consent always required)
- **Subscription & Premium (FR28–31):** In-app purchase via Apple/Google billing; increased limits for premium; trial expiry and limit-reached prompts
- **Safety & Content Moderation (FR32–40):** Image scanning at upload (SafeSearch/Azure Content Moderator); CSAM hash-matching (PhotoDNA/NCMEC) before storage; automated text flagging; one-tap reporting; reveal suspension on report; moderator admin interface with ban/warn/restore
- **Analytics & Compliance (FR41–45):** DB-backed analytics (reveal event tracking stored in PostgreSQL); near-real-time gender ratio dashboard; GDPR data export and erasure; tamper-evident moderation audit log

**Non-Functional Requirements (30 total):**

- **Performance:** <500ms chat delivery; <3s app launch; <10s image scan pipeline; <300ms API P95 (Poland)
- **Security:** AES-256 at rest; TLS 1.2+; photos via signed time-limited URLs only — accessible only after both reveal flags are set; GDPR special category data EU-only; no card data stored (PCI delegated); 2-year tamper-evident audit log
- **Scalability:** 10K concurrent users (launch); 100K registered users without architectural change; horizontal chat scaling; 1K concurrent image upload throughput
- **Accessibility:** WCAG 2.1 AA for core flows; VoiceOver/TalkBack; Dynamic Type/font scaling; AA contrast ratios
- **Reliability:** 99.5% monthly uptime; message queue resilience with no-loss guarantee; content scan hard-fail on API outage (images not accepted if scanning cannot be confirmed); CSAM pipeline failure = immediate ops alert
- **Integration:** Apple IAP + Google Play Billing (60s state sync); FCM/APNs >95% delivery in 60s; reveal events are DB-transactional (no event loss); PhotoDNA tested with known hashes pre-launch

**Scale & Complexity:**

- Primary domain: Full-stack mobile-first (React Native + backend API + real-time infrastructure + cloud storage + third-party compliance integrations)
- Complexity level: **Medium** (elevated by real-time chat infrastructure, two-sided marketplace mechanics, content scanning pipeline, and CSAM/GDPR compliance requirements)
- Estimated architectural components: 8–10 distinct backend service domains; 2 mobile clients (iOS/Android as single RN codebase); admin interface

### Technical Constraints & Dependencies

- **React Native** — single codebase for iOS 16+ and Android 10+ (API 29+); native modules only where required (push, camera)
- **Microsoft SignalR** — specified for real-time chat; handles transport fallback automatically (WebSockets → Server-Sent Events → Long Polling) with built-in reconnect — no custom lifecycle management needed for MVP
- **EU data residency** — all personal data (chat, photos, profile) must reside in EU-region infrastructure; no transit through non-EU cloud regions; each third-party integration (PhotoDNA, FCM/APNs, Apple/Google Sign In) must be individually audited for data flow compliance; analytics data stays in the main PostgreSQL DB on the EU-region VPS
- **PhotoDNA/NCMEC** — legal agreements must be initiated at project start (external lead time: weeks to months); integration must be tested with known hashes before launch; this is a launch blocker. **Action item: initiate NCMEC legal process as a parallel workstream immediately — this is not a code concern and cannot be unblocked by development.**
- **GDPR compliance** — special category data obligations (consent flows, retention limits, right to erasure, DPO) are deferred from MVP feature scope but GDPR posture documentation must be in place before first real user onboards per PRD launch blocker definition
- **In-app billing** — Apple StoreKit and Google Play Billing mandatory for digital subscriptions on their respective platforms; Apple anti-steering rules restrict web subscription promotion from within the iOS app
- **Firebase Cloud Messaging (FCM) + APNs** — push infrastructure abstraction required (OneSignal or direct integration)
- **Analytics** — None — custom DB-backed analytics; reveal events and gender ratio data stored in PostgreSQL
- **Content scanning** — Google Vision SafeSearch or Azure Content Moderator; pipeline must hard-fail (not pass-through) if scan API is unavailable
- **Private photo storage** — no public object storage URLs permitted; photo delivery via authenticated signed URLs only, and only after both `reveal_ready` flags on a conversation are confirmed

### Reveal System: Architectural Decision

The reveal mechanic is implemented as **two independent async state flags** per conversation, not a synchronous simultaneous exchange:

- `user_a_reveal_ready: bool`
- `user_b_reveal_ready: bool`

A user sees their match's photo on the **next app open** after both flags are true AND `message_count >= threshold`. There is no requirement for both users to be online simultaneously. The "mutual reveal" experience is a UX presentation decision, not a technical coordination constraint. Photo access authorization is a simple server-side check: signed URL generation is gated on both flags being set.

### Matching: Configurable Threshold Decision

The compatibility threshold for the rules-based matching algorithm (the point at which the system falls back to demographic matching) **must be admin-configurable from day one** — not hardcoded. This threshold will require tuning as the user pool grows, and a release cycle to change it is unacceptable in an early-stage product.

### Cross-Cutting Concerns Identified

1. **Content scanning pipeline** — Image scan-before-store is a cross-cutting gate affecting photo upload, reveal delivery, and moderation flows. Decision required: synchronous (user waits up to 10s for upload confirmation) vs. asynchronous with quarantine state. This architectural decision must be made explicitly before implementation.
2. **Reveal authorization** — `reveal_ready` flag state is checked at photo access time (signed URL generation). Spans storage, API, and conversation state services.
3. **Real-time push routing** — New match, new message, reveal readiness events, trial expiry, moderation follow-up: affects matching, chat, reveal, subscription, and moderation services. SignalR handles active-session delivery; FCM/APNs covers background.
4. **Account deletion cascade** — FR4 requires full purge of photos, chat history, profile, and analytics user references on account deletion. Requires a deletion orchestrator with service-level deletion contracts. **Exception:** moderation audit logs are retained for 2 years minimum (NFR12) even after account deletion — the deletion orchestrator must know which data categories are exempt.
5. **Gender ratio monitoring** — Near-real-time dashboard and invite-only female onboarding gating spans user registration, invite link system, and analytics. Invite lineage (which female member referred whom) must be tracked for safety and ratio management.
6. **i18n / Localisation** — Polish (pl-PL) primary, English fallback; must be embedded in component architecture from day one to support geographic expansion without refactoring.
7. **In-app billing state management** — Subscription state changes must propagate within 60 seconds; premium feature gates depend on accurate billing state across both platforms.
8. **Admin-configurable matching threshold** — Compatibility threshold and fallback logic exposed via admin config, not hardcoded.

---

## Starter Template Evaluation

### Primary Technology Domain

Full-stack mobile-first — Expo (React Native) mobile client + ASP.NET Core .NET 10 backend (REST API + SignalR hub + Razor Pages admin) + PostgreSQL database, hosted on a Linux VPS.

### Repository Structure

```
blinder/
├── backend/          # ASP.NET Core .NET 10 (API + SignalR + Razor Pages admin)
└── mobile/           # Expo SDK 55 (iOS + Android)
```

Single monorepo, two top-level projects.

### Selected Starters

#### Backend — ASP.NET Core .NET 10

```bash
dotnet new sln -n Blinder
dotnet new webapi -n Blinder.Api --use-controllers --framework net10.0
dotnet sln add Blinder.Api/Blinder.Api.csproj
```

Key packages to add immediately:

```bash
dotnet add package Microsoft.AspNetCore.SignalR
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.AspNetCore.RazorPages
```

#### Mobile — Expo SDK 55

```bash
npx create-expo-app@latest --template default@sdk-55
```

Key packages to add:

```bash
npx expo install expo-router expo-notifications expo-image-picker expo-secure-store @microsoft/signalr nativewind
```

### Architectural Decisions Established by Starters

**Language & Runtime:**
- Backend: C# 14 on .NET 10 LTS (LTS support through May 2027 — aligns with post-MVP expansion window)
- Mobile: TypeScript with `strict: true` enabled in `tsconfig.json` from day one — set before writing the first component; retrofitting strict mode is costly

**Project Structure (Backend):**
- `Controllers/` — REST API endpoints
- `Hubs/` — SignalR real-time chat hub
- `Pages/` — Razor Pages admin interface (`/admin/**`)
- `Models/`, `DTOs/`, `Services/` — standard layered separation
- `appsettings.json` + `appsettings.Production.json` for environment config
- `Migrations/` — EF Core migrations committed to source control

**Project Structure (Mobile):**
- File-based routing via Expo Router (`app/` directory)
- `app/(auth)/` — onboarding and login screens
- `app/(tabs)/` — main app shell (chat list, profile, settings)
- `components/` — shared UI components
- `services/` — API client and SignalR connection management

**ORM & Database:**
- Entity Framework Core 10 with Npgsql provider (PostgreSQL)
- **PostGIS extension enabled from day one** — required for geographic radius matching (FR15); provides spatial indexing for "users within X km" queries; far more efficient than runtime haversine calculations on unindexed coordinate columns
- Migration strategy: code-first migrations only; EF CLI tools (`dotnet-ef`) are **only** available in the SDK build stage — they are stripped from the production runtime image by design
- **Dev workflow:** `docker compose run --rm api dotnet ef migrations add <Name>` — works because `docker-compose.override.yml` targets the SDK build stage
- **Production deployment:** generate an idempotent SQL script (`dotnet ef migrations script --idempotent --output migrations/latest.sql`) in dev or CI, commit it to the repository, and apply during deployment via `docker compose exec -T db psql -U $POSTGRES_USER -d $POSTGRES_DB < migrations/latest.sql`
- `--idempotent` flag makes the script check the EF migrations history table — safe to re-run on every deploy without double-applying migrations
- **Never** auto-applied on startup via `Database.MigrateAsync()` in `Program.cs` (this pattern causes production incidents and is explicitly prohibited)

**Styling (Mobile):**
- **NativeWind** (Tailwind CSS utility classes for React Native) — locks in consistent styling conventions from the first screen; prevents ad-hoc per-developer styling patterns; compatible with Phase 2 web app if Tailwind is used there

**Real-time:**
- SignalR hub in backend; `@microsoft/signalr` npm package in mobile client
- Transport negotiation handled automatically by SignalR: WebSockets → Server-Sent Events → Long Polling
- No custom reconnection logic needed — SignalR client handles this natively

**Authentication Token Storage (Mobile):**
- **`expo-secure-store` for all JWT token storage** — maps to iOS Keychain and Android Keystore
- `AsyncStorage` is explicitly prohibited for token storage — it is unencrypted and fails basic security requirements

**Admin Interface:**
- Razor Pages with cookie authentication (separate auth scheme from mobile JWT tokens)
- Server-rendered — no additional frontend build pipeline
- `/admin` path additionally protected by Nginx IP allowlisting in production — application-level auth alone is insufficient for a route with access to user PII and moderation actions

**CI/CD (Mobile):**
- **EAS Build** for App Store and Play Store builds and submissions — free tier is sufficient for MVP velocity; eliminates manual native build pipeline setup
- `eas.json` committed to the repository from the first commit
- Note: EAS Build is for *mobile store delivery only* — backend deployment is a separate concern (see Hosting below)

**Hosting (Backend) — Docker-first from day one:**
- All backend services run in Docker containers via Docker Compose — API, PostgreSQL + PostGIS, and Nginx are each a separate named service in `docker-compose.yml`; nothing runs directly on the host OS at runtime
- **VPS required** (e.g., Hetzner, DigitalOcean) — SignalR relies on persistent HTTP connections (WebSocket upgrades); true shared hosting providers commonly block or aggressively timeout long-lived connections; verify WebSocket support before committing to any host
- ASP.NET Core API container built from a multi-stage `Dockerfile` (SDK image for build → ASP.NET runtime image for production); SDK layer excluded from the final image to minimise size
- Nginx container: TLS termination, reverse proxy to the API container, and `/admin` IP allowlisting; Nginx config mounted as a bind-mounted volume from `nginx/nginx.conf` in the repository
- **Nginx WebSocket headers are mandatory for SignalR** — without them, SignalR silently falls back to long polling, which breaks the `<500ms chat delivery` NFR. The following directives are required in `nginx/nginx.conf` for the `/hubs/` location block:
  ```nginx
  proxy_http_version 1.1;
  proxy_set_header Upgrade $http_upgrade;
  proxy_set_header Connection "upgrade";
  proxy_read_timeout 3600s;
  ```
- PostgreSQL + PostGIS: official `postgis/postgis` Docker image; data persisted via a named Docker volume (`db-data`) — never an anonymous volume, which would be silently destroyed on `docker compose down`
- **Environment configuration:** all secrets injected via a `.env` file on the VPS (never committed to source control); `.env.example` committed with placeholder values documenting every required variable — kept in sync with `.env` requirements in the same commit as any new variable
- **EF Core migrations:** `dotnet-ef` CLI tools exist only in the SDK build stage; the production runtime image does not include them. Migration approach: generate `migrations/latest.sql` via `dotnet ef migrations script --idempotent` in dev or CI, commit to the repository, and apply via `docker compose exec -T db psql` during deployment — never auto-applied inside a running container on startup
- `restart: unless-stopped` set on all production services — containers recover automatically on VPS reboot without `systemd` unit files
- Backend deployment at MVP: `git pull && docker compose build && docker compose up -d db && docker compose exec -T db psql -U $POSTGRES_USER -d $POSTGRES_DB < migrations/latest.sql && docker compose up -d`

**Note:** Project initialization using the commands above constitutes the first two implementation stories — backend scaffolding and mobile scaffolding respectively.

---

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**
- Authentication: ASP.NET Core Identity + JWT Bearer
- File Storage: S3-compatible object storage (EU-region bucket)
- Background Processing: Coravel (in-memory)
- API pattern: REST with controllers
- Object mapping: Mapperly (source generator)

**Deferred Decisions (Post-MVP):**
- Horizontal scaling of SignalR (requires Redis backplane — not needed at Poland-launch scale)
- Managed database service (PostgreSQL on VPS sufficient until load warrants separation)
- ML-based matching (no training data at launch)

---

### Authentication & Security

**Decision: ASP.NET Core Identity + JWT Bearer tokens**

ASP.NET Core Identity provides scaffolded registration, login, password reset, and account management flows. JwtBearer middleware validates tokens on every API request. All auth runs on the VPS — no external identity provider dependency, no non-EU data flows.

- Use a custom `ApplicationUser : IdentityUser` class from day one — adding custom fields (gender, quiz answers, invite link reference) directly to `IdentityUser` causes painful schema migrations; the custom subclass costs nothing up front
- Social login (Apple, Google, Facebook) uses `ExternalLoginAsync` flows in Identity — **not** covered by default scaffolded templates; this wiring must be explicitly planned in the social login implementation stories
- Social provider ID tokens validated server-side; backend issues its own JWT — no OAuth redirect dance
- `expo-secure-store` on mobile maps to iOS Keychain / Android Keystore for token storage — `AsyncStorage` is explicitly prohibited for tokens
- JWT tokens expire after 30 days of inactivity (NFR10)
- Admin Razor Pages use cookie authentication (separate scheme from API JWT)
- `/admin` path additionally protected by Nginx IP allowlist — application-level auth alone is insufficient for a route with access to user PII and moderation actions

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

---

### API & Communication Patterns

**Decision: REST with ASP.NET Core Web API controllers**

Standard HTTP controllers in `Controllers/` directory. Controller-per-domain: `AuthController`, `MatchController`, `ConversationController`, `RevealController`, `ModerationController`, `SubscriptionController`.

- OpenAPI/Swagger documentation via `Microsoft.AspNetCore.OpenApi` (built into .NET 10)
- SignalR hub at `/hubs/chat` for real-time message delivery — separate from REST API surface
- API versioning not required at MVP; add `Asp.Versioning` when Phase 2 web app requires it
- Error responses follow RFC 7807 Problem Details via `AddProblemDetails()` + `UseExceptionHandler()` — consistent error shape across every controller from day one

---

### Data Architecture

**Decision: EF Core 10 + Npgsql + PostgreSQL + PostGIS**

- Code-first migrations committed to source control
- Migrations applied via SQL script during deployment: `dotnet ef migrations script --idempotent --output migrations/latest.sql` (generated in dev/CI using SDK stage), then `docker compose exec -T db psql -U $POSTGRES_USER -d $POSTGRES_DB < migrations/latest.sql` — never `Database.MigrateAsync()` on startup (production incident risk; explicitly prohibited)
- PostGIS enabled from day one for geographic radius matching (FR15) — spatial index on user coordinates; never haversine on raw columns
- Compatibility threshold for matching algorithm stored as admin-configurable database value — not hardcoded
- No caching layer at MVP — PostgreSQL at VPS scale is sufficient for Poland-first volume; add Redis if query latency becomes an issue post-launch

---

### Object Mapping

**Decision: Mapperly (source generator)**

Mapperly generates mapping code at compile time via C# source generators — zero runtime reflection, zero allocations, full IDE support and compile-time error detection. Used for all Entity ↔ DTO mappings throughout the backend.

- Mapping classes live in a `Mappings/` folder as `partial` mapper classes decorated with `[Mapper]`
- No manual `new DTO { ... }` mapping in controllers or services — all entity-to-DTO and DTO-to-entity conversions go through a Mapperly mapper
- Compile-time errors on unmapped properties — prevents silent data leaks in API responses

```bash
dotnet add package Riok.Mapperly
```

---

### File Storage

**Decision: S3-compatible object storage (EU-region)**

User photos stored in an S3-compatible object store with private ACL. Recommended provider: **Hetzner Object Storage** (native EU, S3-compatible, cost-effective).

- `AWSSDK.S3` NuGet package used with custom endpoint configuration
- **Hetzner Object Storage requires `ForcePathStyle = true`** in the S3 client config — non-standard endpoint; spike this before writing the upload story
- Photos stored under a path that does not expose user identity in the key (e.g., `photos/{guid}`)
- No public bucket ACL — all access via pre-signed URLs generated server-side
- Signed URL generated only after both `reveal_ready` flags are confirmed for the conversation
- URL expiry: short-lived (e.g., 1 hour) — client requests a fresh signed URL on each reveal screen open
- Image scanning pipeline executes synchronously before the upload is confirmed to the client

```bash
dotnet add package AWSSDK.S3
```

---

### Background Processing

**Decision: Coravel (in-memory task scheduling and queuing)**

Coravel provides fluent task scheduling, fire-and-forget queuing, in-process event broadcasting, and Razor-templated mailing — zero external infrastructure dependencies.

**Acknowledged trade-off:** Coravel is in-memory. Jobs queued at the moment of a process restart are lost. Acceptable at MVP scale on a stable VPS. The image scanning pipeline is unaffected (synchronous, blocks the upload response). Fire-and-forget jobs (push dispatch, moderator email, match triggers) may occasionally drop on restart — tolerable at MVP.

**Critical exception — match generation job:** This job must be **idempotent and re-triggered on startup**. On boot, the application checks for un-matched users and queues matches immediately — not waiting for the next cron tick. Rationale: un-matched users post-restart stare at an empty "finding your match" screen indefinitely, which is the worst possible UX failure for the dropout cohort.

**Coravel usage by feature:**
- **Push notification dispatch** — queued `IInvocable` after message send / reveal state change
- **Moderator alert email** — queued `IInvocable` on report submission
- **Match generation** — scheduled `IInvocable` (periodic) + idempotent startup check
- **Trial expiry notifications** — scheduled `IInvocable` checking premium trial expiry dates daily

```bash
dotnet add package Coravel
```

---

### Email / Notifications

**Decision: SMTP via hosting provider + MailKit**

VPS hosting provider's included SMTP service is sufficient at MVP volume. MailKit (`MimeKit` + `MailKit`) is the standard .NET SMTP client. Email templates rendered via Coravel's built-in Razor mailing support.

- SMTP credentials configured via `IOptions<SmtpSettings>` bound to `appsettings.json` — credentials sourced from environment variables on the VPS, **never committed to source control**
- If hosting SMTP proves unreliable, swap to Resend.com free tier (100 emails/day) without architectural change

```bash
dotnet add package MailKit
```

---

### Push Notifications

**Decision: Direct FCM + APNs from the .NET backend (no third-party routing service)**

All push notification dispatch runs inside the .NET process on the VPS. Mobile client registers its native device token with the backend on login; backend dispatches directly to Google FCM (Android) and Apple APNs (iOS) via official SDKs.

- `FirebaseAdmin` NuGet — official Google Firebase Admin SDK for .NET; dispatches via FCM HTTP v1 API to Android devices
- `dotAPNS` NuGet — APNs HTTP/2 client for iOS devices
- Mobile client calls `getDevicePushTokenAsync()` (raw native token, **not** `getExpoPushTokenAsync()`) and POSTs it to `POST /api/account/device-token` on login and re-authentication
- `DeviceToken` table stores `user_id`, `token`, `platform` (Android/iOS), `created_at` — indexed on `user_id`
- **`SendPushNotificationJob.cs` must delete stale tokens** on FCM `registration-token-not-registered` or APNs `BadDeviceToken` error responses — otherwise the table grows unboundedly
- APNs credentials: **Auth Key (`.p8` file, 10-year validity)** — never certificate (requires annual rotation); stored as `APNS_KEY` env var, never committed to source control
- Firebase credentials: service account JSON stored as `FIREBASE_CREDENTIALS_JSON` env var (content, not file path); `FirebaseAdmin` initialized from a stream
- APNs environment: `PushNotifications:UseApnsSandbox` flag in `appsettings.json` — `true` in Development/Staging, `false` in Production
- `IPushNotificationService` interface with platform-branching dispatcher — mock-friendly for unit and integration tests; `FakePushNotificationService` in test environment

```bash
dotnet add package FirebaseAdmin
dotnet add package dotAPNS
```

---

### In-App Purchase Webhook Security

**Decision: Server-side JWT signature verification required on all IAP webhooks**

Both Apple and Google deliver subscription state change notifications as signed JWTs to `SubscriptionController`. **Without verification, any actor can POST a fake webhook to unlock premium features (OWASP A01 — Broken Access Control).** Verification is mandatory and must not be skippable in production.

**Apple AppStore Server Notifications:**
- Payload is a signed JWT (RS256); verify against Apple's JWKS endpoint: `https://appleid.apple.com/auth/keys`
- Verify `iss` claim = `"https://appleid.apple.com"` and `bundle_id` claim matches app
- Use `Microsoft.IdentityModel.Tokens` (already present via `JwtBearer` package) — no additional dependency
- **Cache JWKS keys with 24h TTL** — never call the JWKS endpoint per-request (rate-limiting and latency risk)

**Google Play Real-Time Developer Notifications:**
- Configure as HTTPS push (direct POST to endpoint) rather than Pub/Sub for MVP simplicity
- Notification carries a JWT in the `Authorization: Bearer` header; verify against Google's JWKS endpoint: `https://www.googleapis.com/oauth2/v3/certs`
- Verify `iss` and `aud` claims match expected Google service account and package name

**Test environment bypass:**
- `appsettings.Testing.json`: `"Subscriptions:SkipWebhookVerification": true` — valid only when `WebApplicationFactory` test host is active
- Production environment must have `SkipWebhookVerification` absent or `false` — enforced via startup assertion

---

### Validation

**Decision: FluentValidation**

All API input validation goes through FluentValidation — not inline `if` statements in controllers or services. Validator classes in `Validators/` directory, one per request type. Integrates with ASP.NET Core model binding and produces Problem Details-compatible error responses automatically.

```bash
dotnet add package FluentValidation.AspNetCore
```

---

### Logging

**Decision: Serilog with rolling file sink**

Serilog replaces the default .NET logging provider. Structured JSON output to rolling daily log files on the VPS — queryable with `grep`/`jq` during production debugging. `ILogger<T>` injection used throughout — no `Console.WriteLine` in application code.

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

---

### Decision Impact: Cross-Component Dependencies

| Decision | Affects |
|---|---|
| ASP.NET Core Identity + custom `ApplicationUser` | Auth flows, social login, admin cookie auth, all API authorization, EF Core schema |
| Mapperly | All controller responses, service layer boundaries, EF entity ↔ DTO conversion |
| S3 + Hetzner Object Storage | Photo upload, content scanning pipeline, reveal signed URL generation, account deletion cascade |
| PostGIS | Matching service geographic queries, user preference radius storage |
| Coravel | Push dispatch, email alerts, match generation scheduling, trial expiry jobs |
| FluentValidation | All API endpoints accepting user input — registration, quiz, preferences, messages |
| Serilog | All services — structured logging convention enforced via `ILogger<T>` injection |
| Match generation startup check | Application startup sequence, `IHostedService` registration order |
| FirebaseAdmin + dotAPNS | `SendPushNotificationJob`, `DeviceToken` table, `POST /api/account/device-token`, stale token cleanup |
| IAP webhook JWT verification | `SubscriptionController`, JWKS key caching, test environment bypass flag |

---

## Implementation Patterns & Consistency Rules

### Critical Conflict Points Identified

8 areas where inconsistent implementation would cause integration failures or maintenance debt across the codebase.

---

### Naming Patterns

**Database Naming Conventions (PostgreSQL via EF Core):**

EF Core configured with `UseSnakeCaseNamingConvention()` — all table and column names are `snake_case` at the database level regardless of C# property casing.

| Element | Convention | Example |
|---|---|---|
| Tables | `snake_case` plural | `users`, `conversations`, `messages`, `reveal_states` |
| Columns | `snake_case` | `created_at`, `user_id`, `is_reveal_ready` |
| Foreign keys | `{singular_table}_id` | `user_id`, `conversation_id` |
| Indexes | `ix_{table}_{columns}` | `ix_users_email`, `ix_messages_conversation_id` |
| Primary keys | `id` (always) | `id` |

**API Endpoint Naming Conventions (REST):**

| Element | Convention | Example |
|---|---|---|
| Resource paths | Plural nouns, `kebab-case` | `/api/conversations`, `/api/reveal-states` |
| Route parameters | `{camelCase}` | `/api/conversations/{conversationId}` |
| Query parameters | `camelCase` | `?pageSize=10&matchedUserId=abc` |
| Controller files | `{Resource}Controller.cs` | `ConversationController.cs` |

**C# Code Naming Conventions:**

| Element | Convention | Example |
|---|---|---|
| Classes, interfaces | `PascalCase` | `ConversationService`, `IRevealRepository` |
| Methods | `PascalCase` | `GetActiveConversationsAsync` |
| Properties | `PascalCase` | `UserId`, `IsRevealReady` |
| Private fields | `_camelCase` | `_userRepository`, `_logger` |
| Local variables / params | `camelCase` | `conversationId`, `matchScore` |
| Constants | `PascalCase` (static) or `UPPER_SNAKE_CASE` (magic values) | `MaxActiveConversations`, `DEFAULT_RADIUS_KM` |
| No abbreviations | Full words always | `conversation` not `conv`; `userId` not `uid` |

**TypeScript / React Native Naming Conventions:**

| Element | Convention | Example |
|---|---|---|
| Component files | `PascalCase.tsx` | `ConversationCard.tsx`, `RevealButton.tsx` |
| Hook files | `camelCase.ts`, `use` prefix | `useConversation.ts`, `useReveal.ts` |
| Service files | `camelCase.ts` | `authService.ts`, `signalrService.ts` |
| Component names | `PascalCase` | `ConversationCard`, `RevealScreen` |
| Hook names | `use` prefix | `useConversation`, `useRevealState` |
| Constants | `UPPER_SNAKE_CASE` | `MAX_ACTIVE_CONVERSATIONS`, `REVEAL_THRESHOLD` |
| Props interfaces | `{ComponentName}Props` | `ConversationCardProps` |
| No abbreviations | Full words always | `conversation` not `conv`; `message` not `msg` |

---

### API Response Formats

**Decision: Direct response (RFC 7807 Problem Details for errors) — Option B**

Successful responses return the resource or collection directly. No response wrapper object.

**Success — single resource:**
```json
{
  "id": "abc123",
  "messageCount": 14,
  "isRevealReady": false
}
```

**Success — collection:**
```json
[
  { "id": "abc123", ... },
  { "id": "def456", ... }
]
```

**Success — 201 Created:** Returns the created resource body + `Location` header.

**Success — 204 No Content:** Empty body (delete, state updates with no return value).

**Error — RFC 7807 Problem Details (all 4xx and 5xx):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "messageThreshold": ["Must be greater than 0"]
  }
}
```

Implemented via `AddProblemDetails()` + `UseExceptionHandler()` in `Program.cs`. FluentValidation validation failures automatically produce this shape via the ASP.NET Core integration.

**HTTP Status Code Usage:**

| Code | When |
|---|---|
| 200 | Successful GET, successful action with response body |
| 201 | Resource created (POST) |
| 204 | Successful action with no response body (DELETE, state update) |
| 400 | Validation failure, malformed request |
| 401 | Unauthenticated |
| 403 | Authenticated but not authorised |
| 404 | Resource not found |
| 409 | Conflict (e.g. duplicate registration, reveal already triggered) |
| 422 | Business rule violation (distinct from validation failure) |
| 500 | Unhandled server error (never expose stack traces in response) |

---

### Date & Time Patterns

- All datetimes stored in PostgreSQL as `timestamptz` (UTC)
- All API responses use ISO 8601 strings: `"2026-03-11T14:00:00Z"`
- C# uses `DateTimeOffset` (not `DateTime`) throughout — `DateTime` is prohibited in new code
- Mobile displays in local time via `Intl.DateTimeFormat` — datetimes are never stored or transmitted as local time
- No Unix timestamps in API contracts — ISO 8601 strings only

---

### Structure Patterns

**Backend Project Structure (`Blinder.Api/`):**

```
Blinder.Api/
├── Controllers/          # One controller per domain resource
├── Hubs/                 # SignalR hubs
├── Pages/                # Razor Pages admin (/admin/**)
├── Models/               # EF Core entity classes
├── DTOs/                 # Request and response data transfer objects
├── Services/             # Business logic (one interface + implementation per service)
├── Repositories/         # Data access (if needed beyond EF Core directly)
├── Mappings/             # Mapperly mapper partial classes
├── Validators/           # FluentValidation validator classes
├── BackgroundJobs/       # Coravel IInvocable background job classes
├── Errors/               # RFC 7807 typed problem codes (AppErrors.cs)
├── Migrations/           # EF Core migrations (committed to source control)
└── Infrastructure/       # Cross-cutting: auth, logging config, S3 client, SMTP
```

**Backend Test Project Structure (`Blinder.Tests/`):**

Mirrors `Blinder.Api/` structure. Test class naming: `{Subject}Tests.cs`.

```
Blinder.Tests/
├── Controllers/
├── Services/
├── Validators/
└── Integration/
```

**Mobile Project Structure (`mobile/`):**

```
mobile/
├── app/                  # Expo Router file-based routing
│   ├── (auth)/           # Unauthenticated screens (login, register, onboarding)
│   └── (tabs)/           # Authenticated app shell
├── components/           # Shared UI components (PascalCase files)
├── hooks/                # Custom hooks (camelCase, use prefix)
├── services/             # API client, SignalR service, storage service
├── constants/            # UPPER_SNAKE_CASE values, theme tokens
├── types/
│   ├── api/              # Response/request types matching backend DTOs
│   └── signalr/          # Hub method names, payload types, connection state enums
└── utils/                # Pure utility functions (no side effects)
```

---

### Error Handling Patterns

**Backend:**
- Global exception handler middleware catches all unhandled exceptions and returns Problem Details 500 — never expose stack traces in responses
- Business rule violations throw typed domain exceptions (e.g., `RevealThresholdNotMetException`) caught by middleware and mapped to 422
- Validation failures handled by FluentValidation middleware — never manually in controllers
- All exceptions logged via Serilog with structured context before response is returned

**Mobile:**
- Service layer functions return typed result objects — never throw to the component level
- Components consume `{ data, error, isLoading }` shaped state — never raw `try/catch` in components
- User-facing error messages are copy-friendly strings defined in `constants/errors.ts` — never raw exception messages shown to users
- No `console.log` in production code — remove before commit

---

### Loading State Patterns (Mobile)

All async operations expose a consistent three-state shape:

```typescript
type AsyncState<T> = {
  data: T | null;
  error: string | null;
  isLoading: boolean;
};
```

- `isLoading: true` while request is in flight
- `error` is a user-displayable string (not a raw exception message)
- `data` and `error` are never both non-null simultaneously
- Loading states are local to the hook or screen — no global loading spinner

---

### SignalR Real-time Patterns

- SignalR connection managed in a singleton `signalrService.ts` — not instantiated per-component
- Hub method names: `PascalCase` on server, matching exactly on client — `ReceiveMessage`, `RevealStateUpdated`
- Mobile components subscribe to hub events via hooks (`useConversation`, `useRevealState`) — never directly in components
- Connection lifecycle (start, stop, reconnect) handled entirely within `signalrService.ts` — components are unaware of transport state

---

### Validation Patterns (Backend)

- Every API endpoint that accepts a request body or complex query params has a corresponding FluentValidation `AbstractValidator<TRequest>` class in `Validators/`
- Validators are registered via `AddValidatorsFromAssemblyContaining<Program>()`
- No inline validation logic in controllers or service methods
- Validation errors automatically produce Problem Details 400 responses via the ASP.NET Core FluentValidation integration

---

### Mapping Patterns (Backend)

- All Entity ↔ DTO conversions go through a Mapperly `[Mapper]` partial class in `Mappings/`
- No manual `new DTO { Property = entity.Property }` construction outside of Mapperly mappers
- One mapper class per domain area: `UserMapper`, `ConversationMapper`, `RevealMapper`
- Mapperly compile-time warnings on unmapped properties are treated as errors — configure `RespectNullableAnnotations = true`

---

### Logging Patterns (Backend)

- `ILogger<T>` injected via constructor — no static `Log.` calls, no `Console.WriteLine`
- Log levels: `Debug` for diagnostic detail (dev only), `Information` for business events (match created, reveal triggered), `Warning` for recoverable issues, `Error` for exceptions
- Structured log properties over string interpolation: `_logger.LogInformation("Reveal triggered {ConversationId}", id)` not `$"Reveal triggered {id}"`
- Never log PII (user names, email addresses, message content) in structured log properties

---

### Enforcement Guidelines

**All developers and AI agents MUST:**

1. Use `snake_case` database names via `UseSnakeCaseNamingConvention()` — never PascalCase columns in migrations
2. Return direct responses (no wrapper) on success; Problem Details on all errors
3. Store all datetimes as `DateTimeOffset` (UTC) in C#; `timestamptz` in PostgreSQL; ISO 8601 in API responses
4. Use Mapperly for all Entity ↔ DTO mapping — no manual property copying
5. Use FluentValidation for all input validation — no inline validation in controllers
6. Use `expo-secure-store` for all token storage in mobile — `AsyncStorage` is prohibited for auth tokens
7. Never log PII in structured log properties
8. Never auto-apply EF Core migrations on startup — generate an idempotent SQL script via `dotnet ef migrations script --idempotent` (SDK stage only) and apply via `docker compose exec -T db psql` during deployment; `dotnet-ef` tools do not exist in the production runtime image
9. Return `AsyncState<T>` shaped state from all async hooks in mobile
10. Never expose raw exception messages or stack traces in API responses or mobile UI
11. Never run the API (or any backend service) directly on the host OS — all execution, in development and production, goes through Docker Compose
12. Keep `.env.example` in sync: when adding a new environment variable, update `.env.example` in the same commit
---

## Project Structure & Boundaries

### Repository Root

```
blinder/
├── backend/
│   ├── Blinder.sln
│   ├── Blinder.Api/
│   ├── Blinder.Tests/
│   └── Dockerfile                     # Multi-stage build: SDK image (build) → ASP.NET runtime image (production)
├── mobile/
├── docker-compose.yml                 # Production service definitions: api, db, nginx
├── docker-compose.override.yml        # Dev overrides: bind mounts, exposed ports, dev appsettings
├── nginx/
│   └── nginx.conf                     # Nginx reverse proxy + TLS + /admin IP allowlist + WebSocket upgrade headers for /hubs/ (SignalR)
├── .env.example                       # All required environment variable names with placeholder values (committed)
└── .dockerignore
```

---

### Backend Structure (`backend/`)

```
Blinder.Api/
├── Controllers/
│   ├── AuthController.cs              # FR1–3: email+social login, token refresh, logout
│   ├── AccountController.cs           # FR4–7: deletion cascade, age gate, profile management; POST /api/account/device-token
│   ├── OnboardingController.cs        # FR8–11: quiz, photo upload, first match drop
│   ├── MatchController.cs             # FR12–16: radius, invite link gen/use, match detail
│   ├── ConversationController.cs      # FR17–21: message list, pagination, active conv limit
│   ├── RevealController.cs            # FR22–27: reveal flag set/get, threshold check
│   ├── SubscriptionController.cs      # FR28–31: IAP webhook, premium state, trial — SPIKE required before first implementation story
│   ├── ModerationController.cs        # FR32–40: reports, bans, audit log (admin-only)
│   └── AnalyticsController.cs         # FR41–45: Gender ratio dashboard, reveal event tracking
│
├── Hubs/
│   └── ChatHub.cs                     # SignalR hub at /hubs/chat (FR17)
│
├── Pages/
│   └── Admin/
│       ├── Dashboard.cshtml           # Gender ratio near-real-time panel (NFR26)
│       ├── Reports/                   # Moderation queues (FR32–40); first story: basic CSV cohort export
│       ├── Users/                     # User list, ban/warn/restore actions (FR36–38)
│       └── Settings/                  # Directory — multiple admin settings will land here
│           └── MatchingThreshold.cshtml  # Admin-configurable compatibility threshold (FR13)
│
├── Models/
│   ├── ApplicationUser.cs             # : IdentityUser — gender, quiz refs, invite FK
│   ├── UserProfile.cs                 # Quiz answers, photo object key, radius preference
│   ├── InviteLink.cs                  # Token, generated_by FK, used_by FK, created_at
│   ├── Match.cs                       # user_a FK, user_b FK, compatibility_score, matched_at, is_active
│   ├── Conversation.cs                # Match FK, message_count, status (active/archived/reported)
│   ├── Message.cs                     # Conversation FK, sender FK, body, sent_at
│   ├── RevealState.cs                 # Conversation FK, user_a_reveal_ready, user_b_reveal_ready
│   ├── Subscription.cs                # User FK, plan, status, trial_expires_at, platform (Apple/Google)
│   ├── Report.cs                      # Reporter FK, reported FK, conversation FK, reason, created_at
│   ├── ModerationAction.cs            # Report FK, moderator FK, action_type, notes, actioned_at
│   ├── AppSettings.cs                 # Key/value admin-configurable values (matching threshold, etc.)
│   └── DeviceToken.cs                 # user_id FK, token, platform (Android/iOS), created_at — indexed on user_id
│
├── DTOs/
│   ├── Auth/                          # LoginRequest, RegisterRequest, TokenResponse, SocialLoginRequest
│   ├── Match/                         # MatchResponse, MatchDetailResponse
│   ├── Conversation/                  # MessageRequest, MessageResponse, ConversationSummaryResponse
│   ├── Reveal/                        # RevealStateResponse, RevealReadyRequest
│   └── Subscription/                  # SubscriptionStatusResponse, IAPWebhookPayload
│
├── Services/                          # Domain orchestration — *Service naming only
│   ├── AuthService.cs
│   ├── MatchService.cs                # Compatibility algorithm, admin threshold lookup, fallback logic
│   ├── ConversationService.cs         # Active limit enforcement, message count tracking
│   ├── RevealService.cs               # Flag logic, threshold gate, signed URL orchestration
│   ├── SubscriptionService.cs         # Premium state, trial management, feature gating
│   └── ModerationService.cs           # Report handling, ban logic, audit log writes (2yr retention)
│
├── Mappings/                          # Mapperly [Mapper] partial classes
│   ├── UserMapper.cs
│   ├── ConversationMapper.cs
│   └── RevealMapper.cs
│
├── Validators/                        # FluentValidation AbstractValidator<TRequest>
│   ├── RegisterRequestValidator.cs
│   ├── MessageRequestValidator.cs
│   └── RevealReadyRequestValidator.cs
│
├── BackgroundJobs/                    # Coravel IInvocable classes (self-documenting name)
│   ├── MatchGenerationJob.cs          # Idempotent — also triggered on startup (critical: see Core Decisions)
│   ├── SendPushNotificationJob.cs     # FirebaseAdmin (FCM/Android) + dotAPNS (iOS); deletes stale tokens on error response
│   ├── SendModeratorAlertJob.cs
│   └── TrialExpiryNotificationJob.cs
│
├── Errors/                            # RFC 7807 typed problem codes — never scatter error strings in controllers
│   └── AppErrors.cs                   # Centralized problem type URIs and application error codes
│
├── Migrations/                        # EF Core migrations — committed to source control
│
├── Infrastructure/
│   ├── Data/
│   │   └── AppDbContext.cs            # EF Core DbContext + PostGIS + UseSnakeCaseNamingConvention
│   ├── Auth/
│   │   └── SocialLoginHandler.cs      # ExternalLoginAsync wiring (Apple, Google, Facebook)
│   ├── Storage/
│   │   └── S3ClientFactory.cs         # Hetzner Object Storage; ForcePathStyle = true required
│   ├── Scanning/
│   │   └── ContentScanningClient.cs   # External scan API client (*Client naming — not a domain service)
│   ├── Email/
│   │   └── SmtpSettings.cs            # IOptions<SmtpSettings> — credentials from env vars only
│   └── Middleware/
│       └── ExceptionHandlerMiddleware.cs  # Global Problem Details error handler (4xx/5xx)
│
└── appsettings.json

Blinder.Tests/
├── Unit/                              # Mirrors Blinder.Api/ structure
│   ├── Services/
│   ├── Validators/
│   └── Mappings/
└── Integration/                       # WebApplicationFactory-based controller tests
    └── Controllers/
```

---

### Mobile Structure (`mobile/`)

```
mobile/
├── app/
│   ├── (auth)/
│   │   ├── login.tsx
│   │   ├── register.tsx
│   │   ├── invite-landing.tsx         # Invite deep link entry — preserves invite context through registration flow
│   │   └── onboarding/
│   │       ├── quiz.tsx               # Values & personality quiz (FR8)
│   │       ├── photo.tsx              # Photo upload + content scan gate (FR9)
│   │       └── preferences.tsx        # Radius and matching preferences (FR15)
│   └── (tabs)/
│       ├── conversations/
│       │   ├── index.tsx              # Conversation list (FR17–21)
│       │   └── [id].tsx               # Conversation detail + SignalR live chat
│       ├── reveal/
│       │   └── index.tsx              # Reveal state + RevealMoment trigger (FR22–27)
│       ├── match/
│       │   └── index.tsx              # Current match view + EmptyMatchState (FR12–16)
│       └── settings/
│           └── index.tsx              # Profile, subscription, preferences
│
├── components/
│   ├── chat/
│   │   ├── MessageBubble.tsx
│   │   └── MessageInput.tsx
│   ├── reveal/
│   │   ├── RevealMoment.tsx           # Emotional centrepiece — isolated, first-class component
│   │   ├── RevealCountdown.tsx        # Pre-reveal state UI
│   │   └── RevealPrompt.tsx           # CTA to set reveal_ready flag
│   ├── onboarding/
│   │   ├── QuizQuestion.tsx
│   │   └── PhotoUpload.tsx
│   ├── common/
│   │   ├── EmptyMatchState.tsx        # Explicit AC in first Match story — never a polish item
│   │   └── LoadingSpinner.tsx
│   └── moderation/
│       ├── ReportButton.tsx           # Reused across Conversation, Match, Profile — not reimplemented inline
│       └── BlockConfirmation.tsx      # Shared modal — defined once, used everywhere
│
├── hooks/
│   ├── useConversation.ts             # Messages, SignalR subscription, send action
│   ├── useRevealState.ts              # Flag state, threshold check, photo URL fetch
│   ├── useMatch.ts                    # Current match, empty state detection
│   ├── useSubscription.ts             # Premium status, trial state, feature gates
│   └── useAuth.ts                     # Token management, login, logout
│
├── services/
│   ├── apiClient.ts                   # Axios/fetch instance + auth header injection
│   ├── signalrService.ts              # Singleton connection manager — never instantiated per-component
│   └── storageService.ts              # expo-secure-store wrapper (JWT tokens only)
│
├── constants/
│   ├── errors.ts                      # User-facing error message strings (no raw exception messages in UI)
│   └── theme.ts                       # NativeWind / Tailwind token overrides
│
├── types/
│   ├── api/                           # Response and request types matching backend DTOs
│   │   └── index.ts
│   └── signalr/                       # Hub method names, payload types, connection state enums
│       └── index.ts
│
└── utils/
    └── dateFormat.ts                  # ISO 8601 → display string; never store/transmit local time
```

---

### Requirements-to-Structure Mapping

| Domain | FR Range | Backend Owner | Mobile Owner |
|---|---|---|---|
| Account & Auth | FR1–7 | `AuthController`, `AccountController`, `AuthService` | `app/(auth)/`, `useAuth`, `storageService` |
| Onboarding & Profile | FR8–11 | `OnboardingController`, `Infrastructure/Scanning/` | `app/(auth)/onboarding/`, `components/onboarding/` |
| Matching | FR12–16 | `MatchController`, `MatchService`, `BackgroundJobs/MatchGenerationJob` | `app/(tabs)/match/`, `useMatch`, `EmptyMatchState` |
| Real-time Chat | FR17–21 | `ConversationController`, `Hubs/ChatHub` | `app/(tabs)/conversations/`, `useConversation`, `signalrService` |
| Reveal System | FR22–27 | `RevealController`, `RevealService`, `Infrastructure/Storage/` | `app/(tabs)/reveal/`, `useRevealState`, `components/reveal/` |
| Subscriptions | FR28–31 | `SubscriptionController`, `SubscriptionService`, `BackgroundJobs/TrialExpiryNotificationJob` | `app/(tabs)/settings/`, `useSubscription` |
| Safety & Moderation | FR32–40 | `ModerationController`, `ModerationService`, `Infrastructure/Scanning/`, `Pages/Admin/` | `components/moderation/` (ReportButton, BlockConfirmation) |
| Analytics & Compliance | FR41–45 | `AnalyticsController`, `Pages/Admin/Dashboard` | DB-backed event tracking in key user journey hooks |

---

### Integration Points

| Integration | Backend Home | Notes |
|---|---|---|
| PostgreSQL + PostGIS | `Infrastructure/Data/AppDbContext.cs` | `postgis/postgis` Docker image; data on named volume `db-data`; `UseSnakeCaseNamingConvention()` + `UseNetTopologySuite()` |
| Hetzner Object Storage | `Infrastructure/Storage/S3ClientFactory.cs` | `ForcePathStyle = true` — spike before upload story |
| Content Scanning (SafeSearch / Azure CM) | `Infrastructure/Scanning/ContentScanningClient.cs` | Synchronous — called by `OnboardingController` before confirming upload; hard-fail on API outage |
| NCMEC / PhotoDNA | `Infrastructure/Scanning/` | Parallel legal workstream — integration after agreement signed |
| SignalR real-time | `Hubs/ChatHub.cs` ↔ `services/signalrService.ts` | Hub methods: `ReceiveMessage`, `RevealStateUpdated`, `MatchAssigned` |
| FCM / APNs push | `BackgroundJobs/SendPushNotificationJob.cs` | Coravel fire-and-forget; acceptable to drop on restart at MVP |
| Apple IAP + Google Play Billing | `SubscriptionController` + `SubscriptionService` | **Spike story required** before any subscription implementation |
| DB-backed analytics | `AnalyticsController` + PostgreSQL | Reveal events and gender ratio data stored in the main PostgreSQL DB; no external analytics service |
| SMTP via hosting | `Infrastructure/Email/SmtpSettings.cs` + Coravel mailing | Credentials from env vars via `IOptions<SmtpSettings>` |
| Apple / Google / Facebook login | `Infrastructure/Auth/SocialLoginHandler.cs` | `ExternalLoginAsync` — not scaffold-covered; explicit implementation stories required |

---

### Structural Constraints Summary

- **`Services/` = domain orchestration only.** `*Service` naming. External API calls go in `Infrastructure/` as `*Client` classes. `ContentScanningClient`, `S3ClientFactory` live in `Infrastructure/` — not `Services/`.
- **`BackgroundJobs/` = Coravel `IInvocable` only.** `MatchGenerationJob` must be idempotent and triggered on startup in addition to its cron schedule.
- **`Errors/AppErrors.cs` = single source of truth for problem type codes.** Never define error type strings inline in controllers or handlers.
- **`types/api/` and `types/signalr/` are separate.** API types track DTO contracts. SignalR types track hub event payloads and connection enums — drift between them is a common bug source.
- **`invite-landing.tsx` is mandatory for the invite conversion flow.** Landing directly on `register.tsx` from a deep link loses invite context and breaks invite lineage tracking.
- **`ReportButton.tsx` and `BlockConfirmation.tsx` are defined once in `components/moderation/`.** They must not be reimplemented inline in Conversation, Match, or Profile screens.
- **`DeviceToken` table is the sole persistence layer for push tokens.** Tokens are registered via `POST /api/account/device-token`; stale tokens are deleted by `SendPushNotificationJob` on FCM/APNs error — never accumulate dead tokens.
- **IAP webhook verification cannot be disabled in production.** `SkipWebhookVerification` flag is valid only in `appsettings.Testing.json`; a startup assertion must enforce this.
- **`docker-compose.yml` is the single source of truth for service topology.** All services (API, database, Nginx) are defined there. Nothing runs directly on the host OS at runtime — no `systemd` units for application services.
- **`nginx/nginx.conf` must include WebSocket upgrade headers on the `/hubs/` location block.** Missing `proxy_http_version 1.1`, `Upgrade`, `Connection "upgrade"`, and `proxy_read_timeout 3600s` causes SignalR to silently fall back to long polling, breaking the `<500ms` chat delivery NFR.
- **`.env.example` must always be kept in sync with actual `.env` requirements.** When a new environment variable is added anywhere in the codebase or infrastructure config, `.env.example` must be updated in the same commit.
- **Named volume `db-data` must never be removed without a verified database backup.** `docker compose down -v` is explicitly prohibited in production — it destroys all named volumes including the database.

---

## Architecture Validation Results

### Coherence Validation ✅

**Decision Compatibility:**

All technology choices are mutually compatible. Key verifications:
- .NET 10 + EF Core 10 + Npgsql 9.x + PostGIS: fully compatible, all target `net10.0`
- ASP.NET Core Identity + JWT Bearer + Razor Pages cookie auth: dual auth schemes (`[Authorize(AuthenticationSchemes = ...)]`) natively supported
- FluentValidation + `AddProblemDetails()` + RFC 7807: FluentValidation ASP.NET Core integration auto-produces Problem Details 400 responses
- Mapperly + EF Core: compile-time generation; no proxy/lazy-loading conflicts
- Coravel + SignalR: separate concerns; jobs use `IHubContext<ChatHub>` to push events to connected clients
- Expo SDK 55 + `@microsoft/signalr` + `expo-secure-store` + NativeWind: all compatible with React Native 0.83
- FirebaseAdmin + dotAPNS: both run in-process inside the API container; no external routing service dependency
- AWSSDK.S3 + Hetzner Object Storage (`ForcePathStyle = true`): standard S3 client, documented
- Docker Compose multi-service setup: all services (API, `postgis/postgis`, Nginx) are standard images with no known compatibility conflicts on a Linux VPS host

No conflicts identified.

**Pattern Consistency:**

- `snake_case` DB + `UseSnakeCaseNamingConvention()` + EF Core migrations: consistent end-to-end
- RFC 7807 Option B + FluentValidation auto-integration + `AppErrors.cs` URIs: single error shape throughout
- `AsyncState<T>` pattern applied to all async hooks: defined and enforceable
- `DateTimeOffset` / `timestamptz` / ISO 8601: three-layer contract consistent
- `*Service` (domain orchestration) / `*Client` (external API) naming split: enforced by directory structure (`Services/` vs `Infrastructure/`)

**Structure Alignment:**

- Controllers → Services → Infrastructure/Data: clean layered boundaries, no circular dependencies
- `ContentScanningClient` in `Infrastructure/Scanning/` (not `Services/`): correct separation
- `BackgroundJobs/` using `IHubContext<ChatHub>` for real-time push from Coravel: valid injection pattern
- Mobile: components → hooks → services → `apiClient`/`signalrService`: unidirectional dependency flow

---

### Requirements Coverage Validation ✅

**Functional Requirements (45 FRs):**

| Domain | FRs | Status |
|---|---|---|
| Account & Auth | FR1–7 | ✅ Full coverage — `AuthController`, `AccountController`, `SocialLoginHandler`, cascade delete, `DeviceToken` registration |
| Onboarding & Profile | FR8–11 | ✅ Full coverage — `OnboardingController`, synchronous scan gate, `app/(auth)/onboarding/` |
| Matching | FR12–16 | ✅ Full coverage — `MatchService`, `MatchGenerationJob` (idempotent + startup), PostGIS radius, `InviteLink`, admin threshold |
| Chat & Conversation | FR17–21 | ✅ Full coverage — `ChatHub`, `ConversationController`, `message_count`, active conv limit |
| Reveal System | FR22–27 | ✅ Full coverage — Dual async flags, threshold gate, signed URL gated via `RevealService` |
| Subscription & Premium | FR28–31 | ✅ Full coverage — `SubscriptionController` with IAP webhook JWT verification, `TrialExpiryNotificationJob` |
| Safety & Moderation | FR32–40 | ✅ Full coverage — `ContentScanningClient` (synchronous, hard-fail), NCMEC slot, `Report`/`ModerationAction`, 2yr audit retention |
| Analytics & Compliance | FR41–45 | ✅ Full coverage — DB-backed analytics, gender ratio dashboard, reveal event tracking, audit log, GDPR deferred with posture documentation required |

**Non-Functional Requirements (30 NFRs):**

| Category | Status | Notes |
|---|---|---|
| Performance | ✅ | SignalR WebSockets; synchronous scan with 10s SLA; VPS + Nginx + PostgreSQL adequate for Poland-first volume |
| Security | ✅ | Nginx TLS; signed URL gating; IAP webhook JWT verification; no card data stored; `expo-secure-store` for tokens; no PII in logs |
| Scalability | ✅ (deferred) | Single VPS architecture adequate for MVP; SignalR Redis backplane deferred post-MVP |
| Accessibility | ✅ | Architecture-neutral; enforced at component layer (NativeWind, Dynamic Type, AA contrast) |
| Reliability | ✅ | Docker Compose `restart: unless-stopped` + Nginx; hard-fail scan policy; push job acceptable drop on restart at MVP |
| Compliance | ✅ (partial) | GDPR posture documentation required before first real user; NCMEC legal workstream as parallel action item |
| Encryption at rest | ✅ | Hetzner Object Storage: encrypted by default; PostgreSQL VPS: **LUKS disk encryption required at provisioning** (deployment checklist item) |

---

### Implementation Readiness Validation ✅

**Decision Completeness:**

All critical decisions are documented with specific versions, package names, and implementation constraints. No decision is left as "TBD" for items on the critical path. Items deferred post-MVP are explicitly scoped.

**Structure Completeness:**

Backend and mobile directory trees are fully specified to file level. All integration points have named homes. Requirements-to-structure mapping table covers all 45 FRs. Boundaries between `Services/`, `Infrastructure/`, and `BackgroundJobs/` are explicitly defined.

**Pattern Completeness:**

All eight identified conflict points are addressed: naming conventions (DB, API, C#, TypeScript), response format, datetime handling, error handling, loading states, SignalR patterns, validation patterns, mapping patterns, logging patterns. Ten enforcement guidelines provide enforceable rules for AI agents.

---

### Gap Analysis Results

| Priority | Gap | Resolution |
|---|---|---|
| 🔴 Critical | IAP webhook signature verification absent — OWASP A01 risk | ✅ Resolved — Apple RS256 JWKS + Google JWKS verification documented in Core Decisions with JWKS caching and test bypass flag |
| 🔴 Critical | Push notification mechanism undefined — blocked 4+ stories | ✅ Resolved — Direct FCM (FirebaseAdmin) + APNs (dotAPNS) from .NET; `DeviceToken` table; stale token cleanup; `IPushNotificationService` interface |
| 🟡 Important | Encryption at rest not documented as deployment requirement | ✅ Resolved — LUKS disk encryption documented as VPS provisioning checklist item |
| 🟡 Important | Admin cookie CSRF protection unconfirmed | ✅ Non-issue — ASP.NET Core Razor Pages enables anti-forgery tokens by default for all POST handlers; confirmation comment sufficient |
| 🟢 Minor | Analytics approach undefined | ✅ Resolved — DB-backed analytics; reveal events and gender ratio data stored in PostgreSQL via `AnalyticsController` |

All gaps resolved. No outstanding critical or important gaps remain.

---

### Architecture Completeness Checklist

**✅ Requirements Analysis**
- [x] Project context thoroughly analyzed (45 FRs, 30 NFRs, 8 domains)
- [x] Scale and complexity assessed (medium complexity, Poland-first, 10K concurrent at launch)
- [x] Technical constraints identified (SignalR WebSocket requirement, EU data residency, NCMEC legal lead time)
- [x] Cross-cutting concerns mapped (content scanning, reveal authorization, push routing, account deletion cascade, audit retention)

**✅ Architectural Decisions**
- [x] Critical decisions documented with specific versions (.NET 10, Expo SDK 55, PostgreSQL, all NuGet/npm packages named)
- [x] Technology stack fully specified (backend + mobile + database + storage + background jobs + push + email + admin)
- [x] Integration patterns defined (S3, FCM, APNs, SignalR, SMTP, IAP webhooks, DB-backed analytics)
- [x] Security requirements addressed (IAP webhook JWT verification, signed URLs, TLS, token storage, no PII in logs, LUKS at rest)

**✅ Implementation Patterns**
- [x] Naming conventions established (DB snake_case, API kebab-case, C# PascalCase, TypeScript conventions)
- [x] Structure patterns defined (`*Service` vs `*Client`, `BackgroundJobs/`, `Errors/`, `types/api/` + `types/signalr/`)
- [x] Communication patterns specified (RFC 7807 Option B, SignalR hub method names, `AsyncState<T>`)
- [x] Process patterns documented (error handling, loading states, validation, mapping, logging, enforcement guidelines)

**✅ Project Structure**
- [x] Complete directory structure defined (backend + tests + mobile to file level)
- [x] Component boundaries established (Controllers / Services / Infrastructure / BackgroundJobs)
- [x] Integration points mapped (9 external integrations, all with named handler locations)
- [x] Requirements to structure mapping complete (all 45 FRs mapped to controllers, services, and mobile screens)

---

### Architecture Readiness Assessment

**Overall Status: READY FOR IMPLEMENTATION**

**Confidence Level: High**

Basis: All 45 FRs and 30 NFRs are architecturally supported, all critical security decisions are documented, all technology choices are version-pinned and mutually compatible, and all known implementation conflict points have explicit resolution rules.

**Key Strengths:**
- Self-sovereign, fully containerized infrastructure: Docker Compose on VPS + direct FCM/APNs + Hetzner Object Storage — minimal third-party SaaS dependencies; environment is reproducible from day one
- Docker-first approach means dev/prod parity from the first commit; no "works on my machine" class of deployment failures
- Security gaps (IAP webhook verification, signed URL gating, LUKS at rest) all resolved before implementation begins
- Coravel + startup idempotent match check prevents the worst UX failure mode (empty match screen after restart); `restart: unless-stopped` ensures containers recover on VPS reboot
- Mapperly + FluentValidation + RFC 7807 produce a consistent, type-safe API surface that AI agents can implement without ambiguity
- `invite-landing.tsx` + invite lineage tracking preserved from first story — no retrofit needed

**Areas for Future Enhancement (Post-MVP):**
- SignalR Redis backplane when horizontal scaling is needed
- Managed PostgreSQL service (separate from VPS) when database load warrants it
- ML-based matching when training data exists
- GDPR compliance feature set (right to erasure flows, DPO tooling) — noted as deferred, required before expansion beyond beta

---

### Implementation Handoff

**AI Agent Guidelines:**
- Follow all architectural decisions exactly as documented — no substitutions without an explicit architecture update
- Use implementation patterns consistently across all components — enforcement guidelines are non-negotiable
- Respect project structure and boundaries — `Services/` is domain orchestration only; `Infrastructure/` contains all external integrations
- Refer to this document for all architectural questions before making any technology or pattern choice

**Deployment Checklist (Non-Code Requirements):**
- [ ] Enable LUKS disk encryption at VPS provisioning (before first user data is written) — host-level requirement; Docker does not protect data at rest on the host filesystem
- [ ] Install Docker Engine and Docker Compose plugin on VPS (`docker compose version` to verify)
- [ ] Create `.env` from `.env.example` on VPS; populate all secrets — never commit `.env` to source control
- [ ] Store `APNS_KEY` (`.p8` file contents) and `FIREBASE_CREDENTIALS_JSON` as values in the VPS `.env` file
- [ ] Configure Nginx IP allowlist on `/admin` in `nginx/nginx.conf` before the Nginx container is started
- [ ] Verify named Docker volume `db-data` persists across `docker compose down` / `up` cycles before first user data is written (run `docker volume inspect db-data`)
- [ ] Generate idempotent migration script before first deployment: `dotnet ef migrations script --idempotent --output migrations/latest.sql` (requires SDK — run in dev environment or CI); commit `migrations/latest.sql` to the repository
- [ ] Apply migrations on every deployment: `docker compose exec -T db psql -U $POSTGRES_USER -d $POSTGRES_DB < migrations/latest.sql` (after `docker compose up -d db`)
- [ ] Initiate NCMEC legal agreement process (parallel workstream — external lead time weeks to months)
- [ ] Confirm GDPR posture documentation in place before first real user onboards

**First Implementation Sequence:**
1. Backend scaffolding: `dotnet new sln -n Blinder && dotnet new webapi -n Blinder.Api --use-controllers --framework net10.0`
2. Docker setup: `Dockerfile` (multi-stage), `docker-compose.yml`, `docker-compose.override.yml`, `nginx/nginx.conf`, `.env.example`, `.dockerignore` — committed from the first development commit; all subsequent development runs through `docker compose up`
3. Mobile scaffolding: `npx create-expo-app@latest --template default@sdk-55`
4. Spike: Apple + Google IAP webhook verification (unblocks subscription stories)
5. Spike: Hetzner Object Storage `ForcePathStyle = true` + signed URL generation (unblocks photo upload story)
6. Spike: Confirm `getDevicePushTokenAsync()` native token retrieval in Expo SDK 55 build (unblocks push notification stories)