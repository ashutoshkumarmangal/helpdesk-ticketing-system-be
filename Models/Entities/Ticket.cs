using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Models.Entities;

public class Ticket
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.OPEN;

    public TicketPriority Priority { get; set; } = TicketPriority.MEDIUM;

    public int CategoryId { get; set; }

    public int CreatedById { get; set; }

    public int? AssignedToId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;

    public User CreatedBy { get; set; } = null!;

    public User? AssignedTo { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}