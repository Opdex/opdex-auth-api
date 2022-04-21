using Ardalis.GuardClauses;
using MediatR;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class SelectAuthCodeByValueQueryHandler : IRequestHandler<SelectAuthCodeByValueQuery, AuthCode?>
{
    private static readonly string SqlQuery =
        @$"SELECT
                {nameof(AuthCodeEntity.AccessCode)},
                {nameof(AuthCodeEntity.Signer)},
                {nameof(AuthCodeEntity.Stamp)},
                {nameof(AuthCodeEntity.Expiry)}
            FROM auth_access_code
            WHERE {nameof(AuthCodeEntity.AccessCode)} = @{nameof(SqlParams.AccessCode)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;

    public SelectAuthCodeByValueQueryHandler(IDbContext dbContext)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
    }
    
    public async Task<AuthCode?> Handle(SelectAuthCodeByValueQuery request, CancellationToken cancellationToken)
    {
        var sqlParams = new SqlParams(request.Value);

        var query = DatabaseQuery.Create(SqlQuery, sqlParams, cancellationToken);

        var result = await _dbContext.ExecuteFindAsync<AuthCodeEntity?>(query);

        return result is null ? null : new AuthCode(result.AccessCode, result.Signer, result.Stamp, result.Expiry);
    }

    private sealed record SqlParams(Guid AccessCode);
}