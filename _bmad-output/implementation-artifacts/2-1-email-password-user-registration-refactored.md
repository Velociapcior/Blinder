# Story 2.1: Email/Password User Registration (OAuth2 Refactored)

Status: ready-for-dev

**Updated:** This story was redesigned after Story 2-0 (OAuth2 Foundation) was approved. The key change: registration no longer issues JWT tokens. Token issuance is delegated to `Blinder.IdentityServer`'s OAuth2 token endpoint (Story 2-0).

**Project ownership:** `POST /api/auth/register` lives in **`Blinder.Api`** — user creation is a resource operation, not a token operation. Nginx routes `/api/auth/register` to `Blinder.Api` (it does not match the `/api/auth/oauth/` prefix that routes to IdentityServer). `Blinder.IdentityServer` is **not touched** by this story.

## Story

As a user,
I want to register with my email address and password,
So that I can create an account and then authenticate to the platform.

## Acceptance Criteria

### 1. Email + Password Registration Endpoint Operational

**Given** a `POST /api/auth/register` request with `email`, `password`, `confirm_password`
**When** a new user is registered
**Then**
- User is created in the database via Identity `UserManager`
- Endpoint returns `{ user_id, email, created_at }` with 201 Created
- On duplicate email: 409 Conflict with Problem Details

---

### 2. Email Validation & Uniqueness

**Given** a registration request with email `test@example.com`
**When** the registration handler processes the request
**Then**
- Email is validated (valid format via FluentValidation or built-in)
- Email is checked for uniqueness against database
- On invalid or duplicate: 400 Bad Request with Problem Details

---

### 3. Password Requirements Enforced

**Given** a `POST /api/auth/register` request with `password`
**When** the password validator processes the field
**Then**
- Password must be >= 10 characters (NFR2: complex password policy)
- Password must include: uppercase + lowercase + digit + symbol (e.g., `@#$%^&*`)
- Password is validated before user creation (no weak passwords persisted)
- On invalid: 400 Bad Request Problem Details with specific validation errors

---

### 4. Confirm Password Validation

**Given** a `POST /api/auth/register` request with `password` and `confirm_password`
**When** the validation handler processes both fields
**Then**
- Both fields are required
- They must match exactly
- On mismatch: 400 Bad Request Problem Details: "Passwords do not match"

---

### 5. Registration Request Rate Limiting

**Given** a malicious actor attempts multiple registrations from the same IP
**When** the registration endpoint receives requests
**Then**
- Rate limit: 10 registrations per IP per hour → 429 Too Many Requests
- Tracking includes: IP address, timestamp
- Subsequent requests from clean IP are not affected

---

### 6. User Record Created with Default State

**Given** a successful registration
**When** the user record is persisted
**Then**
- `ApplicationUser` record is created with: `Id` (UUID), `Email`, `NormalizedEmail`, `UserName` (=Email), `PasswordHash` (via Identity), `CreatedAt`, `UpdatedAt`
- All date fields are `DateTimeOffset` (UTC)
- No additional fields are populated until onboarding (Gender, QuizCompletedAt, etc. remain null)
- User is marked as `EmailConfirmed = false` (email verification not implemented in MVP)

---

### 7. No JWT Token Issued at Registration

**Given** a successful registration response
**When** the client receives the response
**Then**
- Response includes `user_id` and `email` only (no tokens)
- Client must call OAuth2 token endpoint (`/api/auth/oauth/token`) separately to obtain credentials
- Avoids issuing tokens before email verification step (future PRD requirement)

---

### 8. Registration Success Audit Logged

**Given** a successful user registration
**When** the user is created
**Then**
- Audit event logged: `{ event: "user_registered", user_id, email, ip_address, timestamp }`
- Logged to application logs (structured logging)

---

## Tasks / Subtasks

- [ ] Task 1: Create Registration Request DTOs & Validators (AC: 1, 2, 3, 4)
  - [ ] Create `DTOs/Auth/RegisterRequest.cs`:
    - `Email` (required, email format)
    - `Password` (required, >= 10 chars, complexity rules)
    - `ConfirmPassword` (required)
  - [ ] Create `DTOs/Auth/RegisterResponse.cs`:
    - `UserId`
    - `Email`
    - `CreatedAt`
  - [ ] Create `Validators/RegisterRequestValidator.cs` using FluentValidation:
    - Email format + uniqueness validation
    - Password complexity validation (regex: `^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@#$%^&*]).{10,}$`)
    - Password confirmation match
  - [ ] Unit tests for all validation rules

