---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments:
  - _bmad-output/planning-artifacts/prd.md
  - _bmad-output/planning-artifacts/ux-design-specification.md
  - _bmad-output/planning-artifacts/product-brief-Blinder.md
  - _bmad-output/planning-artifacts/product-brief-Blinder-distillate.md
workflowType: 'architecture'
project_name: 'Blinder'
user_name: 'Piotr.palej'
date: '2026-04-03'
lastStep: 8
status: 'complete'
completedAt: '2026-04-03'
---

# Architecture Decision Document - Blinder

_This document builds collaboratively through step-by-step discovery. Sections are appended as we work through each architectural decision together._

> **Design System Alignment.** The mobile UI layer described throughout this document is bound to the packaged Blinder design system. Tamagui tokens (`tamagui.config.ts`), the web CSS variables in [`colors_and_type.css`](./colors_and_type.css), the component showcase in [`ui_kits/Blinder/`](./ui_kits/Blinder/), and the written spec in [`ux-design-specification.md`](./ux-design-specification.md) + [`README.md`](./README.md) + [`SKILL.md`](./SKILL.md) are all materializations of the same source. When this architecture document references UI primitives, colour tokens, motion, or component names, treat the packaged design system as canonical and keep token values and semantic intent in lockstep across platforms.

## Project Context Analysis

### Requirements Overview

**Functional Requirements:**
Blinder is centered on a conversation-first dating flow with a tightly defined lifecycle: onboarding, daily match delivery, blind conversation, dual-trigger decision gate, private simultaneous decisioning, reveal or dignified ending, and re-entry into the queue. Architecturally, this means the system is less like a feed-based social app and more like a stateful workflow platform with messaging as its primary interface.

The functional scope clusters into several capability areas:
- user identity, onboarding, consent, and age gating;
- match generation and daily delivery;
- blind-phase messaging with one active conversation focus;
- decision gate orchestration driven by both time-floor and message-threshold events;
- outcome handling for reveal, continue, abandon, timeout, and non-mutual endings;
- moderated photo upload and controlled reveal access;
- direct push notifications for meaningful events only;
- internal support, moderation, and quality-ops tooling;
- first-party telemetry for trust and quality metrics.

These requirements imply a relatively small number of major domains, but each domain is trust-critical and tightly coupled to state correctness.

**Non-Functional Requirements:**
The dominant NFRs are reliability, privacy, emotional safety, and compliance. The PRD explicitly requires a deterministic and auditable decision-gate state machine, resilient recovery from app restarts and network interruptions, strict prevention of pre-reveal photo exposure, high reliability in upload-time moderation, and disciplined notifications that avoid pressure or spam.

The UX specification adds further architectural pressure:
- strong support for clear phase-based states;
- performant and intentional transitions for gate and reveal moments;
- privacy-preserving lock-screen notification behavior;
- accessibility support, including reduced-motion handling and scalable text;
- consistent mobile behavior across iOS and Android.

Compliance requirements include GDPR-first handling, account deletion, retention/export expectations, policy acceptance, and age-gate enforcement. Operationally, the architecture must also support support-timeline reconstruction and moderation auditability.

**Scale & Complexity:**
This is a high-complexity product, not because of broad feature count, but because the core loop has several trust-sensitive branching states that must always resolve correctly.

- Primary domain: cross-platform mobile application with backend workflow orchestration
- Complexity level: high
- Estimated architectural components: 9 major components

The likely major components are:
- mobile client application;
- authentication and profile/onboarding domain;
- matching service;
- messaging service;
- conversation-state and decision-gate orchestration service;
- photo storage and moderation pipeline;
- notification service;
- admin/support operations tooling;
- telemetry and analytics pipeline.

### Technical Constraints & Dependencies

Known constraints and dependencies already identified in the source documents include:
- server-authoritative handling of all trust-critical conversation transitions;
- no photo visibility before mutual reveal across any client or API path;
- direct APNS/FCM push only in MVP;
- self-hosted or first-party leaning storage, moderation, telemetry, and ops tooling;
- no offline write support for MVP;
- staged rollout by cohort/city;
- internal support and moderation capabilities required from MVP rather than deferred to third-party tooling.

These constraints bias the architecture toward explicit workflow boundaries, auditable event handling, and lean ownership of core infrastructure.

### Cross-Cutting Concerns Identified

Several concerns will affect multiple architectural components:
- privacy and access control, especially around pre-reveal media protection;
- deterministic workflow resolution for simultaneous or conflicting events;
- audit logging for safety, moderation, and support reconstruction;
- notification governance to preserve the anti-urgency product stance;
- telemetry for quality and safety without creating rejection-scoring artifacts;
- moderation fallback paths for uncertain photo-scan outcomes;
- recovery and synchronization after reconnect or app relaunch;
- accessibility and calm-performance requirements in trust-critical UI states.

These cross-cutting concerns should be treated as architectural drivers rather than implementation details.

## Starter Template Evaluation

### Primary Technology Domain

Mobile application foundation, based on project requirements analysis.

Blinder is primarily a cross-platform mobile product with a trust-sensitive conversation workflow and a highly intentional UI system. The backend is essential, but the starter-template decision that most affects day-one implementation consistency is the mobile app foundation. This is therefore a mobile foundation decision, not a whole-system bootstrap choice.

### Starter Options Considered

