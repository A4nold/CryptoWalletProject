using BlockchainGateway.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlockchainGateway.Domain.Entities
{
    public class BlockchainAddress
    {
        public Guid Id { get; set; }

        public Guid NetworkId { get; set; }
        public BlockchainNetwork Network { get; set; } = default!;

        public string Address { get; set; } = default!; // On-chain address

        public AddressType AddressType { get; set; } = AddressType.Deposit;

        // Optional label for debugging / backoffice
        public string? Label { get; set; }              // "User deposit", "Hot Wallet"

        public bool IsActive { get; set; } = true;

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
