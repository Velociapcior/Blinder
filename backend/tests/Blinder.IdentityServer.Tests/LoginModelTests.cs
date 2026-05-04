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
        var signInManager = new StubSignInManager
        {
            PasswordSignInResult = IdentitySignInResult.Success,
        };
        var model = new LoginModel(signInManager, NullLogger<LoginModel>.Instance)
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
        var model = new LoginModel(
            new StubSignInManager
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
        var model = new LoginModel(
            new StubSignInManager
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

    private sealed class StubSignInManager() : SignInManager<ApplicationUser>(
        CreateUserManager(),
        new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
        new UserClaimsPrincipalFactory<ApplicationUser>(
            CreateUserManager(),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions())),
        Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
        NullLogger<SignInManager<ApplicationUser>>.Instance,
        new AuthenticationSchemeProvider(Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions())),
        new DefaultUserConfirmation<ApplicationUser>())
    {
        public bool LockoutOnFailure { get; private set; }

        public required IdentitySignInResult PasswordSignInResult { get; init; }

        public override Task<IdentitySignInResult> PasswordSignInAsync(
            string userName,
            string password,
            bool isPersistent,
            bool lockoutOnFailure)
        {
            LockoutOnFailure = lockoutOnFailure;
            return Task.FromResult(PasswordSignInResult);
        }

        private static UserManager<ApplicationUser> CreateUserManager() => new(
            new StubUserStore(),
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            new EmptyServiceProvider(),
            NullLogger<UserManager<ApplicationUser>>.Instance);
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