using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Models.DTOs.Tickets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "ADMIN")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLogResponse>>> GetAuditLogs([FromQuery] int? ticketId)
    {
        var result = await _auditLogService.GetAsync(ticketId);
        return Ok(result);
    }
}