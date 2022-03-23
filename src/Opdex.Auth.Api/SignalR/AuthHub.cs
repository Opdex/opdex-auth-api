using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Opdex.Auth.Api.Encryption;
using Opdex.Auth.Api.Helpers;
using Opdex.Auth.Domain.Requests;
using SSAS.NET;

namespace Opdex.Auth.Api.SignalR;

public class AuthHub : Hub<IAuthClient>
{
    private readonly IMediator _mediator;
    private readonly ITwoWayEncryptionProvider _twoWayEncryptionProvider;
    private readonly IJwtIssuer _jwtIssuer;

    public AuthHub(IMediator mediator, ITwoWayEncryptionProvider twoWayEncryptionProvider, IJwtIssuer jwtIssuer)
    {
        _mediator = Guard.Against.Null(mediator, nameof(mediator));
        _twoWayEncryptionProvider = Guard.Against.Null(twoWayEncryptionProvider, nameof(twoWayEncryptionProvider));
        _jwtIssuer = Guard.Against.Null(jwtIssuer, nameof(jwtIssuer));
    }

    /// <summary>
    /// Returns a Stratis ID that can be used for authentication.
    /// </summary>
    /// <returns>Stratis ID.</returns>
    public string GetStratisId()
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        var encryptedConnectionId = _twoWayEncryptionProvider.Encrypt($"{Context.ConnectionId}{expiry}");
        var uid = Base64Extensions.UrlSafeBase64Encode(encryptedConnectionId);
        var stratisId = new StratisId($"{Context.GetHttpContext()!.Request.BaseUrl()}/v1/auth/callback", uid, expiry);
        return stratisId.ToString();
    }

    public async Task<bool> Reconnect(string previousConnectionId, string sid)
    {
        if (!StratisId.TryParse(sid, out var stratisId) || stratisId.Expired) return false;

        const int unixTimestampLength = 10;
        var decryptedUid = _twoWayEncryptionProvider.Decrypt(Base64Extensions.UrlSafeBase64Decode(stratisId.Uid));
        var expectedConnectionId = decryptedUid[..^unixTimestampLength];
        if (previousConnectionId != expectedConnectionId) return false;

        var authSuccess = await _mediator.Send(new SelectAuthSuccessByConnectionIdQuery(previousConnectionId));
        if (authSuccess is null || authSuccess.Expiry < DateTime.UtcNow) return false;

        var bearerToken = await _jwtIssuer.Create(authSuccess.Signer);
        await Clients.Caller.OnAuthenticated(bearerToken);

        return true;
    }
}