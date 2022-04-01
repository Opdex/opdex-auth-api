using FluentValidation;
using Opdex.Auth.Api.Models;

namespace Opdex.Auth.Api.Validation;

public class AccessTokenRequestBodyValidator : AbstractValidator<AccessTokenRequestBody>
{
    public AccessTokenRequestBodyValidator()
    {
        RuleFor(request => request.Code)
            .NotEmpty().WithMessage("Authorization code must not be empty.");
        RuleFor(request => request.CodeVerifier)
            .NotEmpty().WithMessage("Code verifier must not be empty.");
    }
}