using System;
using System.Text.RegularExpressions;
using FluentValidation;
using Opdex.Auth.Api.Models;

namespace Opdex.Auth.Api.Validation;

public class TokenRequestBodyValidator : AbstractValidator<TokenRequestBody>
{
    private static readonly Regex RefreshTokenRegex = new ("^[a-zA-Z0-9]{24}$", RegexOptions.Compiled);
    
    public TokenRequestBodyValidator()
    {
        RuleFor(request => request.GrantType)
            .MustBeValidEnumValue().WithMessage("Grant type must be authorization_code, ssas or refresh_token");
        
        When(request => request.GrantType == GrantType.AuthorizationCode, () =>
        {
            RuleFor(request => request.Code)
                .NotEmpty().WithMessage("Authorization code must not be empty");
            RuleFor(request => request.CodeVerifier)
                .NotEmpty().WithMessage("Code verifier must not be empty")
                .MinimumLength(43).WithMessage("Code verifier must contain a minimum of 43 characters")
                .MaximumLength(128).WithMessage("Code verifier must contain a maximum of 128 characters")
                .Matches(OAuth2Standards.CodeVerifierAndChallengeRegex).WithMessage("Code verifier must only contain characters defined in RFC7636");
        });

        When(request => request.GrantType == GrantType.Sid, () =>
        {
            RuleFor(request => request.Sid)
                .NotNull().WithMessage("Stratis id must not be empty");
            When(request => request.Sid is not null, () =>
            {
                RuleFor(request => request.Sid)
                    .Must(sid => sid!.Expiry != DateTime.MaxValue).WithMessage("Stratis id must contain expiry")
                    .Must(sid => !sid!.Expired).WithMessage("Stratis id must not have expired");
            });
            RuleFor(request => request.PublicKey)
                .NotEmpty().WithMessage("Public key must not be empty")
                .MustBeNetworkAddress().WithMessage("Public key must be a valid address");
            RuleFor(request => request.Signature)
                .NotEmpty().WithMessage("Signature must not be empty")
                .Length(88).WithMessage("Signature should be 88 characters in length")
                .MustBeBase64Encoded().WithMessage("Signature must be base-64 encoded string");
        });
        
        When(request => request.GrantType == GrantType.RefreshToken, () =>
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty().WithMessage("Refresh token must be provided")
                .Matches(RefreshTokenRegex).WithMessage("Refresh token not correctly formed");
        });
    }
}