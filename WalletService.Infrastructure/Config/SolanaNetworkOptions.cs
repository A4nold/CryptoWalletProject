namespace WalletService.Infrastructure.Config
{
    public class SolanaNetworkOptions
    {
        /// <summary>
        /// Map logical network key → RPC endpoint, e.g. "devnet" → "https://api.devnet.solana.com"
        /// </summary>
        public Dictionary<string, string> RpcEndpoints { get; set; } = new();
    }
}

