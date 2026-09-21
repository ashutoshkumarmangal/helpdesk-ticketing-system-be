using HelpDesk.Api.Data;
using HelpDesk.Api.Exceptions;
using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Mappings;
using HelpDesk.Api.Models.DTOs.Users;
using HelpDesk.Api.Models.Entities;
using HelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UserService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<UserResponse>> GetUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<List<UserResponse>> GetAgentsAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == UserRole.AGENT && u.IsActive)
            .OrderBy(u => u.Name)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<UserResponse> GetUserAsync(int id)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return user?.ToResponse() ?? throw new NotFoundException("User not found");
    }

    public async Task<UserResponse> UpdateRoleAsync(int id, UpdateUserRoleRequest request)
    {
        var user = await GetUserForUpdateAsync(id);

        if (user.Id == _currentUser.Id)
        {
            throw new BadRequestException("You cannot change your own role");
        }

        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user.ToResponse();
    }

    public async Task<UserResponse> UpdateStatusAsync(int id, UpdateUserStatusRequest request)
    {
        var user = await GetUserForUpdateAsync(id);

        if (user.Id == _currentUser.Id)
        {
            throw new BadRequestException("You cannot change your own account status");
        }

        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return user.ToResponse();
    }

    private async Task<User> GetUserForUpdateAsync(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        return user ?? throw new NotFoundException("User not found");
    }
}