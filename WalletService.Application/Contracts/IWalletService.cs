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
}

