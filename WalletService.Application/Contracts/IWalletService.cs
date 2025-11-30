using WalletService.Application.Models;

namespace WalletService.Application.Contracts;

public interface IWalletService
{
    Task<Result<WalletDto>> GetOrCreateDefaultWalletAsync(Guid userId);

    Task<Result<WalletDto>> DepositAsync(Guid userId, DepositRequest request);

    Task<Result<WalletDto>> WithdrawAsync(Guid userId, WithdrawRequest request);

    Task<Result<IReadOnlyList<TransactionDto>>> GetTransactionsAsync(
        Guid userId,
        TransactionsQuery query);

    // NEW
    Task<Result<WalletDto>> LinkSolanaWalletAsync(Guid userId, LinkSolanaWalletRequest request);

    // NEW: list all external wallets linked to this user's default wallet
    Task<Result<IReadOnlyList<ExternalWalletDto>>> GetExternalWalletsAsync(Guid userId);

    // NEW: set primary external wallet for its network
    Task<Result<ExternalWalletDto>> SetPrimaryExternalWalletAsync(Guid userId, Guid externalWalletId);
}

