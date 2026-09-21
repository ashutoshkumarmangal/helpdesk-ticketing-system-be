using HelpDesk.Api.Data;
using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Models.DTOs.Dashboard;
using HelpDesk.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync()
    {
        var tickets = _context.Tickets.AsNoTracking();

        var totalTickets = await tickets.CountAsync();

        var statusCounts = await tickets
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var priorityCounts = await tickets
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Priority, x => x.Count);

        var categoryCounts = await _context.Categories
            .AsNoTracking()
            .Select(c => new CategoryCountResponse
            {
                Category = c.Name,
                Count = c.Tickets.Count()
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync();

        return new DashboardSummaryResponse
        {
            TotalTickets = totalTickets,
            OpenTickets = GetCount(statusCounts, TicketStatus.OPEN),
            AssignedTickets = GetCount(statusCounts, TicketStatus.ASSIGNED),
            InProgressTickets = GetCount(statusCounts, TicketStatus.IN_PROGRESS),
            ResolvedTickets = GetCount(statusCounts, TicketStatus.RESOLVED),
            ClosedTickets = GetCount(statusCounts, TicketStatus.CLOSED),
            HighPriorityTickets = GetCount(priorityCounts, TicketPriority.HIGH),
            TicketsByStatus = BuildStatusList(statusCounts),
            TicketsByPriority = BuildPriorityList(priorityCounts),
            TicketsByCategory = categoryCounts
        };
    }

    private static int GetCount<T>(Dictionary<T, int> counts, T key) where T : notnull
    {
        return counts.TryGetValue(key, out var count) ? count : 0;
    }

    private static List<StatusCountResponse> BuildStatusList(Dictionary<TicketStatus, int> counts)
    {
        return Enum.GetValues<TicketStatus>()
            .Select(status => new StatusCountResponse
            {
                Status = status.ToString(),
                Count = GetCount(counts, status)
            })
            .OrderByDescending(s => s.Count)
            .ToList();
    }

    private static List<PriorityCountResponse> BuildPriorityList(Dictionary<TicketPriority, int> counts)
    {
        return Enum.GetValues<TicketPriority>()
            .Select(priority => new PriorityCountResponse
            {
                Priority = priority.ToString(),
                Count = GetCount(counts, priority)
            })
            .OrderByDescending(p => p.Count)
            .ToList();
    }
}