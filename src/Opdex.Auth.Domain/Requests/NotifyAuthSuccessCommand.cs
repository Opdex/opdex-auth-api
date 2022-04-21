using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record NotifyAuthSuccessCommand(string ConnectionId, Guid AuthCode) : IRequest;