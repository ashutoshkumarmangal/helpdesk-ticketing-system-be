using HelpDesk.Api.Data;
using HelpDesk.Api.Exceptions;
using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Models.DTOs.Notifications;
using HelpDesk.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public NotificationService(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<NotificationResponse>> GetMineAsync()
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == _currentUser.Id)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationResponse
            {
                Id = n.Id,
                TicketId = n.TicketId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == _currentUser.Id && !n.IsRead);
    }

    public async Task MarkReadAsync(int id)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);

        if (notification is null || notification.UserId != _currentUser.Id)
        {
            throw new NotFoundException("Notification not found");
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync();
    }

    public async Task CreateAsync(int userId, int? ticketId, string message)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            TicketId = ticketId,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await Task.CompletedTask;
    }
}