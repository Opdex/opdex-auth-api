using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class PersistAuthSuccessCommandHandler : IRequestHandler<PersistAuthSuccessCommand, bool>
{
    private static readonly string InsertSqlCommand =
        $@"INSERT INTO auth_success (
                {nameof(AuthSuccessEntity.Audience)},
                {nameof(AuthSuccessEntity.Address)},
                {nameof(AuthSuccessEntity.Expiry)}
            ) VALUES (
                @{nameof(AuthSuccessEntity.Audience)},
                @{nameof(AuthSuccessEntity.Address)},
                @{nameof(AuthSuccessEntity.Expiry)}
            );
            INSERT INTO token_log (
                {nameof(TokenLogEntity.RefreshToken)},
                {nameof(TokenLogEntity.AuthSuccessId)},
                {nameof(TokenLogEntity.CreatedAt)}
            ) VALUES (
                @{nameof(TokenLogEntity.RefreshToken)},
                LAST_INSERT_ID(),
                @{nameof(TokenLogEntity.CreatedAt)}
            );".RemoveExcessWhitespace();

    private static readonly string ApplyNewTokenCommand =
        $@"INSERT INTO token_log (
                {nameof(TokenLogEntity.RefreshToken)},
                {nameof(TokenLogEntity.AuthSuccessId)},
                {nameof(TokenLogEntity.CreatedAt)}
            ) VALUES (
                @{nameof(TokenLogEntity.RefreshToken)},
                @{nameof(TokenLogEntity.AuthSuccessId)},
                @{nameof(TokenLogEntity.CreatedAt)}
            );
            UPDATE auth_success
                SET
                    {nameof(AuthSuccessEntity.Expiry)} = @{nameof(AuthSuccessEntity.Expiry)}
                WHERE {nameof(AuthSuccessEntity.Id)} = {nameof(TokenLogEntity.AuthSuccessId)};".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;
    private readonly ILogger<PersistAuthSuccessCommandHandler> _logger;

    public PersistAuthSuccessCommandHandler(IDbContext dbContext, ILogger<PersistAuthSuccessCommandHandler> logger)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
        _logger = Guard.Against.Null(logger, nameof(logger));
    }

    public async Task<bool> Handle(PersistAuthSuccessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var authSuccessEntity = new AuthSuccessEntity(request.AuthSuccess);
            var tokenEntity = new TokenLogEntity(authSuccessEntity.Id, request.AuthSuccess.Tokens.Peek());

            var sql = authSuccessEntity.Id == 0 ? InsertSqlCommand : ApplyNewTokenCommand;
            
            var command = DatabaseQuery.Create(sql, new { authSuccessEntity, tokenEntity }, CancellationToken.None);

            var result = await _dbContext.ExecuteCommandAsync(command);

            return result > 0;
        }
        catch (Exception ex)
        {
            var customProperties = new Dictionary<string, object>
            {
                { nameof(request.AuthSuccess.Audience), request.AuthSuccess.Audience },
                { nameof(request.AuthSuccess.Address), request.AuthSuccess.Address },
            };
            
            using (_logger.BeginScope(customProperties))
            {
                _logger.LogError(ex, $"Failure persisting auth success");
            }

            return false;
        }
    }
}