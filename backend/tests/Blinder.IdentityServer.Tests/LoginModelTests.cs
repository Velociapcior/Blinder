using Blinder.IdentityServer.Pages.Account;
using Blinder.IdentityServer.Persistence;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;

namespace Blinder.IdentityServer.Tests;

public sealed class LoginModelTests
{
    [Fact]
    public async Task OnPostAsync_WhenCredentialsAreValid_ReturnsLocalRedirect()
    {
        var userManager = new StubUserManager();
        var signInManager = new StubSignInManager(userManager)
        {
            PasswordSignInResult = IdentitySignInResult.Success,
        };
        var model = new LoginModel(userManager, signInManager, NullLogger<LoginModel>.Instance)
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