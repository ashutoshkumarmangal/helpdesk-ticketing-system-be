using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Models.DTOs.Tickets;

public class TicketListItemResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public TicketStatus Status { get; set; }

    public TicketPriority Priority { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string CreatedByName { get; set; } = string.Empty;

    public string? AssignedToName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}