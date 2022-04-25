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
            FROM auth_success a INNER JOIN token_log tl
                ON a.{nameof(AuthSuccessEntity.Id)} = tl.{nameof(TokenLogEntity.AuthSuccessId)}
            WHERE
                tl.{nameof(TokenLogEntity.RefreshToken)} = @{nameof(SqlParams.RefreshToken)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private static readonly string LatestRefreshTokenQuery =
        @$"SELECT
                {nameof(TokenLogEntity.RefreshToken)},
                {nameof(TokenLogEntity.AuthSuccessId)},
                {nameof(TokenLogEntity.CreatedAt)}
            FROM token_log
            ORDER BY {nameof(TokenLogEntity.CreatedAt)} DESC
            LIMIT 1;".RemoveExcessWhitespace();
    
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

        var latestTokenLogQuery = DatabaseQuery.Create(LatestRefreshTokenQuery, cancellationToken);
        var latestTokenLog = await _dbContext.ExecuteFindAsync<TokenLogEntity>(latestTokenLogQuery);
        var isLatestRefreshToken = latestTokenLog.RefreshToken == request.RefreshToken;

        return new AuthSuccess(authSuccessResult.Id, authSuccessResult.Audience, authSuccessResult.Address, authSuccessResult.Expiry,
                         new []{ new TokenLog(latestTokenLog.RefreshToken, latestTokenLog.AuthSuccessId, latestTokenLog.CreatedAt) }, !isLatestRefreshToken);
    }
    
    private sealed record SqlParams(string RefreshToken);
}