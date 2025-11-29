using Microsoft.EntityFrameworkCore;
using WalletService.Application.Contracts;
using WalletService.Application.Models;
using WalletService.Domain.Entities;
using WalletService.Domain.Entities.Enum;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Services;

public class WalletService : IWalletService
{
    private readonly WalletDbContext _db;

    public WalletService(WalletDbContext db)
    {
        _db = db;
    }

    public async Task<Result<WalletDto>> GetOrCreateDefaultWalletAsync(Guid userId)
    {
        // Try to find a default wallet for this user
        var wallet = await _db.Wallets
            .Include(w => w.Assets)
            .SingleOrDefaultAsync(w => w.UserId == userId && w.IsDefault);

        // If none exists, create one
        if (wallet is null)
        {
            wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WalletName = "Main Wallet",
                IsDefault = true,
                Status = WalletStatus.Active
            };

            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync();
        }

        return Result<WalletDto>.Success(ToWalletDto(wallet));
    }

    public async Task<Result<WalletDto>> DepositAsync(Guid userId, DepositRequest request)
    {
        if (request.Amount <= 0)
            return Result<WalletDto>.Failure("Amount must be greater than zero.");

        var walletResult = await GetOrCreateDefaultWalletAsync(userId);
        var wallet = await _db.Wallets
            .Include(w => w.Assets)
            .SingleAsync(w => w.Id == walletResult.Value!.Id);

        var asset = await GetOrCreateAssetAsync(wallet.Id, request.Symbol, request.Network);

        // Increase available balance
        asset.AvailableBalance += request.Amount;
        asset.UpdatedAt = DateTimeOffset.UtcNow;

        // Create transaction
        var tx = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Wallet = wallet,
            WalletAssetId = asset.Id,
            WalletAsset = asset,
            Direction = WalletTransactionDirection.Inbound,
            Type = WalletTransactionType.Deposit,
            Status = WalletTransactionStatus.Confirmed,
            Amount = request.Amount,
            FeeAmount = 0m,
            FeeAssetSymbol = null,
            ExternalTransactionId = null,
            RequestedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            Note = request.Note
        };

        _db.WalletTransactions.Add(tx);
        await _db.SaveChangesAsync();

        // Reload wallet with updated assets
        await _db.Entry(wallet).Collection(w => w.Assets).LoadAsync();

        return Result<WalletDto>.Success(ToWalletDto(wallet));
    }

    public async Task<Result<WalletDto>> WithdrawAsync(Guid userId, WithdrawRequest request)
    {
        if (request.Amount <= 0)
            return Result<WalletDto>.Failure("Amount must be greater than zero.");

        var walletResult = await GetOrCreateDefaultWalletAsync(userId);
        var wallet = await _db.Wallets
            .Include(w => w.Assets)
            .SingleAsync(w => w.Id == walletResult.Value!.Id);

        var asset = await GetOrCreateAssetAsync(wallet.Id, request.Symbol, request.Network);

        if (asset.AvailableBalance < request.Amount)
            return Result<WalletDto>.Failure("Insufficient available balance.");

        asset.AvailableBalance -= request.Amount;
        asset.UpdatedAt = DateTimeOffset.UtcNow;

        // For MVP, we mark withdrawal as Completed immediately.
        // Later, this will become Pending until the BlockchainGateway confirms.
        var tx = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Wallet = wallet,
            WalletAssetId = asset.Id,
            WalletAsset = asset,
            Direction = WalletTransactionDirection.Outbound,
            Type = WalletTransactionType.Withdrawal,           // adjust name if needed
            Status = WalletTransactionStatus.Pending,        // or Pending if async
            Amount = request.Amount,
            FeeAmount = 0m,
            FeeAssetSymbol = null,
            ExternalTransactionId = null,                      // will be tx hash later
            RequestedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            Note = request.Note
        };

        _db.WalletTransactions.Add(tx);

        // Optional: create a WithdrawalRequest row if you want a separate table tracking it
        var withdrawalRequest = new WithdrawalRequest
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Wallet = wallet,
            WalletAssetId = asset.Id,
            WalletAsset = asset,
            ToAddress = request.ToAddress,
            Network = request.Network,
            Amount = request.Amount,
            RequestedAt = DateTimeOffset.UtcNow
            // You can add Status, etc. if your entity has those
        };

        _db.WithdrawalRequests.Add(withdrawalRequest);

        await _db.SaveChangesAsync();

        await _db.Entry(wallet).Collection(w => w.Assets).LoadAsync();

        return Result<WalletDto>.Success(ToWalletDto(wallet));
    }

    public async Task<Result<IReadOnlyList<TransactionDto>>> GetTransactionsAsync(
        Guid userId,
        TransactionsQuery query)
    {
        var walletResult = await GetOrCreateDefaultWalletAsync(userId);
        var walletId = walletResult.Value!.Id;

        var txQuery = _db.WalletTransactions
            .Include(t => t.WalletAsset)
            .Where(t => t.WalletId == walletId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Symbol))
            txQuery = txQuery.Where(t => t.WalletAsset.Symbol == query.Symbol);

        if (!string.IsNullOrWhiteSpace(query.Network))
            txQuery = txQuery.Where(t => t.WalletAsset.Network == query.Network);

        txQuery = txQuery.OrderByDescending(t => t.RequestedAt);

        var items = await txQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var dtoList = items.Select(ToTransactionDto).ToList();

        return Result<IReadOnlyList<TransactionDto>>.Success(dtoList);
    }

    // ----------------- helpers -----------------

    private async Task<WalletAsset> GetOrCreateAssetAsync(Guid walletId, string symbol, string network)
    {
        var asset = await _db.WalletAssets
            .SingleOrDefaultAsync(a => a.WalletId == walletId &&
                                       a.Symbol == symbol &&
                                       a.Network == network);

        if (asset is not null)
            return asset;

        asset = new WalletAsset
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Symbol = symbol,
            Network = network,
            AvailableBalance = 0m,
            PendingBalance = 0m
        };

        _db.WalletAssets.Add(asset);
        await _db.SaveChangesAsync();

        return asset;
    }

    private static WalletDto ToWalletDto(Wallet wallet)
    {
        var assets = wallet.Assets.Select(a =>
            new WalletAssetDto(
                a.Id,
                a.Symbol,
                a.Network,
                a.AvailableBalance,
                a.PendingBalance));

        return new WalletDto(
            wallet.Id,
            wallet.UserId,
            wallet.WalletName,
            wallet.IsDefault,
            assets);
    }

    private static TransactionDto ToTransactionDto(WalletTransaction tx)
    {
        return new TransactionDto(
            tx.Id,
            tx.WalletId,
            tx.WalletAssetId,
            tx.Direction,
            tx.Type,
            tx.Status,
            tx.Amount,
            tx.FeeAmount,
            tx.FeeAssetSymbol,
            tx.ExternalTransactionId,
            tx.RequestedAt,
            tx.CompletedAt,
            tx.Note);
    }
}
