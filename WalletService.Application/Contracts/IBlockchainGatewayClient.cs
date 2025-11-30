using WalletService.Application.Models;

namespace WalletService.Application.Contracts;

public interface IBlockchainGatewayClient
{
    /// <summary>
    /// Ask the BlockchainGateway to perform a Solana withdraw from our hot wallet
    /// to the user's external wallet.
    /// </summary>
    Task<Result<BlockchainWithdrawResponseDto>> WithdrawSolanaAsync(
        string networkCode,
        string fromAddress,
        string toAddress,
        decimal amount,
        string? correlationId);
}
