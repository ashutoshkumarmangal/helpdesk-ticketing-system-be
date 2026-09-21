using HelpDesk.Api.Models.DTOs.Notifications;

namespace HelpDesk.Api.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetMineAsync();

    Task<int> GetUnreadCountAsync();

    Task MarkReadAsync(int id);

    Task CreateAsync(int userId, int? ticketId, string message);
}