namespace BlockchainGateway.Application.Models;

public class SolanaHotWalletOptions
{
    public string PublicKey { get; set; } = default!;
    public string PrivateKey { get; set; } = default!; // e.g. base64/base58; handle carefully
}
