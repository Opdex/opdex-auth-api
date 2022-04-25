using Ardalis.GuardClauses;
using Opdex.Auth.Domain;

namespace Opdex.Auth.Infrastructure.Data.Entities;

public class AuthSuccessEntity
{
    public AuthSuccessEntity()
    {
    }
    
    internal AuthSuccessEntity(AuthSuccess authSuccess)
    {
        Guard.Against.Null(authSuccess, nameof(authSuccess));
        Id = authSuccess.Id;
        Audience = authSuccess.Audience;
        Address = authSuccess.Address;
        Expiry = authSuccess.Expiry;
    }
    
    public ulong Id { get; set; }
    public string Audience { get; set; }
    public string Address { get; set; }
    public DateTime Expiry { get; set; }
}