namespace HelpDesk.Api.Models.DTOs.Dashboard;

public class DashboardSummaryResponse
{
    public int TotalTickets { get; set; }

    public int OpenTickets { get; set; }

    public int AssignedTickets { get; set; }

    public int InProgressTickets { get; set; }

    public int ResolvedTickets { get; set; }

    public int ClosedTickets { get; set; }

    public int HighPriorityTickets { get; set; }

    public List<StatusCountResponse> TicketsByStatus { get; set; } = new();

    public List<PriorityCountResponse> TicketsByPriority { get; set; } = new();

    public List<CategoryCountResponse> TicketsByCategory { get; set; } = new();
}