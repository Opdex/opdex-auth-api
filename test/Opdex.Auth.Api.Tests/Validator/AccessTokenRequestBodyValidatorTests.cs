using System;
using FluentValidation.TestHelper;
using Opdex.Auth.Api.Models;
using Opdex.Auth.Api.Validation;
using Xunit;

namespace Opdex.Auth.Api.Tests.Validator;

public class AccessTokenRequestBodyValidatorTests
{
    private readonly AccessTokenRequestBodyValidator _validator;

    public AccessTokenRequestBodyValidatorTests()
    {
        _validator = new AccessTokenRequestBodyValidator();
    }

    [Fact]
    public void AuthCode_Invalid()
    {
        // Arrange
        var request = new AccessTokenRequestBody();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(r => r.Code);
    }

    [Fact]
    public void AuthCode_Valid()
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
}