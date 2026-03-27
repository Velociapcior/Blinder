using Blinder.Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using Microsoft.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Blinder.IdentityServer.Controllers.Auth;

/// <summary>
/// OAuth2 token endpoint controller in passthrough mode.
/// OpenIddict owns token issuance, serialization, rotation, and revocation.
/// This controller owns: credential validation and <c>SignIn()</c> for each grant type.
///
/// Supported grant types:
/// - ROPC (password): validates email + password via UserManager (AC2)
/// - Refresh token: OpenIddict rotates automatically — controller just re-signs (AC4)
/// - Authorization code: passthrough for social login (AC3) — Stories 2-3/2-4 add validators
/// </summary>
[ApiController]
public sealed class OAuth2Controller(UserManager<ApplicationUser> userManager) : ControllerBase
{
    /// <summary>
    /// OAuth2 token endpoint. Handles all grant types in passthrough mode.
    /// Rate-limited to 5 requests per IP per minute (AC11).
    /// </summary>
    [HttpPost("/api/auth/oauth/token")]
    [EnableRateLimiting("token-endpoint")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict server request not found.");

        // ── ROPC (Resource Owner Password Credentials) ────────────────────────
        // AC2: validate email/password, issue 15-min access + 30-day refresh token.
        if (request.IsPasswordGrantType())
        {
            var user = await userManager.FindByEmailAsync(request.Username!);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password!))
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                Claims.Name, Claims.Role);

            identity.SetClaim(Claims.Subject, user.Id.ToString())
                    .SetClaim(Claims.Email, user.Email!)
                    .SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // ── Refresh Token ─────────────────────────────────────────────────────
        // AC4: OpenIddict validates and rotates automatically.
        // Controller re-signs the authenticated principal with updated claims.
        if (request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal is null)
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Re-set destinations so refreshed access token carries the same claims.
            result.Principal.SetDestinations(GetDestinations);

            return SignIn(result.Principal,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // ── Authorization Code (Social Login) ─────────────────────────────────
        // AC3: OpenIddict validates code (one-time use, 10-min expiry) automatically.
        // Stories 2-3/2-4 add ISocialLoginTokenValidator implementations — no changes
        // to this controller required when those stories are implemented.
        if (request.IsAuthorizationCodeGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            if (!result.Succeeded || result.Principal is null)
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            result.Principal.SetDestinations(GetDestinations);

            return SignIn(result.Principal,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var properties = new AuthenticationProperties();
        properties.Items[OpenIddictServerAspNetCoreConstants.Properties.Error] =
            Errors.UnsupportedGrantType;
        properties.Items[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
            "The specified grant type is not supported.";

        return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Declares which destinations each claim flows to.
    /// NOTE: OpenIddict v4+ requires SetDestinations() — the old 3-arg AddClaim overload is removed.
    /// </summary>
    private static IEnumerable<string> GetDestinations(Claim claim) =>
        claim.Type switch
        {
            Claims.Subject or Claims.Email =>
                [Destinations.AccessToken, Destinations.IdentityToken],
            _ =>
                [Destinations.AccessToken]
        };
}
