using System;
using FluentValidation;
using FluentValidation.Validators;
using Opdex.Auth.Domain.Helpers;

namespace Opdex.Auth.Api.Validation;

public class Base64UrlStringValidator<T> : PropertyValidator<T, string>, IBase64UrlStringValidator
{
    public override string Name => "Base64Url";

    public override bool IsValid(ValidationContext<T> context, string? value) =>
        value is not null && Base64Extensions.TryUrlSafeBase64Decode(value, out _);

    protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} must be base-64 URL encoded string.";
}

public interface IBase64UrlStringValidator : IPropertyValidator
{
}

public static class Base64UrlStringValidatorExtensions
{
    /// <summary>
    /// Validates the value is a base-64 URL encoded string.
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustBeBase64UrlEncoded<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new Base64UrlStringValidator<T>());
    }
}
