using HelpDesk.Api.Data;
using HelpDesk.Api.Exceptions;
using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Models.DTOs.Comments;
using HelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notificationService;

    public CommentService(
        AppDbContext context,
        ICurrentUserService currentUser,
        INotificationService notificationService)
    {
        _context = context;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<List<CommentResponse>> GetCommentsAsync(int ticketId)
    {
        var ticket = await LoadAuthorizedTicketAsync(ticketId);

        return await _context.Comments
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponse
            {
                Id = c.Id,
                UserId = c.UserId,
                UserName = c.User.Name,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CommentResponse> AddCommentAsync(int ticketId, CreateCommentRequest request)
    {
        var ticket = await LoadAuthorizedTicketAsync(ticketId);

        var comment = new Models.Entities.Comment
        {
            TicketId = ticketId,
            UserId = _currentUser.Id,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);

        var notifiedUserId = _currentUser.Id == ticket.CreatedById
            ? ticket.AssignedToId
            : ticket.CreatedById;

        if (notifiedUserId.HasValue)
        {
            await _notificationService.CreateAsync(
                notifiedUserId.Value,
                ticketId,
                $"New comment on ticket #{ticketId} by {_currentUser.Name}");
        }

        await _context.SaveChangesAsync();

        return new CommentResponse
        {
            Id = comment.Id,
            UserId = comment.UserId,
            UserName = _currentUser.Name,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }

    private async Task<Models.Entities.Ticket> LoadAuthorizedTicketAsync(int ticketId)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket is null)
        {
            throw new NotFoundException("Ticket not found");
        }

        switch (_currentUser.Role)
        {
            case UserRole.ADMIN:
                return ticket;
            case UserRole.AGENT when ticket.AssignedToId != _currentUser.Id:
                throw new ForbiddenException("You can only access tickets assigned to you");
            case UserRole.CUSTOMER when ticket.CreatedById != _currentUser.Id:
                throw new ForbiddenException("You can only access your own tickets");
        }

        return ticket;
    }
}