using System.Data;

namespace Opdex.Auth.Infrastructure.Data;

public readonly struct DatabaseQuery
{
    internal string Sql { get; }
    internal object? Parameters { get; }
    internal CommandType Type { get; }
    internal CancellationToken Token { get; }

    private DatabaseQuery(string sql, object? parameters = null, CancellationToken token = default, CommandType type = CommandType.Text)
    {
        Sql = sql;
        Parameters = parameters;
        Token = token;
        Type = type;
    }

    internal static DatabaseQuery Create(string sql, CancellationToken token = default)
    {
        return new DatabaseQuery(sql, token: token);
    }

    internal static DatabaseQuery Create(string sql, object? parameters, CancellationToken token = default)
    {
        return new DatabaseQuery(sql, parameters, token);
    }

    internal static DatabaseQuery Create(string sql, CommandType type, object? parameters = null, CancellationToken token = default)
    {
        return new DatabaseQuery(sql, parameters, token, type);
    }
}