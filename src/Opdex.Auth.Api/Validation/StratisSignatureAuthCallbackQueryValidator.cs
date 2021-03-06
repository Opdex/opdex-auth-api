using FluentValidation;
using SSAS.NET;

namespace Opdex.Auth.Api.Validation;

public class StratisSignatureAuthCallbackQueryValidator : AbstractValidator<StratisSignatureAuthCallbackQuery>
{
    public StratisSignatureAuthCallbackQueryValidator()
    {
        RuleFor(request => request.Uid)
            .NotEmpty().WithMessage("Unique identifier must not be empty");
        RuleFor(request => request.Exp)
            .MustBeUnixTimestamp().WithMessage("Expiration date must be a unix timestamp");
    }
}