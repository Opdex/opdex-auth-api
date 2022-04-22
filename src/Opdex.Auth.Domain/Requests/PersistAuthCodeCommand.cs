using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record PersistAuthCodeCommand(AuthCode AuthCode) : IRequest<bool>;