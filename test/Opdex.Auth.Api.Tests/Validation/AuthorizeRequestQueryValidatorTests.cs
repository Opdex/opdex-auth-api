using FluentValidation.TestHelper;
using Opdex.Auth.Api.Models;
using Opdex.Auth.Api.Validation;
using Opdex.Auth.Domain;
using Xunit;

namespace Opdex.Auth.Api.Tests.Validation;

public class AuthorizeRequestQueryValidatorTests
{
    private readonly AuthorizeRequestQueryValidator _validator;

    public AuthorizeRequestQueryValidatorTests()
    {
        _validator = new AuthorizeRequestQueryValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("app.opdex.com")]
    [InlineData("http://app.opdex.com")]
    [InlineData("https://app.opdex.com#fragment")]
    [InlineData("https://morethan255characterslong.com/abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcefghijk")]
    public void RedirectUri_Invalid(string redirectUri)
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            RedirectUri = redirectUri
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.RedirectUri);
    }

    [Fact]
    public void RedirectUri_Valid()
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            RedirectUri = "https://app.opdex.com:1111/success?abc=xyz"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.RedirectUri);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ZUhvMkw4dEw0Y2c5SE1ubjRnN2lVSnBhZzhKa01BQT*")] // invalid character
    public void CodeChallenge_Invalid(string challenge)
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            CodeChallenge = challenge
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.CodeChallenge);
    }
    
    [Theory]
    [InlineData("ZUhvMkw4dEw0Y2c5SE1ubjRnN2lVSnBhZzhKa01BQT")] // 42 chars
    [InlineData("lBJhzyRl561MJTS4iQb0JwX1tH51yV5WAcBP2CLZc2GhU9fQbccT1MMRgaxYriQupqs86aAvsE0OGIPQKzzwJsT361cAWaDIcvqcCmP4GjOoXRRQbdpUEtn8iB3U3DjRC")] // 129 chars
    public void CodeChallenge_Plain_Invalid(string challenge)
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = CodeChallengeMethod.Plain
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.CodeChallenge);
    }

    [Theory]
    [InlineData("ZUhvMkw4dEw0Y2c5SE1ubjRnN2lVSnBhZzhKa01BQTA")] // plain min
    [InlineData("lBJhzyRl561MJTS4iQb0JwX1tH51yV5WAcBP2CLZc2GhU9fQbccT1MMRgaxYriQupqs86aAvsE0OGIPQKzzwJsT361cAWaDIcvqcCmP4GjOoXRRQbdpUEtn8iB3U3DjR")] // plain max
    public void CodeChallenge_Plain_Valid(string challenge)
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = CodeChallengeMethod.Plain
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.CodeChallenge);
    }
    
    [Fact]
    public void CodeChallenge_S256_Invalid()
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            CodeChallenge = "ZUhvMkw4dEw0Y2c5SE1ubjRnN2lVSnBhZzhKa01BQTA", // plain
            CodeChallengeMethod = CodeChallengeMethod.S256
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.CodeChallenge);
    }

    [Theory]
    [InlineData("NzkzMDI2NGQ0Yjc5YmU2YWI1M2RhMDBmNTM2NGJlOTkxZjhhYzUwMWVhNGFiNWVkNTEyODA0MDMzYmQzYzdmNA")] // un-padded
    [InlineData("NzkzMDI2NGQ0Yjc5YmU2YWI1M2RhMDBmNTM2NGJlOTkxZjhhYzUwMWVhNGFiNWVkNTEyODA0MDMzYmQzYzdmNA==")] // padded
    public void CodeChallenge_S256_Valid(string challenge)
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = CodeChallengeMethod.S256
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.CodeChallenge);
    }

    [Fact]
    public void CodeChallengeMethod_Invalid()
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            CodeChallengeMethod = (CodeChallengeMethod)255
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.CodeChallengeMethod);
    }

    [Theory]
    [InlineData(CodeChallengeMethod.Plain)]
    [InlineData(CodeChallengeMethod.S256)]
    public void CodeChallengeMethod_Valid(CodeChallengeMethod method)
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            CodeChallengeMethod = method
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.CodeChallengeMethod);
    }

    [Fact]
    public void State_Invalid()
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            State = "£" // non ascii
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.State);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("xyz")]
    [InlineData("iTDj8rXF")]
    public void State_Valid(string state)
    {
        // Arrange
        var request = new AuthorizeRequestQuery
        {
            State = state
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.State);
    }
}