using System;
using FluentValidation;
using Opdex.Auth.Api.Models;
using Opdex.Auth.Domain;

namespace Opdex.Auth.Api.Validation;

public class AuthorizeRequestQueryValidator : AbstractValidator<AuthorizeRequestQuery>
{
    public AuthorizeRequestQueryValidator()
    {
        RuleFor(request => request.RedirectUri)
            .NotEmpty().WithMessage("Redirect URI must be provided")
            .DependentRules(() =>
            {
                RuleFor(request => request.RedirectUri)
                    .MaximumLength(255)
                    .WithMessage("Redirect URI can only contain a maximum of 255 characters")
                    .Must(redirectUri => Uri.TryCreate(redirectUri, UriKind.Absolute, out _))
                    .WithMessage("Redirect URI must be absolute URL")
                    .Must(uri => uri.StartsWith("https://")).WithMessage("Redirect URI must be https")
                    .Must(uri => !uri.Contains('#')).WithMessage("Redirect URI must not contain fragment");
            });

        RuleFor(request => request.CodeChallenge)
            .NotEmpty().WithMessage("Code challenge must be provided")
            .DependentRules(() =>
            {
                RuleFor(request => request.CodeChallenge)
                    .MinimumLength(43).WithMessage("Code challenge must contain a minimum of 43 characters")
                    .MaximumLength(128).WithMessage("Code challenge must contain a maximum of 128 characters")
                    .Matches(OAuth2Standards.CodeVerifierAndChallengeRegex).WithMessage("Code challenge must only contain characters defined in RFC7636")
                    .When(request => request.CodeChallengeMethod == CodeChallengeMethod.Plain);
                RuleFor(request => request.CodeChallenge)
                    .MustBeBase64UrlEncoded().WithMessage("Code challenge must be base-64 URL encoded for method S256")
                    .When(request => request.CodeChallengeMethod == CodeChallengeMethod.S256);
                // length without base64url padding
                RuleFor(request => request.CodeChallenge)
                    .Length(86).WithMessage("Code challenge must be base-64 URL encoded SHA256 hash for method S256")
                    .Matches(OAuth2Standards.CodeVerifierAndChallengeRegex).WithMessage("Code challenge must only contain characters defined in RFC7636")
                    .When(request => !request.CodeChallenge.EndsWith("==") && request.CodeChallengeMethod == CodeChallengeMethod.S256);
                // length with base64url padding
                RuleFor(request => request.CodeChallenge)
                    .Length(88).WithMessage("Code challenge must be base-64 URL encoded SHA256 hash for method S256")
                    .Matches(OAuth2Standards.Base64UrlEncodedCodeChallenge).WithMessage("Code challenge must only contain characters defined in RFC7636")
                    .When(request => request.CodeChallenge.EndsWith("==") && request.CodeChallengeMethod == CodeChallengeMethod.S256);
            });
        
        RuleFor(request => request.CodeChallengeMethod)
            .MustBeValidEnumValue().WithMessage("Code challenge method must be plain or S256");
        
        RuleFor(request => request.State)
            .MustContainOnlyAscii().WithMessage("State must only contain ASCII characters");
    }
}