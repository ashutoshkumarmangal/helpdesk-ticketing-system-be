using HelpDesk.Api.Models.DTOs.Comments;
using HelpDesk.Api.Models.Enums;
using HelpDesk.Api.Models.DTOs.Categories;
using HelpDesk.Api.Models.DTOs.Users;

namespace HelpDesk.Api.Models.DTOs.Tickets;

public class TicketDetailResponse
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; }

    public TicketPriority Priority { get; set; }

    public CategoryResponse Category { get; set; } = new();

    public UserResponse CreatedBy { get; set; } = new();

    public UserResponse? AssignedTo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<CommentResponse> Comments { get; set; } = new();

    public List<AuditLogResponse> AuditLogs { get; set; } = new();
}