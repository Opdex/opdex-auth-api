namespace Opdex.Auth.Infrastructure.Data;

public interface IDbContext
{
    Task<TScalar> ExecuteScalarAsync<TScalar>(DatabaseQuery query);
    Task<TEntity> ExecuteFindAsync<TEntity>(DatabaseQuery query);
    Task<IEnumerable<TEntity>> ExecuteQueryAsync<TEntity>(DatabaseQuery query);
    Task<int> ExecuteCommandAsync(DatabaseQuery query);
}
