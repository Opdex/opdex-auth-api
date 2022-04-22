using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Opdex.Auth.Api.Conventions;

public class SnakeCaseModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        Guard.Against.Null(context);

        return context.Metadata.IsEnum ? new SnakeCaseEnumModelBinder() : null;
    }
}