**1. Expo official default template (`default@sdk-55`)**
This is the current official Expo starter and includes Expo Router, TypeScript, and the updated SDK 55 project structure.

Pros:
- Officially maintained by Expo
- Current SDK 55-compatible initialization path
- Good default structure and tooling
- Strong long-term maintenance confidence

Cons:
- The SDK 55 default template is designed around native tabs out of the box
- That conflicts with Blinder's explicit UX decision to avoid persistent tab navigation

## Project Structure & Boundaries

- It introduces a navigation metaphor that contradicts the one-conversation, calm-focus product model

- It would require undoing starter assumptions early

**2. Expo blank TypeScript template (`blank-typescript`)**
This is the most minimal official Expo foundation.

Pros:
- No conflicting navigation assumptions
- Maximum architectural control
- Clean fit for Blinder's single-focus navigation model

Cons:
- Does not include Tamagui integration
- Does not establish a design-system-aware structure
- Pushes more foundational work into the first implementation story
- Makes it easier for ad hoc styling and token drift to appear before the design system is locked

**3. Tamagui Expo Router starter**
Tamagui's Expo guide provides a current starter path for an Expo Router app with Tamagui integration.

Pros:
- Best alignment with the UX specification's explicit Tamagui decision
- Supports a token-first design system from the start
- Better fit for custom, animation-aware UI work than a plain Expo starter
- Compatible with a stack-based, no-tab navigation model
- Helps enforce the product rule that screen work should begin from tokens and shared primitives rather than hardcoded values

Cons:
- Less "official Expo default" than the core Expo starter
- Requires Yarn 4.4.0+ according to Tamagui's current guide
- Still needs deliberate pruning of any example/demo structure

**4. Ignite**
Ignite remains a respected React Native starter ecosystem, but it is a weaker fit here.

Pros:
- Mature community and recipes
- Good for teams that want a broader prebuilt app foundation

Cons:
- More opinionated than needed for Blinder
- Less aligned with the documented Tamagui-first design decision
- Not the cleanest fit for a custom, calm, trust-sensitive UX foundation

### Selected Starter: Tamagui Expo Router starter

**Rationale for Selection:**
This starter best matches the documented product and UX decisions already made for Blinder.

The deciding factors are:
- the design specification explicitly chose Tamagui;
- the app needs a token-first design system before screen work begins;
- the app requires custom navigation rather than tab-centric defaults;
- the UI depends on intentional motion and custom components in trust-critical flows;
- the starter helps protect the UX system from early implementation drift.

Using the Expo SDK 55 default starter would mean starting from a tabs-oriented template and immediately undoing foundational decisions. Using `blank-typescript` would avoid that problem, but it would push too much setup work into implementation. The Tamagui Expo Router starter gives a better middle ground: current Expo-compatible foundations plus the design-system integration the product already requires.

This starter should be treated as a bootstrap layer only, not as a structure to preserve verbatim. An early implementation story should prune demo structure and reorganize the app into domain-first modules aligned to the Blinder lifecycle: onboarding, waiting, match entry, conversation, gate, reveal, ending, and settings.

**Initialization Command:**

```bash
yarn create tamagui@latest --template expo-router
```

**Architectural Decisions Provided by Starter:**

**Language & Runtime:**
- TypeScript-based React Native application
- Expo-based runtime and toolchain
- Expo Router-compatible application structure
- Compatible with the current Expo SDK 55 ecosystem

**Styling Solution:**
- Tamagui-based design system foundation
- Token-driven theming and component primitives
- Clear path for enforcing the "no hardcoded values" design rule
- Good fit for custom reveal, gate, and conversation components
- **Must stay in lockstep with the packaged web design system.** The Tamagui token file (`mobile/blinder-app/tamagui.config.ts`) and the packaged CSS variables in [`colors_and_type.css`](./colors_and_type.css) are two materializations of the same source (`ux-design-specification.md`, `README.md`, `SKILL.md`). Token names, numeric values, and semantic intent must match one-to-one; drift is a bug. The live web showcase in [`ui_kits/Blinder/`](./ui_kits/Blinder/) is the visual-diff reference for the Tamagui token showcase screen shipped by Story 1.5.

**Build Tooling:**
- Expo CLI development workflow
- Compatible with development builds, which should be the expected dev path for this app given notifications and native integrations
- Strong fit for EAS-based mobile build and release workflow later

**Testing Framework:**
- No major testing advantage over the official Expo starter by itself
- Testing choices should still be made deliberately in architecture decisions
- Suitable foundation for adding Jest and React Native Testing Library in early implementation

**Code Organization:**
- Router-based application structure
- Good fit for phase-based screen organization
- Better starting point than tabs-first starters for Blinder's single-focus navigation model
- Supports separating app flows from design-system and domain logic cleanly
- Should be restructured early into domain-first modules rather than left in generic starter form

**Development Experience:**
- Current maintained setup path from Tamagui documentation
- Faster start on token and component architecture
- Better alignment with the chosen UX foundation than a generic starter
- Reduces rework in the first mobile implementation stories

**Companion backend note:**
The mobile starter does not replace the need for a backend foundation. The backend should be initialized separately as an ASP.NET Core 10 Web API in an early implementation story so that the client and server foundations evolve in parallel.

**Note:** Project initialization using this command should be the first mobile implementation story.

## Core Architectural Decisions

### Decision Priority Analysis

