using System.ComponentModel.DataAnnotations;
using HelpDesk.Api.Models.Enums;

namespace HelpDesk.Api.Models.DTOs.Tickets;

public class CreateTicketRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Description must be at least 10 characters")]
    public string Description { get; set; } = string.Empty;

    [EnumDataType(typeof(TicketPriority), ErrorMessage = "Invalid priority")]
    public TicketPriority Priority { get; set; } = TicketPriority.MEDIUM;

    [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }
}