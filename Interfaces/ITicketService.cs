using HelpDesk.Api.Helpers;
using HelpDesk.Api.Models.DTOs.Tickets;

namespace HelpDesk.Api.Interfaces;

public interface ITicketService
{
    Task<PagedResult<TicketListItemResponse>> GetTicketsAsync(TicketQuery query);

    Task<TicketDetailResponse> GetTicketAsync(int id);

    Task<TicketDetailResponse> CreateTicketAsync(CreateTicketRequest request);

    Task<TicketDetailResponse> UpdateTicketAsync(int id, UpdateTicketRequest request);

    Task<TicketDetailResponse> AssignTicketAsync(int id, AssignTicketRequest request);

    Task<TicketDetailResponse> UpdateStatusAsync(int id, UpdateStatusRequest request);

    Task<TicketDetailResponse> UpdatePriorityAsync(int id, UpdatePriorityRequest request);
}