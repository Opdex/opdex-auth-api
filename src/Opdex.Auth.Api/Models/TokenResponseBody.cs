using Opdex.Auth.Api.Encryption;

namespace Opdex.Auth.Api.Models;

public record TokenResponseBody(string AccessToken, string RefreshToken)
{
    public ulong ExpiresIn => (ulong)JwtIssuer.TokenLifetime.TotalMilliseconds;
    public string TokenType => "bearer";
}