using System.Threading;
using System.Threading.Tasks;

namespace Opdex.Auth.Api.Encryption;

public interface IJwtIssuer
{
    Task<string> Create(string walletAddress, CancellationToken cancellationToken = default);
}