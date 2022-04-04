using System.Collections.Generic;
using System.Threading.Tasks;

namespace Opdex.Auth.Api.Encryption;

public interface IJwtIssuer
{
    string Create(string walletAddress, string audience);

    Task<IReadOnlyCollection<RsaPubJsonWebKeyItem>> GetPublicKeys();
}