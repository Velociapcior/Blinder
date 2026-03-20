# Story 2.1: Email/Password User Registration

Status: review

## Story

As a new user,
I want to register with my email address and password,
so that I can create a Blinder account and begin the onboarding process.

## Implementation Directive

This story must use ASP.NET Core Identity as the backend registration backbone, with a single shared registration logic path for both Razor Pages and mobile API usage.

- Use Microsoft.AspNetCore.Identity UI scaffolding in Razor Pages for web/admin and reference flow.
- Keep mobile registration UI fully native (React Native), not scaffolded Razor UI.
- Reuse Identity UserManager/SignInManager + shared registration service for all entry points.
- Keep API wrappers minimal and delegate to the same Identity-backed registration logic.
- Do not create parallel registration logic or duplicate validation rules.

## Acceptance Criteria

1. Given Identity is configured with ApplicationUser and Razor Pages
When Identity UI scaffolding is added for account pages
Then registration support exists in scaffolded Identity Razor Pages under Pages/Account or Pages/Identity/Account for web/admin scenarios and compiles successfully.

2. Given ApplicationUser is defined in Models/ApplicationUser.cs
When the class definition is reviewed
Then it inherits from IdentityUser<Guid>, keeps existing fields (Gender, QuizCompletedAt, InviteLinkId, IsOnboardingComplete), and adds AgeDeclarationAcceptedAt as DateTimeOffset? mapped to snake_case age_declaration_accepted_at.

3. Given the registration input model is extended in the scaffolded Register PageModel
When validation executes
Then email format, password policy, gender required (not Unspecified), and Over18Declaration true are enforced using DataAnnotations and/or FluentValidation without bypassing Identity validation.

4. Given an email already exists
When registration is submitted through scaffolded Identity registration flow
Then registration fails with an RFC 7807-compatible error response or mapped validation output and uses AppErrors.DuplicateEmail type URI where Problem Details is returned.

5. Given registration succeeds through Identity UserManager.CreateAsync
When the user row is inspected
Then AgeDeclarationAcceptedAt is set to DateTimeOffset.UtcNow and persisted.

6. Given this story uses Identity-backed shared registration logic
When code is reviewed
Then there is no second, divergent registration ruleset; Razor PageModel and mobile API path call the same shared registration service.

7. Given EF schema changes are introduced for AgeDeclarationAcceptedAt
When migrations/latest.sql is regenerated with idempotent script generation
Then the migration remains safe to rerun and is committed.

8. Given mobile registration UX remains required for product flow
When mobile submits registration data
Then backend handling routes through the same Identity-backed registration rules (single source of truth), and mobile does not consume scaffolded Razor UI directly.

## Tasks / Subtasks

- [x] Task 1: Scaffold Identity Razor Pages registration for web/admin (AC: 1)
- [x] Add Identity UI scaffolding for Account/Register and dependent pages into backend Blinder.Api Razor Pages structure.
- [x] Ensure Program.cs keeps AddRazorPages and correct Identity setup.
- [x] Confirm scaffolded Register PageModel delegates to shared registration logic and uses UserManager/CreateAsync.

- [x] Task 2: Extend ApplicationUser for age declaration timestamp (AC: 2, 5)
- [x] Add AgeDeclarationAcceptedAt DateTimeOffset? in Models/ApplicationUser.cs.
- [x] Set AgeDeclarationAcceptedAt on successful registration in the scaffolded Register PageModel flow.

- [x] Task 3: Extend scaffolded registration input model and validation (AC: 3)
- [x] Add fields to registration input: Gender and Over18Declaration.
- [x] Enforce validation for over-18 consent and non-Unspecified gender.
- [x] Keep password and email validation aligned with Identity defaults/policies.

- [x] Task 4: Error mapping and problem details consistency (AC: 4)
- [x] Ensure duplicate email is surfaced consistently and maps to AppErrors.DuplicateEmail when returning Problem Details.
- [x] Keep AppErrors.cs as single source of problem type URIs.