**Critical Decisions (Block Implementation):**
- Use ASP.NET Core 10 / .NET 10 for all backend applications.
- Split the backend into three applications: IdentityServer, Api, and AdminPanel.
- Use a single PostgreSQL database with logical ownership boundaries instead of separate physical databases.
- Use OpenIddict 7.4.0 as the self-hosted OAuth2/OIDC server foundation.
- Use Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1 with EF Core 10 for relational data access.
- Keep IdentityServer as the sole writer of identity-owned data.
- Keep Api as the sole writer of business-owned data.
- Route all AdminPanel operations through Api endpoints rather than direct database access.

**Important Decisions (Shape Architecture):**
- Use schema-level separation inside one database: `identity.*` and `app.*`.
- Use Razor Pages for AdminPanel and authenticate it with OIDC against IdentityServer.
- Use REST/JSON APIs as the primary integration style between clients and Api.
- Use built-in ASP.NET Core OpenAPI 3.1 generation for API documentation.
- Use SignalR selectively for trust-critical real-time conversation updates where polling would degrade experience.
- Use Problem Details-based error responses consistently across the API surface.

**Deferred Decisions (Post-MVP or later refinement):**
- Distributed cache introduction (likely Redis) is deferred until measured need appears.
- Search specialization and advanced PostgreSQL extensions are deferred.
- Event bus / message broker is deferred unless the single-deployment architecture shows operational pressure.
- Advanced read models or CQRS splits are deferred.
- Detailed CI/CD platform selection is deferred, though container-first deployment remains the expected path.

### Data Architecture

**Primary database: PostgreSQL 18**
- PostgreSQL 18 is the current major release line, with 18.3 listed as current stable patch release.
- One shared PostgreSQL database will be used for both identity and business data.
- Rationale: preserves transactional simplicity across trust-critical workflows while avoiding cross-database coordination overhead.

**Database ownership model**
- IdentityServer owns the `identity.*` schema.
- Api owns the `app.*` schema.
- Shared physical storage does not imply shared write responsibility.
- Api may read selected identity data needed for business projections and authorization-aware workflows, but it must not mutate identity-owned tables.
- IdentityServer may read selected application data only when required, but it must not mutate business-owned tables.

**ORM and migrations**
- Use EF Core 10 with Npgsql.EntityFrameworkCore.PostgreSQL 10.0.1.
- Maintain separate migration sets by application/schema ownership.
- Api migrations may only alter `app.*`.
- IdentityServer migrations may only alter `identity.*`.

**Validation and data integrity**
- Use database constraints for invariants that must never be bypassed:
  - foreign keys,
  - uniqueness constraints,
  - check constraints where appropriate,
  - row-version/concurrency controls on trust-critical aggregates.
- Use application-level validation for workflow rules and product behavior.
- Keep business invariants in Api, not in AdminPanel UI code.

**Caching strategy**
- Start without distributed cache in MVP.
- Prefer database correctness and simplicity first.
- Re-evaluate after real usage pressure around profile lookups, match candidate generation, or token/session hot paths.

### Authentication & Security

**Identity authority**
- IdentityServer is a self-hosted OAuth 2.0 / OpenID Connect provider implemented on OpenIddict 7.4.0.
- MobileApp and AdminPanel both authenticate against IdentityServer.
- IdentityServer is the only application allowed to create or modify identity-owned data.

**Supported identity capabilities**
- Local first-party accounts.
- External federation for Google, Apple, and Facebook sign-in.
- Refresh tokens, revocation, session management, and claim/scopes issuance.
- OIDC for interactive sign-in and OAuth2 token issuance for API access.
- MFA/passkey readiness should remain open in the design even if not delivered in MVP.

**Authorization model**
- Api uses bearer-token authorization for mobile clients.
- AdminPanel uses OIDC sign-in against IdentityServer and then calls Api using admin-authorized access tokens.
- Admin endpoints live in Api but are isolated via dedicated scopes, roles, and policies.
- Moderation and user-management commands remain business workflows executed by Api, not direct UI/database actions.

**Security boundaries**
- Api must not directly modify identity data.
- If business workflows require identity changes, Api triggers IdentityServer-owned commands for those operations.
- Examples:
  - account disablement,
  - session revocation,
  - forced re-consent,
  - external login unlinking.

**Data protection and privacy**
- No pre-reveal media access through any API path.
- Lock-screen and notification payloads must remain privacy-preserving.

### Architectural Boundaries

**API Boundaries:**
- `Blinder.IdentityServer` is the authentication authority only.
- `Blinder.Api` is the only business API and exposes:
  - mobile endpoints under `/api/v1/...`
  - admin endpoints under `/api/v1/admin/...`
  - SignalR hubs under a dedicated real-time route surface
- `Blinder.AdminPanel` does not perform direct business writes to the database.
- `Blinder.AdminPanel` authenticates with IdentityServer and calls Api for moderation, support, and user-management workflows.

**Component Boundaries:**
- Mobile UI domains are split by lifecycle stage, not by generic component type alone.
- Backend feature slices in `Blinder.Api/Features` own their contracts, application logic, domain logic, and infrastructure concerns for that feature.
- `Blinder.SharedKernel` is for small foundational primitives only, not a dumping ground for business logic.
- `Blinder.Contracts` contains only intentionally shared contracts needed across process boundaries.

