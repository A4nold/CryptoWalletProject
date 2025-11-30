using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WalletService.Application.Contracts;
using WalletService.Application.Models;
using WalletService.Domain.Entities;
using WalletService.Domain.Entities.Enum;
using WalletService.Infrastructure.Crypto;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Services;

public class WalletService : IWalletService
{
    private readonly WalletDbContext _db;
    private readonly IBlockchainGatewayClient _blockchainGatewayClient;
    private readonly BlockchainGatewayOptions _blockchainOptions;

    public WalletService(WalletDbContext db, IBlockchainGatewayClient blockchainGatewayClient,
        IOptions<BlockchainGatewayOptions> blockchainOptions)
    {
        _db = db;
        _blockchainGatewayClient = blockchainGatewayClient;
        _blockchainOptions = blockchainOptions.Value;
    }

    public async Task<Result<WalletDto>> GetOrCreateDefaultWalletAsync(Guid userId)
    {
        // Try to find a default wallet for this user
        var wallet = await _db.Wallets
            .Include(w => w.Assets)
            .Include(w => w.ExternalWallets)
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
            .Include(w => w.ExternalWallets)
            .SingleAsync(w => w.Id == walletResult.Value!.Id);

        var asset = await _db.WalletAssets
            .SingleOrDefaultAsync(a =>
                a.WalletId == wallet.Id &&
                a.Symbol == request.Symbol &&
                a.Network == request.Network);

        if (asset is null)
            return Result<WalletDto>.Failure("Asset not found for this wallet.");

        if (asset.AvailableBalance < request.Amount)
            return Result<WalletDto>.Failure("Insufficient available balance.");

        // 🔐 We branch here: on-chain for Solana, or fallback off-chain.
        var isSolana = string.Equals(asset.Network, _blockchainOptions.SolanaNetworkCode, StringComparison.OrdinalIgnoreCase);

        if (!isSolana)
        {
            // --- Existing OFF-CHAIN behavior (you can keep this) ---
            asset.AvailableBalance -= request.Amount;
            asset.UpdatedAt = DateTimeOffset.UtcNow;

            var txOffChain = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Wallet = wallet,
                WalletAssetId = asset.Id,
                WalletAsset = asset,
                Direction = WalletTransactionDirection.Outbound,
                Type = WalletTransactionType.Withdrawal,
                Status = WalletTransactionStatus.Confirmed,
                Amount = request.Amount,
                FeeAmount = 0m,
                FeeAssetSymbol = null,
                ExternalTransactionId = null,
                RequestedAt = DateTimeOffset.UtcNow,
                CompletedAt = DateTimeOffset.UtcNow,
                Note = request.Note
            };

            _db.WalletTransactions.Add(txOffChain);

            // You can still create WithdrawalRequest row here if you want
            await _db.SaveChangesAsync();

            await _db.Entry(wallet).Collection(w => w.Assets).LoadAsync();
            await _db.Entry(wallet).Collection(w => w.ExternalWallets).LoadAsync();

            return Result<WalletDto>.Success(ToWalletDto(wallet));
        }

        // --- ON-CHAIN SOLANA WITHDRAWAL PATH ---

        // 1. Find user's primary external wallet for this network
        var externalWallet = wallet.ExternalWallets
            .Where(e => string.Equals(e.Network, asset.Network, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.IsPrimary)
            .ThenBy(e => e.LinkedAt)
            .FirstOrDefault();

        if (externalWallet is null)
            return Result<WalletDto>.Failure($"No linked external wallet found for network '{asset.Network}'.");

        var toAddress = externalWallet.PublicKey;
        var fromAddress = _blockchainOptions.SolanaHotWalletAddress;

        if (string.IsNullOrWhiteSpace(fromAddress))
            return Result<WalletDto>.Failure("Hot wallet address is not configured.");

        // 2. Call BlockchainGateway
        // correlationId can be a placeholder or derived from a future walletTx id
        var correlationId = $"wallet-withdraw-{Guid.NewGuid():N}";

        var gatewayResult = await _blockchainGatewayClient.WithdrawSolanaAsync(
            networkCode: asset.Network,
            fromAddress: fromAddress,
            toAddress: toAddress,
            amount: request.Amount,
            correlationId: correlationId);

        if (!gatewayResult.IsSuccess || gatewayResult.Value is null)
        {
            return Result<WalletDto>.Failure(
                "Failed to initiate on-chain withdrawal via BlockchainGateway.");
        }

        var bcTx = gatewayResult.Value;

        // 3. If gateway accepted the tx, debit the wallet and create a PENDING WalletTransaction
        asset.AvailableBalance -= request.Amount;
        asset.UpdatedAt = DateTimeOffset.UtcNow;

        var walletTx = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Wallet = wallet,
            WalletAssetId = asset.Id,
            WalletAsset = asset,
            Direction = WalletTransactionDirection.Outbound,
            Type = WalletTransactionType.Withdrawal,
            Status = WalletTransactionStatus.Pending, // pending on-chain confirmation
            Amount = request.Amount,
            FeeAmount = bcTx.Fee,
            FeeAssetSymbol = asset.Symbol, // or use "SOL" explicitly, up to you
            ExternalTransactionId = bcTx.TxHash, // tie to on-chain tx
            RequestedAt = DateTimeOffset.UtcNow,
            CompletedAt = null,
            Note = request.Note
        };

        _db.WalletTransactions.Add(walletTx);

