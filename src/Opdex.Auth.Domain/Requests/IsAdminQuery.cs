using Ardalis.GuardClauses;
using MediatR;

namespace Opdex.Auth.Domain.Requests;

public class IsAdminQuery : IRequest<bool>
{
    public IsAdminQuery(string walletAddress)
    {
        WalletAddress = Guard.Against.NullOrEmpty(walletAddress, nameof(walletAddress));
    }
    
    public string WalletAddress { get; }
}