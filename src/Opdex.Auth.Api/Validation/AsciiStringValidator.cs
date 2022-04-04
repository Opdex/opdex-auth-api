using System;
using System.Text;
using FluentValidation;
using FluentValidation.Validators;

namespace Opdex.Auth.Api.Validation;

public class AsciiStringValidator<T> : PropertyValidator<T, string?>, IAsciiStringValidator
{
    public override string Name => "Ascii";

    public override bool IsValid(ValidationContext<T> context, string? value) =>
        value is null || Encoding.UTF8.GetByteCount(value) == value.Length;

    protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} must contain only ASCII characters.";
}

public interface IAsciiStringValidator : IPropertyValidator
{
}

public static class AsciiStringValidatorExtensions
{
    /// <summary>
    /// Validates the value is an ASCII string.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustContainOnlyAscii<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new AsciiStringValidator<T>());
    }
}
