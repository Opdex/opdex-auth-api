using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using FluentResults;
using Opdex.Auth.Domain.Cirrus;
using Opdex.Auth.Domain.Helpers;
using SSAS.NET;

namespace Opdex.Auth.Api.Encryption;

public class StratisIdValidator
{
    private readonly IWalletModule _walletModule;
    private readonly ITwoWayEncryptionProvider _twoWayEncryptionProvider;

    public StratisIdValidator(ITwoWayEncryptionProvider twoWayEncryptionProvider, IWalletModule walletModule)
    {
        _twoWayEncryptionProvider = Guard.Against.Null(twoWayEncryptionProvider, nameof(twoWayEncryptionProvider));
        _walletModule = Guard.Against.Null(walletModule);
    }
    
    public async Task<Result<string>> RetrieveConnectionId(string callbackUri, StratisSignatureAuthCallbackQuery query,
                                                           StratisSignatureAuthCallbackBody body, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(callbackUri, nameof(callbackUri));
        Guard.Against.Null(query, nameof(query));
        Guard.Against.Null(body, nameof(body));
        
        var expectedId = new StratisId(callbackUri, query.Uid, query.Exp);
    
        if (expectedId.Expired) return Result.Fail(new ExceededExpError());
    
        var verified = await _walletModule.VerifySignedMessage(expectedId.Callback, body.PublicKey, body.Signature, cancellationToken);
        if (!verified) return Result.Fail(new CannotVerifySignatureError());
    
        string connectionId;
        long actualExp;
    
        try
        {
            var actualUid = _twoWayEncryptionProvider.Decrypt(Base64Extensions.UrlSafeBase64Decode(expectedId.Uid));
            const int unixTimestampLength = 10;
            connectionId = actualUid[..^unixTimestampLength];
            actualExp = long.Parse(actualUid[^unixTimestampLength..]);
        }
        catch (Exception)
        {
            return Result.Fail(new InvalidUidError());
        }
    
        return actualExp != query.Exp ? Result.Fail(new DifferentExpError()) : Result.Ok(connectionId);
    }

    public async Task<Result<string>> RetrieveConnectionId(string callbackUri, string uid, DateTimeOffset exp, string publicKey,
                                                           string signature, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(callbackUri, nameof(callbackUri));
        Guard.Against.NullOrEmpty(uid, nameof(uid));
        Guard.Against.NullOrEmpty(publicKey, nameof(publicKey));
        Guard.Against.NullOrEmpty(signature, nameof(signature));

        var query = new StratisSignatureAuthCallbackQuery { Uid = uid, Exp = exp.ToUnixTimeSeconds() };
        var body = new StratisSignatureAuthCallbackBody { PublicKey = publicKey, Signature = signature };
        return await RetrieveConnectionId(callbackUri, query, body, cancellationToken);
    }

    private class CannotVerifySignatureError : Error
    {
        public CannotVerifySignatureError() : base("Signer could not be verified")
        {
        }
    }

    private class InvalidUidError : Error
    {
        public InvalidUidError() : base("Uid is malformed")
        {
        }
    }

    private class ExceededExpError : Error
    {
        public ExceededExpError() : base("Stratis id expired")
        {
        }
    }

    private class DifferentExpError : Error
    {
        public DifferentExpError() : base("Invalid exp timestamp")
        {
        }
    }
}