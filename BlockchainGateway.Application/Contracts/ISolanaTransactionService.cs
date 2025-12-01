using BlockchainGateway.Application.Models;

namespace BlockchainGateway.Application.Contracts;

public interface ISolanaTransactionService
{
    /// <summary>
    /// Submit a withdraw transaction on Solana and record it internally.
    /// This will:
    /// - validate the network
    /// - optionally call the Solana RPC node
    /// - create a BlockchainTransaction row
    /// - return tx hash and persisted data
    /// </summary>
    Task<Result<BlockchainTransactionDto>> WithdrawAsync(SolanaWithdrawRequest request);
}
