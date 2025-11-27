using WalletService.Domain.Entities.Enum;

namespace WalletService.Domain.Entities
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; } = default!;

        public Guid WalletAssetId { get; set; }
        public WalletAsset WalletAsset { get; set; } = default!;

        public WalletTransactionDirection Direction { get; set; }
        public WalletTransactionType Type { get; set; }
        public WalletTransactionStatus Status { get; set; } = WalletTransactionStatus.Pending;

        public decimal Amount { get; set; }

        public decimal FeeAmount { get; set; }
        public string? FeeAssetSymbol { get; set; }  // e.g. "ETH" for gas

        // Link to blockchain gateway side (tx hash or internal id)
        public string? ExternalTransactionId { get; set; }

        public DateTimeOffset RequestedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedAt { get; set; }

        public string? Note { get; set; }
    }
}
