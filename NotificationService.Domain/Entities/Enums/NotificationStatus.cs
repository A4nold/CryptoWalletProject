using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Domain.Entities.Enums
{
    public enum NotificationStatus
    {
        Pending = 1,
        Sending = 2,
        Sent = 3,
        Failed = 4
    }
}
