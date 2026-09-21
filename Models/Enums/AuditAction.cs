namespace HelpDesk.Api.Models.Enums;

public static class AuditAction
{
    public const string TicketCreated = "Ticket Created";
    public const string TicketUpdated = "Ticket Updated";
    public const string TicketAssigned = "Ticket Assigned";
    public const string StatusChanged = "Status Changed";
    public const string PriorityChanged = "Priority Changed";
    public const string TicketResolved = "Ticket Resolved";
    public const string TicketClosed = "Ticket Closed";
}