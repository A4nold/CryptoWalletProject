namespace WalletService.Domain.Entities
{
    public class DepositAddress
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; } = default!;

        public Guid WalletAssetId { get; set; }
        public WalletAsset WalletAsset { get; set; } = default!;

        public string Address { get; set; } = default!;
        public string Network { get; set; } = default!;   // e.g. "bitcoin", "ethereum"

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
