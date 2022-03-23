using Ardalis.GuardClauses;

namespace Opdex.Auth.Domain;

public class AuthSuccess
{
    public AuthSuccess(string connectionId, string signer) : this(connectionId, signer, DateTime.UtcNow.AddMinutes(1))
    {
    }

    public AuthSuccess(string connectionId, string signer, DateTime expiry)
    {
        Guard.Against.NullOrEmpty(connectionId, nameof(connectionId));
        Guard.Against.NullOrEmpty(signer, nameof(signer));
        Guard.Against.OutOfRange(expiry, nameof(expiry), DateTime.MinValue, DateTime.UtcNow.AddMinutes(1));

        ConnectionId = connectionId;
        Signer = signer;
        Expiry = expiry;
    }

    public string ConnectionId { get; }
    public string Signer { get; }
    public DateTime Expiry { get; }
}