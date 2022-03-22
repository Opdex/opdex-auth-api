using System.Diagnostics;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Opdex.Auth.Api.Conventions;

public static class ProblemDetailsBuilder
{
    public static IActionResult BuildResponse(HttpContext context, int statusCode, string title, string? detail = null)
    {
        Guard.Against.OutOfRange(statusCode, nameof(statusCode), StatusCodes.Status400BadRequest, StatusCodes.Status511NetworkAuthenticationRequired);
        Guard.Against.NullOrWhiteSpace(title, nameof(title));
        
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Detail = detail,
            Extensions = { ["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier }
        };
        
        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}