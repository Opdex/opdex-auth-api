using Ardalis.GuardClauses;
using Opdex.Auth.Domain;

namespace Opdex.Auth.Infrastructure.Data.Entities;

public class AuthSuccessEntity
{
    internal AuthSuccessEntity(AuthSuccess authSuccess)
    {
        Guard.Against.Null(authSuccess, nameof(authSuccess));
        ConnectionId = authSuccess.ConnectionId;
        Signer = authSuccess.Signer;
        Expiry = authSuccess.Expiry;
    }
    
    public string ConnectionId { get; set; }
    public string Signer { get; set; }
    public DateTime Expiry { get; set; }
}