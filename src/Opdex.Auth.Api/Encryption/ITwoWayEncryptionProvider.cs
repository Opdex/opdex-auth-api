using System;

namespace Opdex.Auth.Api.Encryption;

public interface ITwoWayEncryptionProvider
{
    string Decrypt(ReadOnlySpan<byte> cipherText);

    ReadOnlySpan<byte> Encrypt(string plainText);
}