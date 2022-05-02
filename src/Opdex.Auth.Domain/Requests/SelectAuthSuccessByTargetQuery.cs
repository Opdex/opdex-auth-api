using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record SelectAuthSuccessByTargetQuery(string Signer, string? Audience = null) : IRequest<AuthSuccess?>;