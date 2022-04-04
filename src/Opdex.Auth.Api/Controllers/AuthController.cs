using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Opdex.Auth.Api.Conventions;
using Opdex.Auth.Api.Encryption;
using Opdex.Auth.Api.Models;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;

namespace Opdex.Auth.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/auth")]
[ApiVersion("1")]
[SnakeCaseJsonSerializationFilter]
public class AuthController : ControllerBase
{
    private readonly IOptionsSnapshot<PromptOptions> _promptOptions;
    private readonly IMediator _mediator;
    private readonly IJwtIssuer _jwtIssuer;

    public AuthController(IMediator mediator, IJwtIssuer jwtIssuer, IOptionsSnapshot<PromptOptions> promptOptions)
    {
        _promptOptions = Guard.Against.Null(promptOptions, nameof(promptOptions));
        _mediator = Guard.Against.Null(mediator, nameof(mediator));
        _jwtIssuer = Guard.Against.Null(jwtIssuer, nameof(jwtIssuer));
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize([FromQuery] AuthorizeRequestQuery query, CancellationToken cancellationToken)
    {
        var session = new AuthSession(new Uri(query.RedirectUri), query.CodeChallenge, query.CodeChallengeMethod);
        var authSessionCreated = await _mediator.Send(new PersistAuthSessionCommand(session), cancellationToken);
        if (!authSessionCreated) return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status500InternalServerError);

        var authPromptUri = $"{_promptOptions.Value.Prompt}?redirect_uri={query.RedirectUri}&stamp={session.Stamp}";
        if (query.State is not null) authPromptUri += $"&state={query.State}";
        return Redirect(authPromptUri);
    }

    [HttpPost("token")]
    public async Task<IActionResult> RequestToken([FromForm] AccessTokenRequestBody body, CancellationToken cancellationToken)
    {
        var authCode = await _mediator.Send(new SelectAuthCodeByValueQuery(body.Code), cancellationToken);
        if (authCode is null || !authCode.Expired)
            return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status400BadRequest, "Invalid or expired authorization code");

        var authSession = await _mediator.Send(new SelectAuthSessionByIdQuery(authCode.Stamp), cancellationToken);
        if (!authSession.Verify(body.CodeVerifier))
            return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status400BadRequest, "Unable to verify code challenge");

        await _mediator.Send(new DeleteAuthCodeCommand(authCode), CancellationToken.None);
        var bearerToken = _jwtIssuer.Create(authCode.Signer, authSession.Audience);
        return Ok(bearerToken);
    }

    [HttpGet("keys")]
    public async Task<ActionResult<JsonWebKeySetResponse>> GetKeys()
    {
        return Ok(new JsonWebKeySetResponse(await _jwtIssuer.GetPublicKeys()));
    }
}