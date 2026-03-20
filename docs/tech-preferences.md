# Blinder — Tech Preferences & Best Practices

> **Purpose:** This file is loaded by BMAD Dev, QA, and Architect agents as authoritative context for all implementation decisions. Every rule here overrides agent defaults. No substitutions without an explicit architecture update.

---

## 1. Repository & Project Structure

```
blinder/
├── backend/
│   ├── Blinder.sln
│   ├── Blinder.Api/
│   │   ├── Controllers/        # One controller per domain resource
│   │   ├── Hubs/               # SignalR hubs only
│   │   ├── Pages/              # Razor Pages admin (/admin/**)
│   │   ├── Models/             # EF Core entity classes
│   │   ├── DTOs/               # Request and response DTOs
│   │   ├── Services/           # Business logic (*Service interface + impl)
│   │   ├── Infrastructure/     # All external integrations (*Client classes)
│   │   │   ├── Auth/           # SocialLoginHandler
│   │   │   ├── Data/           # AppDbContext
│   │   │   ├── Email/          # SmtpSettings, mailer
│   │   │   ├── Scanning/       # ContentScanningClient, NCMEC slot
│   │   │   └── Storage/        # S3ClientFactory
│   │   ├── Repositories/       # Data access (if needed beyond EF directly)
│   │   ├── Mappings/           # Mapperly mapper partial classes
│   │   ├── Validators/         # FluentValidation validator classes
│   │   ├── BackgroundJobs/     # Coravel IInvocable job classes
│   │   ├── Errors/             # AppErrors.cs — single source for problem codes
│   │   └── Migrations/         # EF Core migrations (committed to source control)
│   ├── Blinder.Tests/
│   │   ├── Controllers/
│   │   ├── Services/
│   │   ├── Validators/
│   │   └── Integration/
│   └── Dockerfile              # Multi-stage: SDK build → ASP.NET runtime
└── mobile/
    ├── app/
    │   ├── (auth)/             # Unauthenticated screens
    │   └── (tabs)/             # Authenticated shell
    ├── components/             # Shared UI (PascalCase files)
    │   ├── shared/             # AccessiblePressable.tsx, ThemedText.tsx (base a11y components)
    │   └── moderation/         # ReportButton.tsx, BlockConfirmation.tsx
    ├── contexts/               # React contexts — AccessibilityContext (app root)
    ├── hooks/                  # Custom hooks (camelCase, use prefix); useAccessibility re-exports context
    ├── services/               # apiClient, signalrService, storageService
    ├── constants/              # UPPER_SNAKE_CASE values, theme tokens
    ├── types/
    │   ├── api/                # Response/request types matching backend DTOs
    │   └── signalr/            # Hub method names, payload types, enums
    └── utils/                  # Pure utility functions (no side effects)
```

**Boundary rules — never violate:**
- `Services/` = domain orchestration only. No HTTP calls, no S3, no SMTP.
- `Infrastructure/` = all external integrations. Named `*Client`, not `*Service`.
- `BackgroundJobs/` = Coravel `IInvocable` only.
- `Errors/AppErrors.cs` = single source of truth for RFC 7807 problem type URIs. Never define error strings inline.
- Auth logic is shared: scaffolded Identity Razor PageModels and mobile auth API endpoints must call the same Identity-backed registration/login rules; mobile app must not render scaffolded Razor pages.
- `types/api/` and `types/signalr/` are separate namespaces — drift between them causes silent bugs.
- `invite-landing.tsx` is mandatory — never deep-link directly to `register.tsx` (breaks invite lineage tracking).
- `ReportButton.tsx` and `BlockConfirmation.tsx` defined once in `components/moderation/` — never reimplemented inline.

---

## 2. Technology Stack (Version-Pinned)

### Backend

