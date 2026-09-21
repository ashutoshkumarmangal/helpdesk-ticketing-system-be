namespace HelpDesk.Api.Models.Entities;

public class Comment
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Ticket Ticket { get; set; } = null!;

    public User User { get; set; } = null!;
}