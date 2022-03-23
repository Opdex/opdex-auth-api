using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ardalis.GuardClauses;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.KeyVaultExtensions;
using Microsoft.IdentityModel.Tokens;

namespace Opdex.Auth.Api.Encryption;

public class JwtIssuer : IJwtIssuer
{
    private readonly IOptionsSnapshot<AzureKeyVaultOptions> _azureKeyVaultOptions;
    private readonly IOptionsSnapshot<JwtOptions> _jwtOptions;
    private readonly IOptionsSnapshot<ApiOptions> _apiOptions;

    public JwtIssuer(IOptionsSnapshot<AzureKeyVaultOptions> azureKeyVaultOptions, IOptionsSnapshot<JwtOptions> jwtOptions,
                     IOptionsSnapshot<ApiOptions> apiOptions)
    {
        _azureKeyVaultOptions = Guard.Against.Null(azureKeyVaultOptions);
        _jwtOptions = Guard.Against.Null(jwtOptions);
        _apiOptions = Guard.Against.Null(apiOptions);
    }

    public string Create(string walletAddress)
    {
        Guard.Against.NullOrEmpty(walletAddress, nameof(walletAddress));
        
        var azureServiceTokenProvider = new AzureServiceTokenProvider();
        var keyVaultSecurityKey = new KeyVaultSecurityKey(
            $"https://{_azureKeyVaultOptions.Value.Name}.vault.azure.net/keys/{_jwtOptions.Value.SigningKeyIdentifier}",
            new KeyVaultSecurityKey.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(),
            Issuer = _apiOptions.Value.Authority,
            // Todo: This should technically be much lower once refresh tokens are implemented, maybe back to 1 hour.
            Expires = DateTime.UtcNow.AddHours(24),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = new SigningCredentials(keyVaultSecurityKey, SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CustomCryptoProvider = new KeyVaultCryptoProvider() }
            }
        };

        tokenDescriptor.Subject.AddClaim(new Claim("wallet", walletAddress));

        var jwt = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(jwt);
    }
}