using BlockchainGateway.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockchainGateway.Domain.Entities
{
    public class BlockchainNetwork
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;        // "Solana Devnet"
        public string Symbol { get; set; } = default!;      // "SOL"
        public string NetworkCode { get; set; } = default!; // "solana-devnet", "eth-mainnet"

        // For EVM-based chains; null for solana chains
        public long? ChainId { get; set; }

        public string RpcEndpoint { get; set; } = default!;
        public string? ExplorerBaseUrl { get; set; }

        public NetworkType NetworkType { get; set; } = NetworkType.Devnet;

        public bool IsEnabled { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        // Navigation
        public ICollection<BlockchainAddress> Addresses { get; set; } = new List<BlockchainAddress>();
    }
}
