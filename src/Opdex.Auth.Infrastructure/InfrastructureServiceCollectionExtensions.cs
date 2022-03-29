using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Opdex.Auth.Domain.Cirrus;
using Opdex.Auth.Infrastructure.Cirrus;
using Opdex.Auth.Infrastructure.Data;

namespace Opdex.Auth.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
        SqlMapper.AddTypeHandler(new MySqlGuidTypeHandler());
        
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.ConfigurationSectionName));
        services.AddTransient<IDbContext, DbContext>();
        services.Configure<CirrusOptions>(configuration.GetSection(CirrusOptions.ConfigurationSectionName));
        services.AddHttpClient<IWalletModule, WalletModule>();
    }
}