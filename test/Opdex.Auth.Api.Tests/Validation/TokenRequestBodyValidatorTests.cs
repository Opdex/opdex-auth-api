using System;
using FluentValidation.TestHelper;
using Opdex.Auth.Api.Models;
using Opdex.Auth.Api.Validation;
using Opdex.Auth.Domain;
using SSAS.NET;
using Xunit;

namespace Opdex.Auth.Api.Tests.Validation;

public class TokenRequestBodyValidatorTests
{
    private readonly TokenRequestBodyValidator _validator;

    public TokenRequestBodyValidatorTests()
    {
        _validator = new TokenRequestBodyValidator();
    }

    [Fact]
    public void GrantType_Invalid()
    {
        // Arrange
        var request = new TokenRequestBody { GrantType = default };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.GrantType);
    }

    [Theory]
    [InlineData(GrantType.AuthorizationCode)]
    [InlineData(GrantType.Sid)]
    [InlineData(GrantType.RefreshToken)]
    public void GrantType_Valid(GrantType grantType)
    {
        // Arrange
        var request = new TokenRequestBody { GrantType = grantType };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.GrantType);
    }

    [Fact]
    public void Code_Invalid()
    {
        // Arrange
        var request = new TokenRequestBody { GrantType = GrantType.AuthorizationCode };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Code);
    }

    [Fact]
    public void Code_Valid()
    {
        // Arrange
        var request = new TokenRequestBody
        {
            GrantType = GrantType.AuthorizationCode,
            Code = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ZUhvMkw4dEw0Y2c5SE1ubjRnN2lVSnBhZzhKa01BQT")] // 42 chars
    [InlineData("lBJhzyRl561MJTS4iQb0JwX1tH51yV5WAcBP2CLZc2GhU9fQbccT1MMRgaxYriQupqs86aAvsE0OGIPQKzzwJsT361cAWaDIcvqcCmP4GjOoXRRQbdpUEtn8iB3U3DjRC")] // 129 chars
    [InlineData("ZUhvMkw4dEw0Y2c5SE1ubjRnN2lVSnBhZzhKa01BQT*")] // invalid character
    public void CodeVerifier_Invalid(string verifier)
    {
        // Arrange
        var request = new TokenRequestBody
        {
            GrantType = GrantType.AuthorizationCode,
            CodeVerifier = verifier
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.CodeVerifier);
    }

    [Theory]
    [InlineData("ZUhvMkw4dEw0Y2c5SE1ubjRnN2lVSnBhZzhKa01BQTX")] // 43 chars
    [InlineData("lBJhzyRl561MJTS4iQb0JwX1tH51yV5WAcBP2CLZc2GhU9fQbccT1MMRgaxYriQupqs86aAvsE0OGIPQKzzwJsT361cAWaDIcvqcCmP4GjOoXRRQbdpUEtn8iB3U3DjR")] // 128 chars
    public void CodeVerifier_Valid(string verifier)
    {
        // Arrange
        var request = new TokenRequestBody
        {
            GrantType = GrantType.AuthorizationCode,
            CodeVerifier = verifier
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.CodeVerifier);
    }

    [Fact]
    public void Sid_Null_Invalid()
    {
        // Arrange
        var request = new TokenRequestBody
        {
            GrantType = GrantType.Sid,
            Sid = null
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Sid);
    }

    [Fact]
    public void Sid_DoesNotExpire_Invalid()
    {
        // Arrange
        var request = new TokenRequestBody
        {
            GrantType = GrantType.Sid,
            Sid = new StratisId("https://app.opdex.com", "unique-id-123456789")
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Sid);
    }

    [Fact]
    public void Sid_AlreadyExpired_Invalid()
    {
        // Arrange
        var request = new TokenRequestBody
        {
            GrantType = GrantType.Sid,
            Sid = new StratisId("https://app.opdex.com", "unique-id-123456789", DateTimeOffset.UtcNow.AddMinutes(-1))
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Sid);
    }

    [Fact]
    public void Sid_Valid()
    {
        // Arrange
        var request = new TokenRequestBody
        {
            GrantType = GrantType.Sid,
            Sid = new StratisId("app.opdex.com", "KI1VrzERA5mbGb6irCLmIn-T2HmBe0YxhdcxP9pbEF_Ii9gVmPSw-LtIatqKhhXzlD3-lFcD38-LKlvuNdcjug", DateTimeOffset.UtcNow.AddMinutes(5))
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.Sid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("23CharacterAlphaNumeric")]
    [InlineData("24_CHAR_NOT_ALPHANUMERIC")]
    [InlineData("AlphaNumericCharsLength25")]
    public void RefreshToken_Invalid(string token)
    {
        // Arrange
        var request = new TokenRequestBody
        {
            GrantType = GrantType.RefreshToken,
            RefreshToken = token
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.RefreshToken);
    }

    [Fact]
    public void RefreshToken_Valid()
    {
        // Arrange
        var request = new TokenRequestBody
        {
            GrantType = GrantType.RefreshToken,
            RefreshToken = "24AlphanumericCharacters"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.RefreshToken);
    }
}