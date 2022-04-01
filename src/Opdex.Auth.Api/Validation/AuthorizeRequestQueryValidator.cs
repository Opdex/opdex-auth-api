using FluentValidation;
using Opdex.Auth.Api.Models;

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
                    .Must(uri => uri.StartsWith("https://")).WithMessage("Redirect URI must be https");
            });
        RuleFor(request => request.CodeChallenge)
            .NotEmpty().WithMessage("Code challenge must be provided");
        RuleFor(request => request.CodeChallengeMethod)
            .MustBeValidEnumValue().WithMessage("Code challenge method must be plain or S256");
        RuleFor(request => request.State)
            .Must(state => state != "").When(state => state is not null).WithMessage("State must not be empty if provided");
    }
}