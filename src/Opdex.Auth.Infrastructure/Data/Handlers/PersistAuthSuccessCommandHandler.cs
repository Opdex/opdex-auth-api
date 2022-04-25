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
                @{nameof(TokenLogParams.Audience)},
                @{nameof(TokenLogParams.Address)},
                @{nameof(TokenLogParams.Expiry)}
            );
            INSERT INTO token_log (
                {nameof(TokenLogEntity.RefreshToken)},
                {nameof(TokenLogEntity.AuthSuccessId)},
                {nameof(TokenLogEntity.CreatedAt)}
            ) VALUES (
                @{nameof(TokenLogParams.RefreshToken)},
                LAST_INSERT_ID(),
                @{nameof(TokenLogParams.CreatedAt)}
            );".RemoveExcessWhitespace();

    private static readonly string ApplyNewTokenCommand =
        $@"INSERT INTO token_log (
                {nameof(TokenLogEntity.RefreshToken)},
                {nameof(TokenLogEntity.AuthSuccessId)},
                {nameof(TokenLogEntity.CreatedAt)}
            ) VALUES (
                @{nameof(TokenLogParams.RefreshToken)},
                @{nameof(TokenLogParams.AuthSuccessId)},
                @{nameof(TokenLogParams.CreatedAt)}
            );
            UPDATE auth_success
                SET
                    {nameof(AuthSuccessEntity.Expiry)} = @{nameof(TokenLogParams.Expiry)}
                WHERE {nameof(AuthSuccessEntity.Id)} = @{nameof(TokenLogParams.AuthSuccessId)};".RemoveExcessWhitespace();
    
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
            var tokenEntity = new TokenLogEntity(request.AuthSuccess.Tokens.Peek());

            var sql = request.AuthSuccess.Id == 0 ? InsertSqlCommand : ApplyNewTokenCommand;
            
            var command = DatabaseQuery.Create(sql, new TokenLogParams(authSuccessEntity, tokenEntity), CancellationToken.None);

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

    private sealed record TokenLogParams
    {
        public TokenLogParams(AuthSuccessEntity authSuccessEntity, TokenLogEntity tokenLogEntity)
        {
            Audience = authSuccessEntity.Audience;
            Address = authSuccessEntity.Address;
            Expiry = authSuccessEntity.Expiry;
            RefreshToken = tokenLogEntity.RefreshToken;
            AuthSuccessId = tokenLogEntity.AuthSuccessId;
            CreatedAt = tokenLogEntity.CreatedAt;
        }
        
        public string Audience { get; }
        public string Address { get; }
        public DateTime Expiry { get; }
        public string RefreshToken { get; }
        public ulong AuthSuccessId { get; }
        public DateTime CreatedAt { get; }
    }
}