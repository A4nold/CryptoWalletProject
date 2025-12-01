using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Solnet.Rpc;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using WalletService.Infrastructure.Config;

namespace WalletService.Infrastructure.Services
{
    public sealed record SolanaSignatureStatus(
        string Signature,
        bool IsFound,
        bool HasError,
        string? ErrorMessage,
        string? ConfirmationStatus
    );

    public interface ISolanaRpcService
    {
        Task<IReadOnlyList<SolanaSignatureStatus>> GetSignatureStatusesAsync(
            string networkKey,
            IReadOnlyList<string> signatures,
            CancellationToken ct);

        Task<ulong?> GetTransactionFeeLamportsAsync(
            string networkKey,
            string signature,
            CancellationToken ct);
    }

    public class SolanaRpcService : ISolanaRpcService
    {
        private readonly ILogger<SolanaRpcService> _logger;
        private readonly SolanaNetworkOptions _options;

        // RPC call limits
        private const int MaxSignaturesPerCall = 100;
        private const int MaxRetries = 3;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);

        public SolanaRpcService(
            ILogger<SolanaRpcService> logger,
            IOptions<SolanaNetworkOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        private IRpcClient GetClient(string networkKey)
        {
            if (!_options.RpcEndpoints.TryGetValue(networkKey, out var url))
                throw new InvalidOperationException($"Unknown Solana network key '{networkKey}'.");

            return ClientFactory.GetClient(url);
        }

        public async Task<IReadOnlyList<SolanaSignatureStatus>> GetSignatureStatusesAsync(
            string networkKey,
            IReadOnlyList<string> signatures,
            CancellationToken ct)
        {
            var client = GetClient(networkKey);

            var results = new List<SolanaSignatureStatus>(signatures.Count);

            // Chunk signatures to respect RPC limits
            for (int i = 0; i < signatures.Count; i += MaxSignaturesPerCall)
            {
                var batch = signatures.Skip(i).Take(MaxSignaturesPerCall).ToList();

                var statusList = await CallWithRetries(
                    async () =>
                    {
                        // NOTE: Solnet GetSignatureStatusesAsync does NOT take a CancellationToken
                        var resp = await client.GetSignatureStatusesAsync(batch, true);
                        if (!resp.WasSuccessful || resp.Result?.Value == null)
                            throw new Exception($"GetSignatureStatuses failed: {resp.Reason}");

                        return resp.Result.Value;
                    },
                    $"GetSignatureStatuses[{networkKey}]",
                    ct);

                for (int idx = 0; idx < batch.Count; idx++)
                {
                    var sig = batch[idx];
                    var value = statusList.Count > idx ? statusList[idx] : null;

                    if (value == null)
                    {
                        results.Add(new SolanaSignatureStatus(
                            sig,
                            IsFound: false,
                            HasError: false,
                            ErrorMessage: null,
                            ConfirmationStatus: null));
                        continue;
                    }

                    string? errorString = null;
                    var hasError = value.Error != null;
                    if (hasError)
                    {
                        // Err is dynamic-like; ToString() is enough for logging/storage
                        errorString = value.Error.ToString();
                    }

                    results.Add(new SolanaSignatureStatus(
                        sig,
                        IsFound: true,
                        HasError: hasError,
                        ErrorMessage: errorString,
                        ConfirmationStatus: value.ConfirmationStatus));
                }
            }

            return results;
        }

        public async Task<ulong?> GetTransactionFeeLamportsAsync(
            string networkKey,
            string signature,
            CancellationToken ct)
        {
            var client = GetClient(networkKey);

            var txResult = await CallWithRetries(
                async () =>
                {
                    // NOTE: Solnet GetTransactionAsync does NOT take a CancellationToken
                    var resp = await client.GetTransactionAsync(signature, Commitment.Confirmed);
                    if (!resp.WasSuccessful)
                        throw new Exception($"GetTransaction failed: {resp.Reason}");

                    return resp.Result;
                },
                $"GetTransaction[{networkKey}] sig={signature}",
                ct);

            // No meta or fee? just return null
            return txResult?.Meta?.Fee;
        }

        private async Task<T> CallWithRetries<T>(
            Func<Task<T>> action,
            string operationName,
            CancellationToken ct)
        {
            Exception? lastEx = null;

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    return await action();
                }
                catch (Exception ex) when (!ct.IsCancellationRequested)
                {
                    lastEx = ex;
                    _logger.LogWarning(ex,
                        "Solana RPC operation {Operation} failed on attempt {Attempt}/{MaxRetries}.",
                        operationName, attempt, MaxRetries);

                    if (attempt < MaxRetries)
                        await Task.Delay(RetryDelay, ct);
                }
            }

            _logger.LogError(lastEx, "Solana RPC operation {Operation} permanently failed.", operationName);
            throw lastEx ?? new Exception($"Solana RPC operation {operationName} failed.");
        }
    }
}
