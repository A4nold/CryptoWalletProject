namespace WalletService.Domain.Entities
{
    public class WalletAsset
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; } = default!;

        public string Symbol { get; set; } = default!;   // e.g. "SOL"
        public string Network { get; set; } = default!;  // e.g. "solana-devnet", "solana-mainet"

        // Monetary values should generally be decimal
        public decimal AvailableBalance { get; set; }
        public decimal PendingBalance { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