**Service Boundaries:**
- `IdentityServer` writes only `identity.*`
- `Api` writes only `app.*`
- `AdminPanel` writes nothing directly to database-owned business tables
- Integration from Api to identity-side mutations goes through explicit service/integration calls, not shared DbContext writes

**Data Boundaries:**
- `identity.*` schema belongs to IdentityServer
- `app.*` schema belongs to Api
- `Persistence/Migrations` folders remain separated by owning app
- Read-only projections of identity data for Api should be implemented through controlled mappings/views/query models, not mutable entity ownership

### Requirements to Structure Mapping

**Feature/Epic Mapping:**
- Onboarding and profile setup:
  - `backend/src/Blinder.Api/Features/Onboarding/`
  - `mobile/blinder-app/src/features/onboarding/`
- Matching:
  - `backend/src/Blinder.Api/Features/Matching/`
  - `mobile/blinder-app/src/features/waiting/`
  - `mobile/blinder-app/src/features/match-entry/`
- Blind conversation:
  - `backend/src/Blinder.Api/Features/Conversations/`
  - `mobile/blinder-app/src/features/conversation/`
- Decision gate and reveal:
  - `backend/src/Blinder.Api/Features/DecisionGates/`
  - `backend/src/Blinder.Api/Features/Reveals/`
  - `mobile/blinder-app/src/features/decision-gate/`
  - `mobile/blinder-app/src/features/reveal/`
- Dignified endings and recovery:
  - `backend/src/Blinder.Api/Features/Support/`
  - `mobile/blinder-app/src/features/ending/`
- Photo upload and moderation:
  - `backend/src/Blinder.Api/Features/Photos/`
  - `backend/src/Blinder.Api/Features/Moderation/`
  - `backend/src/Blinder.AdminPanel/Pages/Moderation/`
- Staff operations:
  - `backend/src/Blinder.Api/Features/Admin/`
  - `backend/src/Blinder.AdminPanel/Pages/Users/`
  - `backend/src/Blinder.AdminPanel/Pages/Reports/`
  - `backend/src/Blinder.AdminPanel/Pages/Appeals/`
  - `backend/src/Blinder.AdminPanel/Pages/Support/`

**Cross-Cutting Concerns:**
- Authentication and federation:
  - `backend/src/Blinder.IdentityServer/`
  - `backend/src/Blinder.AdminPanel/Services/Auth/`
  - `mobile/blinder-app/src/services/auth/`
- Real-time communication:
  - `backend/src/Blinder.Api/Realtime/`
  - `mobile/blinder-app/src/services/realtime/`
- Object storage / photo pipeline:
  - `backend/src/Blinder.Api/Integrations/Minio/`
  - `deploy/hetzner/`
- Deployment and containerization:
  - `deploy/hetzner/`
  - root `docker-compose.yml`
- Architecture governance and docs:
  - `docs/architecture/`

### Integration Points

**Internal Communication:**
- Mobile app communicates with Api over HTTPS and SignalR.
- AdminPanel communicates with Api over HTTPS using access tokens from IdentityServer.
- Api communicates with IdentityServer for identity-owned commands only.
- Api communicates with PostgreSQL, MinIO, push providers, and telemetry infrastructure.
- IdentityServer communicates with PostgreSQL and social providers.

**External Integrations:**
- Google / Apple / Facebook login integration through IdentityServer federation
- APNS / FCM via Api notification integration layer
- MinIO object storage for photos
- Traefik ingress/reverse proxy in deployment layer
- Registry-driven image deployment in CI/CD

**Data Flow:**
- Authentication flow:
  - Mobile/AdminPanel → IdentityServer → tokens → Api
- Business flow:
  - Client → Api → `app.*` schema / integrations
- Identity mutation flow:
  - Admin workflow in Api → explicit IdentityServer command → `identity.*`
- Real-time flow:
  - committed domain state in Api → SignalR hub notification → client reconciliation read

### File Organization Patterns

**Configuration Files:**
- Each app owns its `appsettings*.json`
- Deployment-specific environment examples live under `deploy/`
- Shared build/package policies live under backend root where appropriate
- Secrets never live in repo-tracked production config files

**Source Organization:**
- Backend source under `backend/src/`
- Backend tests under `backend/tests/`
- Mobile source under `mobile/blinder-app/src/`
- Deployment manifests separated from app source under `deploy/`

**Test Organization:**
- Unit tests per backend app in dedicated test projects
- Cross-app integration tests in `backend/tests/Blinder.IntegrationTests/`
- Architecture tests for boundary enforcement in `backend/tests/Blinder.ArchitectureTests/`
- Mobile test layers split into unit, integration, and e2e

**Asset Organization:**
- Mobile static assets in `mobile/blinder-app/assets/`
- AdminPanel static assets in `backend/src/Blinder.AdminPanel/wwwroot/`
- Deployment assets and scripts under `deploy/`
- Documentation artifacts under `docs/`

### Development Workflow Integration

**Development Server Structure:**
- Developers can run backend apps independently or together from `backend/`
- Mobile app runs independently from `mobile/blinder-app/`
- Local infrastructure can be started from compose definitions without requiring production deploy structure

**Build Process Structure:**
- CI builds separate backend app artifacts/images
- Mobile CI builds the Expo app independently
- Container build context remains clean because app boundaries are explicit
- Deployment pipeline references image tags, not source copies on server

