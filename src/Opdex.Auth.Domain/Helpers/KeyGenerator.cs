using System.Security.Cryptography;
using System.Text;

namespace Opdex.Auth.Domain.Helpers;

/// <summary>
/// Pseudo-random alphanumeric key generator
/// </summary>
public static class KeyGenerator
{
    private static readonly char[] CharacterSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public static string Random(int size)
    {
        var data = new byte[4 * size];
        using var crypto = RandomNumberGenerator.Create();
        {
            crypto.GetBytes(data);
        }

        var result = new StringBuilder(size);

        for (var i = 0; i < size; i++)
        {
            var random = BitConverter.ToUInt32(data, i * 4);
            var index = random % CharacterSet.Length;

            result.Append(CharacterSet[index]);
        }

        return result.ToString();
    }
}