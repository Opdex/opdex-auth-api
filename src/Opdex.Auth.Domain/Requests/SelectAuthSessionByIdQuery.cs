using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class SelectAuthSessionByIdQuery : IRequest<AuthSession>
{
    public SelectAuthSessionByIdQuery(Guid id)
    {
        Id = id;
    }
    
    public Guid Id { get; }
}