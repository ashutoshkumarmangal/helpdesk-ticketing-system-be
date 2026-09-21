using HelpDesk.Api.Models.DTOs.Tickets;

namespace HelpDesk.Api.Interfaces;

public interface IAuditLogService
{
    Task<List<AuditLogResponse>> GetAsync(int? ticketId);

    Task LogAsync(int userId, int? ticketId, string action, string? oldValue, string? newValue);
}