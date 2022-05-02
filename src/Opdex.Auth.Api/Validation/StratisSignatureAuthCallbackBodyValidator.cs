using FluentValidation;
using SSAS.NET;

namespace Opdex.Auth.Api.Validation;

public class StratisSignatureAuthCallbackBodyValidator : AbstractValidator<StratisSignatureAuthCallbackBody>
{
    public StratisSignatureAuthCallbackBodyValidator()
    {
        RuleFor(request => request.Signature)
            .NotEmpty().WithMessage("Signature must not be empty")
            .Length(88).WithMessage("Signature should be 88 characters in length")
            .MustBeBase64Encoded().WithMessage("Signature must be base-64 encoded string");
        RuleFor(request => request.PublicKey)
            .NotEmpty().WithMessage("Public key must not be empty")
            .MustBeNetworkAddress().WithMessage("Public key must be a valid address");
    }
}