using Ardalis.GuardClauses;
using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class SelectAuthSuccessByConnectionIdQuery : IRequest<AuthSuccess?>
{
    public SelectAuthSuccessByConnectionIdQuery(string connectionId)
    {
        ConnectionId = Guard.Against.NullOrEmpty(connectionId, nameof(connectionId));
    }
    
    public string ConnectionId { get; }
}