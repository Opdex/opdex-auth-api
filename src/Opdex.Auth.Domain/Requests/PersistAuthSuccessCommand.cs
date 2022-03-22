using Ardalis.GuardClauses;
using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class PersistAuthSuccessCommand : IRequest<bool>
{
    public PersistAuthSuccessCommand(AuthSuccess authSuccess)
    {
        AuthSuccess = Guard.Against.Null(authSuccess, nameof(authSuccess));
    }

    public AuthSuccess AuthSuccess { get; }
}