using Ardalis.GuardClauses;
using Opdex.Auth.Domain;

namespace Opdex.Auth.Infrastructure.Data.Entities;

public class AuthCodeEntity
{
    public AuthCodeEntity()
    {
    }
    
    internal AuthCodeEntity(AuthCode authCode)
    {
        Guard.Against.Null(authCode, nameof(authCode));
        AccessCode = authCode.Value;
        Signer = authCode.Signer;
        Stamp = authCode.Stamp;
        Expiry = authCode.Expiry;
    }
    
    public Guid AccessCode { get; set; }
    public string Signer { get; set;  }
    public Guid Stamp { get; set; }
    public DateTime Expiry { get; set; }
}