- [ ] Task 2: Create Registration Endpoint in `Blinder.Api` (AC: 1, 5, 6, 7, 8)
  - [ ] Create `Blinder.Api/Controllers/Auth/RegisterController.cs`:
    - `POST /api/auth/register(RegisterRequest request)`
    - Call `UserManager<ApplicationUser>.CreateAsync(user, password)`
    - On success: return RegisterResponse + 201 Created
    - On Identity errors (duplicate, validation): return 400/409 Problem Details
  - [ ] Add rate limiter policy to **`Blinder.Api/Program.cs`** (separate from IdentityServer's token-endpoint policy):
    ```csharp
    builder.Services.AddRateLimiter(options =>
        options.AddFixedWindowLimiter("registration-endpoint", o =>
        {
            o.PermitLimit = 10;
            o.Window = TimeSpan.FromHours(1);
            o.QueueLimit = 0;
        }));
    ```
  - [ ] Apply `[EnableRateLimiting("registration-endpoint")]` on `RegisterController`
  - [ ] Log audit event on success
  - [ ] Return 201 Created, not 200 OK

- [ ] Task 3: Integration Test Suite (AC: 1-8)
  - [ ] Test: successful registration with valid credentials
  - [ ] Test: duplicate email returns 409
  - [ ] Test: password validation failures (too short, missing complexity)
  - [ ] Test: password mismatch returns error
  - [ ] Test: rate limiting triggers after 10 requests from same IP
  - [ ] Test: registered user can subsequently authenticate via OAuth2 token endpoint
  - [ ] Test: audit log contains registration event
  - [ ] Test: database records created with correct timestamps + default state

- [ ] Task 4: Documentation & Contracts
  - [ ] Document registration API contract: request/response examples
  - [ ] Document password requirements: length, complexity, examples of valid/invalid passwords
  - [ ] Document flow chart: Registration → (client calls OAuth2 token endpoint) → Login

---

## Dev Notes

### Key Architectural Change from Original Story

**Previous Approach:** Registration endpoint created user AND generated JWT token directly
**New Approach:** Registration creates user only (in `Blinder.Api`); token issuance delegated to `Blinder.IdentityServer`'s OAuth2 token endpoint (Story 2-0)

**Rationale:**
- Separation of concerns: registration (user creation, resource operation) lives in `Blinder.Api`; authentication (token issuance) lives exclusively in `Blinder.IdentityServer`
- Enables email verification step before token issuance (future enhancement)
- OpenIddict in `Blinder.IdentityServer` is the single source of all token logic — no JWT generation anywhere in `Blinder.Api`
- Aligns with standard OAuth2 practices and the two-project topology established in Story 2-0

### Rate Limiting Notes

- Registration endpoint uses separate rate limit policy from token endpoint
- Key for scope: IP address (no user context, registration is unauthenticated)
- Limit: 10 per hour per IP (per-IP scale reasonable; allows testing, prevents bulk account creation)

### Password Policy Enforcement

- ASP.NET Core Identity provides `PasswordValidator<ApplicationUser>` but is limited
- Implement custom complexity rules via FluentValidation (server-side)
- No client-side password generation; client validates before submission
- On form submission failure, client shows specific error (e.g., "Password must include uppercase letter")

### Audit Logging

- Minimal fields: event type, user_id, email, IP address, timestamp
- Logged to application logs (structured logging via Serilog or built-in ILogger)
- Future enhancement: centralized audit table for compliance/forensics

### Testing Strategy

- Unit tests: validators in isolation
- Integration tests: endpoint with in-memory database (no external dependencies)
- Load test: 100 requests per second to registration endpoint
- Security test: password tampering, duplicate prevention

---

## Definition of Done

- [ ] Registration endpoint implemented and compiles
- [ ] Request/response DTOs created and validated
- [ ] All acceptance criteria tested (AC 1-8)
- [ ] Rate limiting active on endpoint
- [ ] Audit logging in place
- [ ] Integration tests passing (register + subsequent OAuth2 token fetch)
- [ ] API documentation complete
- [ ] Code review approved by backend lead
- [ ] Ready for mobile client implementation (Story 2-1 Mobile)

---

## Success Metrics

- ✅ Users can register with email/password in <= 300ms
- ✅ Duplicate email attempts rejected with 409
- ✅ Password validation catches all common weak patterns
- ✅ Rate limiting prevents bulk registration attacks
- ✅ Audit log shows all registration events

---
