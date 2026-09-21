using HelpDesk.Api.Models.DTOs.Users;

namespace HelpDesk.Api.Models.DTOs.Tickets;

public class AuditLogResponse
{
    public int Id { get; set; }

    public UserResponse? User { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public DateTime CreatedAt { get; set; }
}