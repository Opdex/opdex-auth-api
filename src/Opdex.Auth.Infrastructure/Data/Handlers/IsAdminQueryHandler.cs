using Ardalis.GuardClauses;
using MediatR;
using Opdex.Auth.Domain.Requests;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class IsAdminQueryHandler : IRequestHandler<IsAdminQuery, bool>
{
    private static readonly string SqlQuery =
        $@"SELECT EXISTS(
            SELECT 1
            FROM admin
            WHERE Address = @{nameof(SqlParams.Address)}
            LIMIT 1);"
            .RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;

    public IsAdminQueryHandler(IDbContext dbContext)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
    }
    
    public async Task<bool> Handle(IsAdminQuery request, CancellationToken cancellationToken)
    {
        var query = DatabaseQuery.Create(SqlQuery, new SqlParams(request.WalletAddress), cancellationToken); 
        return await _dbContext.ExecuteScalarAsync<bool>(query);
    }

    private sealed class SqlParams
    {
        internal SqlParams(string address)
        {
            Address = address;
        }

        public string Address { get; }
    }
}