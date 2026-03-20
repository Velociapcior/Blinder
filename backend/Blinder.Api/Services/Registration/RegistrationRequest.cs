using Blinder.Api.Models;

namespace Blinder.Api.Services.Registration;

/// <summary>
/// Input to the shared registration service — used by both Razor PageModel and mobile API endpoint.
/// Validation is the caller's responsibility before invoking <see cref="IRegistrationService"/>.
/// </summary>
public sealed record RegistrationRequest(
    string Email,
    string Password,
    UserGender Gender,
    bool Over18Declaration
)
{
    /// <inheritdoc />
    /// Password is redacted to prevent it appearing in structured logs or telemetry.
    public sealed override string ToString() =>
        $"RegistrationRequest {{ Email = {Email}, Password = [REDACTED], Gender = {Gender}, Over18Declaration = {Over18Declaration} }}";
}
