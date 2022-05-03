using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class DeleteAuthSessionCommandHandler : AsyncRequestHandler<DeleteAuthSessionCommand>
{
    private static readonly string SqlCommand =
        $@"DELETE FROM auth_session
            WHERE {nameof(AuthSessionEntity.Id)} = @{nameof(AuthSessionEntity.Id)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;
    private readonly ILogger<DeleteAuthSessionCommandHandler> _logger;

    public DeleteAuthSessionCommandHandler(IDbContext dbContext, ILogger<DeleteAuthSessionCommandHandler> logger)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
        _logger = Guard.Against.Null(logger, nameof(logger));
    }
    
    protected override async Task Handle(DeleteAuthSessionCommand request, CancellationToken cancellationToken)
    {
        var command = DatabaseQuery.Create(SqlCommand, new AuthSessionEntity(request.AuthSession), CancellationToken.None);
        
        try
        {
            await _dbContext.ExecuteCommandAsync(command);
        }
        catch (Exception ex)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
                   {
                       { nameof(request.AuthSession.Stamp), request.AuthSession.Stamp },
                       { nameof(request.AuthSession.ConnectionId), request.AuthSession.ConnectionId! }
                   }))
            {
                _logger.LogError(ex, $"Failure deleting auth session");
            }
        }
    }
}