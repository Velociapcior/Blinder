using Blinder.IdentityServer.Pages.Account;
using Blinder.IdentityServer.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;

namespace Blinder.IdentityServer.Tests;

public sealed class RegisterModelTests
{
    [Fact]
    public async Task OnPostAsync_WhenRegistrationSucceeds_SignsInAndRedirects()
    {
        var userManager = new StubUserManager
        {
            CreateResult = IdentityResult.Success,
        };
        var signInManager = new StubSignInManager(userManager);
        var model = new RegisterModel(userManager, signInManager, NullLogger<RegisterModel>.Instance)
        {
            Input = new RegisterModel.InputModel
            {
                Email = "person@example.com",
                Password = "Complex!Pass1",
                ConfirmPassword = "Complex!Pass1",
            },
        };

        var result = await model.OnPostAsync("/connect/authorize?client_id=blinder-mobile");

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/connect/authorize?client_id=blinder-mobile", redirect.Url);
        Assert.NotNull(userManager.CreatedUser);
        Assert.Equal("person@example.com", userManager.CreatedUser!.Email);
        Assert.Equal("person@example.com", userManager.CreatedUser.UserName);
        Assert.Equal("Complex!Pass1", userManager.CreatedPassword);
        Assert.Same(userManager.CreatedUser, signInManager.SignedInUser);
    }

    [Fact]
    public async Task OnPostAsync_WhenRegistrationFails_AddsIdentityErrorsToModelState()
    {
        var userManager = new StubUserManager
        {
            CreateResult = IdentityResult.Failed(
                new IdentityError { Description = "Email is already taken." },
                new IdentityError { Description = "Passwords must contain a digit." }),
        };
        var signInManager = new StubSignInManager(userManager);
        var model = new RegisterModel(userManager, signInManager, NullLogger<RegisterModel>.Instance)
        {
            Input = new RegisterModel.InputModel
            {
                Email = "person@example.com",
                Password = "Complex!Pass1",
                ConfirmPassword = "Complex!Pass1",
            },
        };

        var result = await model.OnPostAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        var errors = model.ModelState[string.Empty]!.Errors.Select(error => error.ErrorMessage).ToArray();
        Assert.Equal(["Email is already taken.", "Passwords must contain a digit."], errors);
        Assert.Null(signInManager.SignedInUser);
    }

    [Fact]
    public async Task OnPostAsync_WhenReturnUrlIsExternal_FallsBackToRoot()
    {
        var userManager = new StubUserManager
        {
            CreateResult = IdentityResult.Success,
        };
        var signInManager = new StubSignInManager(userManager);
        var model = new RegisterModel(userManager, signInManager, NullLogger<RegisterModel>.Instance)
        {
            Input = new RegisterModel.InputModel
            {
                Email = "person@example.com",
                Password = "Complex!Pass1",
                ConfirmPassword = "Complex!Pass1",
            },
        };

        var result = await model.OnPostAsync("https://malicious.example/steal");

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/", redirect.Url);
    }

    [Fact]
    public async Task OnPostAsync_WhenSignInPolicyBlocksUser_ReturnsPageWithVerificationMessage()
    {
        var userManager = new StubUserManager
        {
            CreateResult = IdentityResult.Success,
        };
        var signInManager = new StubSignInManager(userManager)
        {
            CanSignInResult = false,
        };
        var model = new RegisterModel(userManager, signInManager, NullLogger<RegisterModel>.Instance)
        {
            Input = new RegisterModel.InputModel
            {
                Email = "person@example.com",
                Password = "Complex!Pass1",
                ConfirmPassword = "Complex!Pass1",
            },
        };

        var result = await model.OnPostAsync("/connect/authorize?client_id=blinder-mobile");

        Assert.IsType<PageResult>(result);
        var error = Assert.Single(model.ModelState[string.Empty]!.Errors);
        Assert.Equal("Account created. Additional verification is required before sign in.", error.ErrorMessage);
        Assert.Null(signInManager.SignedInUser);
    }

    private sealed class StubUserManager() : UserManager<ApplicationUser>(
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
        public required IdentityResult CreateResult { get; init; }

        public ApplicationUser? CreatedUser { get; private set; }

        public string? CreatedPassword { get; private set; }

        public override Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            CreatedUser = user;
            CreatedPassword = password;
            return Task.FromResult(CreateResult);
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
        public bool CanSignInResult { get; init; } = true;

        public ApplicationUser? SignedInUser { get; private set; }

        public override Task<bool> CanSignInAsync(ApplicationUser user) => Task.FromResult(CanSignInResult);

        public override Task SignInAsync(ApplicationUser user, bool isPersistent, string? authenticationMethod = null)
        {
            SignedInUser = user;
            return Task.CompletedTask;
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