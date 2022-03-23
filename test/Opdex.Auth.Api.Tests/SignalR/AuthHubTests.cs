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
using Opdex.Auth.Api.Helpers;
using Opdex.Auth.Api.SignalR;
using Opdex.Auth.Domain;
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
    private readonly Mock<IJwtIssuer> _jwtIssuerMock;
    
    private readonly AuthHub _hub;

    public AuthHubTests()
    {
        _twoWayEncryptionProvider = new FakeTwoWayEncryptionProvider();
        _hubCallerContextMock = new Mock<HubCallerContext>();
        _callerClientMock = new Mock<IAuthClient>();
        var hubCallerClientsMock = new Mock<IHubCallerClients<IAuthClient>>();
        hubCallerClientsMock.Setup(callTo => callTo.Caller).Returns(_callerClientMock.Object);

        _mediatorMock = new Mock<IMediator>();
        _jwtIssuerMock = new Mock<IJwtIssuer>();
        var apiOptionsMock = new Mock<IOptionsSnapshot<ApiOptions>>();
        apiOptionsMock.Setup(callTo => callTo.Value).Returns(new ApiOptions { Authority = _baseUri.ToString().TrimEnd('/') });

        _hub = new AuthHub(_mediatorMock.Object, _twoWayEncryptionProvider, _jwtIssuerMock.Object, apiOptionsMock.Object)
        {
            Context = _hubCallerContextMock.Object,
            Clients = hubCallerClientsMock.Object
        };
    }

    [Fact]
    public void GetStratisId_TwoWayEncryptionProvider_Encrypt()
    {
        // Arrange
        const string connectionId = "MY_C8NN3CTI8N_ID";
        _hubCallerContextMock.Setup(callTo => callTo.ConnectionId).Returns(connectionId);

        // Act
         _hub.GetStratisId();

        // Assert
        _twoWayEncryptionProvider.EncryptCalls.Count.Should().Be(1);
        var decrypted = _twoWayEncryptionProvider.EncryptCalls.Dequeue();

        var decryptedConnectionId = decrypted[..^10];
        var decryptedExpiration = long.Parse(decrypted[^10..]);
        decryptedConnectionId.Should().Be(connectionId);
        decryptedExpiration.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetStratisId_StratisId_WellFormatted()
    {
        // Arrange
        _hubCallerContextMock.Setup(callTo => callTo.ConnectionId).Returns("8rn4UxxPl2m4jd8DDa9fir920");

        var expected = Encoding.UTF8.GetBytes("3NCRYPT3DCONNECTIONID");
        _twoWayEncryptionProvider.WhenEncryptCalled(() => expected);

        // Act
        var encrypted = _hub.GetStratisId();

        // Assert
        StratisId.TryParse(encrypted, out _).Should().Be(true);
    }

    [Fact]
    public void GetStratisId_Callback_FromConfig()
    {
        // Arrange
        _hubCallerContextMock.Setup(callTo => callTo.ConnectionId).Returns("8rn4UxxPl2m4jd8DDa9fir920");

        var expected = Encoding.UTF8.GetBytes("3NCRYPT3DCONNECTIONID");
        _twoWayEncryptionProvider.WhenEncryptCalled(() => expected);

        // Act
        var encrypted = _hub.GetStratisId();

        // Assert
        _ = StratisId.TryParse(encrypted, out var stratisId);
        stratisId.Callback.Should().StartWith($"{_baseUri.Host}/v1/auth/callback");
    }

    [Fact]
    public void GetStratisId_Uid_ConnectionIdUrlSafeBase64Encoded()
    {
        // Arrange
        _hubCallerContextMock.Setup(callTo => callTo.ConnectionId).Returns("8rn4UxxPl2m4jd8DDa9fir920");

        var expected = Encoding.UTF8.GetBytes("3NCRYPT3DCONNECTIONID");
        _twoWayEncryptionProvider.WhenEncryptCalled(() => expected);

        // Act
        var encrypted = _hub.GetStratisId();

        // Assert
        _ = StratisId.TryParse(encrypted, out var stratisId);
        stratisId.Uid.Should().Be(Base64Extensions.UrlSafeBase64Encode(expected));
    }

    [Fact]
    public async Task Reconnect_InvalidStratisId_DoNotAuthenticate()
    {
        // Arrange
        var previousConnectionId = "QU5FWENSWVBURURDT05ORUNUSU9OSUQ";
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
        var stratisId = $"sid:{_baseUri.Host}/v1/auth/callback?uid=MtLXa7ZbmtGjKeCpZC-Y1cjNLDsVz4tDfBqahJssXOvsmUVSnYa5nclYnSZxhwcN1gjxrp4ydqoo3KRSKMdBaw&exp={unixTime10MinsAgo}";

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
        var stratisId = $"sid:{_baseUri.Host}/v1/auth/callback?uid=JztkuBy8zCCHSoPBmQ1D9YEUnNGYmRGE8j6EshsLRiSIF2aYLQiemjKsfHtqBFEJhxLjwtGRrzS3CZk6MDxa0A&exp={unixTime10MinsFromNow}";

        _twoWayEncryptionProvider.WhenDecryptCalled(() => $"{connectionId}{unixTime10MinsFromNow}");

        // Act
        var succeeded = await _hub.Reconnect(previousConnectionId, stratisId);

        // Assert
        succeeded.Should().Be(false);
        _callerClientMock.Verify(callTo => callTo.OnAuthenticated(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Reconnect_NoAuthSuccessRecord_DoNotAuthenticate()
    {
        // Arrange
        var unixTime10MinsFromNow = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
        const string previousConnectionId = "QU5FWENSWVBURURDT05ORUNUSU9OSUQ";
        const string uid = "JztkuBy8zCCHSoPBmQ1D9YEUnNGYmRGE8j6EshsLRiSIF2aYLQiemjKsfHtqBFEJhxLjwtGRrzS3CZk6MDxa0A";
        var stratisId = $"sid:{_baseUri.Host}/v1/auth/callback?uid={uid}&exp={unixTime10MinsFromNow}";

        _twoWayEncryptionProvider.WhenDecryptCalled(() => $"{previousConnectionId}{unixTime10MinsFromNow}");

        _mediatorMock.Setup(callTo => callTo.Send(It.IsAny<SelectAuthSuccessByConnectionIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthSuccess?)null);

        // Act
        var succeeded = await _hub.Reconnect(previousConnectionId, stratisId);

        // Assert
        succeeded.Should().Be(false);
        _mediatorMock.Verify(callTo => callTo.Send(It.IsAny<SelectAuthSuccessByConnectionIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _callerClientMock.Verify(callTo => callTo.OnAuthenticated(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Reconnect_ExpiredAuthSuccessRecord_DoNotAuthenticate()
    {
        // Arrange
        var unixTime10MinsFromNow = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
        const string previousConnectionId = "QU5FWENSWVBURURDT05ORUNUSU9OSUQ";
        const string connectionId = "GO73rdOHET7W1FAuWp96Tw205af2011";
        const string uid = "JztkuBy8zCCHSoPBmQ1D9YEUnNGYmRGE8j6EshsLRiSIF2aYLQiemjKsfHtqBFEJhxLjwtGRrzS3CZk6MDxa0A";
        var stratisId = $"sid:{_baseUri.Host}/v1/auth/callback?uid={uid}&exp={unixTime10MinsFromNow}";

        _twoWayEncryptionProvider.WhenDecryptCalled(() => $"{previousConnectionId}{unixTime10MinsFromNow}");

        _mediatorMock.Setup(callTo => callTo.Send(It.IsAny<SelectAuthSuccessByConnectionIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthSuccess(connectionId, "PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ", DateTime.UtcNow.AddMinutes(-5)));

        // Act
        var succeeded = await _hub.Reconnect(previousConnectionId, stratisId);

        // Assert
        succeeded.Should().Be(false);
        _mediatorMock.Verify(callTo => callTo.Send(It.IsAny<SelectAuthSuccessByConnectionIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        _callerClientMock.Verify(callTo => callTo.OnAuthenticated(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Reconnect_Valid_Authenticate()
    {
        // Arrange
        var unixTime10MinsFromNow = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
        const string previousConnectionId = "QU5FWENSWVBURURDT05ORUNUSU9OSUQ";
        const string connectionId = "GO73rdOHET7W1FAuWp96Tw205af2011";
        const string uid = "JztkuBy8zCCHSoPBmQ1D9YEUnNGYmRGE8j6EshsLRiSIF2aYLQiemjKsfHtqBFEJhxLjwtGRrzS3CZk6MDxa0A";
        var stratisId = $"sid:{_baseUri.Host}/v1/auth/callback?uid={uid}&exp={unixTime10MinsFromNow}";
        const string jwt = "bearer-token";

        _twoWayEncryptionProvider.WhenDecryptCalled(() => $"{previousConnectionId}{unixTime10MinsFromNow}");

        _mediatorMock.Setup(callTo => callTo.Send(It.IsAny<SelectAuthSuccessByConnectionIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthSuccess(connectionId, "PAe1RRxnRVZtbS83XQ4soyjwJUDSjaJAKZ"));
        _jwtIssuerMock.Setup(callTo => callTo.Create(It.IsAny<string>())).Returns(jwt);

        // Act
        var succeeded = await _hub.Reconnect(previousConnectionId, stratisId);

        // Assert
        succeeded.Should().Be(true);
        _callerClientMock.Verify(callTo => callTo.OnAuthenticated(jwt), Times.Once);
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