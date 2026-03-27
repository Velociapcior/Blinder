using Blinder.Api.Models;
using Blinder.IdentityServer.Controllers.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;

namespace Blinder.Tests.IdentityServer;

/// <summary>
/// Unit tests for OAuth2Controller.
/// Each test creates the controller with mocked UserManager and a synthetic HttpContext
/// that carries the OpenIddict server feature — no real server or database required.
/// </summary>
public class OAuth2ControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly OAuth2Controller _controller;

    public OAuth2ControllerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
#pragma warning disable CS8625
            store.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625
        _controller = new OAuth2Controller(_userManagerMock.Object);
    }

    // ── ROPC (Resource Owner Password Credentials) ────────────────────────────

    [Fact]
    public async Task ROPC_ValidCredentials_ReturnsSignIn()
    {
        var user = new ApplicationUser { Email = "valid@blinder.app", UserName = "valid@blinder.app" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("valid@blinder.app")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "ValidPass1!")).ReturnsAsync(true);

        SetupHttpContext(new OpenIddictRequest
        {
            GrantType = OpenIddictConstants.GrantTypes.Password,
            Username = "valid@blinder.app",
            Password = "ValidPass1!",
        });

        var result = await _controller.Exchange();

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.SignInResult>();
    }

    [Fact]
    public async Task ROPC_InvalidPassword_ReturnsForbid()
    {
        var user = new ApplicationUser { Email = "user@blinder.app", UserName = "user@blinder.app" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("user@blinder.app")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "WrongPass!")).ReturnsAsync(false);

        SetupHttpContext(new OpenIddictRequest
        {
            GrantType = OpenIddictConstants.GrantTypes.Password,
            Username = "user@blinder.app",
            Password = "WrongPass!",
        });

        var result = await _controller.Exchange();

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task ROPC_UnknownEmail_ReturnsForbid()
    {
        _userManagerMock.Setup(m => m.FindByEmailAsync("nobody@blinder.app"))
            .ReturnsAsync((ApplicationUser?)null);

        SetupHttpContext(new OpenIddictRequest
        {
            GrantType = OpenIddictConstants.GrantTypes.Password,
            Username = "nobody@blinder.app",
            Password = "AnyPass1!",
        });

        var result = await _controller.Exchange();

        result.Should().BeOfType<ForbidResult>();
    }

    // ── Refresh Token ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshToken_ValidPrincipal_ReturnsSignIn()
    {
        var principal = BuildPrincipal();

        SetupHttpContext(
            new OpenIddictRequest { GrantType = OpenIddictConstants.GrantTypes.RefreshToken },
            authenticatedPrincipal: principal);

        var result = await _controller.Exchange();

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.SignInResult>();
    }

    [Fact]
    public async Task RefreshToken_AuthenticationFails_ReturnsForbid()
    {
        SetupHttpContext(
            new OpenIddictRequest { GrantType = OpenIddictConstants.GrantTypes.RefreshToken },
            authenticationResult: AuthenticateResult.Fail("invalid refresh token"));

        var result = await _controller.Exchange();

        result.Should().BeOfType<ForbidResult>();
    }

    // ── Authorization Code ────────────────────────────────────────────────────

    [Fact]
    public async Task AuthorizationCode_ValidPrincipal_ReturnsSignIn()
    {
        var principal = BuildPrincipal();

        SetupHttpContext(
            new OpenIddictRequest { GrantType = OpenIddictConstants.GrantTypes.AuthorizationCode },
            authenticatedPrincipal: principal);

        var result = await _controller.Exchange();

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.SignInResult>();
    }

    [Fact]
    public async Task AuthorizationCode_AuthenticationFails_ReturnsForbid()
    {
        SetupHttpContext(
            new OpenIddictRequest { GrantType = OpenIddictConstants.GrantTypes.AuthorizationCode },
            authenticationResult: AuthenticateResult.Fail("invalid authorization code"));

        var result = await _controller.Exchange();

        result.Should().BeOfType<ForbidResult>();
    }

    // ── Unsupported Grant Type ────────────────────────────────────────────────

    [Fact]
    public async Task UnsupportedGrantType_ReturnsForbid()
    {
        SetupHttpContext(new OpenIddictRequest { GrantType = "unknown_grant" });

        var result = await _controller.Exchange();

        var forbid = result.Should().BeOfType<ForbidResult>().Subject;
        forbid.AuthenticationSchemes.Should()
            .ContainSingle(s => s == OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        forbid.Properties.Should().NotBeNull();
        forbid.Properties!.Items[OpenIddictServerAspNetCoreConstants.Properties.Error]
            .Should().Be(Errors.UnsupportedGrantType);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetupHttpContext(
        OpenIddictRequest request,
        ClaimsPrincipal? authenticatedPrincipal = null,
        AuthenticateResult? authenticationResult = null)
    {
        var transaction = new OpenIddictServerTransaction { Request = request };
        var feature = new OpenIddictServerAspNetCoreFeature { Transaction = transaction };

        var services = new ServiceCollection();

        if (authenticatedPrincipal is not null || authenticationResult is not null)
        {
            var authServiceMock = new Mock<IAuthenticationService>();
            var result = authenticationResult
                ?? AuthenticateResult.Success(new AuthenticationTicket(
                    authenticatedPrincipal!,
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme));

            authServiceMock
                .Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>(),
                    OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
                .ReturnsAsync(result);

            services.AddSingleton(authServiceMock.Object);
        }

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
        };
        httpContext.Features.Set(feature);

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    private static ClaimsPrincipal BuildPrincipal() =>
        new(new ClaimsIdentity(
            [new Claim(OpenIddictConstants.Claims.Subject, Guid.NewGuid().ToString())],
            authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role));
}
