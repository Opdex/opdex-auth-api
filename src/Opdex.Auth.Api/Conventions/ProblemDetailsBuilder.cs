using System.Diagnostics;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Opdex.Auth.Api.Conventions;

public static class ProblemDetailsBuilder
{
    public static ProblemDetails PrepareResponse(HttpContext context, int statusCode, string? detail = null)
    {
        Guard.Against.OutOfRange(statusCode, nameof(statusCode), StatusCodes.Status400BadRequest, StatusCodes.Status511NetworkAuthenticationRequired);

        context.Response.ContentType = "application/problem+json; charset=utf-8";
        
        return new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = ReasonPhrases.GetReasonPhrase(statusCode),
            Detail = detail,
            Extensions = { ["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier }
        };
    }
    
    public static IActionResult BuildResponse(HttpContext context, int statusCode, string? detail = null)
    {
        return new ObjectResult(PrepareResponse(context, statusCode, detail)) { StatusCode = statusCode };
    }
}