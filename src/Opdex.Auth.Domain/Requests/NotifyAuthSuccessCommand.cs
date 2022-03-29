using Ardalis.GuardClauses;
using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class NotifyAuthSuccessCommand : IRequest
{
    public NotifyAuthSuccessCommand(string connectionId, Guid authCode)
    {
        ConnectionId = Guard.Against.NullOrEmpty(connectionId, nameof(connectionId));
        AuthCode = authCode;
    }

    public string ConnectionId { get; }
    public Guid AuthCode { get; }
}