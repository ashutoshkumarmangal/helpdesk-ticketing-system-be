using HelpDesk.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureCategory(modelBuilder);
        ConfigureTicket(modelBuilder);
        ConfigureComment(modelBuilder);
        ConfigureNotification(modelBuilder);
        ConfigureAuditLog(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();

        user.Property(u => u.Name).HasMaxLength(100).IsRequired();
        user.Property(u => u.Email).HasMaxLength(255).IsRequired();
        user.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();

        user.HasIndex(u => u.Email).IsUnique();
        user.HasIndex(u => u.Role);

        // Store enums as readable strings in the database.
        user.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
    }

    private static void ConfigureCategory(ModelBuilder modelBuilder)
    {
        var category = modelBuilder.Entity<Category>();

        category.Property(c => c.Name).HasMaxLength(100).IsRequired();
        category.Property(c => c.Description).HasMaxLength(500);

        category.HasIndex(c => c.Name).IsUnique();
    }

    private static void ConfigureTicket(ModelBuilder modelBuilder)
    {
        var ticket = modelBuilder.Entity<Ticket>();

        ticket.Property(t => t.Title).HasMaxLength(200).IsRequired();
        ticket.Property(t => t.Description).HasMaxLength(4000).IsRequired();

        ticket.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        ticket.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);

        ticket.HasIndex(t => t.Status);
        ticket.HasIndex(t => t.Priority);
        ticket.HasIndex(t => t.CreatedById);
        ticket.HasIndex(t => t.AssignedToId);
        ticket.HasIndex(t => t.CreatedAt);

        ticket.HasOne(t => t.Category)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        ticket.HasOne(t => t.CreatedBy)
            .WithMany(u => u.CreatedTickets)
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        ticket.HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTickets)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureComment(ModelBuilder modelBuilder)
    {
        var comment = modelBuilder.Entity<Comment>();

        comment.Property(c => c.Content).HasMaxLength(2000).IsRequired();

        comment.HasIndex(c => new { c.TicketId, c.CreatedAt });

        comment.HasOne(c => c.Ticket)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        comment.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureNotification(ModelBuilder modelBuilder)
    {
        var notification = modelBuilder.Entity<Notification>();

        notification.Property(n => n.Message).HasMaxLength(500).IsRequired();

        notification.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

        notification.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        var auditLog = modelBuilder.Entity<AuditLog>();

        auditLog.Property(a => a.Action).HasMaxLength(100).IsRequired();
        auditLog.Property(a => a.OldValue).HasMaxLength(500);
        auditLog.Property(a => a.NewValue).HasMaxLength(500);

        auditLog.HasIndex(a => new { a.TicketId, a.CreatedAt });
        auditLog.HasIndex(a => a.UserId);

        auditLog.HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        auditLog.HasOne(a => a.Ticket)
            .WithMany(t => t.AuditLogs)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}