namespace Opdex.Auth.Infrastructure.Data;

public class DatabaseOptions
{
    public const string ConfigurationSectionName = "Database";
    
    public string ConnectionString { get; set; }
}