using System;
using System.Text;
using FluentAssertions;
using Opdex.Auth.Domain.Helpers;
using Xunit;

namespace Opdex.Auth.Domain.Tests;

public class AuthSessionTests
{
    [Fact]
    public void EstablishPrompt_AlreadyEstablishedDifferentConnection_ThrowInvalidOperationException()
    {
        // Arrange
        var authSession = new AuthSession("code_challenge", CodeChallengeMethod.Plain);
        authSession.EstablishPrompt("connection_id");

        // Act
        Action Act() => () => authSession.EstablishPrompt("different_connection_id");

        // Assert
        Act().Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void EstablishPrompt_AlreadyEstablishedSameConnection_DoNotThrow()
    {
        // Arrange
        var authSession = new AuthSession("code_challenge", CodeChallengeMethod.Plain);
        authSession.EstablishPrompt("connection_id");

        // Act
        Action Act() => () => authSession.EstablishPrompt("connection_id");

        // Assert
        Act().Should().NotThrow();
    }

    [Fact]
    public void S256_Verification()
    {
        const string codeVerifier = "081431351748401491c87831fc6f63e5";
        const string expectedSha = "aebfba231d8770fb1a5199207321ac0921089d9c47e32c63a0d69d5ce4f6d43f";
        var sb = new StringBuilder();

        var sha = Sha256Extensions.Hash(codeVerifier);
        foreach (var b in sha) sb.Append(b.ToString("x2"));
        sb.ToString().Should().Be(expectedSha);

        var challenge = Base64Extensions.UrlSafeBase64Encode(Encoding.UTF8.GetBytes(sb.ToString()));

        challenge.Should().Be("YWViZmJhMjMxZDg3NzBmYjFhNTE5OTIwNzMyMWFjMDkyMTA4OWQ5YzQ3ZTMyYzYzYTBkNjlkNWNlNGY2ZDQzZg");
    }
}