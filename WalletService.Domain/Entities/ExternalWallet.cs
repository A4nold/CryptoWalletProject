namespace WalletService.Domain.Entities;

public class ExternalWallet
{
    public Guid Id { get; set; }

    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = default!;

    /// <summary>
    /// Network identifier, e.g. "solana", "evm", "bitcoin".
    /// For Phantom, this will be "solana".
    /// </summary>
    public string Network { get; set; } = default!;

    /// <summary>
    /// Public address / public key in network-specific format
    /// (for Solana, base58-encoded Ed25519 public key).
    /// </summary>
    public string PublicKey { get; set; } = default!;

    /// <summary>
    /// Optional display label (e.g. "Main Phantom wallet").
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Whether this is the primary wallet for this network.
    /// </summary>
    public bool IsPrimary { get; set; }

    public DateTimeOffset LinkedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Last time we successfully verified ownership via signature.
    /// </summary>
    public DateTimeOffset? LastVerifiedAt { get; set; }
}
