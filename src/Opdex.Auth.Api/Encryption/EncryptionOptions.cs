namespace Opdex.Auth.Api.Encryption;

/// <summary>
/// Configuration options for encryption
/// </summary>
public class EncryptionOptions
{
    /// <summary>
    /// Configuration options section name
    /// </summary>
    public const string Name = "Encryption";

    /// <summary>
    /// Secret encryption key
    /// </summary>
    public string Key { get; set; }
}