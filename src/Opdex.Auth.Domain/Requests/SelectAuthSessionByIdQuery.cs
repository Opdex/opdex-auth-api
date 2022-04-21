using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record SelectAuthSessionByIdQuery(Guid Id) : IRequest<AuthSession>;