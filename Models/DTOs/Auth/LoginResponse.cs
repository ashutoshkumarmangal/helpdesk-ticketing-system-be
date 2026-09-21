using HelpDesk.Api.Models.DTOs.Users;

namespace HelpDesk.Api.Models.DTOs.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public UserResponse User { get; set; } = new();
}