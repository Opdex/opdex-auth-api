using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Contrib.HttpClient;
using Opdex.Auth.Infrastructure.Cirrus;
using Xunit;

namespace Opdex.Auth.Infrastructure.Tests.Cirrus;

public class WalletModuleTests
{
    [Fact]
    public async Task VerifySignedMessage_SendRequest()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.OK, "true", MediaTypeNames.Application.Json);
        var httpClient = handler.CreateClient();

        var cirrusOptions = new CirrusOptions { ApiUrl = "https://cirrus.opdex.com" };
        var cirrusOptionsMock = new Mock<IOptions<CirrusOptions>>();
        cirrusOptionsMock.Setup(callTo => callTo.Value).Returns(cirrusOptions);

        var walletModule = new WalletModule(httpClient, cirrusOptionsMock.Object, new NullLogger<WalletModule>());
        
        // Act
        await walletModule.VerifySignedMessage("message", "tDrNbZKsbYPvike4RfddzESXZoPwUMm5pL", "signature");
        
        // Assert
        handler.VerifyRequest(HttpMethod.Post, "https://cirrus.opdex.com/api/Wallet/verifymessage", Times.Once());
    }
    
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task VerifySignedMessage_ReturnResponse(bool isValid)
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler.SetupAnyRequest().ReturnsResponse(HttpStatusCode.OK, $"\"{isValid.ToString()}\"", MediaTypeNames.Application.Json);
        var httpClient = handler.CreateClient();

        var cirrusOptions = new CirrusOptions { ApiUrl = "https://cirrus.opdex.com" };
        var cirrusOptionsMock = new Mock<IOptions<CirrusOptions>>();
        cirrusOptionsMock.Setup(callTo => callTo.Value).Returns(cirrusOptions);

        var walletModule = new WalletModule(httpClient, cirrusOptionsMock.Object, new NullLogger<WalletModule>());
        
        // Act
        var response = await walletModule.VerifySignedMessage("message", "tDrNbZKsbYPvike4RfddzESXZoPwUMm5pL", "signature");
        
        // Assert
        response.Should().Be(isValid);
    }
}