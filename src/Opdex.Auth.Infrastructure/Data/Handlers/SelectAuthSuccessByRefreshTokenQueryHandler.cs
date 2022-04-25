using Ardalis.GuardClauses;
using MediatR;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class SelectAuthSuccessByRefreshTokenQueryHandler : IRequestHandler<SelectAuthSuccessByRefreshTokenQuery, AuthSuccess?>
{
    private static readonly string AuthSuccessQuery =
        @$"SELECT
                a.{nameof(AuthSuccessEntity.Id)},
                a.{nameof(AuthSuccessEntity.Audience)},
                a.{nameof(AuthSuccessEntity.Address)},
                a.{nameof(AuthSuccessEntity.Expiry)}
            FROM auth_success as INNER JOIN token_log tl
                ON a.{nameof(AuthSuccessEntity.Id)} = tl.{nameof(TokenLogEntity.AuthSuccessId)}
            WHERE
                tl.{nameof(TokenLogEntity.RefreshToken)} = @{nameof(SqlParams.RefreshToken)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private static readonly string IsLatestRefreshTokenLogQuery =
        @$"SELECT {nameof(TokenLogEntity.RefreshToken)} = @{nameof(SqlParams.RefreshToken)} FROM token_log
            WHERE {nameof(TokenLogEntity.AuthSuccessId)} = (
                SELECT {nameof(TokenLogEntity.AuthSuccessId)} FROM token_log
                WHERE {nameof(TokenLogEntity.RefreshToken)} = @{nameof(SqlParams.RefreshToken)}
            )
            AND {nameof(TokenLogEntity.CreatedAt)} = (
                SELECT MAX({nameof(TokenLogEntity.CreatedAt)}) FROM token_log
                WHERE {nameof(TokenLogEntity.AuthSuccessId)} = (
                    SELECT {nameof(TokenLogEntity.AuthSuccessId)} FROM token_log
                    WHERE {nameof(TokenLogEntity.RefreshToken)} = @{nameof(SqlParams.RefreshToken)}
                )
            );".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;

    public SelectAuthSuccessByRefreshTokenQueryHandler(IDbContext dbContext)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
    }
    
    public async Task<AuthSuccess?> Handle(SelectAuthSuccessByRefreshTokenQuery request, CancellationToken cancellationToken)
    {
        var sqlParams = new SqlParams(request.RefreshToken);
        
        var authSuccessQuery = DatabaseQuery.Create(AuthSuccessQuery, sqlParams, cancellationToken);
        var authSuccessResult = await _dbContext.ExecuteFindAsync<AuthSuccessEntity?>(authSuccessQuery);
        if (authSuccessResult is null) return null;

        var isLatestRefreshTokenQuery = DatabaseQuery.Create(IsLatestRefreshTokenLogQuery, sqlParams, cancellationToken);
        var isLatestRefreshToken = await _dbContext.ExecuteScalarAsync<bool>(isLatestRefreshTokenQuery);

        return new AuthSuccess(authSuccessResult.Audience, authSuccessResult.Address, authSuccessResult.Expiry,
                         Enumerable.Empty<TokenLog>(), !isLatestRefreshToken);
    }
    
    private sealed record SqlParams(string RefreshToken);
}