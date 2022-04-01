using Ardalis.GuardClauses;
using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class PersistAuthSessionCommand : IRequest<bool>
{
    public PersistAuthSessionCommand(AuthSession authSession)
    {
        AuthSession = Guard.Against.Null(authSession);
    }
    
    public AuthSession AuthSession { get; }
}