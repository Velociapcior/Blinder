using System.Security.Claims;
using Blinder.IdentityServer.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blinder.IdentityServer.Pages.Account;

[AllowAnonymous]
public sealed class ExternalLoginModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<ExternalLoginModel> logger) : PageModel
{
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
    {
        var safeReturnUrl = GetSafeReturnUrl(returnUrl);

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            logger.LogWarning("External login callback arrived with no external login info.");
            ErrorMessage = "Sign in could not be completed. Please try again.";
            return Page();
        }

        var signInResult = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: false);

        if (signInResult.Succeeded)
        {
            logger.LogInformation("User signed in via {Provider}.", info.LoginProvider);
            return LocalRedirect(safeReturnUrl);
        }

        if (signInResult.IsLockedOut)
        {
            ErrorMessage = "Account temporarily locked. Try again later.";
            return Page();
        }

        if (signInResult.RequiresTwoFactor || signInResult.IsNotAllowed)
        {
            logger.LogInformation(
                "External sign-in with {Provider} was rejected by policy. RequiresTwoFactor={RequiresTwoFactor} IsNotAllowed={IsNotAllowed}",
                info.LoginProvider,
                signInResult.RequiresTwoFactor,
                signInResult.IsNotAllowed);
            ErrorMessage = "Sign in could not be completed. Please try again.";
            return Page();
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (email is null)
        {
            logger.LogWarning("{Provider} did not return an email claim.", info.LoginProvider);
            ErrorMessage = "Sign in could not be completed. Please try again.";
            return Page();
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            // Email already belongs to a local account - require explicit confirmation before linking.
            TempData["LinkReturnUrl"] = safeReturnUrl;
            TempData["LinkExpectedEmail"] = email;
            TempData["LinkProvider"] = info.LoginProvider;
            TempData["LinkProviderKey"] = info.ProviderKey;
            return RedirectToPage("./LinkAccount");
        }

        var newUser = new ApplicationUser { UserName = email, Email = email };
        var createResult = await userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            logger.LogWarning("Failed to create local account for {Provider} user.", info.LoginProvider);
            ErrorMessage = "Sign in could not be completed. Please try again.";
            return Page();
        }

        var addLoginResult = await userManager.AddLoginAsync(newUser, info);
        if (!addLoginResult.Succeeded)
        {
            var rollbackResult = await userManager.DeleteAsync(newUser);
            if (!rollbackResult.Succeeded)
            {
                logger.LogWarning("Failed to roll back newly created local account after external login attach failure.");
            }

            logger.LogWarning("Failed to attach {Provider} login to new account.", info.LoginProvider);
            ErrorMessage = "Sign in could not be completed. Please try again.";
            return Page();
        }

        await signInManager.SignInAsync(newUser, isPersistent: false);
        logger.LogInformation("New account created and signed in via {Provider}.", info.LoginProvider);
        return LocalRedirect(safeReturnUrl);
    }

    private string GetSafeReturnUrl(string? returnUrl)
    {
        if (IsSafeLocalReturnUrl(returnUrl))
        {
            return returnUrl!;
        }

        return "/";
    }

    private static bool IsSafeLocalReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl)
        && returnUrl.StartsWith("/", StringComparison.Ordinal)
        && !returnUrl.StartsWith("//", StringComparison.Ordinal)
        && !returnUrl.StartsWith("/\\", StringComparison.Ordinal);
}
