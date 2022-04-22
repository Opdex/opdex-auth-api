using Opdex.Auth.Api.Encryption;

namespace Opdex.Auth.Api.Models;

public record TokenResponseBody(string AccessToken, string RefreshToken)
{
    public ulong ExpiresIn => (ulong)JwtIssuer.TokenLifetime.TotalSeconds;
    public string TokenType => "bearer";
}