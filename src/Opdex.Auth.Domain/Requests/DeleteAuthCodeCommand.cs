using Ardalis.GuardClauses;
using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class DeleteAuthCodeCommand : IRequest
{
    public DeleteAuthCodeCommand(AuthCode authCode)
    {
        AuthCode = Guard.Against.Null(authCode);
    }
    
    public AuthCode AuthCode { get; }
}