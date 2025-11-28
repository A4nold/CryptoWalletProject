using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<UserNotificationPreference> UserNotificationPreferences => Set<UserNotificationPreference>();
    public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("notification");

        ConfigureNotification(modelBuilder);
        ConfigureNotificationTemplate(modelBuilder);
        ConfigureUserNotificationPreference(modelBuilder);
        ConfigureDeliveryAttempt(modelBuilder);
    }

    private static void ConfigureNotification(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Notification>();

        entity.ToTable("Notifications");

        entity.HasKey(n => n.Id);

        entity.Property(n => n.UserId)
            .IsRequired()
            .HasMaxLength(100);

        entity.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(n => n.Body)
            .IsRequired();

        entity.Property(n => n.PayloadJson)
            .HasColumnType("text");

        entity.Property(n => n.LastError)
            .HasMaxLength(500);
    }

    private static void ConfigureNotificationTemplate(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<NotificationTemplate>();

        entity.ToTable("NotificationTemplates");

        entity.HasKey(t => t.Id);

        entity.Property(t => t.SubjectTemplate)
            .HasMaxLength(200);

        entity.Property(t => t.BodyTemplate)
            .IsRequired();
    }

    private static void ConfigureUserNotificationPreference(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserNotificationPreference>();

        entity.ToTable("UserNotificationPreferences");

        entity.HasKey(p => p.Id);

        entity.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(100);
    }

    private static void ConfigureDeliveryAttempt(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DeliveryAttempt>();

        entity.ToTable("DeliveryAttempts");

        entity.HasKey(a => a.Id);

        entity.Property(a => a.ProviderResponse)
            .HasMaxLength(500);

        entity.HasOne(a => a.Notification)
            .WithMany(n => n.DeliveryAttempts)
            .HasForeignKey(a => a.NotificationId);
    }
}

