using BlockchainGateway.Application.Contracts;
using BlockchainGateway.Application.Models;
using BlockchainGateway.Domain.Entities;
using BlockchainGateway.Domain.Entities.Enums;
using BlockchainGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
// using Solnet.Rpc;
// using Solnet.Wallet;
// using Solnet.Rpc.Builders;

namespace BlockchainGateway.Infrastructure.Services;

public class SolanaTransactionService : ISolanaTransactionService
{
    private readonly BlockchainDbContext _db;
    private readonly ILogger<SolanaTransactionService> _logger;
    private readonly SolanaHotWalletOptions _hotWalletOptions;

    public SolanaTransactionService(
        BlockchainDbContext db,
        ILogger<SolanaTransactionService> logger,
        IOptions<SolanaHotWalletOptions> hotWalletOptions)
    {
        _db = db;
        _logger = logger;
        _hotWalletOptions = hotWalletOptions.Value;
    }

    public async Task<Result<BlockchainTransactionDto>> WithdrawAsync(SolanaWithdrawRequest request)
    {
        // 1. Validate network
        var network = await _db.BlockchainNetworks
            .SingleOrDefaultAsync(n =>
                n.NetworkCode == request.NetworkCode &&
                n.IsEnabled);

        if (network is null)
        {
            return Result<BlockchainTransactionDto>.Failure("Network not found or disabled.");
        }

        // 2. Basic validation
        if (request.Amount <= 0)
            return Result<BlockchainTransactionDto>.Failure("Amount must be greater than zero.");

        // 3. OPTIONAL: For safety, enforce that FromAddress is our configured hot wallet
        if (!string.Equals(request.FromAddress, _hotWalletOptions.PublicKey, StringComparison.Ordinal))
        {
            return Result<BlockchainTransactionDto>.Failure("FromAddress must be the configured hot wallet.");
        }

        // 4. Build and send Solana transaction (simplified / TODO)
        string txHash;
        decimal fee = 0m;

        try
        {
            // TODO: real Solana integration using Solnet:
            //
            // var rpcClient = ClientFactory.GetClient(network.RpcEndpoint);
            // var wallet = new Wallet(_hotWalletOptions.PrivateKey);
            //
            // ulong lamports = (ulong)(request.Amount * SolanaConstants.LAMPORTS_PER_SOL);
            //
            // var blockHash = await rpcClient.GetRecentBlockHashAsync();
            //
            // var txBuilder = new TransactionBuilder()
            //     .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
            //     .SetFeePayer(wallet.Account)
            //     .AddInstruction(TokenProgram.Transfer(
            //         source: wallet.Account.PublicKey,
            //         destination: new PublicKey(request.ToAddress),
            //         owner: wallet.Account.PublicKey,
            //         amount: lamports));
            //
            // var tx = txBuilder.Build(wallet.Account);
            // var sendResult = await rpcClient.SendTransactionAsync(tx);
            //
            // txHash = sendResult.Result; // the Solana tx signature

            // For now, we stub a fake hash to keep the flow working:
            txHash = $"fake-solana-tx-{Guid.NewGuid():N}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Solana transaction.");

            return Result<BlockchainTransactionDto>.Failure("Failed to send Solana transaction.");
        }

        // 5. Persist BlockchainTransaction
        var bcTx = new BlockchainTransaction
        {
            Id = Guid.NewGuid(),
            NetworkId = network.Id,
            Network = network,
            TxHash = txHash,
            FromAddress = request.FromAddress,
            ToAddress = request.ToAddress,
            Amount = request.Amount,
            Fee = fee,
            BlockNumber = null,
            Confirmations = 0,
            Direction = BlockchainTransactionDirection.Outbound,
            Status = BlockchainTransactionStatus.Pending,
            CorrelationId = request.CorrelationId,
            FirstSeenAt = DateTimeOffset.UtcNow
        };

        _db.BlockchainTransactions.Add(bcTx);
        await _db.SaveChangesAsync();

        var dto = new BlockchainTransactionDto(
            bcTx.Id,
            network.NetworkCode,
            bcTx.TxHash,
            bcTx.FromAddress,
            bcTx.ToAddress,
            bcTx.Amount,
            bcTx.Fee,
            bcTx.Status.ToString(),
            bcTx.Direction.ToString(),
            bcTx.CorrelationId,
            bcTx.FirstSeenAt,
            bcTx.ConfirmedAt);

        return Result<BlockchainTransactionDto>.Success(dto);
    }
}
