using CoachPlatform.Application.Shared.Interfaces;
using CoachPlatform.Application.Features.Authentication.Commands.LoginUser;
using CoachPlatform.Application.Features.Authentication.Commands.RefreshToken;
using CoachPlatform.Application.Features.Authentication.Commands.LogoutUser;
using CoachPlatform.Application.Features.Authentication.Commands.CreateUser;
using CoachPlatform.Application.Features.Authentication.Commands.UpdateUser;
using CoachPlatform.Application.Features.Authentication.Commands.ResetPassword;
using CoachPlatform.Application.Features.Authentication.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoachPlatform.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Login with email and password. Returns JWT access + refresh tokens.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginUserCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>Refresh access token using a valid refresh token (token rotation).</summary>
    [Authorize]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>Logout current user — invalidates refresh token.</summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _mediator.Send(new LogoutUserCommand());
        return NoContent();
    }

    /// <summary>Get the currently authenticated user's profile from the DB.</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser(
        [FromServices] IUserRepository userRepository,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var user = await userRepository.GetByIdAsync(currentUserService.UserId.Value);
        if (user == null)
            return Unauthorized();

        return Ok(new UserDto(user.Id, user.Email, user.Username, user.Role.ToString(), user.CoachId, user.AthleteId, user.IsActive));
    }

    // ── Admin-only user management ──

    /// <summary>List all users. Admin only.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var result = await _mediator.Send(new GetUsersQuery());
        return Ok(result);
    }

    /// <summary>Create a new user account. Admin only.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("users")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetCurrentUser), result);
    }

    /// <summary>Activate or deactivate a user. Admin only.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("users/{userId:guid}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
    {
        var result = await _mediator.Send(new UpdateUserCommand(userId, request.Username, request.IsActive));
        return Ok(result);
    }

    /// <summary>Reset a user's password. Admin only.</summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpPost("users/{userId:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid userId, [FromBody] ResetPasswordRequest request)
    {
        await _mediator.Send(new ResetPasswordCommand(userId, request.NewPassword));
        return NoContent();
    }
}

public record UpdateUserRequest(string? Username, bool? IsActive);
public record ResetPasswordRequest(string NewPassword);
