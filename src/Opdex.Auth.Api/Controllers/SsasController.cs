using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Opdex.Auth.Api.Conventions;
using Opdex.Auth.Api.Encryption;
using Opdex.Auth.Domain;
using Opdex.Auth.Domain.Requests;
using SSAS.NET;

namespace Opdex.Auth.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/ssas")]
[ApiVersion("1")]
public class SsasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly StratisIdValidator _stratisIdValidator;
    private readonly IOptionsSnapshot<ApiOptions> _apiOptions;

    public SsasController(IMediator mediator, StratisIdValidator stratisIdValidator, IOptionsSnapshot<ApiOptions> apiOptions)
    {
        _mediator = Guard.Against.Null(mediator);
        _stratisIdValidator = Guard.Against.Null(stratisIdValidator);
        _apiOptions = Guard.Against.Null(apiOptions);
    }
    
    [HttpPost]
    [Route("callback")]
    public async Task<IActionResult> StratisSignatureAuthCallback([FromQuery] StratisSignatureAuthCallbackQuery query,
        [FromBody] StratisSignatureAuthCallbackBody body,
        CancellationToken cancellationToken)
    {
        var result = await _stratisIdValidator.RetrieveConnectionId($"{_apiOptions.Value.Authority}{Request.Path}", query, body, cancellationToken);
        if (result.IsFailed) return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status400BadRequest, result.Errors[0].Message);

        var session = await _mediator.Send(new SelectAuthSessionByConnectionIdQuery(result.Value), cancellationToken);
        if (session is null) return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status400BadRequest, "Auth session has expired");
        
        var authCode = new AuthCode(body.PublicKey, session.Stamp);
        var authCodePersisted = await _mediator.Send(new PersistAuthCodeCommand(authCode), cancellationToken);
        if (!authCodePersisted) return ProblemDetailsBuilder.BuildResponse(HttpContext, StatusCodes.Status500InternalServerError);
        
        await _mediator.Send(new NotifyAuthSuccessCommand(result.Value, authCode.Value), cancellationToken);
        return NoContent();
    }
}