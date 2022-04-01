using System.Globalization;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Opdex.Auth.Api.Conventions;

public class SnakeCaseValueProviderFactory : IValueProviderFactory
{
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        Guard.Against.Null(context);
        context.ValueProviders.Add(new SnakeCaseQueryValueProvider(BindingSource.Query, context.ActionContext.HttpContext.Request.Query, CultureInfo.InvariantCulture));
        return Task.CompletedTask;
    }
}