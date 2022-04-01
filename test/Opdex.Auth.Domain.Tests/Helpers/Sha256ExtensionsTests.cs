using System.Text;
using FluentAssertions;
using Opdex.Auth.Domain.Helpers;
using Xunit;

namespace Opdex.Auth.Domain.Tests.Helpers;

public class Sha256ExtensionsTests
{
    [Fact]
    public void Sha256_Hash_Success()
    {
        // Arrange
        const string value = "de09b1a8de6c41cd9ec220bf8bf3ea88";
        var sb = new StringBuilder();

        // Act
        var encoded = Sha256Extensions.Hash(value);

        // Assert
        foreach (var b in encoded) sb.Append(b.ToString("x2"));
        sb.ToString().Should().Be("108b9bf599069ba2955a89debc8a02f8f39b98eb7412cf18281c2862e1e907b2");
    }
}