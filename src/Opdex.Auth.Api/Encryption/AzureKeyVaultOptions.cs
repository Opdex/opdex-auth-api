namespace Opdex.Auth.Api.Encryption;

public class AzureKeyVaultOptions
{
    public const string ConfigurationSectionName = "Azure:KeyVault";
    
    public string Name { get; set; }
}