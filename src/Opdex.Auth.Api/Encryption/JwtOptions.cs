namespace Opdex.Auth.Api.Encryption;

public class JwtOptions
{
    public const string Name = "Jwt";

    public string SigningKey { get; set; }
}