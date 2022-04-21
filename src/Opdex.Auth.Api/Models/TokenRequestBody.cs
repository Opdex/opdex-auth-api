using System;

namespace Opdex.Auth.Api.Models;

public class TokenRequestBody
{
    public GrantType GrantType { get; set; }
    
    public string RefreshToken { get; set; }
    
    public Guid Code { get; set; }

    public string CodeVerifier { get; set; }
}