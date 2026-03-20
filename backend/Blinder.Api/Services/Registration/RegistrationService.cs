using Blinder.Api.Errors;
using Blinder.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Blinder.Api.Services.Registration;

/// <summary>
/// Identity-backed user registration implementation.
/// Uses <see cref="UserManager{TUser}"/> as the canonical registration engine so that
/// Identity password policy, email uniqueness checks, and user creation all flow through
/// the single Identity pipeline. Never add duplicate registration logic elsewhere.
/// </summary>
public sealed class RegistrationService(UserManager<ApplicationUser> userManager) : IRegistrationService
{
    public async Task<RegistrationResult> RegisterAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Over18Declaration)
            return RegistrationResult.Failure(["Age declaration must be accepted to register."]);

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            Gender = request.Gender,
            AgeDeclarationAcceptedAt = DateTimeOffset.UtcNow,
        };

        // ThrowIfCancellationRequested before the DB write so a cancelled request
        // cannot orphan a newly created user row (userManager.CreateAsync has no
        // CancellationToken overload, so we guard synchronously beforehand).
        cancellationToken.ThrowIfCancellationRequested();
        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var isDuplicate = result.Errors.Any(e =>
                e.Code is "DuplicateEmail" or "DuplicateUserName");

            return RegistrationResult.Failure(
                result.Errors.Select(e => e.Description),
                isDuplicate ? AppErrors.DuplicateEmail : null);
        }

        return RegistrationResult.Success(user.Id);
    }
}
