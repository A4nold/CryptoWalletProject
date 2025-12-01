using BlockchainGateway.Application.Contracts;
using BlockchainGateway.Application.Models;
using BlockchainGateway.Domain.Entities;
using BlockchainGateway.Domain.Entities.Enums;
using BlockchainGateway.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;
using Solnet.Programs;

namespace BlockchainGateway.Infrastructure.Services
{
    public class SolanaTransactionService : ISolanaTransactionService
    {
        private readonly BlockchainDbContext _db;
        private readonly ILogger<SolanaTransactionService> _logger;
        private readonly SolanaHotWalletOptions _hotWalletOptions;

        private const decimal LamportsPerSol = 1_000_000_000m;

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

            if (string.IsNullOrWhiteSpace(_hotWalletOptions.PublicKey) ||
                string.IsNullOrWhiteSpace(_hotWalletOptions.PrivateKey))
            {
                return Result<BlockchainTransactionDto>.Failure("Hot wallet configuration is missing.");
            }

            // 3. For safety, FromAddress must be our configured hot wallet
            if (!string.Equals(request.FromAddress, _hotWalletOptions.PublicKey, StringComparison.Ordinal))
            {
                return Result<BlockchainTransactionDto>.Failure("FromAddress must be the configured hot wallet.");
            }

            string txHash;
            decimal fee = 0m;

            try
            {
                // 4. Decode Phantom base58 secret key
                byte[] secretKey = Encoders.Base58.DecodeData(_hotWalletOptions.PrivateKey);

                if (secretKey.Length != 64)
                {
                    _logger.LogError("Invalid Solana private key length. Got {Length}, expected 64.", secretKey.Length);
                    return Result<BlockchainTransactionDto>.Failure("Invalid Solana private key format.");
                }

                // First 32 bytes = private key, next 32 = public key
                //byte[] privateKey = keyPair[..32];
                //byte[] publicKey = keyPair[32..];

                var account = Account.FromSecretKey(_hotWalletOptions.PrivateKey);

                // Sanity check public key
                var derivedPubKey = account.PublicKey.Key;
                if (!string.Equals(derivedPubKey, _hotWalletOptions.PublicKey, StringComparison.Ordinal))
                {
                    _logger.LogWarning(
                        "Hot wallet public key mismatch. Derived: {Derived}, Config: {Config}",
                        derivedPubKey, _hotWalletOptions.PublicKey);
                }

                // 5. Create RPC client for this network (e.g. devnet)
                var rpcClient = ClientFactory.GetClient(network.RpcEndpoint);

                // 6. Convert SOL to lamports
                ulong lamports = (ulong)(request.Amount * LamportsPerSol);

                // 7. Get recent blockhash
                var blockHashResult = await rpcClient.GetLatestBlockHashAsync(Commitment.Confirmed);
                if (!blockHashResult.WasSuccessful || blockHashResult.Result is null)
                {
                    _logger.LogError("GetLatestBlockHashAsync failed: {Error}", blockHashResult.Reason);
                    return Result<BlockchainTransactionDto>.Failure("Failed to get recent block hash from Solana.");
                }

                var blockHash = blockHashResult.Result.Value.Blockhash;

                var fromPublicKey = account.PublicKey;
                var toPublicKey = new PublicKey(request.ToAddress);

                // 8. Build SOL transfer transaction
                var txBuilder = new TransactionBuilder()
                    .SetRecentBlockHash(blockHash)
                    .SetFeePayer(fromPublicKey)
                    .AddInstruction(
                        SystemProgram.Transfer(
                            fromPublicKey,
                            toPublicKey,
                            lamports));

                // 9. Sign transaction
                var txBytes = txBuilder.Build(account);

                // 10. Send transaction
                var sendResult = await rpcClient.SendTransactionAsync(
                    txBytes,
                    commitment: Commitment.Confirmed);

                if (!sendResult.WasSuccessful || string.IsNullOrWhiteSpace(sendResult.Result))
                {
                    _logger.LogError("SendTransactionAsync failed: {Reason}", sendResult.Reason);
                    return Result<BlockchainTransactionDto>.Failure("Failed to send Solana transaction.");
                }

                txHash = sendResult.Result; // Solana transaction signature

                // You can optionally calculate fee later by querying RPC for fee details.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Solana transaction.");
                return Result<BlockchainTransactionDto>.Failure("Failed to send Solana transaction.");
            }

            // 11. Persist BlockchainTransaction
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
}
