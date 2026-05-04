# Story 2.2: Local Account Registration and Sign-In via IdentityServer OIDC

Status: in-progress

## Story

As a user,
I want to register and sign in through the IdentityServer OIDC flow,
so that my credentials are managed entirely by the identity authority and never pass through the business API.

## Acceptance Criteria

1. **Given** IdentityServer exposes a registration page under its own host (`auth.<domain>`)
   **When** a user submits a valid email and password for registration
   **Then** the account is created in `identity.*` by IdentityServer — `Blinder.Api` performs no writes to identity data
   **And** password is stored as a salted hash using ASP.NET Core Identity hashing
   **And** after registration the user is directed into the OIDC Authorization Code + PKCE flow to receive tokens — there is no separate login call to `Blinder.Api`

2. **Given** a user initiates sign-in
   **When** they complete the OIDC Authorization Code + PKCE flow against IdentityServer
   **Then** IdentityServer issues an access token, ID token, and refresh token
   **And** the access token contains the user's subject ID and configured API scopes consumable by `Blinder.Api`
   **And** failed authentication attempts return standard OIDC error responses with no credential detail leakage
   **And** `Blinder.Api` validates incoming access tokens as a resource server — it does not issue or manage tokens itself

## Tasks / Subtasks

- [x] Create `Pages/Account/Login.cshtml.cs` — LoginModel with InputModel, OnGetAsync, OnPostAsync (AC: 2)
    - [x] Inject `SignInManager<ApplicationUser>`, `ILogger<LoginModel>`
    - [x] `OnPostAsync`: call `PasswordSignInAsync`, handle `Succeeded` / `IsLockedOut` / failure cases
    - [x] Return `LocalRedirect(ReturnUrl ?? Url.Content("~/"))` on success — never open redirect
    - [x] Lockout: add non-specific `ModelState` error ("Account temporarily locked. Try again later.")
    - [x] Invalid credentials: add `ModelState` error ("Invalid email or password.") — do not reveal which field failed
- [x] Create `Pages/Account/Login.cshtml` — functional Razor view with email/password form (AC: 2)
    - [x] Preserve `returnUrl` via `asp-route-returnUrl="@Model.ReturnUrl"` on the form tag
    - [x] `<div asp-validation-summary="ModelOnly">` for error display
    - [x] Field validation spans for each input
    - [x] Link to Register page
- [x] Create `Pages/Account/Register.cshtml.cs` — RegisterModel with InputModel, OnGetAsync, OnPostAsync (AC: 1)
    - [x] Inject `UserManager<ApplicationUser>`, `SignInManager<ApplicationUser>`, `ILogger<RegisterModel>`
    - [x] `OnPostAsync`: `CreateAsync(user, Input.Password)` → `SignInAsync` → `LocalRedirect` on success
    - [x] On `IdentityResult` failure: add each `IdentityError.Description` to `ModelState`
- [x] Create `Pages/Account/Register.cshtml` — functional Razor view with email/password/confirm form (AC: 1)
    - [x] Preserve `returnUrl` via `asp-route-returnUrl` on form tag
    - [x] `<div asp-validation-summary="ModelOnly">` for IdentityError messages
    - [x] Link to Login page
- [ ] End-to-end manual smoke test — register, login, lockout, duplicate email (see Testing section)
- [ ] Full OIDC Authorization Code + PKCE flow smoke test — simulate mobile client flow end-to-end (see Testing section, steps 13–16)
- [x] `dotnet test Blinder.sln` — all existing tests green, no regressions

## Dev Notes

### Scope of This Story

This story delivers the **IdentityServer login and registration Razor Pages** so the OIDC Authorization Code + PKCE flow works end-to-end with real credentials.

**In scope:**
- `Pages/Account/Login` — sign-in form
- `Pages/Account/Register` — account creation form
- Confirming the existing `Pages/Connect/Authorize.cshtml.cs` redirect lands on the new Login page

**Out of scope (do not implement):**
- Mobile app OIDC client integration (Story 2.6)
- Social federation / external providers (Story 2.3)
- Age gate, policy acceptance, or extra `ApplicationUser` properties (Story 2.4)
- Blinder Warm Dusk design tokens — IdentityServer pages are operational utility pages, not branded mobile UX

---

### What Story 2.1 Already Built — Do Not Recreate

