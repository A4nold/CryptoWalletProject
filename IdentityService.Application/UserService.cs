using IdentityService.Application.Contracts;
using IdentityService.Application.Models;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application;

public class UserService : IUserService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IJwtTokenGenerators _jwtTokenGenerator;

    public UserService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IJwtTokenGenerators jwtTokenGenerator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            return Result<AuthResponse>.Failure("Email is already in use.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Username,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToArray();
            return Result<AuthResponse>.Failure(errors);
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return Result<AuthResponse>.Success(
            new AuthResponse(token, user.Id.ToString(), user.Email!));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result<AuthResponse>.Failure("Invalid credentials.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user, request.Password, lockoutOnFailure: false);

        if (!signInResult.Succeeded)
        {
            return Result<AuthResponse>.Failure("Invalid credentials.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return Result<AuthResponse>.Success(
            new AuthResponse(token, user.Id.ToString(), user.Email!));
    }
}

