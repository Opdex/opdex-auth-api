using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record SelectAuthSuccessByTargetQuery(string Audience, string Signer) : IRequest<AuthSuccess?>;