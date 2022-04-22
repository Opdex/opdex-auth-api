using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record DeleteAuthSuccessCommand(AuthSuccess AuthSuccess) : IRequest;