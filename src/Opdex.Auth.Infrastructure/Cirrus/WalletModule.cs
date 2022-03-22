using System.Net.Http.Json;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opdex.Auth.Domain.Cirrus;
using Opdex.Auth.Infrastructure.Cirrus.Models;

namespace Opdex.Auth.Infrastructure.Cirrus;

public class WalletModule : IWalletModule
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WalletModule> _logger;

    public WalletModule(HttpClient httpClient, IOptions<CirrusOptions> cirrusOptions, ILogger<WalletModule> logger)
    {
        _httpClient = Guard.Against.Null(httpClient, nameof(httpClient));
        _logger = Guard.Against.Null(logger, nameof(logger));

        Guard.Against.Null(cirrusOptions, nameof(cirrusOptions));
        
        var baseAddress = cirrusOptions.Value.ApiPort is > 0
            ? $"{cirrusOptions.Value.ApiUrl}:{cirrusOptions.Value.ApiPort}"
            : $"{cirrusOptions.Value.ApiUrl}";
        _httpClient.BaseAddress = new Uri(baseAddress);
    }
    
    public async Task<bool> VerifySignedMessage(string message, string signer, string signature, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(message, nameof(message));
        Guard.Against.NullOrEmpty(signer, nameof(signer));
        Guard.Against.NullOrEmpty(signature, nameof(signature));
        
        var logDetails = new Dictionary<string, object>
        {
            ["Message"] = message,
            ["Signer"] = signature,
        };

        var request = new VerifyMessageRequestDto(message, signer, signature);

        using (_logger.BeginScope(logDetails))
        {
            var response = await _httpClient.PostAsJsonAsync("/api/Wallet/verifymessage", request, cancellationToken);
            return await response.Content.ReadFromJsonAsync<bool>(CirrusSerialization.DefaultOptions, cancellationToken);
        }
    }
}