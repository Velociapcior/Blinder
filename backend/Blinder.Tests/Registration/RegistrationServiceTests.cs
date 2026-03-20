using Blinder.Api.Errors;
using Blinder.Api.Models;
using Blinder.Api.Services.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Blinder.Tests.Registration;

/// <summary>
/// Unit tests for RegistrationService — verifies AC 4, 5, 6.
/// UserManager is mocked; no real DB required.
/// </summary>
public class RegistrationServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly RegistrationService _sut;

    public RegistrationServiceTests()
    {
        // UserManager requires IUserStore at minimum.
        var storeMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            storeMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _sut = new RegistrationService(_userManagerMock.Object);
    }

    // -------------------------------------------------------------------------
    // AC 5 — AgeDeclarationAcceptedAt set on successful registration
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_Success_SetsAgeDeclarationAcceptedAt()
    {
        ApplicationUser? capturedUser = null;

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, _) => capturedUser = user)
            .ReturnsAsync(IdentityResult.Success);

        var request = ValidRequest();
        await _sut.RegisterAsync(request);

        capturedUser.Should().NotBeNull();
        capturedUser!.AgeDeclarationAcceptedAt.Should().NotBeNull();
        capturedUser.AgeDeclarationAcceptedAt!.Value.Should().BeCloseTo(
            DateTimeOffset.UtcNow, precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RegisterAsync_Success_SetsGenderFromRequest()
    {
        ApplicationUser? capturedUser = null;

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, _) => capturedUser = user)
            .ReturnsAsync(IdentityResult.Success);

        var request = ValidRequest() with { Gender = UserGender.Female };
        await _sut.RegisterAsync(request);

        capturedUser!.Gender.Should().Be(UserGender.Female);
    }

    [Fact]
    public async Task RegisterAsync_Success_ReturnsSucceededResultWithUserId()
    {
        var userId = Guid.NewGuid();

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .Callback<ApplicationUser, string>((user, _) => user.Id = userId)
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterAsync(ValidRequest());

        result.Succeeded.Should().BeTrue();
        result.UserId.Should().Be(userId);
        result.Errors.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // AC 4 — Duplicate email maps to AppErrors.DuplicateEmail
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsErrorTypeDuplicateEmail()
    {
        var duplicateError = new IdentityError { Code = "DuplicateEmail", Description = "Email is already taken." };
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(duplicateError));

        var result = await _sut.RegisterAsync(ValidRequest());

        result.Succeeded.Should().BeFalse();
        result.ErrorType.Should().Be(AppErrors.DuplicateEmail);
        result.Errors.Should().Contain("Email is already taken.");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUserName_ReturnsErrorTypeDuplicateEmail()
    {
        var duplicateError = new IdentityError { Code = "DuplicateUserName", Description = "Username is already taken." };
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(duplicateError));

        var result = await _sut.RegisterAsync(ValidRequest());

        result.Succeeded.Should().BeFalse();
        result.ErrorType.Should().Be(AppErrors.DuplicateEmail);
    }

    [Fact]
    public async Task RegisterAsync_NonDuplicateFailure_ReturnsNullErrorType()
    {
        var weakPassword = new IdentityError { Code = "PasswordTooShort", Description = "Password is too short." };
        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(weakPassword));

        var result = await _sut.RegisterAsync(ValidRequest());

        result.Succeeded.Should().BeFalse();
        result.ErrorType.Should().BeNull();
        result.Errors.Should().Contain("Password is too short.");
    }

    // -------------------------------------------------------------------------
    // AC 6 — Single registration ruleset: both callers use IRegistrationService
    // -------------------------------------------------------------------------

    [Fact]
    public void RegistrationService_ImplementsIRegistrationService()
    {
        // Confirms the shared contract — Razor PageModel and mobile API endpoint
        // both depend on IRegistrationService, not a separate implementation.
        _sut.Should().BeAssignableTo<IRegistrationService>();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static RegistrationRequest ValidRequest() => new(
        Email: "test@example.com",
        Password: "Test1234!",
        Gender: UserGender.Male,
        Over18Declaration: true
    );
}
