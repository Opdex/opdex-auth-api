using MySqlConnector;

namespace Opdex.Auth.Infrastructure.Data;

internal sealed class DatabaseSettings
{
    private readonly string _connectionString;

    public DatabaseSettings(string connectionString)
    {
        _connectionString = connectionString;
    }

    public MySqlConnection Create()
    {
        return new MySqlConnection(_connectionString);
    }
}