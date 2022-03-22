using System.Diagnostics;
using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Polly;
using Polly.Retry;

namespace Opdex.Auth.Infrastructure.Data;

public class DbContext : IDbContext
{
    private readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<MySqlException>(ex => ex.IsTransient)
        .Or<TimeoutException>()
        .WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(x));

    private readonly ILogger<DbContext> _logger;
    private readonly DatabaseSettings _databaseSettings;

    public DbContext(IOptions<DatabaseOptions> databaseOptions, ILogger<DbContext> logger)
    {
        _logger = Guard.Against.Null(logger, nameof(logger));
        Guard.Against.Null(databaseOptions, nameof(databaseOptions));

        var connectionStringBuilder = new MySqlConnectionStringBuilder(databaseOptions.Value.ConnectionString);
        _databaseSettings = new DatabaseSettings(connectionStringBuilder.ConnectionString);
    }

    public async Task<TEntity> ExecuteScalarAsync<TEntity>(DatabaseQuery query)
    {
        Guard.Against.Null(query, nameof(query));
        
        await using var connection = _databaseSettings.Create();
        return await Execute(query, async c => await connection.ExecuteScalarAsync<TEntity>(c));
    }

    public async Task<TEntity> ExecuteFindAsync<TEntity>(DatabaseQuery query)
    {
        Guard.Against.Null(query, nameof(query));
        
        await using var connection = _databaseSettings.Create();
        return await Execute(query, async c => await connection.QuerySingleOrDefaultAsync<TEntity>(c));
    }

    public async Task<int> ExecuteCommandAsync(DatabaseQuery query)
    {
        Guard.Against.Null(query, nameof(query));
        
        await using var connection = _databaseSettings.Create();
        return await Execute(query, async c => await connection.ExecuteAsync(c));
    }

    private async Task<TR> Execute<TR>(DatabaseQuery query, Func<CommandDefinition, Task<TR>> action)
    {
        var command = new CommandDefinition(query.Sql, query.Parameters, commandType: query.Type, cancellationToken: query.Token);

        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await action.Invoke(command);
            stopwatch.Stop();

            using (_logger.BeginScope(new Dictionary<string, object>
                   {
                       { "Command", command.CommandText }
                   }))
            {
                _logger.LogDebug("Executed database query in {ExecutionTimeMs} ms", stopwatch.ElapsedMilliseconds);
            }

            return result;
        });
    }
}