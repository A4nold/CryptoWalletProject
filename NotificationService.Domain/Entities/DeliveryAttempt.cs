using NotificationService.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Domain.Entities
{
    public class DeliveryAttempt
    {
        public Guid Id { get; set; }

        public Guid NotificationId { get; set; }
        public Notification Notification { get; set; } = default!;

        public int AttemptNumber { get; set; }

        public DateTimeOffset AttemptedAt { get; set; } = DateTimeOffset.UtcNow;

        public NotificationStatus Status { get; set; }

        public string? ProviderResponse { get; set; }
    }
}
