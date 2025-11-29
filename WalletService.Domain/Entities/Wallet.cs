using WalletService.Domain.Entities.Enum;

namespace WalletService.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; }

        // Coming from IdentityService – store as value only, no FK
        public Guid UserId { get; set; } = default!;

        public string WalletName { get; set; } = default!;
        public bool IsDefault { get; set; }

        public WalletStatus Status { get; set; } = WalletStatus.Active;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        // Navigation
        public ICollection<WalletAsset> Assets { get; set; } = new List<WalletAsset>();
    }
}
