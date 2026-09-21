namespace HelpDesk.Api.Models.Entities;

public class Notification
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? TicketId { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}