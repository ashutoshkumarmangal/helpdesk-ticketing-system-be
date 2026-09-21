using HelpDesk.Api.Models.DTOs.Dashboard;

namespace HelpDesk.Api.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync();
}