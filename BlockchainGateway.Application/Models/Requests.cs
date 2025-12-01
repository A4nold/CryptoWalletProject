namespace BlockchainGateway.Application.Models;

/// <summary>
/// Request from another service (e.g. WalletService) to send funds on Solana.
/// </summary>
public record SolanaWithdrawRequest(
    string NetworkCode,   // e.g. "solana-devnet"
    string FromAddress,   // your hot wallet address
    string ToAddress,     // user's Phantom address
    decimal Amount,       // in SOL or lamports-equivalent, we'll discuss later
    string? CorrelationId // e.g. wallet transaction id
);
