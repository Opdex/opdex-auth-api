using Ardalis.GuardClauses;
using Opdex.Auth.Domain;

namespace Opdex.Auth.Infrastructure.Data.Entities;

public class AuthCodeEntity
{
    internal AuthCodeEntity(AuthCode authCode)
    {
        Guard.Against.Null(authCode, nameof(authCode));
        AccessCode = authCode.Value;
        Signer = authCode.Signer;
        Expiry = authCode.Expiry;
    }
    
    public Guid AccessCode { get; }
    public string Signer { get; }
    public DateTime Expiry { get; }
}