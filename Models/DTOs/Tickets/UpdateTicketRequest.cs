using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Api.Models.DTOs.Tickets;

public class UpdateTicketRequest
{
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
    public string? Title { get; set; }

    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Description must be at least 10 characters")]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
    public int? CategoryId { get; set; }
}