**Deployment Structure:**
- Production deployment is driven from `deploy/hetzner/`
- Compose references separate images for `identityserver`, `api`, and `adminpanel`
- Stateful service data paths and Traefik config are isolated from source code
- The structure supports current single-node Compose deployment and later migration to a stronger orchestrator without changing service boundaries
- Sensitive moderation/support access must be fully auditable.

### API & Communication Patterns

**Primary API style**
- Use RESTful JSON APIs as the system default.
- Rationale: predictable, debuggable, well supported in mobile and admin clients, and a better fit than GraphQL for trust-sensitive workflow control and explicit authorization.

**API structure**
- Single Api application exposes:
  - mobile client endpoints,
  - admin endpoints,
  - internal business workflow surfaces.
- Separate route groups and authorization policies must distinguish mobile and admin surfaces.

**Documentation**
- Use built-in ASP.NET Core OpenAPI support in .NET 10.
- Generate OpenAPI 3.1 documents.
- Enable XML documentation comments for public endpoints and DTOs used in generated docs.

**Error handling**
- Standardize on RFC 9457 Problem Details responses.
- Centralize exception handling and avoid leaking implementation details.
- Validation failures, authorization failures, and workflow-rule violations should map to stable, documented error shapes.

**Real-time communication**
- Use ASP.NET Core SignalR for live conversation updates and decision-gate state transitions where near-real-time UX matters.
- Do not make SignalR the sole source of truth.
- All trust-critical state remains server-authoritative and recoverable via normal API reads after reconnect or app restart.
- A SignalR backplane is not required for a single `api` instance.
- If `api` is scaled horizontally, introduce Redis as the SignalR backplane.

**Rate limiting**
- Apply stricter rate limits to authentication, onboarding, messaging, moderation/reporting, and upload-related endpoints.
- Admin endpoints should have tighter authorization, but not necessarily the same rate policies as public mobile endpoints.

### Frontend Architecture

**Mobile application**
- Continue with Expo SDK 55 foundation and Tamagui Expo Router starter.
- Preserve the single-focus navigation model and domain-first module structure.

**State management**
- Keep state management lean and domain-oriented.
- Recommended split:
  - server state through explicit API queries and synchronization,
  - minimal client state for session/UI flow,
  - avoid introducing heavyweight client-state frameworks unless real complexity proves necessary.
- The core architectural rule is that trust-critical workflow state is authoritative on the server.

**AdminPanel**
- Use Razor Pages on .NET 10.
- Treat AdminPanel as a server-rendered staff client, not as a second business backend.
- AdminPanel authenticates with IdentityServer and performs all business/admin actions via Api.

### Infrastructure & Deployment

**Initial production deployment model**
- Use Docker Compose as the initial production deployment format.
- Deploy the solution as a set of immutable container images pulled onto the target system and started via `docker compose up -d`.
- This choice optimizes for repeatable deployments, simple rollback, operational clarity, and vertical scaling on a single VPS.

**Container boundaries**
- Run separate containers for:
  - `traefik`
  - `identityserver`
  - `api`
  - `adminpanel`
  - `postgres`
  - `minio`
- `redis` is optional initially and planned for introduction when horizontal scaling or distributed coordination needs appear.

**Network and exposure model**
- Only Traefik exposes public ports `80/443`.
- All other services communicate on an internal Docker network.
- Host-based routing should separate:
  - `api.<domain>`
  - `auth.<domain>`
  - `admin.<domain>`
  - storage endpoints as needed, without exposing internal consoles unnecessarily.

**State and persistence rules**
- `identityserver`, `api`, and `adminpanel` must remain stateless containers.
- Persistent storage is limited to stateful infrastructure services and persisted application key material.
- PostgreSQL and MinIO use persistent volumes.
- ASP.NET Core Data Protection keys must be persisted outside ephemeral container filesystems.
- Identity signing and encryption keys must be persisted and shared appropriately across deployments and restarts.

**Scalability posture**
- Docker Compose on a single VPS is the MVP production architecture.
- It supports clean packaging, isolation, and vertical scaling, but does not by itself provide multi-node high availability.
- Application-level scaling is expected before infrastructure-level HA:
  - `api` can scale first,
  - `adminpanel` can scale if needed,
  - `identityserver` can scale later if load requires it.
- Stateful services remain the harder scaling boundary and should be treated conservatively.

**Future scale path**
- Treat Docker Compose as the initial deployment format, not the permanent orchestration ceiling.
- If product growth demands it, the same containerized services can later move to a stronger orchestrator or multi-node topology without changing the logical service split.
- Redis is the expected first addition when introducing:
  - SignalR horizontal scale,
  - distributed cache,
  - short-lived cross-instance coordination.

**Operational requirements**
- Every app service must expose health checks.
- Images should be built in CI and pushed to a registry, never built on the production VPS.
- Deployments should pull tagged images and restart services with minimal impact.
- Rollback should be performed by redeploying a previous image tag.

### Decision Impact Analysis

