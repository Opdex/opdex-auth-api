using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SSAS.NET;

namespace Opdex.Auth.Api.Conventions;

public class StratisIdModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        Guard.Against.Null(context);
        return context.Metadata.ModelType == typeof(StratisId) ? new StratisIdModelBinder() : null;
    }
}