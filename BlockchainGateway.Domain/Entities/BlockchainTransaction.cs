using BlockchainGateway.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockchainGateway.Domain.Entities
{
    public class BlockchainTransaction
    {
        public Guid Id { get; set; }

        public Guid NetworkId { get; set; }
        public BlockchainNetwork Network { get; set; } = default!;

        public string TxHash { get; set; } = default!;

        public string FromAddress { get; set; } = default!;
        public string ToAddress { get; set; } = default!;

        public decimal Amount { get; set; }
        public decimal Fee { get; set; }

        public long? BlockNumber { get; set; }
        public int Confirmations { get; set; }

        public BlockchainTransactionStatus Status { get; set; } = BlockchainTransactionStatus.Pending;
        public BlockchainTransactionDirection Direction { get; set; }

        // Used to correlate with WalletService, etc
        public string? CorrelationId { get; set; }   // e.g. WalletTransactionId

        public DateTimeOffset FirstSeenAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ConfirmedAt { get; set; }
    }
}