**Implementation Sequence:**
1. Initialize IdentityServer on ASP.NET Core 10 with OpenIddict 7.4.0.
2. Initialize Api on ASP.NET Core 10 with EF Core 10 and Npgsql 10.0.1.
3. Establish single PostgreSQL database with `identity.*` and `app.*` schemas and separate DB permissions.
4. Initialize AdminPanel as Razor Pages application using OIDC against IdentityServer.
5. Containerize `identityserver`, `api`, and `adminpanel` as separate images.
6. Define Docker Compose production stack with Traefik, PostgreSQL, MinIO, and internal networking.
7. Persist signing keys, encryption keys, and Data Protection keys outside ephemeral containers.
8. Implement shared auth flows and token validation between IdentityServer and Api.
9. Implement Api route separation for mobile and admin surfaces.
10. Implement trust-critical business domains: onboarding, matching, conversation, gate, reveal, moderation.
11. Add SignalR where live updates materially improve UX without becoming the source of truth.
12. Add Redis when horizontal scaling or distributed coordination justifies it.
13. Add observability, audit logging, and deployment hardening.

**Cross-Component Dependencies:**
- IdentityServer choices constrain Api token validation and AdminPanel sign-in flow.
- Single-database ownership rules constrain migration strategy, DB users, and schema design.
- AdminPanel design depends on Api authorization model and admin route organization.
- SignalR design depends on Api workflow resolution, not the other way around.
- Auditability requirements affect IdentityServer, Api, AdminPanel, and schema design simultaneously.

## Implementation Patterns & Consistency Rules

### Pattern Categories Defined

**Critical Conflict Points Identified:**
12 major areas where AI agents could make incompatible choices:
- database schema ownership and naming
- migration ownership
- API route naming
- DTO and JSON field naming
- admin vs mobile endpoint separation
- identity vs business mutation boundaries
- error response format
- date/time handling
- event naming and payload contracts
- SignalR usage patterns
- project/module organization
- container/configuration conventions

### Naming Patterns

**Database Naming Conventions:**
- Schemas use snake_case and explicit ownership:
  - `identity`
  - `app`
- Tables use snake_case plural nouns:
  - `identity.users`
  - `app.matches`
  - `app.conversations`
- Columns use snake_case:
  - `user_id`
  - `created_at`
  - `decision_state`
- Foreign keys use the referenced entity name plus `_id`:
  - `user_id`
  - `conversation_id`
  - `match_id`
- Indexes use `ix_<table>_<column_list>`:
  - `ix_users_email`
  - `ix_conversations_match_id_created_at`

**API Naming Conventions:**
- REST endpoints use plural nouns for collections:
  - `/api/v1/users`
  - `/api/v1/conversations`
  - `/api/v1/matches`
- Route segments use kebab-case only when multi-word:
  - `/api/v1/decision-gates`
  - `/api/v1/admin/photo-reviews`
- Route parameters use `{id}` style:
  - `/api/v1/conversations/{conversationId}`
- Query parameters use camelCase:
  - `?pageSize=20&beforeCursor=...`
- Admin API routes are always prefixed with `/api/v1/admin/...`
- Identity endpoints are never implemented inside Api unless they are pure business-facing projections

**Code Naming Conventions:**
- C# types, public members, Razor Page models, DTO classes: PascalCase
- C# local variables and parameters: camelCase
- Private fields: `_camelCase`
- TypeScript/Expo components and files for components: PascalCase
  - `ConversationScreen.tsx`
  - `DecisionGateCard.tsx`
- TypeScript hooks, utilities, stores, helpers: camelCase or kebab-case by existing local convention, but choose one style per folder and do not mix
- Domain folders use kebab-case in frontend and PascalCase-neutral feature folder names in backend only if already aligned to project structure; otherwise prefer clear feature names over framework defaults

### Structure Patterns

**Project Organization:**
- Organize by feature/domain first, not by technical layer alone.
- Backend applications:
  - `IdentityServer`: identity, federation, token issuance, account/session concerns
  - `Api`: onboarding, matching, conversations, gate/reveal, moderation, notifications, support
  - `AdminPanel`: Razor Pages grouped by staff workflows, not by database tables
- Mobile app:
  - organize around lifecycle domains:
    - onboarding
    - waiting
    - match-entry
    - conversation
    - decision-gate
    - reveal
    - ending
    - settings

**File Structure Patterns:**
- Tests are co-located or mirrored consistently per project, but one project must not mix incompatible test layouts arbitrarily.
- Backend recommendation:
  - production code under feature folders
  - tests in dedicated test projects mirroring feature namespaces
- Shared contracts that cross app boundaries belong in explicit shared libraries only when truly shared; do not create premature `common` dumping grounds.
- Configuration files stay close to their owning application.
- Container and deployment configuration lives outside application source folders in deployment-focused directories.

### Format Patterns

**API Response Formats:**
- Success responses return direct resource payloads or explicit result DTOs, not generic `{ data, error }` wrappers.
- List endpoints use explicit paged/result envelopes when needed:
  - `items`
  - `nextCursor`
  - `hasMore`
- Command endpoints may return explicit workflow result DTOs when state transitions matter.

**Error Response Structure:**
- All API errors use Problem Details (`application/problem+json`) with stable extension fields where needed.
- Validation errors, domain-rule failures, authorization failures, and not-found errors must preserve a consistent shape.
- Do not invent custom per-endpoint error objects.

**Data Exchange Formats:**
- Public JSON fields use camelCase.
- IDs remain opaque strings or typed identifiers as chosen per domain; never expose internal implementation assumptions unnecessarily.
- Dates/times use ISO 8601 UTC timestamps in API payloads.
- Do not send local server time in contracts.
- Boolean values remain booleans, never `0/1` in JSON.

### Communication Patterns

