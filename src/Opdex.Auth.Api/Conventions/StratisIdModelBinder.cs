using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SSAS.NET;

namespace Opdex.Auth.Api.Conventions;

public class StratisIdModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        Guard.Against.Null(bindingContext);

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
        if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;
        
        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value)) return Task.CompletedTask;

        if (!StratisId.TryParse(value, out var stratisId))
        {
            bindingContext.ModelState.TryAddModelError(modelName, "Stratis id could not be parsed");
            return Task.CompletedTask;
        }
        
        bindingContext.Result = ModelBindingResult.Success(stratisId!);
        return Task.CompletedTask;
    }
}