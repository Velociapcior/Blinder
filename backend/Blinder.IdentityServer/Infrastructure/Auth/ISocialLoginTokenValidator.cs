namespace Blinder.IdentityServer.Infrastructure.Auth;

/// <summary>
/// Contract for provider-specific social login token validation.
/// Stories 2-3 (Apple) and 2-4 (Google/Facebook) each implement this interface.
/// Injected into <see cref="Controllers.Auth.OAuth2Controller"/> to extend the
/// authorization code grant without changes to the token endpoint controller.
/// </summary>
public interface ISocialLoginTokenValidator
{
    /// <summary>The social identity provider name (e.g., "apple", "google").</summary>
    string ProviderName { get; }

    /// <summary>
    /// Validates a provider-issued identity token and returns a normalized principal.
    /// Returns <see langword="null"/> if the token is invalid or cannot be verified.
    /// </summary>
    Task<SocialLoginPrincipal?> ValidateAsync(string idToken, CancellationToken ct);
}

/// <summary>Normalized identity from a validated social provider token.</summary>
public record SocialLoginPrincipal(string ProviderId, string Email, string? DisplayName);
