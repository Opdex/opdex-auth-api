using Ardalis.GuardClauses;
using MediatR;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class SelectAuthSuccessByTargetQueryHandler : IRequestHandler<SelectAuthSuccessByTargetQuery, AuthSuccess?>
{
    private static readonly string AuthSuccessQuery =
        @$"SELECT
                {nameof(AuthSuccessEntity.Id)},
                {nameof(AuthSuccessEntity.Audience)},
                {nameof(AuthSuccessEntity.Address)},
                {nameof(AuthSuccessEntity.Expiry)}
            FROM auth_success
            WHERE
                {nameof(AuthSuccessEntity.Audience)} = @{nameof(AuthSuccessQuerySqlParams.Audience)}
            AND {nameof(AuthSuccessEntity.Address)} = @{nameof(AuthSuccessQuerySqlParams.Address)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private static readonly string TokenLogQuery =
        @$"SELECT
                {nameof(TokenLogEntity.RefreshToken)},
                {nameof(TokenLogEntity.CreatedAt)},
            FROM token_log
            WHERE
                {nameof(TokenLogEntity.AuthSuccessId)} = @{nameof(TokenLogQuerySqlParams.AuthSuccessId)}
            ORDER BY {nameof(TokenLogEntity.CreatedAt)} ASC;".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;

    public SelectAuthSuccessByTargetQueryHandler(IDbContext dbContext)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
    }
    
    public async Task<AuthSuccess?> Handle(SelectAuthSuccessByTargetQuery request, CancellationToken cancellationToken)
    {
        var authSuccessQuery = DatabaseQuery.Create(AuthSuccessQuery, new AuthSuccessQuerySqlParams(request.Audience, request.Signer), cancellationToken);

        var result = await _dbContext.ExecuteFindAsync<AuthSuccessEntity?>(authSuccessQuery);

        if (result is null) return null;

        var tokenQuery = DatabaseQuery.Create(TokenLogQuery, new TokenLogQuerySqlParams(result.Id), cancellationToken);
        var tokenLogs = await _dbContext.ExecuteQueryAsync<TokenLogEntity>(tokenQuery);
        return new AuthSuccess(result.Audience, result.Address, result.Expiry, tokenLogs.Select(t => new TokenLog(t.RefreshToken, t.CreatedAt)));
    }

    private sealed record AuthSuccessQuerySqlParams(string Audience, string Address);

    private sealed record TokenLogQuerySqlParams(ulong AuthSuccessId);
}