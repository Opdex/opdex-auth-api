using System;
using FluentValidation.TestHelper;
using Opdex.Auth.Api.Models;
using Opdex.Auth.Api.Validation;
using Xunit;

namespace Opdex.Auth.Api.Tests.Validation;

public class AccessTokenRequestBodyValidatorTests
{
    private readonly AccessTokenRequestBodyValidator _validator;

    public AccessTokenRequestBodyValidatorTests()
    {
        _validator = new AccessTokenRequestBodyValidator();
    }

    [Fact]
    public void Code_Invalid()
    {
        // Arrange
        var request = new AccessTokenRequestBody();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Code);
    }

    [Fact]
    public void Code_Valid()
    {
        // Arrange
        var request = new AccessTokenRequestBody
        {
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
        var request = new AccessTokenRequestBody
        {
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
        var request = new AccessTokenRequestBody
        {
            CodeVerifier = verifier
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.CodeVerifier);
    }
}