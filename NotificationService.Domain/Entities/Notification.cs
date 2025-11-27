using NotificationService.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }

        // UserId coming from IdentityService by value only
        public string UserId { get; set; } = default!;

        public NotificationType Type { get; set; }
        public NotificationChannel Channel { get; set; }

        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;

        // Extra data as JSON for clients
        public string? PayloadJson { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? SentAt { get; set; }

        public string? LastError { get; set; }

        // Navigation
        public ICollection<DeliveryAttempt> DeliveryAttempts { get; set; } = new List<DeliveryAttempt>();
    }
}
