using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.Models.DTOs.Tickets;

public class AssignTicketRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Agent is required")]
    public int AgentId { get; set; }
}