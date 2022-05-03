using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using FluentValidation;
using FluentValidation.Results;
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
    private readonly IMediator _mediator;
    private readonly IJwtIssuer _jwtIssuer;
    private readonly IOptionsSnapshot<PromptOptions> _promptOptions;
    private readonly IOptionsSnapshot<ApiOptions> _apiOptions;
    private readonly IStratisIdGenerator _stratisIdGenerator;
    private readonly StratisIdValidator _stratisIdValidator;

    public AuthController(IMediator mediator, IJwtIssuer jwtIssuer,
                          IOptionsSnapshot<ApiOptions> apiOptions, IOptionsSnapshot<PromptOptions> promptOptions,
                          IStratisIdGenerator stratisIdGenerator, StratisIdValidator stratisIdValidator)
    {
        _mediator = Guard.Against.Null(mediator, nameof(mediator));
        _jwtIssuer = Guard.Against.Null(jwtIssuer, nameof(jwtIssuer));
        _apiOptions = Guard.Against.Null(apiOptions, nameof(apiOptions));
        _promptOptions = Guard.Against.Null(promptOptions, nameof(promptOptions));
        _stratisIdGenerator = Guard.Against.Null(stratisIdGenerator, nameof(stratisIdGenerator));
        _stratisIdValidator = Guard.Against.Null(stratisIdValidator, nameof(stratisIdValidator));
    }

    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize([FromQuery] AuthorizeRequestQuery query, CancellationToken cancellationToken)
    {
        AuthSession? session;
        bool authSessionCreated;
        
        switch (query.ResponseType)
        {
            case ResponseType.Code:
                session = new AuthSession(new Uri(query.RedirectUri!), query.CodeChallenge!, query.CodeChallengeMethod);
                authSessionCreated = await _mediator.Send(new PersistAuthSessionCommand(session), cancellationToken);
                if (!authSessionCreated) return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status500InternalServerError);

                var authPromptUri = $"{_promptOptions.Value.Prompt}?redirect_uri={query.RedirectUri}&stamp={session.Stamp}";
                if (query.State is not null) authPromptUri += $"&state={query.State}";
                return Redirect(authPromptUri);
            case ResponseType.Sid:
                var sid = _stratisIdGenerator.Create("v1/auth/token", Guid.NewGuid().ToString());
                session = new AuthSession(sid.Uid);
                authSessionCreated = await _mediator.Send(new PersistAuthSessionCommand(session), cancellationToken);
                return !authSessionCreated ? ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status500InternalServerError) : Ok(sid.ToUriString());
            default:
                // should already have been validated by fluent validation
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(query.ResponseType), "Invalid response type")
                });
        }
    }

    [HttpPost("token")]
    public async Task<IActionResult> RequestToken([FromForm] TokenRequestBody body, CancellationToken cancellationToken)
    {
        var accessToken = "";
        AuthSuccess? authSuccess;
        AuthSession? authSession;
        
        switch (body.GrantType)
        {
            case GrantType.AuthorizationCode:
            {
                var authCode = await _mediator.Send(new SelectAuthCodeByValueQuery((Guid)body.Code!), cancellationToken);
                if (authCode is null || !authCode.Expired)
                    return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status400BadRequest, "Invalid or expired authorization code");

                authSession = await _mediator.Send(new SelectAuthSessionByIdQuery(authCode.Stamp), cancellationToken);
                if (!authSession!.Verify(body.CodeVerifier!))
                    return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status400BadRequest, "Unable to verify code challenge");

                await _mediator.Send(new DeleteAuthCodeCommand(authCode), CancellationToken.None);
        
                accessToken = _jwtIssuer.Create(authCode.Signer, authSession.Audience);

                // invalidate old refresh tokens
                authSuccess = await _mediator.Send(new SelectAuthSuccessByTargetQuery(authCode.Signer, authSession.Audience), cancellationToken);
                if (authSuccess is not null)  await _mediator.Send(new DeleteAuthSuccessCommand(authSuccess), cancellationToken);
                authSuccess = new AuthSuccess(authCode.Signer, authSession.Audience);
                
                break;
            }
            case GrantType.Sid:
                authSession = await _mediator.Send(new SelectAuthSessionByConnectionIdQuery(body.Sid!.Uid), cancellationToken);
                if (authSession is null) return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status400BadRequest, "Unable to verify sid");

                // delete session to prevent sid reuse
                await _mediator.Send(new DeleteAuthSessionCommand(authSession), cancellationToken);
                
                var callbackUri = $"{_apiOptions.Value.Authority}{Request.Path}";
                var result = await _stratisIdValidator.RetrieveConnectionId(callbackUri, body.Sid!.Uid, body.Sid!.Expiry, body.PublicKey!, body.Signature!, cancellationToken);
                if (result.IsFailed) return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status400BadRequest, result.Errors[0].Message);
                
                accessToken = _jwtIssuer.Create(body.PublicKey!);
                
                // invalidate old refresh tokens
                authSuccess = await _mediator.Send(new SelectAuthSuccessByTargetQuery(body.PublicKey!), cancellationToken);
                if (authSuccess is not null)  await _mediator.Send(new DeleteAuthSuccessCommand(authSuccess), cancellationToken);
                authSuccess = new AuthSuccess(body.PublicKey!);
                break;
            case GrantType.RefreshToken:
                authSuccess = await _mediator.Send(new SelectAuthSuccessByRefreshTokenQuery(body.RefreshToken!), cancellationToken);
                if (authSuccess is null) return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status403Forbidden, "Invalid refresh token");
                
                // invalidate expired or old refresh tokens
                if (!authSuccess.Valid)
                {
                    await _mediator.Send(new DeleteAuthSuccessCommand(authSuccess), cancellationToken);
                    return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status403Forbidden, "Invalid refresh token");
                }
                
                accessToken = _jwtIssuer.Create(authSuccess.Address, authSuccess.Audience!);
                break;
            default:
                // should already have been validated by fluent validation
                throw new ValidationException(new[]
                {
                    new ValidationFailure(nameof(body.GrantType), "Invalid grant type")
                });
        }

        var refreshToken = authSuccess.NewRefreshToken();
        await _mediator.Send(new PersistAuthSuccessCommand(authSuccess), cancellationToken);

        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";
        return Ok(new TokenResponseBody(accessToken, refreshToken));
    }

    [HttpGet("keys")]
    public async Task<ActionResult<JsonWebKeySetResponseBody>> GetKeys()
    {
        return Ok(new JsonWebKeySetResponseBody(await _jwtIssuer.GetPublicKeys()));
    }
}