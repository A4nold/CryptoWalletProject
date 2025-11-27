using BlockchainGateway.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockchainGateway.Domain.Entities
{
    public class BlockchainNetwork
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;        // "Bitcoin Mainnet"
        public string Symbol { get; set; } = default!;      // "BTC"
        public string NetworkCode { get; set; } = default!; // "bitcoin-mainnet", "eth-mainnet"

        // For EVM-based chains; null for Bitcoin-like chains
        public long? ChainId { get; set; }

        public string RpcEndpoint { get; set; } = default!;
        public string? ExplorerBaseUrl { get; set; }

        public NetworkType NetworkType { get; set; } = NetworkType.Mainnet;

        public bool IsEnabled { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        // Navigation
        public ICollection<BlockchainAddress> Addresses { get; set; } = new List<BlockchainAddress>();
    }
}
