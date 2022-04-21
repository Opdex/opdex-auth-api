using Opdex.Auth.Domain;

namespace Opdex.Auth.Infrastructure.Data.Entities;

public class TokenLogEntity
{
    public TokenLogEntity()
    {
    }
    
    public TokenLogEntity(ulong authSuccessId, TokenLog tokenLog)
    {
        AuthSuccessId = authSuccessId;
        var (refreshToken, createdAt) = tokenLog;
        RefreshToken = refreshToken;
        CreatedAt = createdAt;
    }

    public string RefreshToken { get; set; }

    public ulong AuthSuccessId { get; set; }

    public DateTime CreatedAt { get; set; }
}