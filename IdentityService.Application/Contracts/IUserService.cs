using IdentityService.Application.Models;

namespace IdentityService.Application.Contracts;

public interface IUserService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
}
