namespace WalletService.Application.Models;

public class BlockchainGatewayOptions
{
    /// <summary>
    /// Base URL of the BlockchainGateway API.
    /// For local dev, something like: https://localhost:7xxx
    /// </summary>
    public string BaseUrl { get; set; } = default!;

    /// <summary>
    /// Network code used for Solana devnet in BlockchainGateway,
    /// e.g. "solana-devnet".
    /// </summary>
    public string SolanaNetworkCode { get; set; } = "solana-devnet";

    /// <summary>
    /// The public address of your Solana hot wallet (the sender).
    /// </summary>
    public string SolanaHotWalletAddress { get; set; } = default!;
}
