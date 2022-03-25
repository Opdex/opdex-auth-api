namespace Opdex.Auth.Api.Encryption;

public class JwtOptions
{
    public const string ConfigurationSectionName = "Jwt";

    public string SigningKeyName { get; set; }

    public string SigningKeyVersion { get; set; }

    public string SigningKeyIdentifier => $"{SigningKeyName}/{SigningKeyVersion}";
}