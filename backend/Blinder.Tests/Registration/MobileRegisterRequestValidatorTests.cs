using Blinder.Api.Controllers.Auth;
using Blinder.Api.Models;
using FluentAssertions;

namespace Blinder.Tests.Registration;

/// <summary>
/// Unit tests for MobileRegisterRequestValidator — verifies AC 3 (gender required,
/// over-18 required, email format, password minimum length).
/// </summary>
public class MobileRegisterRequestValidatorTests
{
    private readonly MobileRegisterRequestValidator _validator = new();

    // -------------------------------------------------------------------------
    // AC 3 — valid request passes
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Validate_ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // AC 3 — Gender must not be Unspecified
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Validate_GenderUnspecified_Fails()
    {
        var request = ValidRequest() with { Gender = UserGender.Unspecified };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Gender));
    }

    [Theory]
    [InlineData(UserGender.Male)]
    [InlineData(UserGender.Female)]
    [InlineData(UserGender.NonBinary)]
    public async Task Validate_ValidGender_Passes(UserGender gender)
    {
        var request = ValidRequest() with { Gender = gender };

        var result = await _validator.ValidateAsync(request);

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Gender));
    }

    // -------------------------------------------------------------------------
    // AC 3 — Over18Declaration must be true
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Validate_Over18DeclarationFalse_Fails()
    {
        var request = ValidRequest() with { Over18Declaration = false };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Over18Declaration));
    }

    // -------------------------------------------------------------------------
    // AC 3 — Email validation
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@nodomain")]
    public async Task Validate_InvalidEmail_Fails(string email)
    {
        var request = ValidRequest() with { Email = email };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Email));
    }

    // -------------------------------------------------------------------------
    // AC 3 — Password minimum length
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Validate_PasswordTooShort_Fails()
    {
        var request = ValidRequest() with { Password = "Ab1!" }; // 4 chars

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Password));
    }

    [Fact]
    public async Task Validate_PasswordAtMinLength_Passes()
    {
        var request = ValidRequest() with { Password = "Abcd1!" }; // exactly 6

        var result = await _validator.ValidateAsync(request);

        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Password));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static MobileRegisterRequest ValidRequest() => new(
        Email: "test@example.com",
        Password: "Test1234!",
        Gender: UserGender.Male,
        Over18Declaration: true
    );
}
