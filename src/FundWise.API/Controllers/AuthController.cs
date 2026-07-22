using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FundWise.Application.Features.Authentication.Commands;
using FundWise.Application.Features.Authentication.DTOs;
using FundWise.Application.Features.Authentication.Queries;

namespace FundWise.API.Controllers;

public sealed class AuthController : BaseApiController
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        => HandleResult(await Mediator.Send(command));

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
        => HandleResult(await Mediator.Send(command));

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthTokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
        => HandleResult(await Mediator.Send(command));

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
        => HandleResult(await Mediator.Send(new LogoutUserCommand(UserId)));

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile()
        => HandleResult(await Mediator.Send(new GetCurrentUserQuery(UserId)));

    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
        => HandleResult(await Mediator.Send(new UpdateUserProfileCommand(UserId, request.Name, request.Currency)));
}

public sealed record UpdateUserProfileRequest(string? Name, string? Currency);
