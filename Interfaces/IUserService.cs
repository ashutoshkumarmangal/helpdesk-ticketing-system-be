using HelpDesk.Api.Models.DTOs.Users;

namespace HelpDesk.Api.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetUsersAsync();

    Task<List<UserResponse>> GetAgentsAsync();

    Task<UserResponse> GetUserAsync(int id);

    Task<UserResponse> UpdateRoleAsync(int id, Models.DTOs.Users.UpdateUserRoleRequest request);

    Task<UserResponse> UpdateStatusAsync(int id, Models.DTOs.Users.UpdateUserStatusRequest request);
}