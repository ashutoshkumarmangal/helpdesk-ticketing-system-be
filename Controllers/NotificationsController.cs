using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Models.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<NotificationsResponse>> GetNotifications()
    {
        var items = await _notificationService.GetMineAsync();
        var unreadCount = await _notificationService.GetUnreadCountAsync();

        return Ok(new NotificationsResponse
        {
            Items = items,
            UnreadCount = unreadCount
        });
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var unreadCount = await _notificationService.GetUnreadCountAsync();
        return Ok(unreadCount);
    }

    [HttpPut("{id:int}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkReadAsync(id);
        return NoContent();
    }
}