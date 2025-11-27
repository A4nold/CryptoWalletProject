using System;
using System.Collections.Generic;
using System.Text;

namespace NotificationService.Domain.Entities.Enums
{
    public enum NotificationType
    {
        TransactionCreated = 1,
        TransactionConfirmed = 2,
        WithdrawalRequested = 3,
        WithdrawalCompleted = 4,
        LoginAlert = 5,
        PasswordReset = 6,
        KycApproved = 7,
        KycRejected = 8
    }
}
