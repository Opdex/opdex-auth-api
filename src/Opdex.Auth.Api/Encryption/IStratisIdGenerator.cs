using SSAS.NET;

namespace Opdex.Auth.Api.Encryption;

public interface IStratisIdGenerator
{
    StratisId Create(string callbackPath, string uid);
}