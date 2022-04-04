using System.Globalization;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Opdex.Auth.Api.Conventions;

public class SnakeCaseValueProviderFactory : IValueProviderFactory
{
    public async Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        Guard.Against.Null(context);
        var request = context.ActionContext.HttpContext.Request;
        
        context.ValueProviders.Add(new SnakeCaseQueryValueProvider(BindingSource.Query, request.Query, CultureInfo.InvariantCulture));

        if (!request.HasFormContentType) return;
        
        var form = await request.ReadFormAsync();
        context.ValueProviders.Add(new SnakeCaseFormValueProvider(BindingSource.Form, form, CultureInfo.InvariantCulture));
    }
}