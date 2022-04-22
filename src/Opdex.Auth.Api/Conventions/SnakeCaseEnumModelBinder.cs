using System;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Opdex.Auth.Api.Conventions;

public class SnakeCaseEnumModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        Guard.Against.Null(bindingContext);

        var modelName = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);
        if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;
        
        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);
        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value))return Task.CompletedTask;
        
        // convert snake case to pascal case
        value = value.Split(new [] {"_"}, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);
        
        if (!Enum.TryParse(bindingContext.ModelType, value, out var enumValue))
        {
            bindingContext.ModelState.TryAddModelError(modelName, "Value must be valid for the enumeration");
            return Task.CompletedTask;
        }
        
        bindingContext.Result = ModelBindingResult.Success(enumValue);
        return Task.CompletedTask;
    }
}