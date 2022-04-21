using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record SelectAuthSessionByConnectionIdQuery(string ConnectionId) : IRequest<AuthSession?>;