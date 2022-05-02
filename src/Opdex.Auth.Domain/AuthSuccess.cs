using Ardalis.GuardClauses;
using Opdex.Auth.Domain.Helpers;

namespace Opdex.Auth.Domain;

public class AuthSuccess
{
    public AuthSuccess(string address, string? audience = null)
    {
        Address = Guard.Against.NullOrWhiteSpace(address, nameof(address));
        Audience = audience;
        Expiry = DateTime.UtcNow.AddDays(30);
        Tokens = new Stack<TokenLog>();
    }

    public AuthSuccess(ulong id, string address, string? audience, DateTime expiry, IEnumerable<TokenLog> tokens, bool invalidate = false)
    {
        Id = id;
        Address = address;
        Audience = string.IsNullOrEmpty(audience) ? null : audience;
        Expiry = expiry;
        Valid = !invalidate && expiry > DateTime.UtcNow;
        Tokens = new Stack<TokenLog>(tokens);
    }
    
    public ulong Id { get; }
    public string Address { get; }
    public string? Audience { get; }
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