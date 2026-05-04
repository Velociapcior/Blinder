using System.ComponentModel.DataAnnotations;
using Blinder.IdentityServer.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blinder.IdentityServer.Pages.Account;

[AllowAnonymous]
public sealed class LinkAccountModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<LinkAccountModel> logger) : PageModel
{
    private const string LinkReturnUrlKey = "LinkReturnUrl";
    private const string LinkExpectedEmailKey = "LinkExpectedEmail";
    private const string LinkProviderKeyName = "LinkProvider";
    private const string LinkProviderUserKeyName = "LinkProviderKey";

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ExternalProvider { get; private set; }
    public string? ReturnUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return RedirectToPage("./Login");
        }

        if (!TryLoadLinkState(out var safeReturnUrl, out var expectedEmail, out var expectedProvider, out var expectedProviderKey))
        {
            return RedirectToPage("./Login");
        }

        if (!string.Equals(expectedProvider, info.LoginProvider, StringComparison.Ordinal)
            || !string.Equals(expectedProviderKey, info.ProviderKey, StringComparison.Ordinal))
        {
            logger.LogWarning("External login state mismatch detected during account link confirmation.");
            return RedirectToPage("./Login");
        }

        ExternalProvider = expectedProvider;
        ReturnUrl = safeReturnUrl;
        Input.Email = expectedEmail;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!TryLoadLinkState(out var safeReturnUrl, out var expectedEmail, out var expectedProvider, out var expectedProviderKey))
        {
            ModelState.AddModelError(string.Empty, "Sign in could not be completed. Please try again.");
            return Page();
        }

        ExternalProvider = expectedProvider;
        ReturnUrl = safeReturnUrl;

        if (!ModelState.IsValid)
        {
            Input.Email = expectedEmail;
            return Page();
        }

        if (!string.Equals(Input.Email, expectedEmail, StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Sign in could not be completed. Please try again.");
            Input.Email = expectedEmail;
            return Page();
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            ModelState.AddModelError(string.Empty, "Sign in could not be completed. Please try again.");
            return Page();
        }

        if (!string.Equals(expectedProvider, info.LoginProvider, StringComparison.Ordinal)
            || !string.Equals(expectedProviderKey, info.ProviderKey, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Sign in could not be completed. Please try again.");
            return Page();
        }

        ExternalProvider = expectedProvider;

        var user = await userManager.FindByEmailAsync(expectedEmail);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        var passwordResult = await signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: true);
        if (!passwordResult.Succeeded)
        {
            var message = passwordResult.IsLockedOut
                ? "Account temporarily locked. Try again later."
                : "Invalid email or password.";
            ModelState.AddModelError(string.Empty, message);
            return Page();
        }

        var existingLoginUser = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (existingLoginUser is not null)
        {
            if (!string.Equals(existingLoginUser.Id, user.Id, StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "Sign in could not be completed. Please try again.");
                return Page();
            }

            await signInManager.SignInAsync(user, isPersistent: false);
            ClearLinkState();
            logger.LogInformation("User attempted to link already-associated {Provider} login.", info.LoginProvider);
            return LocalRedirect(safeReturnUrl);
        }

        var addLoginResult = await userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Sign in could not be completed. Please try again.");
            return Page();
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        ClearLinkState();
        logger.LogInformation("User linked {Provider} to existing account.", info.LoginProvider);
        return LocalRedirect(safeReturnUrl);
    }

    private bool TryLoadLinkState(
        out string safeReturnUrl,
        out string expectedEmail,
        out string expectedProvider,
        out string expectedProviderKey)
    {
        safeReturnUrl = GetSafeReturnUrl(TempData.Peek(LinkReturnUrlKey) as string);
        expectedEmail = TempData.Peek(LinkExpectedEmailKey) as string ?? string.Empty;
        expectedProvider = TempData.Peek(LinkProviderKeyName) as string ?? string.Empty;
        expectedProviderKey = TempData.Peek(LinkProviderUserKeyName) as string ?? string.Empty;

        return !string.IsNullOrWhiteSpace(expectedEmail)
            && !string.IsNullOrWhiteSpace(expectedProvider)
            && !string.IsNullOrWhiteSpace(expectedProviderKey);
    }

    private void ClearLinkState()
    {
        TempData.Remove(LinkReturnUrlKey);
        TempData.Remove(LinkExpectedEmailKey);
        TempData.Remove(LinkProviderKeyName);
        TempData.Remove(LinkProviderUserKeyName);
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

    public sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
