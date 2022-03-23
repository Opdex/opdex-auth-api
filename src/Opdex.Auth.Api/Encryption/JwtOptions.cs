namespace Opdex.Auth.Api.Encryption;

public class JwtOptions
{
    public const string ConfigurationSectionName = "Jwt";

    public string SigningKeyIdentifier { get; set; }
}