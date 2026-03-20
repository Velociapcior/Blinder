# API Specification

## Register a new user

**Endpoint:** `POST /api/auth/register`

**Authentication:** Not required (public endpoint)

### Request

```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "gender": 1,
  "over18Declaration": true
}
```

**Fields:**
- `email` (string, required): Valid email address. Must not be already registered.
- `password` (string, required): 6–128 characters. Must contain uppercase, lowercase, digit, and non-alphanumeric character to match ASP.NET Identity defaults.
- `gender` (integer, required): One of `1` (Male), `2` (Female), or `3` (Non-Binary). `0` (Unspecified) is rejected.
- `over18Declaration` (boolean, required): Must be `true` to confirm age 18+.

### Success Response

**Status:** `202 Accepted`

**Body:**
```json
{
  "message": "If this email is not already registered, your account has been created."
}
```

### Error Responses

#### Validation Error

**Status:** `422 Unprocessable Entity`

**Body (example):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 422,
  "errors": {
    "email": ["'Email' is not a valid email address."],
    "password": ["Password must contain at least one digit."]
  }
}
```

#### Database Error / Server Error

**Status:** `400 Bad Request` (for non-duplicate failures) or `500 Internal Server Error`

**Body (example):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Registration failed.",
  "status": 400
}
```

### Implementation Notes

#### Anti-Enumeration Pattern (OWASP)

Both successful registrations and duplicate-email scenarios return `202 Accepted` with **identical body and status code**. This prevents attackers from enumerating registered email addresses through timing or response analysis.

- **Success** (new account created): `202 Accepted`
- **Duplicate email** (account already exists): `202 Accepted` (same)
- **Validation error** (e.g. bad password): `422 Unprocessable Entity` (distinct from duplicate)

The mobile app shows a neutral "Registration submitted" confirmation screen in both the duplicate and success cases. Password policy validation failures surface as distinct `422` errors.

#### Shared Registration Logic (ARCH Rule #17)

Both the Razor Pages web endpoint and the mobile API endpoint route through the same `IRegistrationService` implementation, ensuring a single source of truth for registration rules:
- Email uniqueness
- Password policy enforcement (Identity defaults)
- Gender validation
- Age declaration timestamp recording

Mobile UI remains native React Native—it does not consume encrypted scaffolded Razor Pages.

#### Age Declaration

On successful registration, `ApplicationUser.AgeDeclarationAcceptedAt` is set to `DateTimeOffset.UtcNow`. This timestamp is required by COPPA/child-safety compliance workflows. The field is nullable to accommodate late-added users or data restoration, but all registrations through this endpoint set it immediately.

---

## Future Endpoints

- **Story 2-2:** `POST /api/auth/login` — JWT bearer token issuance
- **Story 2-3:** `POST /api/auth/apple-sign-in` — Apple Sign-In integration
- **Story 3-1:** `POST /api/users/{userId}/photos` — Profile photo upload
- **Story 5-1:** WebSocket upgrade to `/hubs/chat` — Real-time messaging
