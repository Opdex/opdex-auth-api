using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record PersistAuthSessionCommand(AuthSession AuthSession) : IRequest<bool>;