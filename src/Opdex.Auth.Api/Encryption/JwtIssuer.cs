using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.KeyVaultExtensions;
using Microsoft.IdentityModel.Tokens;

namespace Opdex.Auth.Api.Encryption;

public class JwtIssuer : IJwtIssuer
{
    private readonly string _keyId;
    private readonly string _keyIdHeaderValue;
    private readonly IOptionsSnapshot<ApiOptions> _apiOptions;

    public JwtIssuer(IOptionsSnapshot<AzureKeyVaultOptions> azureKeyVaultOptions, IOptionsSnapshot<JwtOptions> jwtOptions,
                     IOptionsSnapshot<ApiOptions> apiOptions)
    {
        Guard.Against.Null(azureKeyVaultOptions);
        Guard.Against.Null(jwtOptions);

        _keyId = $"https://{azureKeyVaultOptions.Value.Name}.vault.azure.net/keys/{jwtOptions.Value.SigningKeyIdentifier}";
        _keyIdHeaderValue = jwtOptions.Value.SigningKeyVersion;
        _apiOptions = Guard.Against.Null(apiOptions);
    }

    public string Create(string walletAddress)
    {
        Guard.Against.NullOrEmpty(walletAddress, nameof(walletAddress));

        var securityKey = new AmendedKeyVaultSecurityKey(_keyId, _keyIdHeaderValue, CreateAuthCallback());
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(),
            Issuer = _apiOptions.Value.Authority,
            // Todo: This should technically be much lower once refresh tokens are implemented, maybe back to 1 hour.
            Expires = DateTime.UtcNow.AddHours(24),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CustomCryptoProvider = new KeyVaultCryptoProvider() }
            }
        };

        tokenDescriptor.Subject.AddClaim(new Claim("wallet", walletAddress));

        var jwt = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(jwt);
    }

    public async Task<IReadOnlyCollection<RsaPubJsonWebKeyItem>> GetPublicKeys()
    {
        var keys = new List<RsaPubJsonWebKeyItem>();
        using var client = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(CreateAuthCallback()));
        {
            var bundle = await client.GetKeyAsync(_keyId, CancellationToken.None);
            keys.Add(RsaPubJsonWebKeyItem.Create(bundle));
        }
        return keys.AsReadOnly();
    }

    private static KeyVaultSecurityKey.AuthenticationCallback CreateAuthCallback()
    {
        var azureServiceTokenProvider = new AzureServiceTokenProvider();
        return new KeyVaultSecurityKey.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
    }
}