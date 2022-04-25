namespace Opdex.Auth.Domain;

public record TokenLog(string RefreshToken, DateTime CreatedAt)
{
    public TokenLog(string refreshToken, ulong authSuccessId, DateTime createdAt) : this(refreshToken, createdAt)
    {
        AuthSuccessId = authSuccessId;
    }
    
    public ulong AuthSuccessId { get; }
};