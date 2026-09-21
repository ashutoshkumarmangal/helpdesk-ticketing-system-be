using HelpDesk.Api.Helpers;
using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Models.DTOs.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TicketListItemResponse>>> GetTickets([FromQuery] TicketQuery query)
    {
        var result = await _ticketService.GetTicketsAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDetailResponse>> GetTicket(int id)
    {
        var result = await _ticketService.GetTicketAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TicketDetailResponse>> CreateTicket(CreateTicketRequest request)
    {
        var result = await _ticketService.CreateTicketAsync(request);
        return CreatedAtAction(nameof(GetTicket), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TicketDetailResponse>> UpdateTicket(int id, UpdateTicketRequest request)
    {
        var result = await _ticketService.UpdateTicketAsync(id, request);
        return Ok(result);
    }

    [HttpPut("{id:int}/assign")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<TicketDetailResponse>> AssignTicket(int id, AssignTicketRequest request)
    {
        var result = await _ticketService.AssignTicketAsync(id, request);
        return Ok(result);
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<TicketDetailResponse>> UpdateStatus(int id, UpdateStatusRequest request)
    {
        var result = await _ticketService.UpdateStatusAsync(id, request);
        return Ok(result);
    }

    [HttpPut("{id:int}/priority")]
    public async Task<ActionResult<TicketDetailResponse>> UpdatePriority(int id, UpdatePriorityRequest request)
    {
        var result = await _ticketService.UpdatePriorityAsync(id, request);
        return Ok(result);
    }
}