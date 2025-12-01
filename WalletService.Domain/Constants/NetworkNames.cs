namespace WalletService.Domain
{
    /// <summary>
    /// Canonical network names used across the system and persisted in the database.
    /// </summary>
    public static class NetworkNames
    {
        public const string SolanaDevnet = "solana-devnet";
        public const string SolanaMainnet = "solana-mainnet";

        // Add more later, e.g.:
        // public const string SolanaTestnet = "solana-testnet";
        // public const string EthereumMainnet = "ethereum-mainnet";
    }
}
