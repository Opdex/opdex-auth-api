using Ardalis.GuardClauses;
using Opdex.Auth.Domain;

namespace Opdex.Auth.Infrastructure.Data.Entities;

public class AuthSessionEntity
{
    public AuthSessionEntity()
    {
    }
    
    internal AuthSessionEntity(AuthSession authSession)
    {
        Guard.Against.Null(authSession, nameof(authSession));
        Id = authSession.Stamp;
        CodeChallenge = authSession.CodeChallenge;
        CodeChallengeMethod = authSession.CodeChallengeMethod;
        ConnectionId = authSession.ConnectionId;
    }

    public Guid Id { get; set; }
    public string CodeChallenge { get; set; }
    public CodeChallengeMethod CodeChallengeMethod { get; set; }
    public string? ConnectionId { get; set; }
}