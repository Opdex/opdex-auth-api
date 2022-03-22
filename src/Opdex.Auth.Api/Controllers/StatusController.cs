using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Opdex.Auth.Api.Models;

namespace Opdex.Auth.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/status")]
[ApiVersion("1")]
public class StatusController : ControllerBase
{
    private readonly StatusOptions _statusOptions;

    public StatusController(IOptions<StatusOptions> statusOptions)
    {
        _statusOptions = statusOptions.Value;
    }
    
    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok(new StatusResponseModel(_statusOptions.CommitHash));
    }
}