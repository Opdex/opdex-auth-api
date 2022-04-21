using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record SelectAuthCodeByStampQuery(Guid Stamp) : IRequest<AuthCode?>;