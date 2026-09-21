using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Models.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "ADMIN")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        var result = await _userService.GetUsersAsync();
        return Ok(result);
    }

    [HttpGet("agents")]
    public async Task<ActionResult<List<UserResponse>>> GetAgents()
    {
        var result = await _userService.GetAgentsAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        var result = await _userService.GetUserAsync(id);
        return Ok(result);
    }

    [HttpPut("{id:int}/role")]
    public async Task<ActionResult<UserResponse>> UpdateRole(int id, UpdateUserRoleRequest request)
    {
        var result = await _userService.UpdateRoleAsync(id, request);
        return Ok(result);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<UserResponse>> UpdateStatus(int id, UpdateUserStatusRequest request)
    {
        var result = await _userService.UpdateStatusAsync(id, request);
        return Ok(result);
    }
}