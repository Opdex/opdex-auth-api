using System;
using FluentAssertions;
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
}