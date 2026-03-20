using Blinder.Api.Errors;
using Blinder.Api.Services.Registration;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blinder.Api.Controllers.Auth;

/// <summary>
/// Mobile registration API endpoint.
/// Delegates to <see cref="IRegistrationService"/> — the same service used by the
/// Razor PageModel — ensuring a single Identity-backed registration ruleset (ARCH rule #17).
/// Both success and duplicate-email cases return 202 Accepted with an identical body
/// to prevent email enumeration (OWASP anti-enumeration pattern).
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class RegistrationController(
    IRegistrationService registrationService,
    IValidator<MobileRegisterRequest> validator) : ControllerBase
{
    // Identical message for success and duplicate-email scenarios.
    // Never change this to be conditional — doing so would reintroduce the enumeration oracle.
    private const string RegistrationAcknowledgement =
        "If this email is not already registered, your account has been created.";

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] MobileRegisterRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));

        var result = await registrationService.RegisterAsync(
            new RegistrationRequest(request.Email, request.Password, request.Gender, request.Over18Declaration),
            cancellationToken);

        // Only surface non-duplicate failures (e.g. Identity password policy violations that
        // slipped past the validator). Duplicate email falls through to the same 202 below.
        if (!result.Succeeded && result.ErrorType != AppErrors.DuplicateEmail)
        {
            var errorDict = result.Errors
                .Select((e, i) => new { Key = $"identity[{i}]", Value = e })
                .ToDictionary(x => x.Key, x => new[] { x.Value });

            if (errorDict.Count == 0)
                return Problem(title: "Registration failed.", statusCode: StatusCodes.Status400BadRequest);

            return ValidationProblem(new ValidationProblemDetails(errorDict));
        }

        return Accepted(new { message = RegistrationAcknowledgement });
    }
}
