namespace Opdex.Auth.Domain.Cirrus;

public interface IWalletModule
{
    Task<bool> VerifySignedMessage(string message, string signer, string signature, CancellationToken cancellationToken = default);
}