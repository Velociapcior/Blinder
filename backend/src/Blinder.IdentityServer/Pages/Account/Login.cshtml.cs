using System.ComponentModel.DataAnnotations;
using Blinder.IdentityServer.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blinder.IdentityServer.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = GetSafeReturnUrl(returnUrl);
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = GetSafeReturnUrl(returnUrl);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            Input.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            logger.LogInformation("User logged in.");
            return LocalRedirect(ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account temporarily locked. Try again later.");
            return Page();
        }

        if (result.IsNotAllowed)
        {
            logger.LogInformation("Login attempt rejected because sign-in is not allowed by policy.");
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        if (result.RequiresTwoFactor)
        {
            logger.LogInformation("Login attempt requires two-factor authentication.");
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
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