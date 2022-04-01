using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using O9d.Json.Formatting;

namespace Opdex.Auth.Api.Conventions;

public class SnakeCaseQueryValueProvider : QueryStringValueProvider
{
    public SnakeCaseQueryValueProvider(BindingSource bindingSource, IQueryCollection values, CultureInfo? culture)
        : base(bindingSource, values, culture)
    {
    }

    public override bool ContainsPrefix(string prefix) => base.ContainsPrefix(prefix.ToSnakeCase());

    public override ValueProviderResult GetValue(string key) => base.GetValue(key.ToSnakeCase());
}