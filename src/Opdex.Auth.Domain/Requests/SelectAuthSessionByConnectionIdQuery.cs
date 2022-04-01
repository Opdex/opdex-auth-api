using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class SelectAuthSessionByConnectionIdQuery : IRequest<AuthSession?>
{
    public SelectAuthSessionByConnectionIdQuery(string connectionId)
    {
        ConnectionId = connectionId;
    }
    
    public string ConnectionId { get; }
}