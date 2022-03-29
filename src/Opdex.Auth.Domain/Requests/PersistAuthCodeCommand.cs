using Ardalis.GuardClauses;
using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class PersistAuthCodeCommand : IRequest<bool>
{
    public PersistAuthCodeCommand(AuthCode authCode)
    {
        AuthCode = Guard.Against.Null(authCode);
    }
    
    public AuthCode AuthCode { get; }
}