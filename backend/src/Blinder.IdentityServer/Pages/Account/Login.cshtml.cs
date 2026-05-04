using System.ComponentModel.DataAnnotations;
using Blinder.IdentityServer.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Blinder.IdentityServer.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IAuthenticationSchemeProvider schemeProvider,
    ILogger<LoginModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public IReadOnlyList<AuthenticationScheme> ExternalProviders { get; private set; } = [];

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = GetSafeReturnUrl(returnUrl);
        await LoadExternalProvidersAsync();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = GetSafeReturnUrl(returnUrl);
        await LoadExternalProvidersAsync();

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

    public async Task<IActionResult> OnPostExternalLogin(string provider, string? returnUrl = null)
    {
        ReturnUrl = GetSafeReturnUrl(returnUrl);
        await LoadExternalProvidersAsync();

        if (string.IsNullOrWhiteSpace(provider)
            || !ExternalProviders.Any(s => string.Equals(s.Name, provider, StringComparison.Ordinal)))
        {
            logger.LogWarning("Rejected external login request for unknown provider '{Provider}'.", provider);
            ModelState.AddModelError(string.Empty, "Sign in could not be completed. Please try again.");
            return Page();
        }

        var redirectUrl = Url.Page("/Account/ExternalLogin", values: new { returnUrl = ReturnUrl });
        if (string.IsNullOrWhiteSpace(redirectUrl))
        {
            logger.LogWarning("Unable to generate external login callback URL for provider '{Provider}'.", provider);
            ModelState.AddModelError(string.Empty, "Sign in could not be completed. Please try again.");
            return Page();
        }

        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    private async Task LoadExternalProvidersAsync()
    {
        var schemes = await schemeProvider.GetAllSchemesAsync();
        ExternalProviders = schemes
            .Where(s => s.DisplayName is not null)
            .ToList()
            .AsReadOnly();
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
