using Ardalis.GuardClauses;
using MediatR;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class SelectAuthSessionByIdQueryHandler : IRequestHandler<SelectAuthSessionByIdQuery, AuthSession?>
{
    private static readonly string SqlQuery =
        @$"SELECT
                {nameof(AuthSessionEntity.Id)},
                {nameof(AuthSessionEntity.CodeChallenge)},
                {nameof(AuthSessionEntity.CodeChallengeMethod)},
                {nameof(AuthSessionEntity.ConnectionId)}
            FROM auth_session
            WHERE {nameof(AuthSessionEntity.Id)} = @{nameof(SqlParams.Stamp)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;

    public SelectAuthSessionByIdQueryHandler(IDbContext dbContext)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
    }
    
    public async Task<AuthSession?> Handle(SelectAuthSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var sqlParams = new SqlParams(request.Id);

        var query = DatabaseQuery.Create(SqlQuery, sqlParams, cancellationToken);

        var result = await _dbContext.ExecuteFindAsync<AuthSessionEntity?>(query);

        return result is null ? null : new AuthSession(result.Id, result.CodeChallenge, result.CodeChallengeMethod, result.ConnectionId);
    }

    private sealed class SqlParams
    {
        internal SqlParams(Guid stamp)
        {
            Stamp = stamp;
        }

        public Guid Stamp { get; }
    }
}