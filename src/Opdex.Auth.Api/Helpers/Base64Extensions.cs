using System;
using Ardalis.GuardClauses;

namespace Opdex.Auth.Api.Helpers;

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
}