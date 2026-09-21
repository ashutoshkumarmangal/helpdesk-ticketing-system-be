using HelpDesk.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Data;

public static class DbSeeder
{
    public static readonly MySqlServerVersion ServerVersion = new(new Version(8, 0, 32));

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Categories.AnyAsync() && await context.Users.AnyAsync())
        {
            return;
        }

        await SeedCategoriesAsync(context);

        var admin = await CreateUserAsync(context, "Admin", "admin@helpdesk.com", "Admin@123", Models.Enums.UserRole.ADMIN);
        var agent = await CreateUserAsync(context, "Emma Agent", "agent@helpdesk.com", "Agent@123", Models.Enums.UserRole.AGENT);
        var agent2 = await CreateUserAsync(context, "Liam Agent", "liam.agent@helpdesk.com", "Agent@123", Models.Enums.UserRole.AGENT);
        var customer = await CreateUserAsync(context, "John Customer", "customer@helpdesk.com", "Customer@123", Models.Enums.UserRole.CUSTOMER);
        var customer2 = await CreateUserAsync(context, "Sara Customer", "sara@helpdesk.com", "Customer@123", Models.Enums.UserRole.CUSTOMER);

        var technical = await context.Categories.FirstAsync(c => c.Name == "Technical");
        var billing = await context.Categories.FirstAsync(c => c.Name == "Billing");
        var account = await context.Categories.FirstAsync(c => c.Name == "Account");
        var payment = await context.Categories.FirstAsync(c => c.Name == "Payment");

        var now = DateTime.UtcNow;
        var tickets = new List<Ticket>
        {
            CreateTicket("Payment failed during checkout", "Payment is failing during checkout with error code E-412.", Models.Enums.TicketStatus.OPEN, Models.Enums.TicketPriority.HIGH, payment, customer, null, now.AddDays(-4)),
            CreateTicket("Cannot access dashboard", "User gets a blank page after login.", Models.Enums.TicketStatus.ASSIGNED, Models.Enums.TicketPriority.MEDIUM, technical, customer, agent, now.AddDays(-3)),
            CreateTicket("Invoice amount seems wrong", "Invoice #2210 shows a higher amount than expected.", Models.Enums.TicketStatus.IN_PROGRESS, Models.Enums.TicketPriority.MEDIUM, billing, customer2, agent, now.AddDays(-2)),
            CreateTicket("Unable to update profile picture", "Uploading a new avatar times out.", Models.Enums.TicketStatus.RESOLVED, Models.Enums.TicketPriority.LOW, technical, customer, agent2, now.AddDays(-6)),
            CreateTicket("Reimbursement query", "Need help understanding the reimbursement process.", Models.Enums.TicketStatus.CLOSED, Models.Enums.TicketPriority.LOW, account, customer2, agent, now.AddDays(-9)),
            CreateTicket("Login not working on mobile app", "Receiving 500 error on login screen.", Models.Enums.TicketStatus.OPEN, Models.Enums.TicketPriority.CRITICAL, technical, customer, null, now.AddHours(-6)),
            CreateTicket("Double charge on my card", "Card was charged twice for the same subscription.", Models.Enums.TicketStatus.ASSIGNED, Models.Enums.TicketPriority.CRITICAL, payment, customer, agent, now.AddHours(-4)),
            CreateTicket("Change plan request", "Would like to downgrade from Premium to Standard.", Models.Enums.TicketStatus.IN_PROGRESS, Models.Enums.TicketPriority.MEDIUM, billing, customer, agent2, now.AddHours(-2)),
            CreateTicket("Email notifications not arriving", "Unsubscribe from daily digest emails.", Models.Enums.TicketStatus.RESOLVED, Models.Enums.TicketPriority.LOW, account, customer2, agent, now.AddDays(-1)),
            CreateTicket("Two factor authentication issue", "OTP is not being received.", Models.Enums.TicketStatus.CLOSED, Models.Enums.TicketPriority.HIGH, account, customer, agent2, now.AddDays(-7)),
            CreateTicket("API rate limit exceeded", "Our integration is hitting rate limits unexpectedly.", Models.Enums.TicketStatus.OPEN, Models.Enums.TicketPriority.HIGH, technical, customer, null, now.AddHours(-1)),
            CreateTicket("Refund status", "Refund submitted over a week ago is still pending.", Models.Enums.TicketStatus.IN_PROGRESS, Models.Enums.TicketPriority.HIGH, payment, customer2, agent, now.AddDays(-2).AddHours(-3))
        };

        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();

        var comments = new List<Comment>
        {
            CreateComment(tickets[1].Id, agent.Id, "I will investigate the dashboard blank page issue today.", now.AddHours(-20)),
            CreateComment(tickets[1].Id, customer.Id, "Thank you. Let me know if you need anything from my side.", now.AddHours(-19)),
            CreateComment(tickets[2].Id, agent.Id, "I found a discrepancy in the line items. Checking with accounting.", now.AddHours(-8)),
            CreateComment(tickets[6].Id, agent.Id, "We found the duplicate charge. A refund has been initiated.", now.AddHours(-2))
        };

        context.Comments.AddRange(comments);

        context.Notifications.AddRange(new List<Notification>
        {
            CreateNotification(agent.Id, tickets[1].Id, $"Ticket #{tickets[1].Id} has been assigned to you", false, now.AddHours(-21)),
            CreateNotification(customer.Id, tickets[3].Id, $"Your ticket #{tickets[3].Id} has been resolved", false, now.AddDays(-1).AddHours(-2)),
            CreateNotification(agent.Id, tickets[6].Id, $"New comment on ticket #{tickets[6].Id} by {customer.Name}", true, now.AddHours(-2)),
            CreateNotification(customer2.Id, tickets[8].Id, $"Your ticket #{tickets[8].Id} has been resolved", true, now.AddHours(-1))
        });

        context.AuditLogs.AddRange(new List<AuditLog>
        {
            CreateAuditLog(admin.Id, tickets[1].Id, Models.Enums.AuditAction.TicketAssigned, "Unassigned", agent.Name, now.AddHours(-21)),
            CreateAuditLog(agent.Id, tickets[2].Id, Models.Enums.AuditAction.StatusChanged, Models.Enums.TicketStatus.ASSIGNED.ToString(), Models.Enums.TicketStatus.IN_PROGRESS.ToString(), now.AddHours(-9)),
            CreateAuditLog(admin.Id, tickets[6].Id, Models.Enums.AuditAction.TicketAssigned, "Unassigned", agent.Name, now.AddHours(-3))
        });

        await context.SaveChangesAsync();
    }

    private static async Task SeedCategoriesAsync(AppDbContext context)
    {
        var categories = new List<Category>
        {
            new() { Name = "Technical", Description = "Technical issues with products or services" },
            new() { Name = "Billing", Description = "Billing inquiries and invoice questions" },
            new() { Name = "Account", Description = "Account management and profile help" },
            new() { Name = "Payment", Description = "Payments, refunds and transaction issues" },
            new() { Name = "Other", Description = "Anything that does not fit another category" }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    private static async Task<User> CreateUserAsync(
        AppDbContext context,
        string name,
        string email,
        string password,
        Models.Enums.UserRole role)
    {
        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static Ticket CreateTicket(
        string title,
        string description,
        Models.Enums.TicketStatus status,
        Models.Enums.TicketPriority priority,
        Category category,
        User createdBy,
        User? assignedTo,
        DateTime createdAt)
    {
        return new Ticket
        {
            Title = title,
            Description = description,
            Status = status,
            Priority = priority,
            CategoryId = category.Id,
            CreatedById = createdBy.Id,
            AssignedToId = assignedTo?.Id,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    private static Comment CreateComment(int ticketId, int userId, string content, DateTime createdAt)
    {
        return new Comment
        {
            TicketId = ticketId,
            UserId = userId,
            Content = content,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    private static Notification CreateNotification(int userId, int ticketId, string message, bool isRead, DateTime createdAt)
    {
        return new Notification
        {
            UserId = userId,
            TicketId = ticketId,
            Message = message,
            IsRead = isRead,
            CreatedAt = createdAt
        };
    }

    private static AuditLog CreateAuditLog(int userId, int ticketId, string action, string? oldValue, string? newValue, DateTime createdAt)
    {
        return new AuditLog
        {
            UserId = userId,
            TicketId = ticketId,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedAt = createdAt
        };
    }
}