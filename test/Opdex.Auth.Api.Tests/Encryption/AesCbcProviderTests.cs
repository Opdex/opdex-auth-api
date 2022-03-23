using System;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Opdex.Auth.Api.Encryption;
using Xunit;

namespace Opdex.Auth.Api.Tests.Encryption;

public class AesCbcProviderTests
{
    [Theory]
    [InlineData("_HAS_LENGTH_15_")]
    [InlineData("_16_CHARS_LONG_")]
    [InlineData("_SEVENTEEN_CHARS_")]
    public void Encrypt_VariableLengthPlaintext_AesCbcResult(string plainText)
    {
        // Arrange
        using var aesCbcProvider = CreateAesCbcProvider();
        
        // Act
        var encrypted = aesCbcProvider.Encrypt(plainText);

        // Assert
        const int blockSize = 16;
        const int ivLength = 16;
        var connectionIdBytes = Encoding.UTF8.GetBytes(plainText).Length;
        var cipherLength = connectionIdBytes + (connectionIdBytes % blockSize == 0 ? 0 : blockSize - (connectionIdBytes % blockSize));
        encrypted.Length.Should().Be(cipherLength + ivLength);
    }

    [Fact]
    public void Encrypt_EncryptTwice_DifferentIV()
    {
        // Arrange
        const string plainText = "PLAINTEXT";
        using var aesCbcProvider = CreateAesCbcProvider();

        // Act
        var encryptedA = aesCbcProvider.Encrypt(plainText);
        var encryptedB = aesCbcProvider.Encrypt(plainText);

        // Assert
        (encryptedA != encryptedB).Should().Be(true);
    }

    [Fact]
    public void Decrypt_SameProviderRoundtrip_Success()
    {
        // Arrange
        const string plainText = "PLAINTEXT";
        using var aesCbcProvider = CreateAesCbcProvider();
        var encrypted = aesCbcProvider.Encrypt(plainText);

        // Act
        var decrypted = aesCbcProvider.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Decrypt_DifferentProviderSameKey_Success()
    {
        // Arrange
        const string plainText = "VERY_SECRET_SECRET";
        ReadOnlySpan<byte> encrypted;
        using var aesCbcProvider = CreateAesCbcProvider("_ENCRYPTION_KEY_");
        {
            encrypted = aesCbcProvider.Encrypt(plainText);
        }

        // Act
        string decrypted;
        using var aesCbcProviderTwo = CreateAesCbcProvider("_ENCRYPTION_KEY_");
        {
            decrypted = aesCbcProviderTwo.Decrypt(encrypted);
        }

        // Assert
        decrypted.Should().Be(plainText);
    }

    [Fact]
    public void Decrypt_DifferentProviderDifferentKey_ThrowCryptographicException()
    {
        // Arrange
        const string plainText = "VERY_SECRET_SECRET";
        byte[] encrypted;
        using var aesCbcProvider = CreateAesCbcProvider("_ENCRYPTION_KEY_");
        {
            encrypted = aesCbcProvider.Encrypt(plainText).ToArray();
        }

        // Act
        void Act()
        {
            using var aesCbcProviderTwo = CreateAesCbcProvider("ANOTHER_DIFFERENT_ENCRYPTION_KEY");
            aesCbcProviderTwo.Decrypt(encrypted);
        }

        // Assert
        Assert.Throws<CryptographicException>(Act);
    }

    private static AesCbcProvider CreateAesCbcProvider(string? key = null)
    {
        key ??= Guid.NewGuid().ToString("N");
        var encryptionOptionsMock = new Mock<IOptionsSnapshot<EncryptionOptions>>();
        encryptionOptionsMock.Setup(callTo => callTo.Value).Returns(new EncryptionOptions { Key = key });
        return new AesCbcProvider(encryptionOptionsMock.Object);
    }
}