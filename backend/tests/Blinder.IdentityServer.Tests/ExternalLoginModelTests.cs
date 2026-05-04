using System.Security.Claims;
using Blinder.IdentityServer.Pages.Account;
using Blinder.IdentityServer.Persistence;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;

namespace Blinder.IdentityServer.Tests;

public sealed class ExternalLoginModelTests
{
    private static ExternalLoginInfo MakeLoginInfo(
        string loginProvider = "Google",
        string providerKey = "google-user-123",
        string? email = "user@gmail.com")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, providerKey),
        };
        if (email is not null)
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        return new ExternalLoginInfo(principal, loginProvider, providerKey, loginProvider)
        {
            AuthenticationTokens = [],
        };
    }

    private static ExternalLoginModel BuildModel(StubUserManager userManager, StubSignInManager signInManager)
    {
        var httpContext = new DefaultHttpContext();
        var model = new ExternalLoginModel(userManager, signInManager, NullLogger<ExternalLoginModel>.Instance);
        model.PageContext = new PageContext { HttpContext = httpContext };
        model.TempData = new TempDataDictionary(httpContext, new StubTempDataProvider());
        return model;
    }

    [Fact]
    public async Task OnGetAsync_WhenExternalInfoIsNull_ReturnsPageWithError()
    {
        var userManager = new StubUserManager();
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = null,
        };

        var model = BuildModel(userManager, signInManager);

        var result = await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        Assert.NotNull(model.ErrorMessage);
    }

    [Fact]
    public async Task OnGetAsync_WhenExistingLoginMappingFound_RedirectsToReturnUrl()
    {
        var userManager = new StubUserManager();
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(),
            ExternalLoginSignInResult = IdentitySignInResult.Success,
        };

        var model = BuildModel(userManager, signInManager);

        var result = await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/connect/authorize?client_id=blinder-mobile", redirect.Url);
    }

    [Fact]
    public async Task OnGetAsync_WhenNewUserEmailUnused_CreatesAccountAndRedirects()
    {
        var userManager = new StubUserManager
        {
            User = null,
            CreateResult = IdentityResult.Success,
            AddLoginResult = IdentityResult.Success,
        };
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(email: "newuser@gmail.com"),
            ExternalLoginSignInResult = IdentitySignInResult.Failed,
        };

        var model = BuildModel(userManager, signInManager);

        var result = await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/connect/authorize?client_id=blinder-mobile", redirect.Url);
        Assert.NotNull(userManager.CreatedUser);
        Assert.Equal("newuser@gmail.com", userManager.CreatedUser!.Email);
        Assert.Equal(userManager.CreatedUser, signInManager.SignedInUser);
    }

    [Fact]
    public async Task OnGetAsync_WhenEmailMatchesExistingLocalAccount_RedirectsToLinkAccount()
    {
        var existingUser = new ApplicationUser
        {
            Id = "existing-user-id",
            Email = "taken@example.com",
            UserName = "taken@example.com",
        };
        var userManager = new StubUserManager { User = existingUser };
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(email: "taken@example.com"),
            ExternalLoginSignInResult = IdentitySignInResult.Failed,
        };

        var model = BuildModel(userManager, signInManager);

        var result = await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("./LinkAccount", redirect.PageName);
        Assert.Equal("/connect/authorize?client_id=blinder-mobile", model.TempData["LinkReturnUrl"]);
    }

    [Fact]
    public async Task OnGetAsync_WhenProviderReturnsNoEmail_ReturnsPageWithError()
    {
        var userManager = new StubUserManager();
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(email: null),
            ExternalLoginSignInResult = IdentitySignInResult.Failed,
        };

        var model = BuildModel(userManager, signInManager);

        var result = await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        Assert.NotNull(model.ErrorMessage);
    }

    [Fact]
    public async Task OnGetAsync_WhenReturnUrlIsExternal_FallsBackToRoot()
    {
        var userManager = new StubUserManager();
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(),
            ExternalLoginSignInResult = IdentitySignInResult.Success,
        };

        var model = BuildModel(userManager, signInManager);

        var result = await model.OnGetAsync("https://malicious.example/steal");

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/", redirect.Url);
    }

    [Fact]
    public async Task OnGetAsync_WhenCreateUserFails_ReturnsPageWithError()
    {
        var userManager = new StubUserManager
        {
            User = null,
            CreateResult = IdentityResult.Failed(new IdentityError { Description = "Email already taken." }),
        };
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(email: "new@gmail.com"),
            ExternalLoginSignInResult = IdentitySignInResult.Failed,
        };

        var model = BuildModel(userManager, signInManager);

        var result = await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        Assert.NotNull(model.ErrorMessage);
    }

    [Fact]
    public async Task OnGetAsync_WhenExternalSignInRequiresTwoFactor_ReturnsPageWithError()
    {
        var userManager = new StubUserManager();
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(),
            ExternalLoginSignInResult = IdentitySignInResult.TwoFactorRequired,
        };

        var model = BuildModel(userManager, signInManager);

        var result = await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        Assert.NotNull(model.ErrorMessage);
    }

    [Fact]
    public async Task OnGetAsync_WhenExternalSignInIsNotAllowed_ReturnsPageWithError()
    {
        var userManager = new StubUserManager();
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(),
            ExternalLoginSignInResult = IdentitySignInResult.NotAllowed,
        };

        var model = BuildModel(userManager, signInManager);

        var result = await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        Assert.NotNull(model.ErrorMessage);
    }

    [Fact]
    public async Task OnGetAsync_WhenAttachLoginFails_DeletesNewlyCreatedUser()
    {
        var userManager = new StubUserManager
        {
            User = null,
            CreateResult = IdentityResult.Success,
            AddLoginResult = IdentityResult.Failed(new IdentityError { Description = "Failed to add login." }),
            DeleteResult = IdentityResult.Success,
        };
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(email: "newuser@gmail.com"),
            ExternalLoginSignInResult = IdentitySignInResult.Failed,
        };

        var model = BuildModel(userManager, signInManager);

        var result = await model.OnGetAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        Assert.NotNull(userManager.CreatedUser);
        Assert.Equal(userManager.CreatedUser, userManager.DeletedUser);
    }

    // ---- Stubs ----

    internal sealed class StubUserManager : UserManager<ApplicationUser>
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
            Id = "existing-user-id",
            UserName = "existing@example.com",
            Email = "existing@example.com",
        };

        public IdentityResult CreateResult { get; init; } = IdentityResult.Success;
        public IdentityResult AddLoginResult { get; init; } = IdentityResult.Success;
        public IdentityResult DeleteResult { get; init; } = IdentityResult.Success;
        public ApplicationUser? CreatedUser { get; private set; }
        public ApplicationUser? DeletedUser { get; private set; }

        public override Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            if (User is not null && string.Equals(User.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ApplicationUser?>(User);
            }

            return Task.FromResult<ApplicationUser?>(null);
        }

        public override Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            CreatedUser = user;
            return Task.FromResult(CreateResult);
        }

        public override Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo login) =>
            Task.FromResult(AddLoginResult);

        public override Task<IdentityResult> DeleteAsync(ApplicationUser user)
        {
            DeletedUser = user;
            return Task.FromResult(DeleteResult);
        }

        public override Task<string> GetUserIdAsync(ApplicationUser user) =>
            Task.FromResult(user.Id);

        public override Task<string?> GetEmailAsync(ApplicationUser user) =>
            Task.FromResult(user.Email);

        public override Task<string?> GetUserNameAsync(ApplicationUser user) =>
            Task.FromResult(user.UserName);
    }

    internal sealed class StubSignInManager(StubUserManager userManager) : SignInManager<ApplicationUser>(
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
        public ExternalLoginInfo? ExternalLoginInfo { get; init; }
        public IdentitySignInResult ExternalLoginSignInResult { get; init; } = IdentitySignInResult.Failed;
        public ApplicationUser? SignedInUser { get; private set; }

        public override Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null) =>
            Task.FromResult(ExternalLoginInfo);

        public override Task<IdentitySignInResult> ExternalLoginSignInAsync(
            string loginProvider,
            string providerKey,
            bool isPersistent,
            bool bypassTwoFactor) =>
            Task.FromResult(ExternalLoginSignInResult);

        public override Task SignInAsync(ApplicationUser user, bool isPersistent, string? authenticationMethod = null)
        {
            SignedInUser = user;
            return Task.CompletedTask;
        }
    }

    private sealed class StubTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) =>
            new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class StubUserStore : IUserStore<ApplicationUser>
    {
        public void Dispose() { }
        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.Id);
        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.UserName);
        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken ct) { user.UserName = userName; return Task.CompletedTask; }
        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(user.NormalizedUserName);
        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken ct) { user.NormalizedUserName = normalizedName; return Task.CompletedTask; }
        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken ct) => Task.FromResult(IdentityResult.Success);
        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken ct) => Task.FromResult<ApplicationUser?>(null);
        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken ct) => Task.FromResult<ApplicationUser?>(null);
    }
}
