using Blinder.IdentityServer.Pages.Account;
using Blinder.IdentityServer.Persistence;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Blinder.IdentityServer.Tests;

public sealed class LoginModelTests
{
    private static IAuthenticationSchemeProvider EmptySchemeProvider() =>
        new StubAuthenticationSchemeProvider([]);

    private static IAuthenticationSchemeProvider ProviderWithExternalSchemes() =>
        new StubAuthenticationSchemeProvider([
            new AuthenticationScheme("Google", "Google", typeof(StubAuthHandler)),
        ]);

    [Fact]
    public async Task OnPostAsync_WhenCredentialsAreValid_ReturnsLocalRedirect()
    {
        var userManager = new StubUserManager();
        var signInManager = new StubSignInManager(userManager)
        {
            PasswordSignInResult = IdentitySignInResult.Success,
        };
        var model = new LoginModel(userManager, signInManager, EmptySchemeProvider(), NullLogger<LoginModel>.Instance)
        {
            Input = new LoginModel.InputModel
            {
                Email = "person@example.com",
                Password = "Complex!Pass1",
            },
        };

        var result = await model.OnPostAsync("/connect/authorize?client_id=blinder-mobile");

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/connect/authorize?client_id=blinder-mobile", redirect.Url);
        Assert.True(signInManager.LockoutOnFailure);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserIsLockedOut_ReturnsPageWithGenericLockoutError()
    {
        var userManager = new StubUserManager();
        var model = new LoginModel(
            userManager,
            new StubSignInManager(userManager)
            {
                PasswordSignInResult = IdentitySignInResult.LockedOut,
            },
            EmptySchemeProvider(),
            NullLogger<LoginModel>.Instance)
        {
            Input = new LoginModel.InputModel
            {
                Email = "person@example.com",
                Password = "Complex!Pass1",
            },
        };

        var result = await model.OnPostAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        var error = Assert.Single(model.ModelState[string.Empty]!.Errors);
        Assert.Equal("Account temporarily locked. Try again later.", error.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenCredentialsAreInvalid_ReturnsPageWithGenericError()
    {
        var userManager = new StubUserManager();
        var model = new LoginModel(
            userManager,
            new StubSignInManager(userManager)
            {
                PasswordSignInResult = IdentitySignInResult.Failed,
            },
            EmptySchemeProvider(),
            NullLogger<LoginModel>.Instance)
        {
            Input = new LoginModel.InputModel
            {
                Email = "person@example.com",
                Password = "WrongPass!1",
            },
        };

        var result = await model.OnPostAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        var error = Assert.Single(model.ModelState[string.Empty]!.Errors);
        Assert.Equal("Invalid email or password.", error.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenSignInIsNotAllowed_ReturnsPageWithGenericError()
    {
        var userManager = new StubUserManager();
        var model = new LoginModel(
            userManager,
            new StubSignInManager(userManager)
            {
                PasswordSignInResult = IdentitySignInResult.NotAllowed,
            },
            EmptySchemeProvider(),
            NullLogger<LoginModel>.Instance)
        {
            Input = new LoginModel.InputModel
            {
                Email = "person@example.com",
                Password = "Complex!Pass1",
            },
        };

        var result = await model.OnPostAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        var error = Assert.Single(model.ModelState[string.Empty]!.Errors);
        Assert.Equal("Invalid email or password.", error.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenTwoFactorIsRequired_ReturnsPageWithGenericError()
    {
        var userManager = new StubUserManager();
        var model = new LoginModel(
            userManager,
            new StubSignInManager(userManager)
            {
                PasswordSignInResult = IdentitySignInResult.TwoFactorRequired,
            },
            EmptySchemeProvider(),
            NullLogger<LoginModel>.Instance)
        {
            Input = new LoginModel.InputModel
            {
                Email = "person@example.com",
                Password = "Complex!Pass1",
            },
        };

        var result = await model.OnPostAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        var error = Assert.Single(model.ModelState[string.Empty]!.Errors);
        Assert.Equal("Invalid email or password.", error.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenReturnUrlIsExternal_FallsBackToRoot()
    {
        var userManager = new StubUserManager();
        var model = new LoginModel(
            userManager,
            new StubSignInManager(userManager)
            {
                PasswordSignInResult = IdentitySignInResult.Success,
            },
            EmptySchemeProvider(),
            NullLogger<LoginModel>.Instance)
        {
            Input = new LoginModel.InputModel
            {
                Email = "person@example.com",
                Password = "Complex!Pass1",
            },
        };

        var result = await model.OnPostAsync("https://malicious.example/steal");

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/", redirect.Url);
    }

    [Fact]
    public async Task OnPostAsync_WhenUserIsMissing_ReturnsPageWithGenericError()
    {
        var userManager = new StubUserManager
        {
            User = null,
        };

        var model = new LoginModel(
            userManager,
            new StubSignInManager(userManager)
            {
                PasswordSignInResult = IdentitySignInResult.Success,
            },
            EmptySchemeProvider(),
            NullLogger<LoginModel>.Instance)
        {
            Input = new LoginModel.InputModel
            {
                Email = "person@example.com",
                Password = "Complex!Pass1",
            },
        };

        var result = await model.OnPostAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        var error = Assert.Single(model.ModelState[string.Empty]!.Errors);
        Assert.Equal("Invalid email or password.", error.ErrorMessage);
    }

    [Fact]
    public async Task OnPostExternalLogin_WhenProviderIsUnknown_ReturnsPageWithGenericError()
    {
        var userManager = new StubUserManager();
        var model = new LoginModel(
            userManager,
            new StubSignInManager(userManager)
            {
                PasswordSignInResult = IdentitySignInResult.Success,
            },
            EmptySchemeProvider(),
            NullLogger<LoginModel>.Instance);

        await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        var result = await model.OnPostExternalLogin("UnknownProvider", "/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        var error = Assert.Single(model.ModelState[string.Empty]!.Errors);
        Assert.Equal("Sign in could not be completed. Please try again.", error.ErrorMessage);
    }

    [Fact]
    public async Task OnPostExternalLogin_WhenProviderIsKnown_ReturnsChallenge()
    {
        var userManager = new StubUserManager();
        var model = new LoginModel(
            userManager,
            new StubSignInManager(userManager)
            {
                PasswordSignInResult = IdentitySignInResult.Success,
            },
            ProviderWithExternalSchemes(),
            NullLogger<LoginModel>.Instance)
        {
            Url = new StubUrlHelper(),
        };

        await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        var result = await model.OnPostExternalLogin("Google", "/connect/authorize?client_id=blinder-mobile");

        var challenge = Assert.IsType<ChallengeResult>(result);
        Assert.Contains("Google", challenge.AuthenticationSchemes);
        Assert.NotNull(challenge.Properties?.RedirectUri);
    }

    private sealed class StubUserManager : UserManager<ApplicationUser>
    {
        public StubUserManager()
            : base(
                new StubUserStore(),
                Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                Array.Empty<IUserValidator<ApplicationUser>>(),
                Array.Empty<IPasswordValidator<ApplicationUser>>(),
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                new EmptyServiceProvider(),
                NullLogger<UserManager<ApplicationUser>>.Instance)
        {
        }

        public ApplicationUser? User { get; init; } = new()
        {
            Id = "test-user-id",
            UserName = "person@example.com",
            Email = "person@example.com",
        };

        public override Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            if (User is not null && string.Equals(User.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ApplicationUser?>(User);
            }

            return Task.FromResult<ApplicationUser?>(null);
        }
    }

    private sealed class StubSignInManager(StubUserManager userManager) : SignInManager<ApplicationUser>(
        userManager,
        new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
        new UserClaimsPrincipalFactory<ApplicationUser>(
            userManager,
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions())),
        Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
        NullLogger<SignInManager<ApplicationUser>>.Instance,
        new AuthenticationSchemeProvider(Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions())),
        new DefaultUserConfirmation<ApplicationUser>())
    {
        public bool LockoutOnFailure { get; private set; }

        public required IdentitySignInResult PasswordSignInResult { get; init; }

        public override Task<IdentitySignInResult> PasswordSignInAsync(
            ApplicationUser user,
            string password,
            bool isPersistent,
            bool lockoutOnFailure)
        {
            LockoutOnFailure = lockoutOnFailure;
            return Task.FromResult(PasswordSignInResult);
        }
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class StubAuthenticationSchemeProvider(IEnumerable<AuthenticationScheme> schemes) : IAuthenticationSchemeProvider
    {
        private readonly List<AuthenticationScheme> _schemes = [.. schemes];

        public Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync() =>
            Task.FromResult<IEnumerable<AuthenticationScheme>>(_schemes);

        public Task<AuthenticationScheme?> GetSchemeAsync(string name) =>
            Task.FromResult(_schemes.FirstOrDefault(s => string.Equals(s.Name, name, StringComparison.Ordinal)));

        public void AddScheme(AuthenticationScheme scheme)
        {
            _schemes.RemoveAll(existing => string.Equals(existing.Name, scheme.Name, StringComparison.Ordinal));
            _schemes.Add(scheme);
        }

        public void RemoveScheme(string name)
        {
            _schemes.RemoveAll(s => string.Equals(s.Name, name, StringComparison.Ordinal));
        }

        public Task<AuthenticationScheme?> GetDefaultAuthenticateSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);

        public Task<AuthenticationScheme?> GetDefaultChallengeSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);

        public Task<AuthenticationScheme?> GetDefaultForbidSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);

        public Task<AuthenticationScheme?> GetDefaultSignInSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);

        public Task<AuthenticationScheme?> GetDefaultSignOutSchemeAsync() => Task.FromResult<AuthenticationScheme?>(null);

        public Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync() =>
            Task.FromResult<IEnumerable<AuthenticationScheme>>([]);
    }

    private sealed class StubAuthHandler : IAuthenticationHandler
    {
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) => Task.CompletedTask;

        public Task<AuthenticateResult> AuthenticateAsync() =>
            Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(AuthenticationProperties? properties) => Task.CompletedTask;

        public Task ForbidAsync(AuthenticationProperties? properties) => Task.CompletedTask;
    }

    private sealed class StubUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext { get; } =
            new(new DefaultHttpContext(), new RouteData(), new PageActionDescriptor());

        public string? Action(UrlActionContext actionContext) => "/Account/ExternalLogin";

        public string Content(string? contentPath) => contentPath ?? string.Empty;

        public bool IsLocalUrl(string? url) => true;

        public string? Link(string? routeName, object? values) => "/Account/ExternalLogin";

        public string? RouteUrl(UrlRouteContext routeContext) => "/Account/ExternalLogin";
    }

    private sealed class StubUserStore : IUserStore<ApplicationUser>
    {
        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.Id);

        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.UserName);

        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(user.NormalizedUserName);

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken) =>
            Task.FromResult(IdentityResult.Success);

        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
            Task.FromResult<ApplicationUser?>(null);

        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
            Task.FromResult<ApplicationUser?>(null);
    }
}
