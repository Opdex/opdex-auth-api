using Ardalis.GuardClauses;

namespace Opdex.Auth.Domain.Helpers;

public static class Base64Extensions
{
    public static string UrlSafeBase64Encode(ReadOnlySpan<byte> value) =>
        Convert.ToBase64String(value).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public static ReadOnlySpan<byte> UrlSafeBase64Decode(string value)
    {
        Guard.Against.NullOrEmpty(value, nameof(value));
        
        value = value.Replace('-', '+').Replace('_', '/');
        if (value.Length % 4 != 0) value = value.PadRight(value.Length + (4 - value.Length % 4), '=');
        return Convert.FromBase64String(value);
    }
    
    public static bool TryUrlSafeBase64Decode(string value, out ReadOnlySpan<byte> bytes)
    {
        bytes = null;
        if (string.IsNullOrEmpty(value) || value.All(character => character == '=')) return false;
        
        if (value.Length % 4 != 0) value = value.PadRight(value.Length + (4 - value.Length % 4), '=');
        
        var base64Data = value.Replace('-', '+').Replace('_', '/');
        var base64DataWithoutPadding = value.TrimEnd('=');
        
        Span<byte> writeTo = stackalloc byte[3 * base64DataWithoutPadding.Length / 4];
        var canConvert = Convert.TryFromBase64String(base64Data, writeTo, out var bytesWritten);
        bytes = writeTo[..bytesWritten].ToArray();
        return canConvert;
    }
}