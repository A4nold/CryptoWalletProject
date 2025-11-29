using IdentityService.Application.Contracts;
using IdentityService.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IdentityService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { errors = result.Errors });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Returns information about the current authenticated user,
    /// based purely on the JWT claims.
    /// 
    /// This endpoint is protected with [Authorize], so you must send a
    /// valid Bearer token in the Authorization header.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        // The JWT was validated by the authentication middleware before hitting this endpoint.
        // Here we just read the claims that JwtTokenGenerator put into the token.

        // Try to get the user id from the "sub" (subject) claim first.
        var userId =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Email can be found under the standard "email" claim type or JwtRegisteredClaimNames.Email.
        var email =
            User.FindFirstValue(JwtRegisteredClaimNames.Email) ??
            User.FindFirstValue(ClaimTypes.Email);

        // "username" is a custom claim we added when generating the JWT.
        var username = User.FindFirstValue("username");

        if (userId is null)
        {
            // This would mean the token didn't contain the claims we expect.
            // It's a good safeguard and helps debugging.
            return BadRequest(new { error = "User id claim not found in token." });
        }

        return Ok(new
        {
            userId,
            email,
            username
        });
    }
}

