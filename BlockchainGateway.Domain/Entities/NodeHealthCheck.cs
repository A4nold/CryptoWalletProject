using System;
using System.Collections.Generic;
using System.Text;

namespace BlockchainGateway.Domain.Entities
{
    public class NodeHealthCheck
    {
        public Guid Id { get; set; }

        public Guid NetworkId { get; set; }
        public BlockchainNetwork Network { get; set; } = default!;

        public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;

        public bool IsHealthy { get; set; }
        public int? ResponseTimeMs { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
