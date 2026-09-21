using System.ComponentModel.DataAnnotations;
using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Models.DTOs.Tickets;

public class UpdateStatusRequest
{
    [Required(ErrorMessage = "Status is required")]
    [EnumDataType(typeof(TicketStatus), ErrorMessage = "Invalid status")]
    public TicketStatus Status { get; set; }
}