using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record DeleteAuthSessionCommand(AuthSession AuthSession) : IRequest;