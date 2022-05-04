using Ardalis.GuardClauses;
using Opdex.Auth.Domain.Helpers;

namespace Opdex.Auth.Domain;

public class AuthSession
{
    public AuthSession(string uid) : this(Guid.NewGuid(), null, null, null, uid)
    {
    }
    
    public AuthSession(Uri redirectUri, string codeChallenge, CodeChallengeMethod codeChallengeMethod)
        : this(Guid.NewGuid(), redirectUri.Authority, codeChallenge, codeChallengeMethod)
    {
    }

    public AuthSession(Guid stamp, string? audience, string? codeChallenge, CodeChallengeMethod? challengeMethod, string? connectionId = null)
    {
        Stamp = stamp;
        Audience = audience;
        CodeChallenge = codeChallenge;
        ChallengeMethod = challengeMethod;
        ConnectionId = connectionId;
    }

    public Guid Stamp { get; }
    public string? Audience { get; }
    public string? CodeChallenge { get; }
    public CodeChallengeMethod? ChallengeMethod { get; }
    public string? ConnectionId { get; private set; }

    public ResponseType SessionType => CodeChallenge is not null ? ResponseType.Code : ResponseType.Sid;

    public void EstablishPrompt(string connectionId)
    {
        if (ConnectionId is not null && ConnectionId != connectionId) throw new InvalidOperationException("Connection already associated with session");
        ConnectionId = Guard.Against.Null(connectionId);
    }

    public bool Verify(string codeVerifier)
    {
        if (ChallengeMethod is null) return true;
        
        return ChallengeMethod.Value switch
        {
            CodeChallengeMethod.Plain => CodeChallenge == codeVerifier,
            CodeChallengeMethod.S256 => CodeChallenge == Base64Extensions.UrlSafeBase64Encode(Sha256Extensions.Hash(codeVerifier)),
            _ => throw new InvalidOperationException("Challenge method not recognized")
        };
    }
}