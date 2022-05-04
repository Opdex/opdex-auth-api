using System;
using Opdex.Auth.Domain;
using SSAS.NET;

namespace Opdex.Auth.Api.Models;

public class TokenRequestBody
{
    public GrantType GrantType { get; set; }
    
    public Guid? Code { get; set; }

    public string? CodeVerifier { get; set; }

    public StratisId? Sid { get; set; }

    public string? PublicKey { get; set; }
    
    public string? Signature { get; set; }
    
    public string? RefreshToken { get; set; }
}