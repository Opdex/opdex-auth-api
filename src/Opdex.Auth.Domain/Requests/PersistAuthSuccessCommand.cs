using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record PersistAuthSuccessCommand(AuthSuccess AuthSuccess) : IRequest<bool>;