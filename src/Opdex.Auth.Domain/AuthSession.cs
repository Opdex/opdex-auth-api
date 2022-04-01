using Ardalis.GuardClauses;
using Opdex.Auth.Domain.Helpers;

namespace Opdex.Auth.Domain;

public class AuthSession
{
    public AuthSession(string codeChallenge, CodeChallengeMethod codeChallengeMethod)
        : this(Guid.NewGuid(), codeChallenge, codeChallengeMethod)
    {
    }

    public AuthSession(Guid stamp, string codeChallenge, CodeChallengeMethod codeChallengeMethod, string? connectionId = null)
    {
        Stamp = stamp;
        CodeChallenge = Guard.Against.Null(codeChallenge);
        CodeChallengeMethod = Guard.Against.EnumOutOfRange(codeChallengeMethod, nameof(codeChallengeMethod));
        ConnectionId = connectionId;
    }

    public Guid Stamp { get; }
    public string CodeChallenge { get; }
    public CodeChallengeMethod CodeChallengeMethod { get; }
    public string? ConnectionId { get; private set; }

    public void EstablishPrompt(string connectionId)
    {
        if (ConnectionId is not null) throw new InvalidOperationException("Connection already associated with session");
        ConnectionId = Guard.Against.Null(connectionId);
    }

    public bool Verify(string codeVerifier)
    {
        return CodeChallengeMethod switch
        {
            CodeChallengeMethod.Plain => CodeChallenge == codeVerifier,
            CodeChallengeMethod.S256 => CodeChallenge == Base64Extensions.UrlSafeBase64Encode(Sha256Extensions.Hash(codeVerifier)),
            _ => throw new InvalidOperationException("Challenge method not recognized")
        };
    }
}