**Event System Patterns:**
- Internal domain events use past-tense PascalCase names in code:
  - `ConversationStarted`
  - `DecisionGateReached`
  - `PhotoApproved`
- If events are serialized externally, use stable event names and version fields explicitly.
- Event payloads must contain:
  - event name
  - event version
  - entity identifier
  - occurred-at timestamp
  - minimal business-relevant payload
- Do not expose full aggregate snapshots by default.

**SignalR / Real-Time Patterns:**
- SignalR is for UX synchronization, not business truth.
- Hub messages must reflect already-committed server state.
- Every hub event must have a corresponding API read model so clients can recover after reconnect or missed delivery.
- Client code must treat hub messages as hints to refresh or reconcile, not as sole authoritative state transitions.

**State Management Patterns:**
- Server owns trust-critical workflow state.
- Client state is limited to UI/session/transient interaction concerns.
- State updates must be immutable in frontend code.
- Domain stores/selectors should be organized by feature, not by primitive type.

### Process Patterns

**Error Handling Patterns:**
- Backend:
  - global exception handling middleware
  - explicit mapping for domain exceptions to Problem Details
  - no raw exception leakage
- Frontend:
  - distinguish blocking errors, retryable errors, and empty states
  - never reuse the same UI treatment for all failures
- AdminPanel:
  - staff-facing errors may be more explicit than mobile-user errors, but must still avoid leaking unsafe internal details

**Loading State Patterns:**
- Use explicit loading states with domain meaning:
  - `isLoadingConversation`
  - `isSubmittingDecision`
  - `isRefreshingMatch`
- Avoid ambiguous global booleans like `isLoading` when multiple concurrent flows exist.
- Long-running operations affecting trust-sensitive steps should always expose deterministic pending UI.

**Validation Patterns:**
- Validate at three layers:
  - client for immediacy
  - API boundary for request correctness
  - domain/application layer for business invariants
- Client validation never replaces server validation.
- IdentityServer validates identity concerns; Api validates business concerns.

**Authorization Process Patterns:**
- Identity mutations can only originate from IdentityServer-owned handlers/services.
- Api may request identity-side actions through explicit service calls or integration commands, never direct writes.
- AdminPanel never bypasses Api for moderation or business operations.

### Enforcement Guidelines

**All AI Agents MUST:**
- Respect schema ownership: `identity.*` is not writable by Api code.
- Use Problem Details for API error responses.
- Use camelCase JSON contracts and ISO 8601 UTC timestamps.
- Keep SignalR non-authoritative and backed by API recovery paths.
- Place admin endpoints under explicit admin route groups and policies.
- Preserve domain-first organization instead of scattering logic into generic utility folders.
- Persist container-independent key material outside ephemeral filesystems.

**Pattern Enforcement:**
- PRs and generated code should be checked for naming, route shape, error format, and schema ownership consistency.
- Violations should be corrected in the implementation PR, not documented as `follow-up`.
- If a pattern needs to change, update the architecture document first, then implement against the revised rule.

### Pattern Examples

**Good Examples:**
- `POST /api/v1/conversations/{conversationId}/decision`
- `app.conversations.decision_state`
- `ConversationDecisionRequest`
- `ConversationDecisionResult`
- Problem Details response with stable `type`, `title`, `status`, and domain extension fields
- SignalR event `DecisionGateReached` followed by client reconciliation call to fetch current conversation state

**Anti-Patterns:**
- Api writing directly into `identity.users`
- Mixed JSON casing across endpoints
- Returning `{ success: false, message: "..." }` on one endpoint and Problem Details on another
- Using SignalR messages as the only way clients learn reveal/gate outcomes
- AdminPanel pages calling the database directly for moderation actions
- Starter-template folder structures left unrefined and treated as final architecture

## Architecture Validation Results

### Coherence Validation ✅

**Decision Compatibility:**
The selected technologies and boundaries are compatible.

- .NET 10 / ASP.NET Core 10, OpenIddict 7.4.0, EF Core 10, Npgsql 10.0.1, and PostgreSQL 18 are aligned.
- The backend split into IdentityServer, Api, and AdminPanel is coherent with the chosen security and deployment model.
- The single-database approach is compatible with the ownership rules because schema-level write boundaries are explicitly defined.
- Docker Compose deployment aligns with the modular service split and does not contradict future scaling plans.
- SignalR usage is constrained appropriately as a UX synchronization layer rather than a source of truth.

No critical decision-level contradictions were found.

**Pattern Consistency:**
The implementation patterns support the architectural decisions.

- Schema ownership rules align with service boundaries.
- API naming, Problem Details usage, and JSON conventions align with the REST-based Api model.
- SignalR consistency rules align with the decision to keep business state server-authoritative.
- AdminPanel patterns correctly reinforce "staff client over Api" instead of "second backend."
- Container patterns align with the deployment decision and future horizontal scaling path.

**Structure Alignment:**
The project structure supports the architecture well.

- Backend apps are physically separated in a way that reflects logical ownership.
- Api feature folders match the product lifecycle and trust-critical workflow model.
- Mobile structure reflects the UX lifecycle rather than generic app boilerplate.
- Deployment structure is separated cleanly from application source.
- Tests and contracts are positioned in a way that can support cross-boundary enforcement.

### Requirements Coverage Validation ✅

**Feature Coverage:**
The architecture supports all major functional areas described in the PRD and UX design:

