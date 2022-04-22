using Ardalis.GuardClauses;
using MediatR;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class SelectAuthSessionByConnectionIdQueryHandler : IRequestHandler<SelectAuthSessionByConnectionIdQuery, AuthSession?>
{
    private static readonly string SqlQuery =
        @$"SELECT
                {nameof(AuthSessionEntity.Id)},
                {nameof(AuthSessionEntity.Audience)},
                {nameof(AuthSessionEntity.CodeChallenge)},
                {nameof(AuthSessionEntity.CodeChallengeMethod)},
                {nameof(AuthSessionEntity.ConnectionId)}
            FROM auth_session
            WHERE {nameof(AuthSessionEntity.ConnectionId)} = @{nameof(SqlParams.ConnectionId)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;

    public SelectAuthSessionByConnectionIdQueryHandler(IDbContext dbContext)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
    }
    
    public async Task<AuthSession?> Handle(SelectAuthSessionByConnectionIdQuery request, CancellationToken cancellationToken)
    {
        var sqlParams = new SqlParams(request.ConnectionId);

        var query = DatabaseQuery.Create(SqlQuery, sqlParams, cancellationToken);

        var result = await _dbContext.ExecuteFindAsync<AuthSessionEntity?>(query);

        return result is null ? null : new AuthSession(result.Id, result.Audience, result.CodeChallenge, result.CodeChallengeMethod, result.ConnectionId);
    }

    private sealed record SqlParams(string ConnectionId);
}