- [x] Task 5: Migration pipeline update (AC: 7)
- [x] Create EF migration for AgeDeclarationAcceptedAt.
- [x] Regenerate migrations/latest.sql using idempotent script generation.
- [x] Verify column exists in asp_net_users after local apply.

- [x] Task 6: Mobile integration with single backend registration source (AC: 6, 8)
- [x] Keep mobile register screen and API contract aligned to backend registration fields.
- [x] Ensure backend path used by mobile ultimately executes the same Identity-backed validation/creation logic as Razor PageModel registration.
- [x] Ensure mobile does not render or depend on scaffolded Razor UI pages.

- [x] Task 7: Tests (AC: 3, 4, 5, 6)
- [x] Add tests for scaffolded registration validation extensions (gender and over-18).
- [x] Add tests for duplicate email handling path.
- [x] Add tests confirming only one registration ruleset is used.

---

## Dev Notes

### Non-Negotiable Constraint for This Story

Use ASP.NET Core Identity as canonical registration logic. Keep presentation split by channel.

- Preferred stack: UserManager<ApplicationUser>, SignInManager<ApplicationUser>, scaffolded Register PageModel.
- Mobile uses native React Native UI, not scaffolded Razor pages.
- Avoid creating a parallel custom registration orchestration that drifts from Identity defaults.
- If an API endpoint is used for mobile, it must delegate to the same underlying registration policy/logic to prevent divergence.

### Existing Foundation Already Available

- ApplicationUser and UserGender exist.
- AppDbContext is wired with IdentityDbContext and snake_case naming conventions.
- AddProblemDetails and UseExceptionHandler are configured.
- Razor Pages infrastructure already exists and should be leveraged.

### Architecture Alignment

- ARCH-6: Keep ApplicationUser as custom Identity subclass.
- ARCH-9: Input validation required for all request bodies and registration inputs.
- ARCH-10: Problem Details consistency and AppErrors usage.
- ARCH-23: DateTimeOffset only for date fields.

### File Structure Guidance

Expected backend focus files:

