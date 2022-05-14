using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Moq;
using Opdex.Auth.Api.Encryption;
using Opdex.Auth.Api.SignalR;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Helpers;
using Opdex.Auth.Domain.Requests;
using SSAS.NET;
using Xunit;

namespace Opdex.Auth.Api.Tests.SignalR;

public class AuthHubTests
{
    private readonly Uri _baseUri = new("https://test-auth-api.opdex.com");
    private readonly FakeTwoWayEncryptionProvider _twoWayEncryptionProvider;
    private readonly Mock<HubCallerContext> _hubCallerContextMock;
    private readonly Mock<IAuthClient> _callerClientMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<IStratisIdGenerator> _stratisIdGeneratorMock;

    private readonly AuthHub _hub;

    public AuthHubTests()
    {
        _twoWayEncryptionProvider = new FakeTwoWayEncryptionProvider();
        _hubCallerContextMock = new Mock<HubCallerContext>();
        _callerClientMock = new Mock<IAuthClient>();
        var hubCallerClientsMock = new Mock<IHubCallerClients<IAuthClient>>();
        hubCallerClientsMock.Setup(callTo => callTo.Caller).Returns(_callerClientMock.Object);

        _mediatorMock = new Mock<IMediator>();

        _stratisIdGeneratorMock = new Mock<IStratisIdGenerator>();

        _hub = new AuthHub(_mediatorMock.Object, _stratisIdGeneratorMock.Object, _twoWayEncryptionProvider)
        {
            Context = _hubCallerContextMock.Object,
            Clients = hubCallerClientsMock.Object
        };
    }

