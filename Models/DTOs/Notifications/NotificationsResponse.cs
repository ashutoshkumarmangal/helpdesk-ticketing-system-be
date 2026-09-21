namespace HelpDesk.Api.Models.DTOs.Notifications;

public class NotificationsResponse
{
    public List<NotificationResponse> Items { get; set; } = new();

    public int UnreadCount { get; set; }
}