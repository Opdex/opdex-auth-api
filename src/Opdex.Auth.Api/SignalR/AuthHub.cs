using System;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Opdex.Auth.Api.Encryption;
using Opdex.Auth.Domain.Helpers;
using Opdex.Auth.Domain.Requests;
using SSAS.NET;

namespace Opdex.Auth.Api.SignalR;

public class AuthHub : Hub<IAuthClient>
{
    private readonly IMediator _mediator;
    private readonly ITwoWayEncryptionProvider _twoWayEncryptionProvider;
    private readonly IOptionsSnapshot<ApiOptions> _apiOptions;

    public AuthHub(IMediator mediator, ITwoWayEncryptionProvider twoWayEncryptionProvider, IOptionsSnapshot<ApiOptions> apiOptions)
    {
        _mediator = Guard.Against.Null(mediator, nameof(mediator));
        _twoWayEncryptionProvider = Guard.Against.Null(twoWayEncryptionProvider, nameof(twoWayEncryptionProvider));
        _apiOptions = Guard.Against.Null(apiOptions, nameof(apiOptions));
    }

    /// <summary>
    /// Returns a Stratis ID that can be used for authentication.
    /// </summary>
    /// <returns>Stratis ID.</returns>
    public async Task<string> GetStratisId(string stamp)
    {
        if (!Guid.TryParse(stamp, out var sessionId)) throw new ArgumentException("Invalid stamp format", nameof(stamp));
        
        var authSession = await _mediator.Send(new SelectAuthSessionByIdQuery(sessionId));
        if (authSession is null) throw new AuthSessionConnectionException();
        
        authSession.EstablishPrompt(Context.ConnectionId);
        
        var sessionLinked = await _mediator.Send(new PersistAuthSessionCommand(authSession));
        if (!sessionLinked) throw new AuthSessionConnectionException();
        
        var expiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        return new StratisId($"{_apiOptions.Value.Authority}/v1/auth/callback", CreateUid(), expiry).ToString();

        string CreateUid()
        {
            var encryptedConnectionId = _twoWayEncryptionProvider.Encrypt($"{Context.ConnectionId}{expiry}");
            return Base64Extensions.UrlSafeBase64Encode(encryptedConnectionId);
        }
    }

    public async Task<bool> Reconnect(string previousConnectionId, string sid)
    {
        if (!StratisId.TryParse(sid, out var stratisId) || stratisId.Expired) return false;

        const int unixTimestampLength = 10;
        var decryptedUid = _twoWayEncryptionProvider.Decrypt(Base64Extensions.UrlSafeBase64Decode(stratisId.Uid));
        var expectedConnectionId = decryptedUid[..^unixTimestampLength];
        if (previousConnectionId != expectedConnectionId) return false;

        var authSession = await _mediator.Send(new SelectAuthSessionByConnectionIdQuery(previousConnectionId));
        if (authSession is null) return false;

        var authCode = await _mediator.Send(new SelectAuthCodeByStampQuery(authSession.Stamp));
        if (authCode is null) return true;
        
        await Clients.Caller.OnAuthenticated(authCode.Value.ToString());

        return true;
    }
}