| Existing File | Relevant Detail |
|---|---|
| `backend/src/Blinder.IdentityServer/Persistence/ApplicationUser.cs` | `sealed class ApplicationUser : IdentityUser` — no custom properties, use as-is |
| `backend/src/Blinder.IdentityServer/Persistence/IdentityDbContext.cs` | `IdentityDbContext<ApplicationUser>`, default schema `identity`, OpenIddict tables wired |
| `backend/src/Blinder.IdentityServer/Workers/OpenIddictSeeder.cs` | Seeds `blinder-mobile`, `blinder-admin` clients and `blinder-api` scope — no changes needed |
| `backend/src/Blinder.IdentityServer/Pages/Connect/Authorize.cshtml.cs` | When not authenticated → `Challenge(new AuthenticationProperties { RedirectUri = Request.PathAndQuery }, IdentityConstants.ApplicationScheme)` → ASP.NET Core Identity default routes this to `/Account/Login?ReturnUrl=<encoded-authorize-URL>`. When authenticated → issues OIDC tokens. DO NOT change this file. |
| `backend/src/Blinder.IdentityServer/Program.cs` | `AddIdentity<ApplicationUser, IdentityRole>` registered; password policy + lockout configured; `AddRazorPages` + `MapRazorPages` already present |
| `backend/src/Blinder.Api/Program.cs` | OpenIddict validation configured — validates tokens issued by IdentityServer; no changes needed |

**No new EF Core migration is needed for this story.** `ApplicationUser` already exists in `identity.*`.

---

### Identity Config Already in Program.cs (Reference Only — Do Not Re-Register)

Password policy configured in Story 2.1:
```
RequireDigit: true
RequireLowercase: true
RequireUppercase: true
RequireNonAlphanumeric: true
RequiredLength: 10
RequiredUniqueChars: 1
```

Lockout policy:
```
MaxFailedAccessAttempts: 5
DefaultLockoutTimeSpan: 15 minutes
AllowedForNewUsers: true
```

`UserOptions.RequireUniqueEmail: true` — duplicate email registration fails at `UserManager.CreateAsync`.

ASP.NET Core Identity's default login path is `/Account/Login` — this is where the Challenge in `Authorize.cshtml.cs` already redirects. **Do not change `builder.Services.AddIdentity(...)` or `LoginPath`.** The pages you create in `Pages/Account/` will be discovered automatically by the existing `MapRazorPages()` call.

---

### How the OIDC Flow Connects (Critical — Read Before Coding)

```
Mobile app (Story 2.6)
  → GET /connect/authorize?response_type=code&code_challenge=...
      → Pages/Connect/Authorize.cshtml.cs
          → Not authenticated → Challenge(ApplicationScheme)
              → ASP.NET Core Identity → GET /Account/Login?ReturnUrl=%2Fconnect%2Fauthorize%3F...
                  → [Your new Login page renders form]
                      → POST /Account/Login
                          → PasswordSignInAsync succeeds
                              → LocalRedirect(ReturnUrl) → back to /connect/authorize?...
                                  → Pages/Connect/Authorize.cshtml.cs
                                      → Authenticated → creates claims → SignIn(OpenIddictScheme) → issues auth code
                                          → Mobile app receives code → exchanges for tokens at /connect/token
```

Registration follows the same tail:
```
POST /Account/Register
  → CreateAsync → SignInAsync
      → LocalRedirect(ReturnUrl) → back to /connect/authorize?...
          → Authorize.cshtml.cs sees authenticated user → issues auth code
```

**The `ReturnUrl` carries the full `/connect/authorize?...` query string. Both Login and Register MUST redirect to it on success for the OIDC flow to complete.**

---

### LoginModel Implementation Reference

```csharp
// Pages/Account/Login.cshtml.cs
[AllowAnonymous]
public sealed class LoginModel(
    SignInManager<ApplicationUser> signInManager,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("User logged in.");
            return LocalRedirect(ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account temporarily locked. Try again later.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
```

**Security constraints:**
- `LocalRedirect` — never use `Redirect(ReturnUrl)` directly; `LocalRedirect` throws on external URLs, preventing open-redirect attacks
- `lockoutOnFailure: true` — always pass this so the existing lockout policy fires
- "Invalid email or password." — intentionally vague; do not reveal which field is wrong

---

### RegisterModel Implementation Reference

```csharp
// Pages/Account/Register.cshtml.cs
[AllowAnonymous]
public sealed class RegisterModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<RegisterModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,   // Identity uses UserName internally; set to Email to avoid mismatch
            Email = Input.Email,
        };

        var result = await userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            logger.LogInformation("New user account created.");
            await signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(ReturnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 10)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
```

**Notes:**
- `UserName = Input.Email` — must set both; ASP.NET Core Identity stores both; setting UserName to Email avoids username-unique conflicts when email uniqueness is enforced
- `userManager.CreateAsync` hashes the password via PBKDF2 — never store or log `Input.Password`
- IdentityError messages from `result.Errors` describe policy violations (e.g. "Passwords must be at least 10 characters") — surface them to the user via `ModelState`

---

### Razor View Structure

Both views are **functional HTML with Bootstrap 5 CDN**. No Tamagui, no Warm Dusk tokens. IdentityServer login/register are backend utility pages for the OIDC flow, not end-user brand touchpoints (the branded experience is the mobile OIDC flow in Story 2.6).

