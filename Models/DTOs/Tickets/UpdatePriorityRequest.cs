using System.ComponentModel.DataAnnotations;
using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Models.DTOs.Tickets;

public class UpdatePriorityRequest
{
    [Required(ErrorMessage = "Priority is required")]
    [EnumDataType(typeof(TicketPriority), ErrorMessage = "Invalid priority")]
    public TicketPriority Priority { get; set; }
}