| Layer | Technology | Version |
|---|---|---|
| Runtime | .NET | 10 LTS |
| Language | C# | 14 |
| Framework | ASP.NET Core (controllers, not minimal APIs) | 10 |
| ORM | Entity Framework Core + Npgsql | 10 / 9.x |
| Spatial | NetTopologySuite + PostGIS | via Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite |
| Auth | ASP.NET Core Identity + JWT Bearer | built-in |
| Validation | FluentValidation | latest stable |
| Mapping | Mapperly (source generator) | latest stable |
| Background jobs | Coravel | latest stable |
| Real-time | Microsoft SignalR | built-in |
| Push (Android) | FirebaseAdmin (.NET) | latest stable |
| Push (iOS) | dotAPNs | latest stable |
| Object storage | AWSSDK.S3 (ForcePathStyle=true for Hetzner) | latest stable |
| Logging | Serilog + Serilog.AspNetCore | latest stable |
| Error format | RFC 7807 Problem Details (AddProblemDetails()) | built-in |
| Admin UI | Razor Pages + cookie auth | built-in |
| Database | PostgreSQL + PostGIS | 16+ / postgis/postgis Docker image |
| Analytics | Custom DB-backed dashboards | PostgreSQL queries |
| Containerisation | Docker Compose | latest stable |

### Mobile

| Layer | Technology | Version |
|---|---|---|
| Framework | Expo | SDK 55 |
| Language | TypeScript (strict: true) | 5.x |
| Navigation | Expo Router (file-based) | SDK 55 bundled |
| Styling | NativeWind (Tailwind utility classes) | latest stable |
| Real-time | @microsoft/signalr | latest stable |
| Token storage | expo-secure-store | SDK 55 bundled |
| Push tokens | expo-notifications | SDK 55 bundled |
| Photo picker | expo-image-picker | SDK 55 bundled |
| Build/deploy | EAS Build | free tier |

---

## 3. Backend — Language & Code Rules

### 3.1 Async / Await

- **All** I/O operations must be async: `async Task<T>`, never `.Result` or `.Wait()`
- Use `ConfigureAwait(false)` in library/infrastructure code where there is no ambient synchronization context
- Use `CancellationToken` on all controller actions and service methods that perform I/O — propagate it down the call chain
- Prefer `ValueTask<T>` over `Task<T>` for hot paths that frequently complete synchronously (e.g. cache hits)
- Never use `async void` — only exception is event handlers

```csharp
// CORRECT
public async Task<ConversationDto> GetConversationAsync(
    Guid conversationId, CancellationToken ct)
{
    var conv = await _context.Conversations
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == conversationId, ct);
    return conv is null
        ? throw new ConversationNotFoundException(conversationId)
        : _mapper.Map(conv);
}

// PROHIBITED
public ConversationDto GetConversation(Guid id) =>
    _context.Conversations.First(c => c.Id == id); // blocking, no token
```

### 3.2 Nullable Reference Types

- `<Nullable>enable</Nullable>` in all `.csproj` files from day one — retrofitting is costly
- All properties with possible null must be explicitly `T?`
- Use `required` keyword on non-nullable properties that must be set at construction
- Never use `!` (null-forgiving operator) without a comment explaining why it's safe

### 3.3 DateTime Handling

- **`DateTimeOffset` everywhere in C#** — `DateTime` is prohibited in new code
- **`timestamptz` in all PostgreSQL columns** — never `timestamp without time zone`
- **ISO 8601 strings in all API responses** — `"2026-03-11T14:00:00Z"` — never Unix timestamps
- All comparisons and storage in UTC

```csharp
// CORRECT
public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

// PROHIBITED
public DateTime CreatedAt { get; set; } = DateTime.Now; // local time, wrong type
```

### 3.4 Records and Immutability

- Use `record` types for DTOs and value objects — they are immutable by default and provide structural equality
- Entities (EF Core models) remain `class` — EF requires mutable properties
- Use `init` setters on entity properties that should only be set during construction

### 3.5 Pattern Matching & Modern C#

