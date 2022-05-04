using System;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Opdex.Auth.Api.Encryption;
using Opdex.Auth.Api.Tests.SignalR;
using Opdex.Auth.Domain.Helpers;
using Xunit;

namespace Opdex.Auth.Api.Tests.Encryption;

public class StratisIdGeneratorTests
{
    private readonly Uri _baseUri = new("https://test-auth-api.opdex.com");
    
    private readonly FakeTwoWayEncryptionProvider _twoWayEncryptionProvider;
    
    private readonly StratisIdGenerator _stratisIdGenerator;

    public StratisIdGeneratorTests()
    {
        _twoWayEncryptionProvider = new FakeTwoWayEncryptionProvider();
        var apiOptionsMock = new Mock<IOptionsSnapshot<ApiOptions>>();
        apiOptionsMock.Setup(callTo => callTo.Value).Returns(new ApiOptions { Authority = _baseUri.ToString().TrimEnd('/') });

        _stratisIdGenerator = new StratisIdGenerator(apiOptionsMock.Object, _twoWayEncryptionProvider);
    }

    [Fact]
    public void Create_TwoWayEncryptionProvider_Encrypt()
    {
        // Arrange
        var uid = Guid.NewGuid().ToString();
        
        // Act
        _stratisIdGenerator.Create("v1/ssas", uid);

        // Assert
        _twoWayEncryptionProvider.EncryptCalls.Count.Should().Be(1);
        var decrypted = _twoWayEncryptionProvider.EncryptCalls.Dequeue();

        var decryptedUid = decrypted[..^10];
        var decryptedExpiration = long.Parse(decrypted[^10..]);
        decryptedUid.Should().Be(uid);
        decryptedExpiration.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("v1/ssas/callback")]
    [InlineData("/v1/ssas/callback")]
    public void Create_Callback_FromConfig(string callback)
    {
        // Arrange
        var expected = Encoding.UTF8.GetBytes("3NCRYPT3DCONNECTIONID");
        _twoWayEncryptionProvider.WhenEncryptCalled(() => expected);

        // Act
        var stratisId = _stratisIdGenerator.Create(callback, Guid.NewGuid().ToString());

        // Assert
        stratisId.Callback.Should().StartWith($"{_baseUri.Host}/v1/ssas/callback");
    }

    [Fact]
    public void Create_Uid_EncodeAsUrlSafeBase64Encoded()
    {
        // Arrange
        var expected = Encoding.UTF8.GetBytes("3NCRYPT3DCONNECTIONID");
        _twoWayEncryptionProvider.WhenEncryptCalled(() => expected);

        // Act
        var stratisId = _stratisIdGenerator.Create("v1/ssas/callback", Guid.NewGuid().ToString());

        // Assert
        stratisId.Uid.Should().Be(Base64Extensions.UrlSafeBase64Encode(expected));
    }
}