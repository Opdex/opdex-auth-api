namespace Opdex.Auth.Api.Encryption;

public interface IJwtIssuer
{
    string Create(string walletAddress);
}