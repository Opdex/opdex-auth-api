using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record SelectAuthSuccessByRefreshTokenQuery(string RefreshToken) : IRequest<AuthSuccess?>;