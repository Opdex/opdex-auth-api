using Ardalis.GuardClauses;

namespace Opdex.Auth.Domain;

public class AuthCode
{
    public AuthCode(string address) : this(Guid.NewGuid(), address, DateTime.UtcNow.AddMinutes(1))
    {
    }

    public AuthCode(Guid value, string address, DateTime expiry)
    {
        Value = value;
        Signer = Guard.Against.NullOrEmpty(address, nameof(address));
        Expiry = Guard.Against.OutOfRange(expiry, nameof(expiry), DateTime.MinValue, DateTime.UtcNow.AddMinutes(1));;
    }

    public Guid Value { get; }
    public string Signer { get; }
    public DateTime Expiry { get; }
}