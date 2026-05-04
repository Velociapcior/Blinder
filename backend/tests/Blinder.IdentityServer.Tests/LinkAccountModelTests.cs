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

public sealed class LinkAccountModelTests
{
    [Fact]
    public async Task OnPostAsync_WhenPostedEmailDoesNotMatchExpected_ReturnsPageAndSkipsLinking()
    {
        var userManager = new StubUserManager();
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(),
        };

        signInManager.PasswordSignInResults.Enqueue(IdentitySignInResult.Success);

        var model = BuildModel(userManager, signInManager);
        SetLinkState(model.TempData, "/connect/authorize?client_id=blinder-mobile", "taken@example.com", "Google", "google-user-123");
        model.Input = new LinkAccountModel.InputModel
        {
            Email = "other@example.com",
            Password = "Complex!Pass1",
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(0, userManager.AddLoginCalls);
        var error = Assert.Single(model.ModelState[string.Empty]!.Errors);
        Assert.Equal("Sign in could not be completed. Please try again.", error.ErrorMessage);
    }

    [Fact]
    public async Task OnPostAsync_WhenFirstAttemptFails_SecondAttemptPreservesReturnUrl()
    {
        var userManager = new StubUserManager();
        var signInManager = new StubSignInManager(userManager)
        {
            ExternalLoginInfo = MakeLoginInfo(),
        };

        signInManager.PasswordSignInResults.Enqueue(IdentitySignInResult.Failed);
        signInManager.PasswordSignInResults.Enqueue(IdentitySignInResult.Success);

        var model = BuildModel(userManager, signInManager);
        SetLinkState(model.TempData, "/connect/authorize?client_id=blinder-mobile", "taken@example.com", "Google", "google-user-123");
        model.Input = new LinkAccountModel.InputModel
        {
            Email = "taken@example.com",
            Password = "Complex!Pass1",
        };

        var firstResult = await model.OnPostAsync();
        Assert.IsType<PageResult>(firstResult);

        model.ModelState.Clear();
        model.Input = new LinkAccountModel.InputModel
        {
            Email = "taken@example.com",
            Password = "Complex!Pass1",
        };

        var secondResult = await model.OnPostAsync();

        var redirect = Assert.IsType<LocalRedirectResult>(secondResult);
        Assert.Equal("/connect/authorize?client_id=blinder-mobile", redirect.Url);
        Assert.Equal(1, userManager.AddLoginCalls);
    }

    private static LinkAccountModel BuildModel(StubUserManager userManager, StubSignInManager signInManager)
    {
        var httpContext = new DefaultHttpContext();
        var model = new LinkAccountModel(userManager, signInManager, NullLogger<LinkAccountModel>.Instance)
        {
            PageContext = new PageContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, new StubTempDataProvider()),
        };

        return model;
    }

    private static ExternalLoginInfo MakeLoginInfo(
        string provider = "Google",
        string providerKey = "google-user-123")
    {
        var principal = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
        return new ExternalLoginInfo(principal, provider, providerKey, provider)
        {
            AuthenticationTokens = [],
        };
    }

    private static void SetLinkState(
        ITempDataDictionary tempData,
        string returnUrl,
        string expectedEmail,
        string provider,
        string providerKey)
    {
        tempData["LinkReturnUrl"] = returnUrl;
        tempData["LinkExpectedEmail"] = expectedEmail;
        tempData["LinkProvider"] = provider;
        tempData["LinkProviderKey"] = providerKey;
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

        public ApplicationUser User { get; init; } = new()
        {
            Id = "existing-user-id",
            Email = "taken@example.com",
            UserName = "taken@example.com",
        };

        public int AddLoginCalls { get; private set; }
        public IdentityResult AddLoginResult { get; init; } = IdentityResult.Success;
        public ApplicationUser? ExistingLoginUser { get; init; }

        public override Task<ApplicationUser?> FindByEmailAsync(string email)
        {
            if (string.Equals(User.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<ApplicationUser?>(User);
            }

            return Task.FromResult<ApplicationUser?>(null);
        }

        public override Task<ApplicationUser?> FindByLoginAsync(string loginProvider, string providerKey) =>
            Task.FromResult(ExistingLoginUser);

        public override Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo login)
        {
            AddLoginCalls++;
            return Task.FromResult(AddLoginResult);
        }

        public override Task<string> GetUserIdAsync(ApplicationUser user) => Task.FromResult(user.Id);
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
        public ExternalLoginInfo? ExternalLoginInfo { get; init; }
        public Queue<IdentitySignInResult> PasswordSignInResults { get; } = new();

        public override Task<ExternalLoginInfo?> GetExternalLoginInfoAsync(string? expectedXsrf = null) =>
            Task.FromResult(ExternalLoginInfo);

        public override Task<IdentitySignInResult> CheckPasswordSignInAsync(
            ApplicationUser user,
            string password,
            bool lockoutOnFailure)
        {
            if (PasswordSignInResults.Count == 0)
            {
                return Task.FromResult(IdentitySignInResult.Failed);
            }

            return Task.FromResult(PasswordSignInResults.Dequeue());
        }

        public override Task SignInAsync(ApplicationUser user, bool isPersistent, string? authenticationMethod = null) =>
            Task.CompletedTask;
    }

    private sealed class StubTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) =>
            new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class StubUserStore : IUserStore<ApplicationUser>
    {
        public void Dispose()
        {
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);

        public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);

        public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedUserName);

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);

        public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(IdentityResult.Success);

        public Task<ApplicationUser?> FindByIdAsync(string userId, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);

        public Task<ApplicationUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) => Task.FromResult<ApplicationUser?>(null);
    }
}