- Prefer `switch` expressions over `switch` statements for exhaustive matching
- Use `is` pattern matching for null checks and type checks
- Use primary constructors (C# 12+) for services — reduces boilerplate

```csharp
// CORRECT — primary constructor
public class MatchService(AppDbContext context, ILogger<MatchService> logger)
    : IMatchService
{
    public async Task<MatchDto?> FindMatchAsync(Guid userId, CancellationToken ct)
    {
        var user = await context.Users.FindAsync([userId], ct)
            ?? throw new UserNotFoundException(userId);
        // ...
    }
}
```

---

## 4. Backend — Architecture Patterns

### 4.1 Controllers

- Thin controllers only — no business logic in controllers
- Controllers call one service method and return the result
- Return `IActionResult` or typed `ActionResult<T>` — never raw objects from controllers
- Route prefix: `[Route("api/[controller]")]` on all API controllers
- Admin controllers under `Pages/` (Razor Pages), never `Controllers/`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationsController(IConversationService conversationService) : ControllerBase
{
    [HttpGet("{conversationId:guid}")]
    [ProducesResponseType<ConversationDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDto>> GetAsync(
        Guid conversationId, CancellationToken ct)
    {
        var dto = await conversationService.GetAsync(conversationId, User.GetUserId(), ct);
        return dto is null ? NotFound() : Ok(dto);
    }
}
```

### 4.2 Services

- One interface + one implementation per service, both in `Services/`
- Service names: `IConversationService` / `ConversationService`
- Services orchestrate domain logic and call repositories or `Infrastructure/*Client` classes
- Services never import `HttpContext` or anything from `Microsoft.AspNetCore.Http` — pass required data as parameters
- Services never call other services directly where it creates circular dependencies — use domain events or background jobs

### 4.3 Validation (FluentValidation)

- Every endpoint that accepts a request body has a corresponding `AbstractValidator<TRequest>` in `Validators/`
- Register via `AddValidatorsFromAssemblyContaining<Program>()` — never manually
- No inline validation in controllers or services
- Validation errors automatically produce RFC 7807 400 responses via FluentValidation ASP.NET Core integration
- Use `RuleForEach` for collection validation — never loop manually

```csharp
public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(2000)
            .WithMessage("Message must not exceed 2000 characters");
    }
}
```

### 4.4 Mapping (Mapperly)

- All Entity ↔ DTO conversions through Mapperly `[Mapper]` partial classes in `Mappings/`
- One mapper class per domain area: `UserMapper`, `ConversationMapper`, `RevealMapper`
- Set `RespectNullableAnnotations = true` — unmapped nullable properties are compile-time warnings treated as errors
- No manual `new DTO { Property = entity.Property }` outside Mapperly — ever

```csharp
[Mapper(RespectNullableAnnotations = true)]
public partial class ConversationMapper
{
    public partial ConversationDto Map(Conversation source);
    public partial IReadOnlyList<ConversationDto> Map(IReadOnlyList<Conversation> source);
}
```

### 4.5 Error Handling

- Global exception handler middleware catches all unhandled exceptions → Problem Details 500
- Business rule violations: throw typed domain exceptions (e.g., `RevealThresholdNotMetException`) mapped to 422
- All exceptions logged via Serilog before response is returned
- Never expose stack traces in responses — ever
- `AppErrors.cs` is the single source of problem type URI strings

```csharp
// AppErrors.cs
public static class AppErrors
{
    public const string RevealThresholdNotMet =
        "https://blinder.app/errors/reveal-threshold-not-met";
    public const string ConversationLimitReached =
        "https://blinder.app/errors/conversation-limit-reached";
    // ...
}
```

### 4.6 HTTP Status Code Rules

| Code | When |
|---|---|
| 200 | Successful GET or action with response body |
| 201 | Resource created (POST) — include `Location` header |
| 204 | Successful action with no response body (DELETE, state update) |
| 400 | Validation failure or malformed request |
| 401 | Unauthenticated |
| 403 | Authenticated but not authorised |
| 404 | Resource not found |
| 409 | Conflict (duplicate registration, reveal already triggered) |
| 422 | Business rule violation (distinct from validation) |
| 429 | Rate limit exceeded — always include `Retry-After` header |
| 500 | Unhandled server error |

---

## 5. Database (PostgreSQL + EF Core)

### 5.1 Schema Conventions

- `UseSnakeCaseNamingConvention()` in `OnModelCreating` — all tables and columns are `snake_case`
- `UseNetTopologySuite()` for PostGIS spatial types
- Primary keys: `id` (always, no exceptions)
- Foreign keys: `{singular_table}_id` pattern
- Index names: `ix_{table}_{columns}`
- All timestamp columns: `timestamptz` (UTC)
- Soft deletes: `deleted_at timestamptz NULL` — never physical deletes for user data except GDPR erasure

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.UseSnakeCaseNamingConvention();
    // All entity configs here...
}
```

### 5.2 EF Core Best Practices

- **`AsNoTracking()`** on all read-only queries — never track entities you won't modify
- **Always project with `Select()`** for list endpoints — never fetch full entity graphs when a DTO subset suffices
- **No lazy loading** — prohibited. Use explicit `.Include()` or separate queries
- **`AsSplitQuery()`** for queries with multiple collection includes to avoid cartesian explosion
- **`ExecuteUpdateAsync()` / `ExecuteDeleteAsync()`** for bulk operations — never load entities just to update/delete them
- **Compiled queries** for hot paths (e.g., message count lookup, reveal flag check):

```csharp
private static readonly Func<AppDbContext, Guid, Task<int>> GetMessageCountQuery =
    EF.CompileAsyncQuery((AppDbContext ctx, Guid conversationId) =>
        ctx.Messages.Count(m => m.ConversationId == conversationId));
```

- **Pagination** with `Skip()` + `Take()` — never load full result sets
- Use `DbContext` pooling (`AddDbContextPool<T>`) for production performance
- `IDbContextFactory<AppDbContext>` for background jobs (Coravel) — never inject `AppDbContext` directly into singletons

### 5.3 Migrations

- Code-first migrations only — EF CLI tools (`dotnet-ef`) available in SDK build stage only
- **Never** `Database.MigrateAsync()` on startup — explicitly prohibited
- Migration workflow:
  ```bash
  # Add migration (dev environment, SDK stage)
  docker compose run --rm api dotnet ef migrations add <Name>
  
  # Generate idempotent SQL script
  dotnet ef migrations script --idempotent --output migrations/latest.sql
  
  # Apply in production
  docker compose exec -T db psql -U $POSTGRES_USER -d $POSTGRES_DB < migrations/latest.sql
  ```
- Commit `migrations/latest.sql` to the repository — it is the production deployment artefact
- Name migrations descriptively: `AddRevealReadyFlagToConversations`, not `Migration1`

### 5.4 Indexing Strategy

- Index every foreign key column by default
- Composite indexes for common query patterns (e.g., `(user_id, created_at)` on messages)
- PostGIS spatial index on `location` column (radius matching): `HasIndex(u => u.Location).HasMethod("GIST")`
- Partial indexes for common filtered queries (e.g., active conversations only)
- Review `EXPLAIN ANALYZE` output for any query serving NFR-critical paths before merging

### 5.5 PostgreSQL-Specific Practices

- Use `JSONB` for flexible semi-structured data (quiz answers, match metadata)
- Use `uuid` type for all IDs — `Guid` in C#, `uuid` in PostgreSQL — never `int` auto-increment for user-facing IDs
- Use `COPY` command via `BinaryImporter` for any bulk insert operation (matching job)
- Connection string: always set `Pooling=true` (default) and tune `MaxPoolSize` based on VPS memory
- Principle of least privilege: application user has `SELECT/INSERT/UPDATE/DELETE` only — schema migrations use a separate `migration_user` with DDL rights

---

## 6. Authentication & Security

### 6.1 JWT Configuration

- JWT tokens issued by backend — no external identity provider
- Token expiry: 30 days of inactivity (sliding window via refresh token pattern)
- Validate: `ValidateIssuer`, `ValidateAudience`, `ValidateLifetime`, `ValidateIssuerSigningKey` — all true
- Store signing key in `.env` (never in `appsettings.json`)
- Refresh tokens stored in `refresh_tokens` table (hashed) — never in JWT claims

### 6.2 Social Login

- Apple, Google, Facebook: validate provider ID tokens server-side; issue our own JWT — no OAuth redirect dance
- Handler lives in `Infrastructure/Auth/SocialLoginHandler.cs`
- Use `ExternalLoginAsync` flows in Identity — not default scaffold coverage; explicit implementation required

### 6.3 API Security Hardening

- **Rate limiting**: `Microsoft.AspNetCore.RateLimiting` middleware — configure per-endpoint limits with IP partitioner
  - Auth endpoints (login, register): strict (e.g., 5 req/min per IP)
  - Chat/reveal endpoints: medium (e.g., 60 req/min per user)
  - Image upload: strict (e.g., 3 req/min per user)
  - Always return `429` with `Retry-After` header
- **CORS**: explicit allow-list — never `AllowAnyOrigin` in production
- **HSTS**: enforce in production via `UseHsts()`
- **Security headers**: add via middleware — `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`
- **OWASP BOLA protection**: always verify `document.OwnerId == User.GetUserId()` before returning any resource — never rely on route parameter alone
- **Input sanitisation**: strip HTML from all text inputs — use `HtmlEncoder` for any user content rendered in admin UI
- **No secrets in code**: all credentials via `IOptions<T>` from `.env` — never hardcoded
- **IAP webhook verification**: Apple RS256 JWKS + Google JWKS verification mandatory — `SkipWebhookVerification` valid only in `appsettings.Testing.json`; startup assertion enforces this

### 6.4 Photo Storage Security

- All photos in private Hetzner Object Storage bucket — zero public URL access
- Serve only via time-limited signed URLs (15-minute expiry)
- Signed URL generation gated: both `user_a_reveal_ready` and `user_b_reveal_ready` must be `true`
- Photos deleted from storage immediately on GDPR erasure request (separate from database deletion)

### 6.5 Logging Security

- **Never log PII** in structured log properties: no emails, names, message content, photo URLs
- Use correlation IDs (auto-generated per request) for tracing — log `CorrelationId`, `UserId` (opaque Guid only)
- Log levels: `Debug` (dev only), `Information` (business events), `Warning` (recoverable), `Error` (exceptions)
- Structured logging only: `_logger.LogInformation("Reveal triggered {ConversationId}", id)` — never string interpolation in log calls

---

## 7. Real-Time (SignalR)

### 7.1 Hub Design

- Single `ChatHub` in `Hubs/ChatHub.cs`
- Hub method names: `PascalCase` on server, matching exactly on client
- Defined hub methods:
  - `ReceiveMessage` — new chat message
  - `RevealStateUpdated` — either party's reveal flag changed
  - `MatchAssigned` — new match conversation created

### 7.2 Nginx Configuration (mandatory)

The following directives **must** be present on the `/hubs/` location block in `nginx/nginx.conf`. Missing them causes SignalR to silently fall back to long polling, breaking the `<500ms` NFR:

```nginx
location /hubs/ {
    proxy_pass http://api:8080;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_read_timeout 3600s;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
}
```

### 7.3 Background Job → Hub Communication

- Coravel jobs use `IHubContext<ChatHub>` to push events to connected clients — valid injection pattern
- `SendPushNotificationJob` fires push notifications as fallback for clients not connected to SignalR

---

## 8. Background Jobs (Coravel)

- All jobs implement `IInvocable` from Coravel
- `MatchGenerationJob`: **idempotent** — must handle being called multiple times safely; also triggered on startup (catches any user who completed onboarding before the last job run)
- `TrialExpiryNotificationJob`: sends warning at day 5 of 7-day trial
- `SendPushNotificationJob`: fire-and-forget; acceptable to drop on container restart at MVP
- Jobs that need database access: inject `IDbContextFactory<AppDbContext>` — never `AppDbContext` directly (jobs are singletons)
- Matching threshold and pool eligibility rules must be admin-configurable via `IOptions<MatchingSettings>` — never hardcoded

---

## 9. Infrastructure Rules

### 9.1 Docker

- **All execution goes through Docker Compose** — nothing runs directly on the host OS
- Multi-stage `Dockerfile`: SDK image for build → ASP.NET runtime image for production (SDK excluded from final image)
- `restart: unless-stopped` on all production services
- Named volume `db-data` — never anonymous volumes (`docker compose down -v` is **prohibited** in production)
- `.env.example` must be kept in sync with `.env` — update both in the same commit when adding any new variable

### 9.2 Environment Configuration

- All secrets injected via `.env` on VPS — never committed to source control
- Use `IOptions<T>` pattern for all configuration binding — never `IConfiguration` directly in services
- Environment-specific overrides via `appsettings.Production.json` (non-secret config only)
- `appsettings.Testing.json` for test-specific overrides (e.g., `SkipWebhookVerification: true`)

### 9.3 Hetzner Object Storage (S3-Compatible)

- **`ForcePathStyle = true`** — mandatory for Hetzner, spike before first upload story
- Signed URLs generated via `GetPreSignedUrlRequest` with 15-minute expiry
- `ContentScanningClient` runs synchronously before any S3 put — hard-fail if scan API unavailable
- Image not accepted if `ContentScanningClient` cannot confirm result (no pass-through on API outage)

---

## 10. Mobile — TypeScript Rules

### 10.1 TypeScript Configuration

```json
{
  "compilerOptions": {
    "strict": true,
    "noImplicitAny": true,
    "strictNullChecks": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "exactOptionalPropertyTypes": true
  }
}
```

- `strict: true` enabled from the first commit — retrofitting is costly
- No `any` type anywhere — use `unknown` and type-narrow
- No `@ts-ignore` without a comment explaining why and a ticket to fix it
- All API response types defined in `types/api/` — matches backend DTOs exactly
- All SignalR payload types defined in `types/signalr/` — separate from API types

### 10.2 Component Rules

- Functional components only — no class components
- One component per file — `PascalCase.tsx`
- Props interface named `{ComponentName}Props`
- No inline styles — NativeWind utility classes only
- No anonymous functions in `renderItem` or event handlers (causes unnecessary re-renders):

```tsx
// CORRECT
const handleRevealPress = useCallback(() => {
  revealService.triggerReveal(conversationId);
}, [conversationId]);

// PROHIBITED
<RevealButton onPress={() => revealService.triggerReveal(conversationId)} />
```

- `Pressable` preferred over `TouchableOpacity` — better accessibility event handling
- All interactive elements: `minHeight: 44`, `minWidth: 44` tap area
- `hitSlop={{ top: 8, bottom: 8, left: 8, right: 8 }}` on icon-only controls
- **`Pressable` style prop — Android flattening rule:** On Android, passing a function to `Pressable`'s `style` prop goes through a different rendering path that can silently drop `backgroundColor`, `borderColor`, and `borderWidth` from nested/merged style arrays. Always use `StyleSheet.flatten()` to produce a single plain object before passing to `Pressable`. When the consumer's style is not a function, pass the flattened object directly (not wrapped in a callback). When it is a function, wrap it but still flatten the result:
  ```tsx
  style={
    typeof style === "function"
      ? (pressState) => StyleSheet.flatten([baseStyle, style(pressState)])
      : StyleSheet.flatten([baseStyle, style])
  }
  ```

### 10.3 Hooks Rules

- All async operations return `AsyncState<T>` — never throw to component level:

```typescript
type AsyncState<T> = {
  data: T | null;
  error: string | null; // user-displayable string, never raw exception message
  isLoading: boolean;
};
```

- `data` and `error` are never both non-null simultaneously
- Custom hooks in `hooks/` directory with `use` prefix: `useConversation.ts`, `useRevealState.ts`
- SignalR subscription inside hooks (`useConversation`, `useRevealState`) — never directly in components

### 10.4 Services Layer

- `signalrService.ts` is a singleton — not instantiated per component
- Connection lifecycle (start, stop, reconnect) handled entirely within `signalrService.ts`
- All API calls through `apiClient.ts` — never raw `fetch` in components or hooks
- `storageService.ts` wraps `expo-secure-store` — never call `expo-secure-store` directly in components
- Error messages returned from services are user-displayable strings from `constants/errors.ts`

### 10.5 State Management

- Local component state (`useState`) for UI-only state
- Custom hooks for shared async state
- No global state management library at MVP (Zustand acceptable if complexity grows post-MVP)
- No `console.log` in production code — remove before commit

### 10.6 Security (Mobile)

- `expo-secure-store` for **all** JWT token storage — maps to iOS Keychain / Android Keystore
- `AsyncStorage` is **explicitly prohibited** for auth tokens — it is unencrypted
- All API calls over HTTPS — no HTTP allowed in production
- No sensitive data in component state beyond what is needed for the current screen
- Deep links validated server-side — never trust client-provided invite tokens without server verification

---

## 11. Performance Best Practices

### 11.1 Backend

- Use `IMemoryCache` for hot, short-lived data (matching threshold config, admin settings)
- `AsNoTracking()` on all read-only EF queries (reduces memory and CPU overhead significantly)
- Avoid N+1 queries — use `Include()` or projections with `Select()` — review all queries with EF Core logging in development
- Use `AddResponseCompression()` for JSON API responses (gzip/brotli)
- Rate limiting protects against query floods — configure before launch
- Set global request timeout (120s default, override for image upload endpoint)
- Use `CancellationToken` everywhere — cancelled requests stop DB queries too

### 11.2 Mobile

- Enable React Compiler (beta) for automatic memoization — run `npx react-compiler-healthcheck@latest` before enabling
- `FlatList` optimisation props: `removeClippedSubviews={true}`, `maxToRenderPerBatch={10}`, `windowSize={5}` on conversation lists
- Optimistic UI for message sending — update local state immediately, reconcile on server response
- Image loading: use `expo-image` (not React Native's `Image`) — supports caching and progressive loading
- Avoid blocking the JavaScript thread — all computation-heavy logic in background via Expo's threading model
- `useWindowDimensions` called once on mount for responsive breakpoints — no runtime resize listeners
- All animations respect `AccessibilityInfo.isReduceMotionEnabled()` — fade-only variant when reduce motion is on

---

## 12. Accessibility (WCAG 2.1 AA — Mandatory)

All core flows (onboarding, chat, reveal) must meet WCAG 2.1 AA. This is a launch blocker, not a nice-to-have.

- Every interactive element declares `accessibilityRole`, `accessibilityLabel`, `accessibilityHint`, `accessibilityState`
- `allowFontScaling={true}` on all `Text` — never disabled
- No fixed-height text containers — layouts tested at system text size ×2.0
- `SafeAreaProvider` + `useSafeAreaInsets()` at app root — never hardcode insets
- Reduce motion: `AccessibilityInfo.isReduceMotionEnabled()` checked on mount, stored in `AccessibilityContext`
- Skeleton screens: `accessibilityLabel="Loading..."`, `aria-busy={true}`
- Back button: `accessibilityLabel="Go back"` — always

**Colour contrast (all pairs meet AA minimum):**

| Pair | Ratio | Level |
|---|---|---|
| `#C8833A` (amber) on `#1A1814` | 4.82:1 | AA ✅ |
| `#F2EDE6` (text) on `#1A1814` | 13.4:1 | AAA ✅ |
| `#9E9790` (secondary) on `#1A1814` | 5.1:1 | AA ✅ |
| `#4A9E8A` (safety teal) on `#1A1814` | 4.6:1 | AA ✅ |
| `#D94F4F` (danger) on `#1A1814` | 4.58:1 | AA ✅ |

---

## 13. Naming Conventions (Absolute)

### Backend (C#)

| Element | Convention | Example |
|---|---|---|
| Classes, interfaces | `PascalCase` | `ConversationService`, `IRevealRepository` |
| Methods | `PascalCase` | `GetActiveConversationsAsync` |
| Properties | `PascalCase` | `UserId`, `IsRevealReady` |
| Private fields | `_camelCase` | `_userRepository`, `_logger` |
| Local variables / params | `camelCase` | `conversationId`, `matchScore` |
| Constants | `PascalCase` or `UPPER_SNAKE_CASE` for magic values | `MaxActiveConversations`, `DEFAULT_RADIUS_KM` |
| No abbreviations | Full words always | `conversation` not `conv`; `userId` not `uid` |

### Database (PostgreSQL)

| Element | Convention | Example |
|---|---|---|
| Tables | `snake_case` plural | `users`, `conversations`, `reveal_states` |
| Columns | `snake_case` | `created_at`, `user_id`, `is_reveal_ready` |
| Foreign keys | `{singular_table}_id` | `user_id`, `conversation_id` |
| Indexes | `ix_{table}_{columns}` | `ix_messages_conversation_id` |
| Primary keys | `id` (always) | `id` |

### API Endpoints (REST)

| Element | Convention | Example |
|---|---|---|
| Resource paths | Plural nouns, `kebab-case` | `/api/conversations`, `/api/reveal-states` |
| Route params | `{camelCase}` | `/api/conversations/{conversationId}` |
| Query params | `camelCase` | `?pageSize=10` |

### Mobile (TypeScript)

| Element | Convention | Example |
|---|---|---|
| Component files | `PascalCase.tsx` | `ConversationCard.tsx` |
| Hook files | `camelCase.ts`, `use` prefix | `useConversation.ts` |
| Service files | `camelCase.ts` | `signalrService.ts` |
| Constants | `UPPER_SNAKE_CASE` | `MAX_ACTIVE_CONVERSATIONS` |
| Props interfaces | `{ComponentName}Props` | `ConversationCardProps` |
| No abbreviations | Full words always | `message` not `msg` |

---

## 14. Testing

### Backend

- xUnit for all tests
- Moq for mocking dependencies — `MockBehavior.Strict` preferred (forces explicit setup)
- Integration tests use `WebApplicationFactory<Program>` + `Testcontainers` (PostgreSQL container)
- Test naming: `{Method}_{Scenario}_{ExpectedResult}` — e.g., `TriggerReveal_WhenThresholdNotMet_Returns422`
- All services must have unit tests covering happy path + key failure paths
- All validators must have unit tests covering valid input + each invalid rule

### Mobile

- Jest + React Native Testing Library for unit/component tests
- Detox for critical user flow integration tests (onboarding, chat, reveal arc)
- Snapshot tests for core UI components — update snapshots intentionally, not automatically

---

## 15. CI/CD

- Backend: Docker build in CI — `docker compose build --no-cache` validates build integrity
- Mobile: EAS Build for App Store and Play Store — `eas.json` committed from day one
- All migrations: idempotent SQL script generated and committed — applied in deployment pipeline, not at startup
- Secrets: never in CI environment variables that are logged — use CI secret stores
- Production deployment:
  ```bash
  git pull
  docker compose build
  docker compose up -d db
  docker compose exec -T db psql -U $POSTGRES_USER -d $POSTGRES_DB < migrations/latest.sql
  docker compose up -d
  ```

---

## 16. Localisation

- Polish (`pl-PL`) primary locale, English (`en`) fallback — i18n architecture required from day one
- All user-facing strings in `constants/strings.ts` (mobile) or resource files (backend email templates)
- Never hardcode Polish or English strings in component JSX or controller responses
- `Intl.DateTimeFormat` for date display in mobile — datetimes always received as ISO 8601 UTC strings from API

## 17. Pre-Launch Checklist (Non-Code)

- [ ] LUKS disk encryption enabled at VPS provisioning (before any user data written)
- [ ] Named Docker volume `db-data` persists across `docker compose down` / `up` cycles — verified
- [ ] Nginx IP allowlist on `/admin` configured before Nginx container is started
- [ ] `.env` populated from `.env.example` on VPS — no placeholder values remain
- [ ] NCMEC legal agreement process initiated (external lead time: weeks to months — **start immediately**)
- [ ] GDPR compliance posture documented before first real user onboards
- [ ] PhotoDNA integration tested with known hash sets before launch
- [ ] IAP webhook verification tested end-to-end on Apple sandbox + Google test environment
- [ ] EAS Build `eas.json` committed and both iOS and Android builds tested on physical devices
- [ ] `migrations/latest.sql` generated, committed, and verified to apply cleanly to empty database