using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Models.Entities;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.CUSTOMER;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}