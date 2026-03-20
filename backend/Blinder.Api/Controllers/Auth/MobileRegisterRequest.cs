using Blinder.Api.Models;
using FluentValidation;

namespace Blinder.Api.Controllers.Auth;

/// <summary>
/// Request body for the mobile registration endpoint.
/// FluentValidation enforces all constraints; no inline validation in the controller.
/// </summary>
public sealed record MobileRegisterRequest(
    string Email,
    string Password,
    UserGender Gender,
    bool Over18Declaration
);

public sealed class MobileRegisterRequestValidator : AbstractValidator<MobileRegisterRequest>
{
    public MobileRegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        // Rules mirror Identity's default PasswordOptions (RequiredLength=6, RequireUppercase,
        // RequireLowercase, RequireDigit, RequireNonAlphanumeric).
        // MaximumLength(128) prevents CPU-bound hashing DoS from arbitrarily large inputs.
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one non-alphanumeric character.");

        // IsInEnum() rejects undefined values (e.g. JSON integer 99 not in UserGender).
        // The Must() then filters out the explicit zero-value sentinel (Unspecified).
        RuleFor(x => x.Gender)
            .IsInEnum()
            .Must(g => g != UserGender.Unspecified)
            .WithMessage("Gender must be Male, Female, or Non-Binary.");

        RuleFor(x => x.Over18Declaration)
            .Must(v => v)
            .WithMessage("You must declare you are 18 years of age or older.");
    }
}