        // Optional: record a WithdrawalRequest too
        var withdrawalRequest = new WithdrawalRequest
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Wallet = wallet,
            WalletAssetId = asset.Id,
            WalletAsset = asset,
            ToAddress = toAddress,
            Network = asset.Network,
            Amount = request.Amount,
            RequestedAt = DateTimeOffset.UtcNow
            // You can add status fields later to track chain result
        };

        _db.WithdrawalRequests.Add(withdrawalRequest);

        await _db.SaveChangesAsync();

        await _db.Entry(wallet).Collection(w => w.Assets).LoadAsync();
        await _db.Entry(wallet).Collection(w => w.ExternalWallets).LoadAsync();

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

    public async Task<Result<WalletDto>> LinkSolanaWalletAsync(Guid userId, LinkSolanaWalletRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PublicKey))
            return Result<WalletDto>.Failure("Public key is required.");

        if (string.IsNullOrWhiteSpace(request.Signature))
            return Result<WalletDto>.Failure("Signature is required.");

        if (string.IsNullOrWhiteSpace(request.Message))
            return Result<WalletDto>.Failure("Message is required.");

        var walletResult = await GetOrCreateDefaultWalletAsync(userId);
        var wallet = await _db.Wallets
            .Include(w => w.Assets)
            .Include(w => w.ExternalWallets)
            .SingleAsync(w => w.Id == walletResult.Value!.Id);


        // 🔐 Real cryptographic verification
        var isValid = SolanaSignatureVerifier.VerifyMessage(
            request.PublicKey,
            request.Signature,
            request.Message);

        if (!isValid)
        {
            return Result<WalletDto>.Failure("Invalid Solana signature or public key.");
        }

        const string network = "solana";

        // Check if this public key is already linked
        var existing = wallet.ExternalWallets
            .SingleOrDefault(e =>
                e.Network == network &&
                e.PublicKey == request.PublicKey);

        if (existing is not null)
        {
            // Just update verification timestamp
            existing.LastVerifiedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            // First wallet on this network: mark as primary
            var isFirstForNetwork = !wallet.ExternalWallets.Any(e => e.Network == network);

            var externalWallet = new ExternalWallet
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                Wallet = wallet,
                Network = network,
                PublicKey = request.PublicKey,
                Label = "Phantom wallet", // could be client-provided later
                IsPrimary = isFirstForNetwork,
                LinkedAt = DateTimeOffset.UtcNow,
                LastVerifiedAt = DateTimeOffset.UtcNow
            };

            _db.ExternalWallets.Add(externalWallet);
        }

        wallet.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        // Reload to capture new external wallets
        wallet = await _db.Wallets
            .Include(w => w.Assets)
            .Include(w => w.ExternalWallets)
            .SingleAsync(w => w.Id == wallet.Id);

        return Result<WalletDto>.Success(ToWalletDto(wallet));
    }

    public async Task<Result<IReadOnlyList<ExternalWalletDto>>> GetExternalWalletsAsync(Guid userId)
    {
        // Ensure the user has a default wallet
        var walletResult = await GetOrCreateDefaultWalletAsync(userId);
        var walletId = walletResult.Value!.Id;

        var externalWallets = await _db.ExternalWallets
            .Where(e => e.WalletId == walletId)
            .OrderBy(e => e.Network)
            .ThenByDescending(e => e.IsPrimary)
            .ThenBy(e => e.LinkedAt)
            .ToListAsync();

        var dtoList = externalWallets.Select(e => new ExternalWalletDto(
            e.Id,
            e.Network,
            e.PublicKey,
            e.Label,
            e.IsPrimary,
            e.LinkedAt,
            e.LastVerifiedAt)).ToList();

        return Result<IReadOnlyList<ExternalWalletDto>>.Success(dtoList);
    }

    public async Task<Result<ExternalWalletDto>> SetPrimaryExternalWalletAsync(Guid userId, Guid externalWalletId)
    {
        // Load the external wallet and ensure it belongs to this user
        var externalWallet = await _db.ExternalWallets
            .Include(e => e.Wallet)
            .SingleOrDefaultAsync(e => e.Id == externalWalletId);

        if (externalWallet is null)
            return Result<ExternalWalletDto>.Failure("External wallet not found.");

        if (externalWallet.Wallet.UserId != userId)
            return Result<ExternalWalletDto>.Failure("You do not have access to this wallet.");

        var network = externalWallet.Network;
        var walletId = externalWallet.WalletId;

        // Clear current primary for this network on this wallet
        var othersOnNetwork = await _db.ExternalWallets
            .Where(e => e.WalletId == walletId && e.Network == network)
            .ToListAsync();

        foreach (var w in othersOnNetwork)
        {
            w.IsPrimary = (w.Id == externalWalletId);
        }

        externalWallet.Wallet.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        var dto = new ExternalWalletDto(
            externalWallet.Id,
            externalWallet.Network,
            externalWallet.PublicKey,
            externalWallet.Label,
            externalWallet.IsPrimary,
            externalWallet.LinkedAt,
            externalWallet.LastVerifiedAt);

        return Result<ExternalWalletDto>.Success(dto);
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

    private WalletDto ToWalletDto(Wallet wallet)
    {
        var assets = wallet.Assets
            .Select(a => new WalletAssetDto(
                a.Id,
                a.Symbol,
                a.Network,
                a.AvailableBalance,
                a.PendingBalance))
            .ToList();

        var externalWallets = wallet.ExternalWallets
            .Select(e => new ExternalWalletDto(
                e.Id,
                e.Network,
                e.PublicKey,
                e.Label,
                e.IsPrimary,
                e.LinkedAt,
                e.LastVerifiedAt))
            .ToList();

        return new WalletDto(
            wallet.Id,
            wallet.UserId,
            wallet.WalletName,
            wallet.IsDefault,
            assets,
            externalWallets);
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
