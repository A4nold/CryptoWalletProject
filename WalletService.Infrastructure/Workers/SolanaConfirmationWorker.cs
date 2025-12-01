using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WalletService.Domain.Entities;
using WalletService.Domain.Entities.Enum;
using WalletService.Infrastructure.Data;
using WalletService.Infrastructure.Services;

namespace WalletService.Infrastructure.Workers
{
    public class SolanaConfirmationWorker : BackgroundService
    {
        private readonly ILogger<SolanaConfirmationWorker> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISolanaRpcService _solanaRpc;

        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _timeout = TimeSpan.FromMinutes(5);
        private const int MaxBatchSize = 200;
        private const decimal LamportsPerSol = 1_000_000_000m;

        public SolanaConfirmationWorker(
            ILogger<SolanaConfirmationWorker> logger,
            IServiceScopeFactory scopeFactory,
            ISolanaRpcService solanaRpc)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _solanaRpc = solanaRpc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Solana Confirmation Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingTransactions(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inside Solana confirmation worker loop.");
                }

                await Task.Delay(_pollInterval, stoppingToken);
            }

            _logger.LogInformation("Solana Confirmation Worker stopped.");
        }

        private async Task ProcessPendingTransactions(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WalletDbContext>();

            var now = DateTimeOffset.UtcNow;

            var pending = await db.WalletTransactions
                .Include(t => t.WalletAsset)
                .Where(t =>
                    t.Status == WalletTransactionStatus.Pending &&
                    t.ExternalTransactionId != null)
                .OrderBy(t => t.RequestedAt)
                .Take(MaxBatchSize)
                .ToListAsync(ct);

            if (!pending.Any())
                return;

            _logger.LogInformation("Checking {Count} pending Solana transactions...", pending.Count);

            // Use existing WalletAsset.Network as the "network key"
            var groupedByNetwork = pending.GroupBy(t => t.WalletAsset.Network);

            foreach (var group in groupedByNetwork)
            {
                var networkKey = group.Key; // e.g. "solana-devnet" or "solana-mainnet"
                var txs = group.ToList();

                var signatureToTx = txs
                    .Where(t => t.ExternalTransactionId != null)
                    .ToDictionary(t => t.ExternalTransactionId!, t => t);

                var signatures = signatureToTx.Keys.ToList();

                IReadOnlyList<SolanaSignatureStatus> statuses;
                try
                {
                    statuses = await _solanaRpc.GetSignatureStatusesAsync(networkKey, signatures, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to fetch signature statuses for network {NetworkKey}. Skipping batch.",
                        networkKey);
                    continue;
                }

                foreach (var status in statuses)
                {
                    if (!signatureToTx.TryGetValue(status.Signature, out var tx))
                        continue;

                    var age = now - tx.RequestedAt;

                    // Not found yet on the network
                    if (!status.IsFound)
                    {
                        if (age > _timeout)
                        {
                            tx.Status = WalletTransactionStatus.Failed;
                            tx.Note = "Solana transaction not found before timeout.";
                            tx.CompletedAt = now;

                            _logger.LogWarning(
                                "Tx {TxId} ({Sig}) FAILED: not found before timeout.",
                                tx.Id, status.Signature);
                        }

                        continue;
                    }

                    var conf = status.ConfirmationStatus;

                    if (conf == "confirmed" || conf == "finalized")
                    {
                        tx.Status = WalletTransactionStatus.Confirmed;
                        tx.CompletedAt = now;

                        _logger.LogInformation(
                            "Tx {TxId} ({Sig}) CONFIRMED with status {Status}.",
                            tx.Id, status.Signature, conf);

                        // Best-effort: fetch & store fee in SOL
                        try
                        {
                            var feeLamports = await _solanaRpc.GetTransactionFeeLamportsAsync(
                                networkKey,
                                status.Signature,
                                ct);

                            if (feeLamports.HasValue)
                            {
                                var feeSol = (decimal)feeLamports.Value / LamportsPerSol;
                                tx.FeeAmount = feeSol;

                                if (string.IsNullOrWhiteSpace(tx.FeeAssetSymbol))
                                    tx.FeeAssetSymbol = "SOL";
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex,
                                "Failed to fetch fee for tx {TxId} ({Sig}).",
                                tx.Id, status.Signature);
                        }
                    }
                    else
                    {
                        // processed / null / other; treat as pending until timeout
                        if (age > _timeout)
                        {
                            tx.Status = WalletTransactionStatus.Failed;
                            tx.Note = $"Timed out waiting for confirmation (status={conf ?? "null"}).";
                            tx.CompletedAt = now;

                            _logger.LogWarning(
                                "Tx {TxId} ({Sig}) FAILED after timeout with status {Status}.",
                                tx.Id, status.Signature, conf);
                        }
                    }
                }
            }

            await db.SaveChangesAsync(ct);
        }
    }
}
