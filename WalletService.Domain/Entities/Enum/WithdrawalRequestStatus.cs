namespace WalletService.Domain.Entities.Enum
{
    public enum WithdrawalRequestStatus
    {
        Requested = 1,
        Approved = 2,
        Rejected = 3,
        Broadcasting = 4,
        Completed = 5,
        Failed = 6
    }
}
