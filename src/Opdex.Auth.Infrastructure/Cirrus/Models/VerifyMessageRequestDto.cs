using Ardalis.GuardClauses;

namespace Opdex.Auth.Infrastructure.Cirrus.Models;

public class VerifyMessageRequestDto
{
    public VerifyMessageRequestDto(string message, string signer, string signature)
    {
        Message = Guard.Against.NullOrEmpty(message, nameof(message));
        ExternalAddress = Guard.Against.NullOrEmpty(signer, nameof(signer));
        Signature = Guard.Against.NullOrEmpty(signature, nameof(signature));
    }

    public string Message { get; }
    public string ExternalAddress { get; }
    public string Signature { get; }
}