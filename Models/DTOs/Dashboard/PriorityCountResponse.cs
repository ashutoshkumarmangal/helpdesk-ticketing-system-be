namespace HelpDesk.Api.Models.DTOs.Dashboard;

public class PriorityCountResponse
{
    public string Priority { get; set; } = string.Empty;

    public int Count { get; set; }
}