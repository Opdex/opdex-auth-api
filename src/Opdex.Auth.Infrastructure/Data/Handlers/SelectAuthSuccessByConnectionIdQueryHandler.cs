using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class SelectAuthSuccessByConnectionIdQueryHandler : IRequestHandler<SelectAuthSuccessByConnectionIdQuery, AuthSuccess?>
{
    private static readonly string SqlQuery =
        @$"SELECT
                {nameof(AuthSuccessEntity.ConnectionId)},
                {nameof(AuthSuccessEntity.Signer)},
                {nameof(AuthSuccessEntity.Expiry)}
            FROM auth_success
            WHERE {nameof(AuthSuccessEntity.ConnectionId)} = @{nameof(SqlParams.ConnectionId)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;
    private readonly ILogger<SelectAuthSuccessByConnectionIdQueryHandler> _logger;

    public SelectAuthSuccessByConnectionIdQueryHandler(IDbContext dbContext, ILogger<SelectAuthSuccessByConnectionIdQueryHandler> logger)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
        _logger = Guard.Against.Null(logger, nameof(logger));
    }
    
    public async Task<AuthSuccess?> Handle(SelectAuthSuccessByConnectionIdQuery request, CancellationToken cancellationToken)
    {
        var sqlParams = new SqlParams(request.ConnectionId);

        var query = DatabaseQuery.Create(SqlQuery, sqlParams, cancellationToken);

        var result = await _dbContext.ExecuteFindAsync<AuthSuccessEntity?>(query);

        return result is null ? null : new AuthSuccess(result.ConnectionId, result.Signer, result.Expiry);
    }

    private sealed class SqlParams
    {
        internal SqlParams(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public string ConnectionId { get; }
    }
}