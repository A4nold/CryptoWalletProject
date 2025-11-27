using WalletService.Domain.Entities.Enum;

namespace WalletService.Domain.Entities
{
    public class WithdrawalRequest
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; } = default!;

        public Guid WalletAssetId { get; set; }
        public WalletAsset WalletAsset { get; set; } = default!;

        public string ToAddress { get; set; } = default!;
        public string Network { get; set; } = default!;

        public decimal Amount { get; set; }

        public WithdrawalRequestStatus Status { get; set; } = WithdrawalRequestStatus.Requested;

        public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }

        // After broadcast via BlockchainGateway
        public string? BlockchainTransactionId { get; set; }
    }
}
