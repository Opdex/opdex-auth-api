using System;
using Microsoft.AspNetCore.Mvc;
using Opdex.Auth.Api.Models;

namespace Opdex.Auth.Api.Conventions;

public static class OAuth2ErrorBuilder
{
    public static IActionResult BuildResponse(OAuth2Error error)
    {
        var body = error switch
        {
            OAuth2Error.InvalidClient => new OAuth2ErrorResponseBody("Something wrong"),
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, "OAuth2 error is not recognized")
        };
        return new BadRequestObjectResult(body);
    }
}