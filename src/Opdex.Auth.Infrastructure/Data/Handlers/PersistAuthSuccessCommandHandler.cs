using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class PersistAuthSuccessCommandHandler : IRequestHandler<PersistAuthSuccessCommand, bool>
{
    private static readonly string SqlCommand =
        $@"INSERT INTO auth_success (
                {nameof(AuthSuccessEntity.ConnectionId)},
                {nameof(AuthSuccessEntity.Signer)},
                {nameof(AuthSuccessEntity.Expiry)}
              ) VALUES (
                @{nameof(AuthSuccessEntity.ConnectionId)},
                @{nameof(AuthSuccessEntity.Signer)},
                @{nameof(AuthSuccessEntity.Expiry)}
              );".RemoveExcessWhitespace();
    
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

            var command = DatabaseQuery.Create(SqlCommand, authSuccessEntity, CancellationToken.None);

            var result = await _dbContext.ExecuteCommandAsync(command);

            return result > 0;
        }
        catch (Exception ex)
        {
            using (_logger.BeginScope(new Dictionary<string, object>()
                   {
                       { nameof(request.AuthSuccess.ConnectionId), request.AuthSuccess.ConnectionId },
                       { nameof(request.AuthSuccess.Signer), request.AuthSuccess.Signer }
                   }))
            {
                _logger.LogError(ex, $"Failure persisting auth success");
            }
            return false;
        }
    }
}