- backend/Blinder.Api/Models/ApplicationUser.cs (update)
- backend/Blinder.Api/Pages/Account/Register.cshtml and Register.cshtml.cs or backend/Blinder.Api/Pages/Identity/Account/Register.cshtml and Register.cshtml.cs (scaffolded Identity)
- backend/Blinder.Api/Errors/AppErrors.cs (reuse)
- backend/Blinder.Api/Program.cs (only minimal Identity/Razor Pages wiring adjustments if needed)
- backend/Blinder.Api/Migrations/* (new migration)
- migrations/latest.sql (regenerated)

Mobile files remain integration-focused and must not introduce backend rule duplication or Razor UI coupling.

### References

- Source: _bmad-output/planning-artifacts/epics.md (Story 2.1 acceptance baseline)
- Source: _bmad-output/planning-artifacts/architecture.md (Identity and backend patterns)
- Source: _bmad-output/implementation-artifacts/1-1-scaffold-backend-project.md
- Source: _bmad-output/implementation-artifacts/1-4-database-setup-with-postgis-and-ef-core-migrations-pipeline.md

---

## Dev Agent Record

### Agent Model Used

claude-sonnet-4-6

### Debug Log References

- Scaffolded Register.cshtml.cs used generic `IdentityUser` instead of `ApplicationUser` — fixed by updating all type references.
- Scaffolded output created a duplicate `Blinder.Api.Data.AppDbContext` in `Areas/Identity/Data/` — removed; project uses `Blinder.Api.Infrastructure.Data.AppDbContext`.
- `RegistrationController.ValidationProblem()` required `new ValidationProblemDetails(...)` wrapper — `IDictionary<string, string[]>` is not a direct overload.
- Added `IEmailSender` (no-op `NoOpEmailSender`) to satisfy DI requirement from scaffolded Identity pages; full email delivery deferred to a later story.

### Completion Notes List

- **Task 1 (AC 1):** Identity UI scaffolded (full flow) into `Areas/Identity/Pages/`. Duplicate `AppDbContext` generated by scaffolding removed. `Register.cshtml.cs` extended with `IRegistrationService` injection; all `IdentityUser` references changed to `ApplicationUser`. `Program.cs` wired `AddRazorPages`, correct Identity setup, `IRegistrationService`, and `NoOpEmailSender`.
- **Task 2 (AC 2, 5):** `AgeDeclarationAcceptedAt DateTimeOffset?` added to `ApplicationUser.cs`. Set to `DateTimeOffset.UtcNow` inside `RegistrationService.RegisterAsync` on successful `UserManager.CreateAsync`.
- **Task 3 (AC 3):** `InputModel` extended with `Gender` (`[Required]`) and `Over18Declaration` (`[Range(bool)]`). `OnPostAsync` adds explicit `ModelState` error when `Gender == Unspecified` before `ModelState.IsValid` check. FluentValidation `MobileRegisterRequestValidator` enforces the same rules for the API endpoint.
- **Task 4 (AC 4):** `RegistrationService` detects `DuplicateEmail`/`DuplicateUserName` Identity error codes and returns `ErrorType = AppErrors.DuplicateEmail`. Razor PageModel maps this to a field-level `ModelState` error. `RegistrationController` maps it to `HTTP 409` Problem Details with `type: AppErrors.DuplicateEmail`.
- **Task 5 (AC 7):** EF Core migration `20260320180702_AddAgeDeclarationAcceptedAt` generated via `dotnet ef migrations add`. `migrations/latest.sql` regenerated with `dotnet ef migrations script --idempotent`.
- **Task 6 (AC 6, 8):** `RegistrationController` (`POST /api/auth/register`) created — delegates to `IRegistrationService`. `authService.ts` wraps the endpoint. `mobile/app/(auth)/register.tsx` is fully native React Native, uses design tokens, `AsyncState<T>` pattern, and calls `authService.register()`. No Razor UI dependency.
- **Task 7 (AC 3, 4, 5, 6):** 18 new unit tests across `RegistrationServiceTests` and `MobileRegisterRequestValidatorTests`. All 21 tests (3 pre-existing + 18 new) pass with 0 failures.

### File List

backend/Blinder.Api/Models/ApplicationUser.cs
backend/Blinder.Api/Areas/Identity/Pages/Account/Register.cshtml.cs
backend/Blinder.Api/Areas/Identity/Pages/Account/Register.cshtml
backend/Blinder.Api/Services/Registration/IRegistrationService.cs
backend/Blinder.Api/Services/Registration/RegistrationRequest.cs
backend/Blinder.Api/Services/Registration/RegistrationResult.cs
backend/Blinder.Api/Services/Registration/RegistrationService.cs
backend/Blinder.Api/Controllers/Auth/MobileRegisterRequest.cs
backend/Blinder.Api/Controllers/Auth/RegistrationController.cs
backend/Blinder.Api/Program.cs
backend/Blinder.Api/Blinder.Api.csproj
backend/Blinder.Api/Migrations/20260320180702_AddAgeDeclarationAcceptedAt.cs
backend/Blinder.Api/Migrations/20260320180702_AddAgeDeclarationAcceptedAt.Designer.cs
backend/Blinder.Api/Migrations/AppDbContextModelSnapshot.cs
migrations/latest.sql
mobile/types/api/index.ts
mobile/services/authService.ts
mobile/app/(auth)/register.tsx
backend/Blinder.Tests/Registration/RegistrationServiceTests.cs
backend/Blinder.Tests/Registration/MobileRegisterRequestValidatorTests.cs

### Change Log

- Story 2.1 implementation (Date: 2026-03-20): Added shared RegistrationService (IRegistrationService), extended scaffolded Identity Register Razor Page with Gender/Over18Declaration fields and AgeDeclarationAcceptedAt persistence, added mobile API endpoint (POST /api/auth/register) with FluentValidation, created native register screen, generated EF migration for age_declaration_accepted_at column, regenerated idempotent migrations/latest.sql, added 18 unit tests covering AC 3/4/5/6.
