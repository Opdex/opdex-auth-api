namespace Opdex.Auth.Infrastructure.Cirrus;

public class CirrusOptions
{
    public const string ConfigurationSectionName = "Cirrus";
    
    public string? ApiUrl { get; set; }
    public int? ApiPort { get; set; }
}