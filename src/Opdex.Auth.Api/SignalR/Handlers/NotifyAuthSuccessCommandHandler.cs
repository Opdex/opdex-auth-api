using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Opdex.Auth.Domain.Requests;

namespace Opdex.Auth.Api.SignalR.Handlers;

public class NotifyAuthSuccessCommandHandler : AsyncRequestHandler<NotifyAuthSuccessCommand>
{
    private readonly IHubContext<AuthHub, IAuthClient> _hubContext;

    public NotifyAuthSuccessCommandHandler(IHubContext<AuthHub, IAuthClient> hubContext)
    {
        _hubContext = Guard.Against.Null(hubContext, nameof(hubContext));
    }
    
    protected override async Task Handle(NotifyAuthSuccessCommand request, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Client(request.ConnectionId).OnAuthenticated(request.AuthCode.ToString());
    }
}