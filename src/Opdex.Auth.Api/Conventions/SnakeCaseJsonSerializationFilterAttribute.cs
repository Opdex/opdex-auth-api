using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using O9d.Json.Formatting;

namespace Opdex.Auth.Api.Conventions;

public class SnakeCaseJsonSerializationFilterAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var serializerSettings = new JsonSerializerOptions { PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy() };
            var jsonOutputFormatter = new SystemTextJsonOutputFormatter(serializerSettings);
            objectResult.Formatters.Add(jsonOutputFormatter);
        }
        
        base.OnResultExecuting(context);
    }
}