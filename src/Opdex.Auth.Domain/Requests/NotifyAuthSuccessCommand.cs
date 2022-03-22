using Ardalis.GuardClauses;
using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class NotifyAuthSuccessCommand : IRequest
{
    public NotifyAuthSuccessCommand(string connectionId, string bearerToken)
    {
        ConnectionId = Guard.Against.Null(connectionId, nameof(connectionId));
        BearerToken = Guard.Against.Null(bearerToken, nameof(bearerToken));
    }

    public string ConnectionId { get; }
    public string BearerToken { get; }
}