- onboarding and policy acceptance
- matching
- blind conversation
- decision gate and reveal
- dignified non-mutual endings
- photo moderation pipeline
- notifications
- support/admin workflows
- operational auditability

Each of these areas has an architectural home in the Api, IdentityServer, AdminPanel, or mobile structure.

**Functional Requirements Coverage:**
All major FR categories are architecturally supported.

- The conversation-first loop is supported by the Api feature structure and mobile lifecycle structure.
- The moderation pipeline is supported by MinIO integration, Moderation features, and AdminPanel workflows.
- Identity and federation needs are supported by the IdentityServer split and OpenIddict choice.
- Admin and support operations are supported without breaking business-boundary rules.
- Real-time conversation needs are supported by SignalR with recovery-through-API patterns.

**Non-Functional Requirements Coverage:**
The architecture addresses the core NFRs effectively.

- Reliability: server-authoritative workflow state, explicit boundaries, health checks, and containerized deployment
- Privacy: pre-reveal access control, admin/API separation, identity/business ownership separation
- Security: first-party identity authority, OIDC/OAuth2, policy-based admin access, explicit identity mutation ownership
- Scalability: stateless app containers, future Redis introduction, modular service split
- Compliance/supportability: auditability, moderation tooling, support workflow structure, deletion/account lifecycle support
- UX integrity: server-authoritative state plus lifecycle-aware mobile structure and real-time reconciliation

### Implementation Readiness Validation ✅

**Decision Completeness:**
The architecture is sufficiently specified for implementation to begin.

- Critical backend and data decisions are documented.
- Deployment and service boundaries are documented.
- Identity and authorization responsibilities are explicit.
- Key versions are identified for the main backend technologies.

**Structure Completeness:**
The project structure is concrete enough for multiple agents to implement against consistently.

- Backend apps are clearly separated.
- Feature slices are defined.
- Deployment folders are defined.
- Mobile structure is defined.
- Test locations and shared-library intent are documented.

**Pattern Completeness:**
The main conflict points for AI implementation have been addressed.

- naming rules
- response formats
- state and SignalR patterns
- schema ownership
- admin/API boundaries
- containerization expectations

This should materially reduce inconsistent implementation choices across agents.

### Gap Analysis Results

**Critical Gaps:** none identified.

**Important Gaps:**
- IdentityServer internal structure is defined, but some operational identity decisions are still intentionally open:
  - passkeys/MFA delivery timing
  - exact signing key storage mechanism
  - exact admin scope/role matrix
- Api internal application style is implied by feature slicing, but not yet locked more formally:
  - for example whether handlers/services use MediatR-style request handling or a lighter internal application service pattern
- Mobile state-management library choice is intentionally still light-touch and may need to be made explicitly once implementation begins.

**Nice-to-Have Gaps:**
- Explicit ADR-style records for a few sensitive decisions could help future changes:
  - single database ownership model
  - SignalR usage constraints
  - identity mutation boundary
- A dedicated deployment runbook and secrets management guide would improve operational readiness.
- A more explicit admin authorization matrix would help staff tooling implementation later.

### Validation Issues Addressed

The architecture is strong enough to proceed without blocking issues.

One minor documentation issue remained:
- the workflow frontmatter step list needed to be synchronized with the current completed step count before final completion.

This has now been corrected.

### Architecture Completeness Checklist

**✅ Requirements Analysis**
- [x] Project context thoroughly analyzed
- [x] Scale and complexity assessed
- [x] Technical constraints identified
- [x] Cross-cutting concerns mapped

**✅ Architectural Decisions**
- [x] Critical decisions documented with versions
- [x] Technology stack fully specified
- [x] Integration patterns defined
- [x] Performance and scaling posture addressed

**✅ Implementation Patterns**
- [x] Naming conventions established
- [x] Structure patterns defined
- [x] Communication patterns specified
- [x] Process patterns documented

**✅ Project Structure**
- [x] Complete directory structure defined
- [x] Component boundaries established
- [x] Integration points mapped
- [x] Requirements to structure mapping complete

### Architecture Readiness Assessment

**Overall Status:** READY FOR IMPLEMENTATION

**Confidence Level:** High

**Key Strengths:**
- Clear backend responsibility split
- Explicit single-database ownership model
- Strong alignment between UX lifecycle and mobile structure
- Good protection against multi-agent implementation drift
- Realistic deployment model with a credible future scaling path
- Strong trust-and-safety alignment across architecture and operations

**Areas for Future Enhancement:**
- formalize admin authorization matrix
- formalize secrets/key management runbook
- add a few focused ADRs for sensitive boundary decisions
- lock down a final internal application pattern for Api if needed during implementation

### Implementation Handoff

**AI Agent Guidelines:**
- Follow all architectural decisions exactly as documented.
- Use implementation patterns consistently across all components.
- Respect project structure and ownership boundaries.
- Treat this document as the source of truth for architectural questions.

**First Implementation Priority:**
1. Initialize the backend solution skeleton with `Blinder.IdentityServer`, `Blinder.Api`, `Blinder.AdminPanel`, shared projects, and test projects.
2. Initialize the mobile app from the Tamagui Expo Router starter and immediately refactor it into the documented domain-first structure.
3. Define the first Docker Compose deployment skeleton with Traefik, PostgreSQL, MinIO, and placeholders for `identityserver`, `api`, and `adminpanel`.