    [Fact]
    public async Task GetStratisId_StampNotGuid_ThrowArgumentNullException()
    {
        // Arrange
        _hubCallerContextMock.Setup(callTo => callTo.ConnectionId).Returns("MY_C8NN3CTI8N_ID");

        // Act
        Func<Task<string>> Act() => () => _hub.GetStratisId("NOT_A_GUID");

        // Assert
        await Act().Should().ThrowExactlyAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetStratisId_AuthSessionDoesntExist_ThrowAuthSessionConnectionException()
    {
        // Arrange
        _mediatorMock
            .Setup(callTo => callTo.Send(It.IsAny<SelectAuthSessionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthSession?) null);
        
        // Act
        Func<Task<string>> Act() => () => _hub.GetStratisId(Guid.NewGuid().ToString());

        // Assert
        await Act().Should().ThrowExactlyAsync<AuthSessionConnectionException>();
    }

    [Fact]
    public async Task GetStratisId_PersistAuthSession_LinkConnectionId()
    {
        // Arrange
        const string connectionId = "8rn4UxxPl2m4jd8DDa9fir920";
        _hubCallerContextMock.Setup(callTo => callTo.ConnectionId).Returns(connectionId);

        _mediatorMock
            .Setup(callTo => callTo.Send(It.IsAny<SelectAuthSessionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthSession(new Uri("https://app.opdex.com"), "C0d3Ch4113ng3", CodeChallengeMethod.Plain));

        _mediatorMock.Setup(callTo => callTo.Send(It.IsAny<PersistAuthSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        try
        {
            await _hub.GetStratisId(Guid.NewGuid().ToString());
        }
        catch (Exception)
        {
            // ignore
        }

        // Assert
        _mediatorMock.Verify(callTo => callTo.Send(It.Is<PersistAuthSessionCommand>(
            c => c.AuthSession.ConnectionId == connectionId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStratisId_CouldNotPersistAuthSession_ThrowAuthSessionConnectionException()
    {
        // Arrange
        _hubCallerContextMock.Setup(callTo => callTo.ConnectionId).Returns("8rn4UxxPl2m4jd8DDa9fir920");

        _mediatorMock
            .Setup(callTo => callTo.Send(It.IsAny<SelectAuthSessionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthSession(new Uri("https://app.opdex.com"), "C0d3Ch4113ng3", CodeChallengeMethod.Plain));

        _mediatorMock.Setup(callTo => callTo.Send(It.IsAny<PersistAuthSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        // Act
        Func<Task<string>> Act() => () => _hub.GetStratisId(Guid.NewGuid().ToString());

        // Assert
        await Act().Should().ThrowExactlyAsync<AuthSessionConnectionException>();
    }

    [Fact]
    public async Task GetStratisId_CreateStratisId_CorrectParameters()
    {
        // Arrange
        const string connectionId = "8rn4UxxPl2m4jd8DDa9fir920";
        _hubCallerContextMock.Setup(callTo => callTo.ConnectionId).Returns(connectionId);

        _mediatorMock
            .Setup(callTo => callTo.Send(It.IsAny<SelectAuthSessionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthSession(new Uri("https://app.opdex.com"), "C0d3Ch4113ng3", CodeChallengeMethod.Plain));

        _mediatorMock.Setup(callTo => callTo.Send(It.IsAny<PersistAuthSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        // Act
        try
        {
            await _hub.GetStratisId(Guid.NewGuid().ToString());
        }
        catch (Exception)
        {
            // ignore
        }

        // Assert
        _stratisIdGeneratorMock.Verify(callTo => callTo.Create("v1/ssas/callback", connectionId), Times.Once);
    }

    [Fact]
    public async Task GetStratisId_Return_StringifiedStratisIdUri()
    {
        // Arrange
        var stubStratisId = new StratisId("app.opdex.com", "f8a9j20jdasiw", 4029420153);
        _hubCallerContextMock.Setup(callTo => callTo.ConnectionId).Returns("8rn4UxxPl2m4jd8DDa9fir920");
        _mediatorMock
            .Setup(callTo => callTo.Send(It.IsAny<SelectAuthSessionByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthSession(new Uri("https://app.opdex.com"), "C0d3Ch4113ng3", CodeChallengeMethod.Plain));
        _mediatorMock.Setup(callTo => callTo.Send(It.IsAny<PersistAuthSessionCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _stratisIdGeneratorMock.Setup(callTo => callTo.Create(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(stubStratisId);
        
        // Act
        var response = await _hub.GetStratisId(Guid.NewGuid().ToString());

        // Assert
        response.Should().Be(stubStratisId.ToUriString());
    }

    [Fact]
    public async Task Reconnect_InvalidStratisId_DoNotAuthenticate()
    {
        // Arrange
        const string previousConnectionId = "QU5FWENSWVBURURDT05ORUNUSU9OSUQ";
        var stratisId = "INVALID STRATIS ID";

        // Act
        var succeeded = await _hub.Reconnect(previousConnectionId, stratisId);

        // Assert
        succeeded.Should().Be(false);
        _callerClientMock.Verify(callTo => callTo.OnAuthenticated(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Reconnect_ValidStratisIdButExpired_DoNotAuthenticate()
    {
        // Arrange
        var unixTime10MinsAgo = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds();
        var previousConnectionId = "QU5FWENSWVBURURDT05ORUNUSU9OSUQ";
        var stratisId = $"sid:{_baseUri.Host}/v1/ssas/callback?uid=MtLXa7ZbmtGjKeCpZC-Y1cjNLDsVz4tDfBqahJssXOvsmUVSnYa5nclYnSZxhwcN1gjxrp4ydqoo3KRSKMdBaw&exp={unixTime10MinsAgo}";

        // Act
        var succeeded = await _hub.Reconnect(previousConnectionId, stratisId);

        // Assert
        succeeded.Should().Be(false);
        _callerClientMock.Verify(callTo => callTo.OnAuthenticated(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Reconnect_DifferentConnectionId_DoNotAuthenticate()
    {
        // Arrange
        const string connectionId = "DIFFERENT CONNECTION ID";
        var unixTime10MinsFromNow = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
        const string previousConnectionId = "QU5FWENSWVBURURDT05ORUNUSU9OSUQ";
        var stratisId = $"sid:{_baseUri.Host}/v1/ssas/callback?uid=JztkuBy8zCCHSoPBmQ1D9YEUnNGYmRGE8j6EshsLRiSIF2aYLQiemjKsfHtqBFEJhxLjwtGRrzS3CZk6MDxa0A&exp={unixTime10MinsFromNow}";

        _twoWayEncryptionProvider.WhenDecryptCalled(() => $"{connectionId}{unixTime10MinsFromNow}");

        // Act
        var succeeded = await _hub.Reconnect(previousConnectionId, stratisId);

        // Assert
        succeeded.Should().Be(false);
        _callerClientMock.Verify(callTo => callTo.OnAuthenticated(It.IsAny<string>()), Times.Never);
    }
}

internal class FakeTwoWayEncryptionProvider : ITwoWayEncryptionProvider
{
    public readonly Queue<byte[]> DecryptCalls = new();
    public readonly Queue<string> EncryptCalls = new();

    private Func<string, byte[]> _encryptFunc = _ => Array.Empty<byte>();
    private Func<byte[], string> _decryptFunc = _ => "";

    public string Decrypt(ReadOnlySpan<byte> cipherText)
    {
        DecryptCalls.Enqueue(cipherText.ToArray());
        return _decryptFunc.Invoke(cipherText.ToArray());
    }

    public ReadOnlySpan<byte> Encrypt(string plainText)
    {
        EncryptCalls.Enqueue(plainText);
        return _encryptFunc.Invoke(plainText).AsSpan();
    }

    public void WhenEncryptCalled(Func<byte[]> expression) => _encryptFunc = _ => expression.Invoke();
    public void WhenEncryptCalled(Func<string, byte[]> expression) => _encryptFunc = expression;

    public void WhenDecryptCalled(Func<string> expression) => _decryptFunc = _ => expression.Invoke();
    public void WhenDecryptCalled(Func<byte[], string> expression) => _decryptFunc = expression;
}