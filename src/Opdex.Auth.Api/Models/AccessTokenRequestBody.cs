using System;

namespace Opdex.Auth.Api.Models;

public class AccessTokenRequestBody
{
    public Guid Code { get; set; }

    public string CodeVerifier { get; set; }
}