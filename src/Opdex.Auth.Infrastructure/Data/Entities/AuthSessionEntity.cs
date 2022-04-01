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
        Challenge = authSession.CodeChallenge;
        ChallengeMethod = authSession.CodeChallengeMethod;
        ConnectionId = authSession.ConnectionId;
    }

    public Guid Id { get; set; }
    public string Challenge { get; set; }
    public CodeChallengeMethod ChallengeMethod { get; set; }
    public string? ConnectionId { get; set; }
}