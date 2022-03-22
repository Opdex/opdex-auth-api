namespace Opdex.Auth.Api.Encryption;

public class JwtOptions
{
    public string Name = "Jwt";
    
    public string SigningKey { get; set; }
}