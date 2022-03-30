using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Opdex.Auth.Domain.Requests;
using Opdex.Auth.Infrastructure.Data.Entities;

namespace Opdex.Auth.Infrastructure.Data.Handlers;

public class DeleteAuthCodeCommandHandler : AsyncRequestHandler<DeleteAuthCodeCommand>
{
    private static readonly string SqlCommand =
        $@"DELETE FROM auth_access_code
            WHERE {nameof(AuthCodeEntity.AccessCode)} = @{nameof(AuthCodeEntity.AccessCode)}
            LIMIT 1;".RemoveExcessWhitespace();
    
    private readonly IDbContext _dbContext;
    private readonly ILogger<DeleteAuthCodeCommandHandler> _logger;

    public DeleteAuthCodeCommandHandler(IDbContext dbContext, ILogger<DeleteAuthCodeCommandHandler> logger)
    {
        _dbContext = Guard.Against.Null(dbContext, nameof(dbContext));
        _logger = Guard.Against.Null(logger, nameof(logger));
    }

    protected override async Task Handle(DeleteAuthCodeCommand request, CancellationToken cancellationToken)
    {
        var sqlParams = new SqlParams(request.AuthCode.Value);
        var command = DatabaseQuery.Create(SqlCommand, sqlParams, CancellationToken.None);
        
        try
        {
            await _dbContext.ExecuteCommandAsync(command);
        }
        catch (Exception ex)
        {
            using (_logger.BeginScope(new Dictionary<string, object>
                   {
                       { nameof(request.AuthCode.Value), request.AuthCode.Value },
                       { nameof(request.AuthCode.Signer), request.AuthCode.Signer }
                   }))
            {
                _logger.LogError(ex, $"Failure deleting auth code");
            }
        }
    }

    private sealed class SqlParams
    {
        internal SqlParams(Guid accessCode)
        {
            AccessCode = accessCode;
        }

        public Guid AccessCode { get; }
    }
}