using MediatR;

namespace Opdex.Auth.Domain.Requests;

public record DeleteAuthCodeCommand(AuthCode AuthCode) : IRequest;