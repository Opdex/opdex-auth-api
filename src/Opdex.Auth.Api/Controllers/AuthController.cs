using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Opdex.Auth.Api.Conventions;
using Opdex.Auth.Api.Encryption;
using Opdex.Auth.Api.Helpers;
using Opdex.Auth.Api.Models;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using SSAS.NET;

namespace Opdex.Auth.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/auth")]
[ApiVersion("1")]
public class AuthController : ControllerBase
{
    private readonly ITwoWayEncryptionProvider _twoWayEncryptionProvider;
    private readonly IMediator _mediator;
    private readonly StratisIdValidator _stratisIdValidator;
    private readonly IJwtIssuer _jwtIssuer;
    private readonly IOptionsSnapshot<ApiOptions> _apiOptions;

    public AuthController(ITwoWayEncryptionProvider twoWayEncryptionProvider, IMediator mediator,
                          StratisIdValidator stratisIdValidator, IJwtIssuer jwtIssuer, IOptionsSnapshot<ApiOptions> apiOptions)
    {
        _twoWayEncryptionProvider = Guard.Against.Null(twoWayEncryptionProvider, nameof(twoWayEncryptionProvider));
        _mediator = Guard.Against.Null(mediator);
        _stratisIdValidator = Guard.Against.Null(stratisIdValidator);
        _jwtIssuer = Guard.Against.Null(jwtIssuer);
        _apiOptions = Guard.Against.Null(apiOptions);
    }
    
    [HttpGet]
    public ActionResult<string> GetStratisId()
    {
        var expiry = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds();
        var uid = Base64Extensions.UrlSafeBase64Encode(_twoWayEncryptionProvider.Encrypt($"{Guid.NewGuid()}{expiry}"));
        return Ok(new StratisId($"{_apiOptions.Value.Authority}{Request.Path}", uid, expiry).ToString());
    }

    [HttpPost]
    public async Task<IActionResult> StratisSignatureAuth([FromQuery] StratisSignatureAuthCallbackQuery query,
        [FromBody] StratisSignatureAuthCallbackBody body,
        CancellationToken cancellationToken)
    {
        var result = await _stratisIdValidator.RetrieveConnectionId($"{_apiOptions.Value.Authority}{Request.Path}", query, body, cancellationToken);
        if (result.IsFailed) return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status400BadRequest, result.Errors[0].Message);

        var bearerToken = _jwtIssuer.Create(body.PublicKey);
        return Ok(bearerToken);
    }
    
    [HttpPost]
    [Route("callback")]
    public async Task<IActionResult> StratisSignatureAuthCallback([FromQuery] StratisSignatureAuthCallbackQuery query,
        [FromBody] StratisSignatureAuthCallbackBody body,
        CancellationToken cancellationToken)
    {
        var result = await _stratisIdValidator.RetrieveConnectionId($"{_apiOptions.Value.Authority}{Request.Path}", query, body, cancellationToken);
        if (result.IsFailed) return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status400BadRequest, result.Errors[0].Message);
        
        var bearerToken = _jwtIssuer.Create(body.PublicKey);
        await _mediator.Send(new PersistAuthSuccessCommand(new AuthSuccess(result.Value, body.PublicKey)), cancellationToken);
        await _mediator.Send(new NotifyAuthSuccessCommand(result.Value, bearerToken), cancellationToken);
        return NoContent();
    }

    [HttpGet]
    [Route("jwks")]
    public async Task<ActionResult<JsonWebKeySetResponseModel>> GetJwks()
    {
        return Ok(new JsonWebKeySetResponseModel(await _jwtIssuer.GetPublicKeys()));
    }
}