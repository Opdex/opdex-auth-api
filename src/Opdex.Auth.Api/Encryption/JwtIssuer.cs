using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Opdex.Auth.Domain.Requests;

namespace Opdex.Auth.Api.Encryption;

public class JwtIssuer : IJwtIssuer
{
    private readonly IMediator _mediator;
    private readonly IOptionsSnapshot<JwtOptions> _jwtOptions;

    public JwtIssuer(IOptionsSnapshot<JwtOptions> jwtOptions, IMediator mediator)
    {
        _jwtOptions = Guard.Against.Null(jwtOptions);
        _mediator = Guard.Against.Null(mediator);
    }

    public async Task<string> Create(string walletAddress, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrEmpty(walletAddress, nameof(walletAddress));
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Value.SigningKey));
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(),
            // Todo: This should technically be much lower once refresh tokens are implemented, maybe back to 1 hour.
            Expires = DateTime.UtcNow.AddHours(24),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        tokenDescriptor.Subject.AddClaim(new Claim("wallet", walletAddress));
        var isAdmin = await _mediator.Send(new IsAdminQuery(walletAddress), cancellationToken);
        if (isAdmin) tokenDescriptor.Subject.AddClaim(new Claim("admin", "true"));

        var jwt = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(jwt);
    }
}