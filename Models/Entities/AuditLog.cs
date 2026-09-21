namespace HelpDesk.Api.Models.Entities;

public class AuditLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? TicketId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public Ticket? Ticket { get; set; }
}