using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

namespace Opdex.Auth.Api.Conventions;

public class ProblemDetailsApiVersionErrorProvider : IErrorResponseProvider
{
    public IActionResult CreateResponse(ErrorResponseContext context)
    {
        Guard.Against.Null(context);
        return ProblemDetailsBuilder.BuildResponse(context.Request.HttpContext, StatusCodes.Status400BadRequest, "Unsupported API version");
    }
}