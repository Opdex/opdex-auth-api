using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class DeleteAuthSuccessCommandHandler : AsyncRequestHandler<DeleteAuthSuccessCommand>
{
    private static readonly string SqlCommand =
        $@"DELETE FROM auth_success
            WHERE {nameof(AuthSuccessEntity.Address)} = @{nameof(AuthSuccessEntity.Address)}
            AND {nameof(AuthSuccessEntity.Audience)} = @{nameof(AuthSuccessEntity.Audience)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;
    private readonly ILogger<DeleteAuthSuccessCommandHandler> _logger;

    public DeleteAuthSuccessCommandHandler(IDbContext dbContext, ILogger<DeleteAuthSuccessCommandHandler> logger)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
        _logger = Guard.Against.Null(logger, nameof(logger));
    }
    
    protected override async Task Handle(DeleteAuthSuccessCommand request, CancellationToken cancellationToken)
    {
        var command = DatabaseQuery.Create(SqlCommand, new AuthSuccessEntity(request.AuthSuccess), CancellationToken.None);
        
        try
        {
            await _dbContext.ExecuteCommandAsync(command);
        }
        catch (Exception ex)
        {
            var customProperties = new Dictionary<string, object>
            {
                { nameof(request.AuthSuccess.Address), request.AuthSuccess.Address }
            };
            if (request.AuthSuccess.Audience is not null) customProperties.Add(nameof(request.AuthSuccess.Audience), request.AuthSuccess.Audience);
            
            using (_logger.BeginScope(customProperties))
            {
                _logger.LogError(ex, $"Failure deleting auth success");
            }
        }
    }
}