using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;

namespace Opdex.Auth.Api.Encryption;

public sealed class AesCbcProvider : ITwoWayEncryptionProvider, IDisposable
{
    private readonly Aes _aes;

    public AesCbcProvider(IOptionsSnapshot<EncryptionOptions> encryptionOptions)
    {
        Guard.Against.Null(nameof(encryptionOptions), nameof(encryptionOptions));
        
        _aes = Aes.Create();
        _aes.Mode = CipherMode.CBC;
        _aes.Padding = PaddingMode.PKCS7;
        _aes.Key = Encoding.UTF8.GetBytes(encryptionOptions.Value.Key);
    }

    public string Decrypt(ReadOnlySpan<byte> encrypted)
    {
        var iv = encrypted[..16];
        var cipher = encrypted[16..];

        _aes.IV = iv.ToArray();
        using var decryptor = _aes.CreateDecryptor();
        using var memoryStream = new MemoryStream(cipher.ToArray());
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);
        return streamReader.ReadToEnd();
    }

    public ReadOnlySpan<byte> Encrypt(string plainText)
    {
        Guard.Against.NullOrEmpty(plainText, nameof(plainText));
        
        _aes.GenerateIV();

        ReadOnlySpan<byte> iv = _aes.IV;

        using var encryptor = _aes.CreateEncryptor();
        using var memoryStream = new MemoryStream();
        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
        using (var streamWriter = new StreamWriter(cryptoStream))
        {
            streamWriter.Write(plainText);
        }

        ReadOnlySpan<byte> cipher = memoryStream.ToArray();

        Span<byte> encrypted = stackalloc byte[iv.Length + cipher.Length];
        iv.CopyTo(encrypted);
        cipher.CopyTo(encrypted[iv.Length..]);

        return encrypted.ToArray();
    }

    public void Dispose()
    {
        _aes.Dispose();
    }
}