using System.Linq;
using System.Text.Json.Serialization;
using Ardalis.GuardClauses;
using Microsoft.Azure.KeyVault.Models;
using Opdex.Auth.Api.Conventions;

namespace Opdex.Auth.Api.Encryption;

public sealed class RsaPubJsonWebKeyItem : JsonWebKeyItem
{
    private RsaPubJsonWebKeyItem(string kid, string kty, byte[] n, byte[] e, string? alg, string? use)
        : base(kid, kty, alg, use)
    {
        N = n;
        E = e;
    }
    
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] N { get; }
    
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] E { get; }
    
    public static RsaPubJsonWebKeyItem Create(KeyBundle keyBundle)
    {
        Guard.Against.Null(keyBundle);
        Guard.Against.InvalidInput(keyBundle.Key.Kty, nameof(keyBundle), kty => kty == "RSA", "Only RSA keys are allowed");
        var kid = keyBundle.KeyIdentifier.Version;
        var alg = $"RS{keyBundle.Key.N.Length}";
        var use = keyBundle.Tags.SingleOrDefault(t => t.Key == "use").Value;
        return new RsaPubJsonWebKeyItem(kid, keyBundle.Key.Kty, keyBundle.Key.N, keyBundle.Key.E, alg, use);
    }
}