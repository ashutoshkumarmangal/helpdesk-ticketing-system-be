using HelpDesk.Api.Data;
using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Models.DTOs.Tickets;
using HelpDesk.Api.Models.Entities;
using HelpDesk.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;

    public AuditLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuditLogResponse>> GetAsync(int? ticketId)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Include(a => a.User)
            .AsQueryable();

        if (ticketId.HasValue)
        {
            query = query.Where(a => a.TicketId == ticketId.Value);
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AuditLogResponse
            {
                Id = a.Id,
                User = a.User == null ? null : new Models.DTOs.Users.UserResponse
                {
                    Id = a.User.Id,
                    Name = a.User.Name,
                    Email = a.User.Email,
                    Role = a.User.Role,
                    IsActive = a.User.IsActive,
                    CreatedAt = a.User.CreatedAt,
                    UpdatedAt = a.User.UpdatedAt
                },
                Action = a.Action,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task LogAsync(int userId, int? ticketId, string action, string? oldValue, string? newValue)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            TicketId = ticketId,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedAt = DateTime.UtcNow
        });

        // The calling service is responsible for SaveChangesAsync so related
        // changes (ticket update + audit log) are persisted atomically.
        await Task.CompletedTask;
    }
}