using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Models.DTOs.Tickets;

public class TicketQuery
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public TicketStatus? Status { get; set; }

    public TicketPriority? Priority { get; set; }

    public int? CategoryId { get; set; }

    public int? AssignedToId { get; set; }
}