using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class PersistAuthSessionCommandHandler : IRequestHandler<PersistAuthSessionCommand, bool>
{
    private static readonly string InsertSqlCommand =
        $@"INSERT INTO auth_session (
                {nameof(AuthSessionEntity.Id)},
                {nameof(AuthSessionEntity.Challenge)},
                {nameof(AuthSessionEntity.ChallengeMethod)}
              ) VALUES (
                @{nameof(AuthSessionEntity.Id)},
                @{nameof(AuthSessionEntity.Challenge)},
                @{nameof(AuthSessionEntity.ChallengeMethod)}
              );".RemoveExcessWhitespace();

    private static readonly string UpdateSqlCommand =
        $@"UPDATE auth_session
                SET
                    {nameof(AuthSessionEntity.ConnectionId)} = @{nameof(AuthSessionEntity.ConnectionId)}
                WHERE {nameof(AuthSessionEntity.Id)} = @{nameof(AuthSessionEntity.Id)};".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;
    private readonly ILogger<PersistAuthSessionCommandHandler> _logger;

    public PersistAuthSessionCommandHandler(IDbContext dbContext, ILogger<PersistAuthSessionCommandHandler> logger)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
        _logger = Guard.Against.Null(logger, nameof(logger));
    }
    
    public async Task<bool> Handle(PersistAuthSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = new AuthSessionEntity(request.AuthSession);

            var sql = entity.ConnectionId is null ? InsertSqlCommand : UpdateSqlCommand;
            
            var command = DatabaseQuery.Create(sql, entity, CancellationToken.None);

            var result = await _dbContext.ExecuteCommandAsync(command);

            return result > 0;
        }
        catch (Exception ex)
        {
            var customProperties = new Dictionary<string, object>
            {
                { nameof(request.AuthSession.Stamp), request.AuthSession.Stamp }
            };
            
            if (request.AuthSession.ConnectionId is not null)
                customProperties.Add(nameof(request.AuthSession.ConnectionId), request.AuthSession.ConnectionId);
            
            using (_logger.BeginScope(customProperties))
            {
                _logger.LogError(ex, $"Failure persisting auth session");
            }

            return false;
        }
    }
}