using HelpDesk.Api.Data;
using HelpDesk.Api.Exceptions;
using HelpDesk.Api.Helpers;
using HelpDesk.Api.Interfaces;
using HelpDesk.Api.Models.DTOs.Categories;
using HelpDesk.Api.Models.DTOs.Comments;
using HelpDesk.Api.Models.DTOs.Tickets;
using HelpDesk.Api.Models.DTOs.Users;
using HelpDesk.Api.Models.Enums;
using HelpDesk.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Services;

public class TicketService : ITicketService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<TicketService> _logger;

    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
    {
        [TicketStatus.OPEN] = new[] { TicketStatus.ASSIGNED, TicketStatus.IN_PROGRESS, TicketStatus.RESOLVED, TicketStatus.CLOSED },
        [TicketStatus.ASSIGNED] = new[] { TicketStatus.IN_PROGRESS, TicketStatus.RESOLVED, TicketStatus.CLOSED },
        [TicketStatus.IN_PROGRESS] = new[] { TicketStatus.RESOLVED, TicketStatus.CLOSED },
        [TicketStatus.RESOLVED] = new[] { TicketStatus.CLOSED },
        [TicketStatus.CLOSED] = Array.Empty<TicketStatus>()
    };

    public TicketService(
        AppDbContext context,
        ICurrentUserService currentUser,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        ILogger<TicketService> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<PagedResult<TicketListItemResponse>> GetTicketsAsync(TicketQuery query)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var baseQuery = _context.Tickets
            .AsNoTracking()
            .AsQueryable();

        // Role-based scoping: customers only see their own tickets,
        // agents only see tickets assigned to them, admins see everything.
        baseQuery = _currentUser.Role switch
        {
            UserRole.CUSTOMER => baseQuery.Where(t => t.CreatedById == _currentUser.Id),
            UserRole.AGENT => baseQuery.Where(t => t.AssignedToId == _currentUser.Id),
            _ => baseQuery
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            var searchId = int.TryParse(search, out var idValue) ? (int?)idValue : null;
            baseQuery = baseQuery.Where(t =>
                (searchId.HasValue && t.Id == searchId.Value) ||
                t.Title.Contains(search) ||
                t.Description.Contains(search) ||
                t.Category.Name.Contains(search) ||
                t.CreatedBy.Name.Contains(search) ||
                (t.AssignedTo != null && t.AssignedTo.Name.Contains(search)));
        }

        if (query.Status.HasValue)
        {
            baseQuery = baseQuery.Where(t => t.Status == query.Status.Value);
        }

        if (query.Priority.HasValue)
        {
            baseQuery = baseQuery.Where(t => t.Priority == query.Priority.Value);
        }

        if (query.CategoryId.HasValue)
        {
            baseQuery = baseQuery.Where(t => t.CategoryId == query.CategoryId.Value);
        }

        if (query.AssignedToId.HasValue)
        {
            baseQuery = baseQuery.Where(t => t.AssignedToId == query.AssignedToId.Value);
        }

        var projected = baseQuery
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketListItemResponse
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                CategoryName = t.Category.Name,
                CreatedByName = t.CreatedBy.Name,
                AssignedToName = t.AssignedTo != null ? t.AssignedTo.Name : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            });

        return await PagedResult<TicketListItemResponse>.CreateAsync(projected, page, pageSize);
    }

    public async Task<TicketDetailResponse> GetTicketAsync(int id)
    {
        var ticket = await LoadTicketAsync(id, tracked: false);
        EnsureCanAccess(ticket);

        return ToDetailResponse(ticket);
    }

    public async Task<TicketDetailResponse> CreateTicketAsync(CreateTicketRequest request)
    {
        await EnsureCategoryExistsAsync(request.CategoryId);

        var ticket = new Models.Entities.Ticket
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Status = TicketStatus.OPEN,
            CategoryId = request.CategoryId,
            CreatedById = _currentUser.Id
        };

        _context.Tickets.Add(ticket);
        await _auditLogService.LogAsync(_currentUser.Id, null, AuditAction.TicketCreated, null, request.Title.Trim());
        await _context.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketId} created by {UserId}", ticket.Id, _currentUser.Id);

        return await GetTicketAsync(ticket.Id);
    }

    public async Task<TicketDetailResponse> UpdateTicketAsync(int id, UpdateTicketRequest request)
    {
        var ticket = await LoadTicketAsync(id, tracked: true);
        EnsureCanModify(ticket);

        if (request.CategoryId.HasValue)
        {
            await EnsureCategoryExistsAsync(request.CategoryId.Value);
        }

        var oldTitle = ticket.Title;

        if (!string.IsNullOrWhiteSpace(request.Title))
        {
            ticket.Title = request.Title.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            ticket.Description = request.Description.Trim();
        }

        if (request.CategoryId.HasValue)
        {
            ticket.CategoryId = request.CategoryId.Value;
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await _auditLogService.LogAsync(_currentUser.Id, ticket.Id, AuditAction.TicketUpdated, oldTitle, ticket.Title);
        await _context.SaveChangesAsync();

        return await GetTicketAsync(ticket.Id);
    }

    public async Task<TicketDetailResponse> AssignTicketAsync(int id, AssignTicketRequest request)
    {
        if (_currentUser.Role != UserRole.ADMIN)
        {
            throw new ForbiddenException("Only administrators can assign tickets");
        }

        var ticket = await LoadTicketAsync(id, tracked: true);

        var agent = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.AgentId && u.Role == UserRole.AGENT && u.IsActive);

        if (agent is null)
        {
            throw new BadRequestException("Agent not found or is not an active agent");
        }

        var oldAssignee = ticket.AssignedTo?.Name ?? "Unassigned";

        ticket.AssignedToId = agent.Id;

        if (ticket.Status == TicketStatus.OPEN)
        {
            ticket.Status = TicketStatus.ASSIGNED;
        }

        ticket.UpdatedAt = DateTime.UtcNow;

        await _auditLogService.LogAsync(_currentUser.Id, ticket.Id, AuditAction.TicketAssigned, oldAssignee, agent.Name);
        await _notificationService.CreateAsync(agent.Id, ticket.Id, $"Ticket #{ticket.Id} has been assigned to you");

        await _context.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketId} assigned to agent {AgentId} by {UserId}", ticket.Id, agent.Id, _currentUser.Id);

        return await GetTicketAsync(ticket.Id);
    }

    public async Task<TicketDetailResponse> UpdateStatusAsync(int id, UpdateStatusRequest request)
    {
        var ticket = await LoadTicketAsync(id, tracked: true);
        EnsureCanModify(ticket);

        if (request.Status == ticket.Status)
        {
            return await GetTicketAsync(ticket.Id);
        }

        if (!IsTransitionAllowed(ticket.Status, request.Status, _currentUser.Role))
        {
            throw new BadRequestException($"Invalid status transition from {ticket.Status} to {request.Status}");
        }

        var oldStatus = ticket.Status.ToString();
        ticket.Status = request.Status;
        ticket.UpdatedAt = DateTime.UtcNow;

        var action = request.Status switch
        {
            TicketStatus.RESOLVED => AuditAction.TicketResolved,
            TicketStatus.CLOSED => AuditAction.TicketClosed,
            _ => AuditAction.StatusChanged
        };

        await _auditLogService.LogAsync(_currentUser.Id, ticket.Id, action, oldStatus, request.Status.ToString());

        if (request.Status == TicketStatus.RESOLVED)
        {
            await _notificationService.CreateAsync(
                ticket.CreatedById,
                ticket.Id,
                $"Your ticket #{ticket.Id} has been resolved");
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketId} status changed from {Old} to {New} by {UserId}",
            ticket.Id, oldStatus, request.Status, _currentUser.Id);

        return await GetTicketAsync(ticket.Id);
    }

    public async Task<TicketDetailResponse> UpdatePriorityAsync(int id, UpdatePriorityRequest request)
    {
        var ticket = await LoadTicketAsync(id, tracked: true);

        if (_currentUser.Role == UserRole.CUSTOMER)
        {
            throw new ForbiddenException("Customers cannot change ticket priority");
        }

        EnsureCanModify(ticket);

        if (request.Priority == ticket.Priority)
        {
            return await GetTicketAsync(ticket.Id);
        }

        var oldPriority = ticket.Priority.ToString();
        ticket.Priority = request.Priority;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _auditLogService.LogAsync(_currentUser.Id, ticket.Id, AuditAction.PriorityChanged, oldPriority, request.Priority.ToString());
        await _context.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketId} priority changed from {Old} to {New} by {UserId}",
            ticket.Id, oldPriority, request.Priority, _currentUser.Id);

        return await GetTicketAsync(ticket.Id);
    }

    private async Task<Models.Entities.Ticket> LoadTicketAsync(int id, bool tracked)
    {
        IQueryable<Models.Entities.Ticket> query = _context.Tickets
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Include(t => t.Category);

        query = query.AsSplitQuery();

        if (tracked)
        {
            query = query.Include(t => t.Comments).Include(t => t.AuditLogs);
        }
        else
        {
            query = query
                .Include(t => t.Comments).ThenInclude(c => c.User)
                .Include(t => t.AuditLogs).ThenInclude(a => a.User)
                .AsNoTracking();
        }

        var ticket = await query.FirstOrDefaultAsync(t => t.Id == id);

        return ticket ?? throw new NotFoundException("Ticket not found");
    }

    private void EnsureCanAccess(Models.Entities.Ticket ticket)
    {
        switch (_currentUser.Role)
        {
            case UserRole.ADMIN:
                return;
            case UserRole.AGENT when ticket.AssignedToId != _currentUser.Id:
                throw new ForbiddenException("You can only access tickets assigned to you");
            case UserRole.CUSTOMER when ticket.CreatedById != _currentUser.Id:
                throw new ForbiddenException("You can only access your own tickets");
        }
    }

    private void EnsureCanModify(Models.Entities.Ticket ticket)
    {
        switch (_currentUser.Role)
        {
            case UserRole.ADMIN:
                return;
            case UserRole.AGENT when ticket.AssignedToId != _currentUser.Id:
                throw new ForbiddenException("You can only modify tickets assigned to you");
            case UserRole.CUSTOMER when ticket.CreatedById != _currentUser.Id:
                throw new ForbiddenException("You can only modify your own tickets");
        }
    }

    private static bool IsTransitionAllowed(TicketStatus from, TicketStatus to, UserRole role)
    {
        if (role == UserRole.CUSTOMER)
        {
            return to == TicketStatus.CLOSED;
        }

        return from == to || AllowedTransitions[from].Contains(to);
    }

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        var exists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
        if (!exists)
        {
            throw new BadRequestException("Category does not exist");
        }
    }

    private static TicketDetailResponse ToDetailResponse(Models.Entities.Ticket ticket)
    {
        return new TicketDetailResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            Status = ticket.Status,
            Priority = ticket.Priority,
            Category = new CategoryResponse
            {
                Id = ticket.Category.Id,
                Name = ticket.Category.Name,
                Description = ticket.Category.Description
            },
            CreatedBy = ticket.CreatedBy.ToResponse(),
            AssignedTo = ticket.AssignedTo?.ToResponse(),
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            Comments = ticket.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponse
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User.Name,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                })
                .ToList(),
            AuditLogs = ticket.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new AuditLogResponse
                {
                    Id = a.Id,
                    User = a.User?.ToResponse(),
                    Action = a.Action,
                    OldValue = a.OldValue,
                    NewValue = a.NewValue,
                    CreatedAt = a.CreatedAt
                })
                .ToList()
        };
    }
}