using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record SelectAuthCodeByValueQuery(Guid Value) : IRequest<AuthCode?>;