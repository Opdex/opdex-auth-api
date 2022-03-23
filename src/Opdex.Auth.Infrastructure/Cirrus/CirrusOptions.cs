namespace Opdex.Auth.Infrastructure.Cirrus;

public class CirrusOptions
{
    public const string Name = "Cirrus";
    
    public string? ApiUrl { get; set; }
    public int? ApiPort { get; set; }
}