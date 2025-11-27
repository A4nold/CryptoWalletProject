using NotificationService.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Domain.Entities
{
    public class UserNotificationPreference
    {
        public Guid Id { get; set; }

        public string UserId { get; set; } = default!;

        public NotificationChannel Channel { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
