using System;
using FluentValidation;

namespace Opdex.Auth.Api.Validation;

public static class EnumRules
{
    public static IRuleBuilderOptions<T, TEnum> MustBeValidEnumValue<T, TEnum>(this IRuleBuilder<T, TEnum> ruleBuilder) where TEnum : Enum
    {
        return ruleBuilder.Must(value => Enum.IsDefined(value.GetType(), value)).WithMessage("Value must be valid for the the enumeration values.");
    }
}