using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class PersistAuthCodeCommandHandler : IRequestHandler<PersistAuthCodeCommand, bool>
{
    private static readonly string SqlCommand =
        $@"INSERT INTO auth_access_code (
                {nameof(AuthCodeEntity.AccessCode)},
                {nameof(AuthCodeEntity.Signer)},
                {nameof(AuthCodeEntity.Stamp)},
                {nameof(AuthCodeEntity.Expiry)}
              ) VALUES (
                @{nameof(AuthCodeEntity.AccessCode)},
                @{nameof(AuthCodeEntity.Signer)},
                @{nameof(AuthCodeEntity.Stamp)},
                @{nameof(AuthCodeEntity.Expiry)}
              );".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;
    private readonly ILogger<PersistAuthCodeCommandHandler> _logger;

    public PersistAuthCodeCommandHandler(IDbContext dbContext, ILogger<PersistAuthCodeCommandHandler> logger)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
        _logger = Guard.Against.Null(logger, nameof(logger));
    }
    
    public async Task<bool> Handle(PersistAuthCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var authCodeEntity = new AuthCodeEntity(request.AuthCode);

            var command = DatabaseQuery.Create(SqlCommand, authCodeEntity, CancellationToken.None);

            var result = await _dbContext.ExecuteCommandAsync(command);

            return result > 0;
        }
        catch (Exception ex)
        {
            using (_logger.BeginScope(new Dictionary<string, object>()
                   {
                       { nameof(request.AuthCode.Signer), request.AuthCode.Signer },
                       { nameof(request.AuthCode.Stamp), request.AuthCode.Stamp },
                   }))
            {
                _logger.LogError(ex, $"Failure persisting auth code");
            }
            return false;
        }
    }
}