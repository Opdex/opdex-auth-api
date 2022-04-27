using System;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Options;
using Opdex.Auth.Domain.Helpers;
using SSAS.NET;

namespace Opdex.Auth.Api.Encryption;

public class StratisIdGenerator : IStratisIdGenerator
{
    private readonly ITwoWayEncryptionProvider _twoWayEncryptionProvider;
    private readonly IOptionsSnapshot<ApiOptions> _apiOptions;

    public StratisIdGenerator(IOptionsSnapshot<ApiOptions> apiOptions, ITwoWayEncryptionProvider twoWayEncryptionProvider)
    {
        _apiOptions = Guard.Against.Null(apiOptions);
        _twoWayEncryptionProvider = Guard.Against.Null(twoWayEncryptionProvider);
    }

    public StratisId Create(string callbackPath, string uid)
    {
        callbackPath = callbackPath.TrimStart('/');
        var expiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        return new StratisId($"{_apiOptions.Value.Authority}/{callbackPath}", EncodeUid(uid, expiry), expiry);
    }

    private string EncodeUid(string uid, long expiry)
    {
        var encryptedConnectionId = _twoWayEncryptionProvider.Encrypt($"{uid}{expiry}");
        return Base64Extensions.UrlSafeBase64Encode(encryptedConnectionId);
    }
}