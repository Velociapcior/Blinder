using System.Security.Claims;
using Blinder.IdentityServer.Persistence;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Blinder.IdentityServer.Pages.Connect;

[AllowAnonymous]
public sealed class AuthorizeModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("The OpenIddict server request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        if (!result.Succeeded)
        {
            // Login UI is implemented in Story 2.2; for now challenge to the (future) login page
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(Request.Query)
                });
        }

        var user = await userManager.GetUserAsync(result.Principal)
            ?? throw new InvalidOperationException("The user principal cannot be resolved to a user record.");

        var identity = new ClaimsIdentity(
            authenticationType: "Bearer",
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity
            .SetClaim(Claims.Subject, await userManager.GetUserIdAsync(user))
            .SetClaim(Claims.Email, await userManager.GetEmailAsync(user))
            .SetClaim(Claims.Name, await userManager.GetUserNameAsync(user));

        identity.SetScopes(request.GetScopes());
        identity.SetResources("blinder-api");
        identity.SetDestinations(GetDestinations);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> OnPostAsync() => await OnGetAsync();

    private static IEnumerable<string> GetDestinations(Claim claim) =>
        claim.Type switch
        {
            Claims.Name or Claims.Subject => [Destinations.AccessToken, Destinations.IdentityToken],
            Claims.Email => [Destinations.AccessToken, Destinations.IdentityToken],
            _ => [Destinations.AccessToken]
        };
}
