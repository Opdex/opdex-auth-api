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
    private readonly IStratisIdGenerator _stratisIdGenerator;
    private readonly ITwoWayEncryptionProvider _twoWayEncryptionProvider;

    public AuthHub(IMediator mediator, IStratisIdGenerator stratisIdGenerator, ITwoWayEncryptionProvider twoWayEncryptionProvider)
    {
        _mediator = Guard.Against.Null(mediator, nameof(mediator));
        _stratisIdGenerator = Guard.Against.Null(stratisIdGenerator, nameof(stratisIdGenerator));
        _twoWayEncryptionProvider = Guard.Against.Null(twoWayEncryptionProvider, nameof(twoWayEncryptionProvider));
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

        return _stratisIdGenerator.Create("v1/ssas/callback", Context.ConnectionId).ToString();
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