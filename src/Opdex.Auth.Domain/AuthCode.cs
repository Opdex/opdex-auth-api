using Ardalis.GuardClauses;

namespace Opdex.Auth.Domain;

public class AuthCode
{
    public AuthCode(string address, Guid stamp)
        : this(Guid.NewGuid(), address, stamp, DateTime.UtcNow.AddMinutes(1))
    {
    }

    public AuthCode(Guid value, string address, Guid stamp, DateTime expiry)
    {
        Value = value;
        Signer = Guard.Against.NullOrEmpty(address, nameof(address));
        Stamp = stamp;
        Expiry = Guard.Against.OutOfRange(expiry, nameof(expiry), DateTime.MinValue, DateTime.UtcNow.AddMinutes(1));;
    }

    public Guid Value { get; }
    public string Signer { get; }
    public Guid Stamp { get; }
    public DateTime Expiry { get; }

    public bool Expired => DateTime.UtcNow < Expiry;
}