namespace Blinder.Api.Services.Registration;

/// <summary>
/// Single-source registration contract shared by Razor PageModel and mobile API endpoint.
/// Both entry points must route through this interface to prevent ruleset divergence (ARCH rule #17).
/// </summary>
public interface IRegistrationService
{
    Task<RegistrationResult> RegisterAsync(
        RegistrationRequest request,
        CancellationToken cancellationToken = default);
}
