using FluentValidation.TestHelper;
using Opdex.Auth.Api.Validation;
using SSAS.NET;
using Xunit;

namespace Opdex.Auth.Api.Tests.Validator;

public class StratisSignatureAuthCallbackBodyValidatorTests
{
    private readonly StratisSignatureAuthCallbackBodyValidator _validator;

    public StratisSignatureAuthCallbackBodyValidatorTests()
    {
        _validator = new StratisSignatureAuthCallbackBodyValidator();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ContainsInvalidBase64Characters_ContainsInvalidBase64Characters_ContainsInvalidBase64Cha")]
    [InlineData("LessThan88Base64CharactersLessThan88Base64CharactersLessThan88Base64CharactersLessThan8")]
    [InlineData("MoreThan88Base64CharactersMoreThan88Base64CharactersMoreThan88Base64CharactersMoreThan88B")]
    public void Signature_Invalid(string signature)
    {
        // Arrange
        var request = new StratisSignatureAuthCallbackBody
        {
            Signature = signature
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Signature);
    }

    [Fact]
    public void Signature_Valid()
    {
        // Arrange
        var request = new StratisSignatureAuthCallbackBody
        {
            Signature = "IPkq8t9M00nKyI3RP4KriflomZpMYDp8+C0RdvvbgNF6bSCAXgp4yFbx+Uscr1EQ2uKGyg6z9wj07sf1ZwH0XmI="
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.Signature);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("PVwyqbwu5CazeACoAMRonaQSyRvTHZvAUl")]
    [InlineData("PVwyqbwu5CazeACoAMRonaQSyRvTHZvAUI")]
    [InlineData("PVwyqbwu5CazeACoAMRonaQSyRvTHZvAUO")]
    [InlineData("PVwyqbwu5CazeACoAMRonaQSyRvTHZvAU0")]
    public void PublicKey_Invalid(string publicKey)
    {
        // Arrange
        var request = new StratisSignatureAuthCallbackBody
        {
            PublicKey = publicKey
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.PublicKey);
    }

    [Fact]
    public void PublicKey_Valid()
    {
        // Arrange
        var request = new StratisSignatureAuthCallbackBody
        {
            PublicKey = "tQ9RukZsB6bBsenHnGSo1q69CJzWGnxohm"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(r => r.PublicKey);
    }
}