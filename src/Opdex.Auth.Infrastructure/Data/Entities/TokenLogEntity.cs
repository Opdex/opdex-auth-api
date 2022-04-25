using Opdex.Auth.Domain;

namespace Opdex.Auth.Infrastructure.Data.Entities;

public class TokenLogEntity
{
    public TokenLogEntity()
    {
    }
    
    public TokenLogEntity(TokenLog tokenLog)
    {
        RefreshToken = tokenLog.RefreshToken;
        AuthSuccessId = tokenLog.AuthSuccessId;
        CreatedAt = tokenLog.CreatedAt;
    }

    public string RefreshToken { get; set; }

    public ulong AuthSuccessId { get; set; }

    public DateTime CreatedAt { get; set; }
}