**Login.cshtml essential structure:**
```html
@page
@model LoginModel
@{ ViewData["Title"] = "Sign in"; }

<h1>Sign in to Blinder</h1>

<form method="post" asp-route-returnUrl="@Model.ReturnUrl">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>

    <div>
        <label asp-for="Input.Email"></label>
        <input asp-for="Input.Email" autocomplete="email" />
        <span asp-validation-for="Input.Email" class="text-danger"></span>
    </div>

    <div>
        <label asp-for="Input.Password"></label>
        <input asp-for="Input.Password" autocomplete="current-password" />
        <span asp-validation-for="Input.Password" class="text-danger"></span>
    </div>

    <button type="submit">Sign in</button>
</form>

<p><a asp-page="./Register" asp-route-returnUrl="@Model.ReturnUrl">Don't have an account? Register</a></p>
```

**Register.cshtml essential structure:**
```html
@page
@model RegisterModel
@{ ViewData["Title"] = "Create account"; }

<h1>Create your Blinder account</h1>

<form method="post" asp-route-returnUrl="@Model.ReturnUrl">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>

    <div>
        <label asp-for="Input.Email"></label>
        <input asp-for="Input.Email" autocomplete="email" />
        <span asp-validation-for="Input.Email" class="text-danger"></span>
    </div>

    <div>
        <label asp-for="Input.Password"></label>
        <input asp-for="Input.Password" autocomplete="new-password" />
        <span asp-validation-for="Input.Password" class="text-danger"></span>
    </div>

    <div>
        <label asp-for="Input.ConfirmPassword"></label>
        <input asp-for="Input.ConfirmPassword" autocomplete="new-password" />
        <span asp-validation-for="Input.ConfirmPassword" class="text-danger"></span>
    </div>

    <button type="submit">Create account</button>
</form>

<p><a asp-page="./Login" asp-route-returnUrl="@Model.ReturnUrl">Already have an account? Sign in</a></p>
```

**Critical:** `asp-route-returnUrl="@Model.ReturnUrl"` on the `<form>` tag causes the returnUrl to be appended as a route/query value to the POST action URL. This is how returnUrl survives from GET render through form POST. Do NOT use a hidden input — use the route helper.

---

### OIDC Errors Are Handled by OpenIddict Automatically

Do NOT implement custom OIDC error serialization in the Razor Pages. When authentication fails or the authorize flow is aborted, OpenIddict handles translating the outcome to a standard OIDC error response (`error=access_denied` etc.) at the `/connect/authorize` level. The Login/Register pages simply return standard HTTP 200 (form re-render) or redirect.

---

### Architecture Boundaries — DO NOT Violate

| Rule | Why |
|---|---|
| No `/api/auth` endpoints in `Blinder.Api` | Violates IdentityServer authority; Api is resource-server-only (architecture constraint, enforced by ArchitectureTests) |
| No direct `identity.*` writes from Api code | `identity.*` schema is IdentityServer-owned; Api has no EF models for ApplicationUser |
| Use `SignInManager` / `UserManager` APIs — never `HttpContext.SignInAsync` directly | SignInManager handles cookie, lockout, and security stamp correctly |
| `[AllowAnonymous]` on both page models | Required — pages are pre-auth by definition; without it ASP.NET Core will redirect to Login recursively |
| No new properties on `ApplicationUser` | Deferred to Story 2.4; do not add `DateOfBirth`, `PolicyAccepted`, etc. now |
| No EF Core migration | ApplicationUser already exists; no schema change needed |

---

### Previous Story Learnings (From Story 2.1 Dev Record)

- **OpenIddict version is 7.5.0** (not 7.4.0 from spec) — kept at newest per explicit user directive; do not downgrade
- **`AddAuthorization()` must be registered in `Blinder.Api`** — already done in Story 2.1 closure to fix regression; do not remove it
- **Npgsql maps PascalCase to snake_case** — `ApplicationUser` persists to `identity.asp_net_users`, `Id` column to `id`; SQL queries (if any) must use lowercase snake_case
- **IdentityDbContext naming conflict** — the `using EfIdentityDbContext = ...` alias is already in place; do not introduce a second import of `IdentityDbContext<T>` without the alias
- **`builder.Services.AddAuthorization()` is required in `Blinder.Api`** — already present; do not remove

---

## Testing Requirements

### Manual Smoke Test (Required Before Closing Story)

1. `docker compose up -d postgres`
2. `dotnet ef database update --project backend/src/Blinder.IdentityServer` (migrations already applied from 2.1 — this is a no-op verification)
3. `dotnet run --project backend/src/Blinder.IdentityServer` (port 5001)

