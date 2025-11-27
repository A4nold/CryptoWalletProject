using NotificationService.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Domain.Entities
{
    public class NotificationTemplate
    {
        public Guid Id { get; set; }

        public NotificationType Type { get; set; }
        public NotificationChannel Channel { get; set; }

        public string? SubjectTemplate { get; set; }   // Mainly for email
        public string BodyTemplate { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
