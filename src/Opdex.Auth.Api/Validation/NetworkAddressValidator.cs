using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

namespace Opdex.Auth.Api.Validation;

public class NetworkAddressValidator<T> : PropertyValidator<T, string?>, INetworkAddressValidator
{
    private readonly Regex _addressRegex = new("^[a-km-zA-HJ-NP-Z1-9]+$", RegexOptions.Compiled);
    
    public override string Name => "NetworkAddress";

    public override bool IsValid(ValidationContext<T> context, string? value)
        => value is not null &&  _addressRegex.IsMatch(value);

    protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} must be valid address.";
}

public interface INetworkAddressValidator : IPropertyValidator
{
}

public static class NetworkAddressValidatorExtensions
{
    /// <summary>
    /// Validates the value is a network address.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustBeNetworkAddress<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder.SetValidator(new NetworkAddressValidator<T>());
    }
}