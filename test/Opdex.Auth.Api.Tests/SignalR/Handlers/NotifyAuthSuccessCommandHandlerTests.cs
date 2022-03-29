using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Opdex.Auth.Api.SignalR;
using Opdex.Auth.Api.SignalR.Handlers;
using Opdex.Auth.Domain.Requests;
using Xunit;

namespace Opdex.Auth.Api.Tests.SignalR.Handlers;

public class NotifyAuthSuccessCommandHandlerTests
{
    private readonly Mock<IHubContext<AuthHub, IAuthClient>> _hubContextMock;
    private readonly IRequestHandler<NotifyAuthSuccessCommand> _handler;

    public NotifyAuthSuccessCommandHandlerTests()
    {
        var clientMock = new Mock<IAuthClient>();
        var hubClientsMock = new Mock<IHubClients<IAuthClient>>();
        hubClientsMock.Setup(callTo => callTo.Client(It.IsAny<string>())).Returns(clientMock.Object);
        _hubContextMock = new Mock<IHubContext<AuthHub, IAuthClient>>();
        _hubContextMock.SetupGet(callTo => callTo.Clients).Returns(hubClientsMock.Object);
        _handler = new NotifyAuthSuccessCommandHandler(_hubContextMock.Object);
    }

    [Fact]
    public async Task Handle_NotifyUser_WithAuthCode()
    {
        // Arrange
        var request = new NotifyAuthSuccessCommand(Guid.NewGuid().ToString(), Guid.NewGuid());

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _hubContextMock.Verify(callTo => callTo.Clients.Client(request.ConnectionId).OnAuthenticated(request.AuthCode.ToString()), Times.Once);
    }
}