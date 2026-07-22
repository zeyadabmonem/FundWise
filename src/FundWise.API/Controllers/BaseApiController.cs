using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using FundWise.Application.Common;

namespace FundWise.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _sender;
    protected ISender Mediator => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected Guid UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
        }
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error.Code switch
        {
            var c when c.Contains("NotFound") => NotFound(new ProblemDetails { Title = "Not Found", Detail = result.Error.Description }),
            var c when c.Contains("Unauthorized") => Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = result.Error.Description }),
            var c when c.Contains("Conflict") || c.Contains("Duplicate") => Conflict(new ProblemDetails { Title = "Conflict", Detail = result.Error.Description }),
            _ => BadRequest(new ProblemDetails { Title = result.Error.Code, Detail = result.Error.Description })
        };
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return Ok(new { success = true });

        return result.Error.Code switch
        {
            var c when c.Contains("NotFound") => NotFound(new ProblemDetails { Title = "Not Found", Detail = result.Error.Description }),
            var c when c.Contains("Unauthorized") => Unauthorized(new ProblemDetails { Title = "Unauthorized", Detail = result.Error.Description }),
            var c when c.Contains("Conflict") || c.Contains("Duplicate") => Conflict(new ProblemDetails { Title = "Conflict", Detail = result.Error.Description }),
            _ => BadRequest(new ProblemDetails { Title = result.Error.Code, Detail = result.Error.Description })
        };
    }
}
