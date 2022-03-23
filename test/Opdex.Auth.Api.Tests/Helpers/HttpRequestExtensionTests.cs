using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Opdex.Auth.Api.Helpers;
using Xunit;

namespace Opdex.Auth.Api.Tests.Helpers;

public class HttpRequestExtensionTests
{
    [Fact]
    public void BaseUrl()
    {
        var httpRequestMock = new Mock<HttpRequest>();
        httpRequestMock.Setup(callTo => callTo.Scheme).Returns("https");
        httpRequestMock.Setup(callTo => callTo.Host).Returns(new HostString("localhost:8080"));
        httpRequestMock.Object.BaseUrl().Should().Be("https://localhost:8080");
    }
    
    [Fact]
    public void BaseUrlWithPath()
    {
        var httpRequestMock = new Mock<HttpRequest>();
        httpRequestMock.Setup(callTo => callTo.Scheme).Returns("https");
        httpRequestMock.Setup(callTo => callTo.Host).Returns(new HostString("localhost:8080"));
        httpRequestMock.Setup(callTo => callTo.Path).Returns(new PathString("/v1/auth/callback"));
        httpRequestMock.Object.BaseUrlWithPath().Should().Be("https://localhost:8080/v1/auth/callback");
    }
}