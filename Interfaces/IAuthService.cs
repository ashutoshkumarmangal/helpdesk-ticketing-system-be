using HelpDesk.Api.Models.DTOs.Auth;

namespace HelpDesk.Api.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);

    Task<LoginResponse> LoginAsync(LoginRequest request);
}