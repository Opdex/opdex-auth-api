using System.Text;
using FluentAssertions;
using Opdex.Auth.Domain.Helpers;
using Xunit;

namespace Opdex.Auth.Api.Tests.Helpers;

public class Base64ExtensionsTests
{
    [Fact]
    public void UrlSafeBase64Encode_AdditionSign_SwapToDash()
    {
        // Arrange
        const string plainText = "14<+_~`/7'88";

        // Act
        var encoded = Base64Extensions.UrlSafeBase64Encode(Encoding.UTF8.GetBytes(plainText));

        // Assert
        encoded.Should().Be("MTQ8K19-YC83Jzg4");
    }

    [Fact]
    public void UrlSafeBase64Encode_ForwardSlashSign_SwapToUnderscore()
    {
        // Arrange
        const string plainText = "ZSWEDXCFRTGVBHYUJNMKIOL<>:P{\"?|}+_)(*&^%$£@!";

        // Act
        var encoded = Base64Extensions.UrlSafeBase64Encode(Encoding.UTF8.GetBytes(plainText));

        // Assert
        encoded.Should().Be("WlNXRURYQ0ZSVEdWQkhZVUpOTUtJT0w8PjpQeyI_fH0rXykoKiZeJSTCo0Ah");
    }

    [Fact]
    public void UrlSafeBase64Encode_Padding_Remove()
    {
        // Arrange
        const string plainText = "1234567890";

        // Act
        var encoded = Base64Extensions.UrlSafeBase64Encode(Encoding.UTF8.GetBytes(plainText));

        // Assert
        encoded.Should().Be("MTIzNDU2Nzg5MA");
    }

    [Fact]
    public void UrlSafeBase64_DashSign_Recognise()
    {
        // Arrange
        const string encoded = "MTQ8K19-YC83Jzg4";

        // Act
        var plainText = Base64Extensions.UrlSafeBase64Decode(encoded);

        // Assert
        plainText.ToArray().Should().BeEquivalentTo(Encoding.UTF8.GetBytes("14<+_~`/7'88"));
    }

    [Fact]
    public void UrlSafeBase64_UnderscoreSign_Recognise()
    {
        // Arrange
        const string encoded = "WlNXRURYQ0ZSVEdWQkhZVUpOTUtJT0w8PjpQeyI_fH0rXykoKiZeJSTCo0Ah";

        // Act
        var plainText = Base64Extensions.UrlSafeBase64Decode(encoded);

        // Assert
        plainText.ToArray().Should().BeEquivalentTo(Encoding.UTF8.GetBytes("ZSWEDXCFRTGVBHYUJNMKIOL<>:P{\"?|}+_)(*&^%$£@!"));
    }

    [Fact]
    public void UrlSafeBase64_NoPadding_Recognise()
    {
        // Arrange
        const string encoded = "MTIzNDU2Nzg5MA";

        // Act
        var plainText = Base64Extensions.UrlSafeBase64Decode(encoded);

        // Assert
        plainText.ToArray().Should().BeEquivalentTo(Encoding.UTF8.GetBytes("1234567890"));
    }

    [Fact]
    public void UrlSafeBase64_Encode_Success()
    {
        // Arrage
        const string value = "60086ac1a0a92156b546d5ad4fc49647732b06abec3c2ee2b08394f1006a1bc3";

        // Act
        var encoded = Base64Extensions.UrlSafeBase64Encode(Encoding.UTF8.GetBytes(value));

        // Assert
        encoded.Should().Be("NjAwODZhYzFhMGE5MjE1NmI1NDZkNWFkNGZjNDk2NDc3MzJiMDZhYmVjM2MyZWUyYjA4Mzk0ZjEwMDZhMWJjMw");
    }

    [Fact]
    public void UrlSafeBase64_Decode_Success()
    {
        // Arrage
        const string value = "MTA4YjliZjU5OTA2OWJhMjk1NWE4OWRlYmM4YTAyZjhmMzliOThlYjc0MTJjZjE4MjgxYzI4NjJlMWU5MDdiMg";

        // Act
        var decoded = Base64Extensions.UrlSafeBase64Decode(value);

        // Assert
        decoded.ToArray().Should().BeEquivalentTo(Encoding.UTF8.GetBytes("108b9bf599069ba2955a89debc8a02f8f39b98eb7412cf18281c2862e1e907b2"));
    }
}