**Register flow:**
4. Navigate to `http://localhost:5001/Account/Register`
5. Submit valid email + password (≥10 chars, mixed case, digit, special char)
6. Verify redirect occurs (302 to `/` or to a returnUrl if provided)
7. Verify new row in `identity.asp_net_users` PostgreSQL table

**Login flow:**
8. Navigate to `http://localhost:5001/Account/Login`
9. Enter registered email/password → verify redirect
10. Enter wrong password → verify "Invalid email or password." error
11. Enter wrong password 5 times → verify "Account temporarily locked" message

**Duplicate registration:**
12. Attempt to register same email again → verify IdentityError message shown ("Email is already taken" or similar from Identity)

**Full OIDC flow (required — story cannot close without this):**
13. Simulate PKCE: build `/connect/authorize?client_id=blinder-mobile&response_type=code&redirect_uri=com.blinder.app%3A%2F%2Fauth%2Fcallback&scope=openid+profile+email+offline_access+blinder-api&code_challenge=<sha256>&code_challenge_method=S256&state=<random>`
14. Navigate to that URL → should redirect to Login page with `returnUrl` set
15. Login → verify redirect back to `/connect/authorize` → which then redirects to `com.blinder.app://auth/callback?code=<auth-code>&state=<random>`
16. Exchange code at `/connect/token` with `code_verifier` → verify access_token, id_token, refresh_token returned

**Regression test:**
17. `dotnet test Blinder.sln` — all 27+ existing tests must pass

### Unit Tests (Optional — Match Existing Test Style)

Project: `backend/tests/Blinder.IdentityServer.Tests/`

Suggested additions — test `LoginModel.OnPostAsync`:
- Successful sign-in returns redirect result with correct returnUrl
- Locked-out result returns Page() with lockout ModelState error
- Failed sign-in returns Page() with generic credential ModelState error

Use mocked `SignInManager<ApplicationUser>` (construct via `MockSignInManager` helper if one exists, or mock the underlying services). Match naming and style of existing tests in that project.

---

## File List

New files created in this story:
- `backend/src/Blinder.IdentityServer/Pages/Account/Login.cshtml`
- `backend/src/Blinder.IdentityServer/Pages/Account/Login.cshtml.cs`
- `backend/src/Blinder.IdentityServer/Pages/Account/Register.cshtml`
- `backend/src/Blinder.IdentityServer/Pages/Account/Register.cshtml.cs`
- `backend/tests/Blinder.IdentityServer.Tests/LoginModelTests.cs`
- `backend/tests/Blinder.IdentityServer.Tests/RegisterModelTests.cs`

Modified files:
- `_bmad-output/implementation-artifacts/2-2-local-account-registration-and-sign-in-via-identityserver-oidc.md`
- `_bmad-output/implementation-artifacts/sprint-status.yaml`

## Dev Agent Record

### Agent Model Used

GitHub Copilot (GPT-5.4)

### Completion Notes

- Implemented `LoginModel` and `RegisterModel` under `Pages/Account/` with `AllowAnonymous`, `SignInManager`/`UserManager` flows, `LocalRedirect` return-url handling, generic login failure messaging, and identity error propagation.
- Added functional Bootstrap-backed Razor pages for login and registration with local tag-helper imports so `asp-route-returnUrl`, `asp-for`, and validation helpers work without shared `_ViewImports.cshtml`.
- Added focused unit tests for `LoginModel` and `RegisterModel` covering success, lockout, invalid credentials, registration success, and surfaced `IdentityError` messages.
- Story file was updated during implementation to split the validation work into two separate manual tasks: general account smoke coverage and a required full OIDC Authorization Code + PKCE flow smoke test.
- Focused validations passed:
    - `dotnet test .\tests\Blinder.IdentityServer.Tests\Blinder.IdentityServer.Tests.csproj --filter LoginModelTests`
    - `dotnet test .\tests\Blinder.IdentityServer.Tests\Blinder.IdentityServer.Tests.csproj --filter RegisterModelTests`
- Additional non-manual validations passed after PostgreSQL was brought up in Linux-container mode:
    - `docker compose up -d postgres`
    - `dotnet ef database update --project backend/src/Blinder.IdentityServer`
    - `dotnet test .\Blinder.sln`
- The first full solution test failure was environmental: a separately running local IdentityServer process was updating the OpenIddict seed data concurrently with the test host. After stopping that background process, `dotnet test .\Blinder.sln` passed cleanly.
- Story remains `in-progress` because the two manual smoke tasks are intentionally deferred for now.

### Change Log

- 2026-04-29: Story 2.2 created — login and registration Razor Pages for IdentityServer OIDC flow.
- 2026-04-29: Implemented IdentityServer account Razor pages and focused page-model tests; PostgreSQL-backed smoke validation remains blocked by the local Docker container mode.
- 2026-04-29: Re-validated in Linux-container mode, confirmed EF migration state, and completed the full solution regression run; manual smoke tasks remain open.
