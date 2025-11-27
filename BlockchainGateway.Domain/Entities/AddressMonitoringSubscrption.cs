using System;
using System.Collections.Generic;
using System.Text;

namespace BlockchainGateway.Domain.Entities
{
    public class AddressMonitoringSubscrption
    {
        public Guid Id { get; set; }

        public Guid NetworkId { get; set; }
        public BlockchainNetwork Network { get; set; } = default!;

        public string Address { get; set; } = default!;

        // Who asked for this (service name)
        public string SourceSystem { get; set; } = default!;  // "WalletService"

        // Arbitrary id that caller can use for correlation (e.g. DepositAddressId)
        public string? CorrelationId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? DeactivatedAt { get; set; }
    }
}
