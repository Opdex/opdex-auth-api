using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class SelectAuthCodeQueryHandler : IRequestHandler<SelectAuthCodeQuery, AuthCode?>
{
    private static readonly string SqlQuery =
        @$"SELECT
                {nameof(AuthCodeEntity.AccessCode)},
                {nameof(AuthCodeEntity.Signer)},
                {nameof(AuthCodeEntity.Expiry)}
            FROM auth_access_code
            WHERE {nameof(AuthCodeEntity.AccessCode)} = @{nameof(SqlParams.AccessCode)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;

    public SelectAuthCodeQueryHandler(IDbContext dbContext)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
    }
    
    public async Task<AuthCode?> Handle(SelectAuthCodeQuery request, CancellationToken cancellationToken)
    {
        var sqlParams = new SqlParams(request.Value);

        var query = DatabaseQuery.Create(SqlQuery, sqlParams, cancellationToken);

        var result = await _dbContext.ExecuteFindAsync<AuthCodeEntity?>(query);

        return result is null ? null : new AuthCode(result.AccessCode, result.Signer, result.Expiry);
    }

    private sealed class SqlParams
    {
        internal SqlParams(Guid accessCode)
        {
            AccessCode = accessCode;
        }

        public Guid AccessCode { get; }
    }
}