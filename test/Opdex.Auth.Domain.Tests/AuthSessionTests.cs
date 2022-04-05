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
        var authSession = new AuthSession(new Uri("https://app.opdex.com"), "code_challenge", CodeChallengeMethod.Plain);
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
        var authSession = new AuthSession(new Uri("https://app.opdex.com"), "code_challenge", CodeChallengeMethod.Plain);
        authSession.EstablishPrompt("connection_id");

        // Act
        Action Act() => () => authSession.EstablishPrompt("connection_id");

        // Assert
        Act().Should().NotThrow();
    }

    [Fact]
    public void Create_SetAudience_FromRedirectUriAuthority()
    {
        var authSession = new AuthSession(new Uri("https://app.opdex.com:1303/success"), "code_challenge", CodeChallengeMethod.Plain);
        authSession.Audience.Should().Be("app.opdex.com:1303");
    }

    [Fact]
    public void Verify_Plain_Invalid()
    {
        // Arrange
        const string codeChallenge = "wyBSD6pIB9DU_dXhEmH2LSkMYNAkxND5mCAh3qzsTFI";
        const string codeVerifier = "WU94eUh0akF4cElUM1o0dDhNUW9VTmxFNFM3MEE4Q3k";
        var authSession = new AuthSession(new Uri("https://app.opdex.com"), codeChallenge, CodeChallengeMethod.Plain);
        
        // Act
        var verify = authSession.Verify(codeVerifier);
        
        // Assert
        verify.Should().Be(false);
    }

    [Fact]
    public void Verify_Plain_Valid()
    {
        // Arrange
        const string codeChallenge = "wyBSD6pIB9DU_dXhEmH2LSkMYNAkxND5mCAh3qzsTFI";
        const string codeVerifier = "wyBSD6pIB9DU_dXhEmH2LSkMYNAkxND5mCAh3qzsTFI";
        var authSession = new AuthSession(new Uri("https://app.opdex.com"), codeChallenge, CodeChallengeMethod.Plain);
        
        // Act
        var verify = authSession.Verify(codeVerifier);
        
        // Assert
        verify.Should().Be(true);
    }
    [Fact]
    public void Verify_S256_Invalid()
    {
        // Arrange
        const string codeChallenge = "wyBSD6pIB9DU_dXhEmH2LSkMYNAkxND5mCAh3qzsTFI";
        const string codeVerifier = "wyBSD6pIB9DU_dXhEmH2LSkMYNAkxND5mCAh3qzsTFI";
        var authSession = new AuthSession(new Uri("https://app.opdex.com"), codeChallenge, CodeChallengeMethod.S256);
        
        // Act
        var verify = authSession.Verify(codeVerifier);
        
        // Assert
        verify.Should().Be(false);
    }

    [Fact]
    public void Verify_S256_Valid()
    {
        // Arrange
        const string codeChallenge = "wyBSD6pIB9DU_dXhEmH2LSkMYNAkxND5mCAh3qzsTFI";
        const string codeVerifier = "WU94eUh0akF4cElUM1o0dDhNUW9VTmxFNFM3MEE4Q3k";
        var authSession = new AuthSession(new Uri("https://app.opdex.com"), codeChallenge, CodeChallengeMethod.S256);
        
        // Act
        var verify = authSession.Verify(codeVerifier);
        
        // Assert
        verify.Should().Be(true);
    }
}