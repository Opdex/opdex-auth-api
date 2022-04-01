using System.Security.Cryptography;
using System.Text;

namespace Opdex.Auth.Domain.Helpers;

public static class Sha256Extensions
{
    public static byte[] Hash(string value)
    {
        using var hash = SHA256.Create();
        return hash.ComputeHash(Encoding.UTF8.GetBytes(value));
    }
}