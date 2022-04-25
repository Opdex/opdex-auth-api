using Ardalis.GuardClauses;
using Opdex.Auth.Domain.Helpers;

namespace Opdex.Auth.Domain;

public class AuthSuccess
{
    public AuthSuccess(string audience, string address)
    {
        Audience = Guard.Against.NullOrWhiteSpace(audience, nameof(audience));
        Address = Guard.Against.NullOrWhiteSpace(address, nameof(address));
        Expiry = DateTime.UtcNow.AddDays(30);
        Tokens = new Stack<TokenLog>();
    }

    public AuthSuccess(ulong id, string audience, string address, DateTime expiry, IEnumerable<TokenLog> tokens, bool invalidate = false)
    {
        Id = id;
        Audience = audience;
        Address = address;
        Expiry = expiry;
        Valid = !invalidate && expiry > DateTime.UtcNow;
        Tokens = new Stack<TokenLog>(tokens);
    }
    
    public ulong Id { get; }
    public string Audience { get; }
    public string Address { get; }
    public DateTime Expiry { get; }
    public bool Valid { get; }

    public Stack<TokenLog> Tokens { get; }

    public string NewRefreshToken()
    {
        var refreshToken = KeyGenerator.Random(24);
        Tokens.Push(new TokenLog(refreshToken, Id, DateTime.UtcNow));
        return refreshToken;
    }
}