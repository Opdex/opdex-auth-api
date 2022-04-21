using System.Text.RegularExpressions;
using FluentValidation;
using Opdex.Auth.Api.Models;

namespace Opdex.Auth.Api.Validation;

public class AccessTokenRequestBodyValidator : AbstractValidator<TokenRequestBody>
{
    private static readonly Regex RefreshTokenRegex = new Regex("^[a-zA-Z0-9]{24}$", RegexOptions.Compiled);
    
    public AccessTokenRequestBodyValidator()
    {
        RuleFor(request => request.GrantType).MustBeValidEnumValue()
            .WithMessage("Grant type must be code or refresh_token");
        
        When(request => request.GrantType == GrantType.Code, () =>
        {
            RuleFor(request => request.Code)
                .NotEmpty().WithMessage("Authorization code must not be empty");
            RuleFor(request => request.CodeVerifier)
                .NotEmpty().WithMessage("Code verifier must not be empty")
                .MinimumLength(43).WithMessage("Code verifier must contain a minimum of 43 characters")
                .MaximumLength(128).WithMessage("Code verifier must contain a maximum of 128 characters")
                .Matches(OAuth2Standards.CodeVerifierAndChallengeRegex).WithMessage("Code verifier must only contain characters defined in RFC7636");
        });
        
        When(request => request.GrantType == GrantType.RefreshToken, () =>
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty().WithMessage("Refresh token must be provided")
                .Matches(RefreshTokenRegex).WithMessage("Refresh token not correctly formed");
        });
    }
}