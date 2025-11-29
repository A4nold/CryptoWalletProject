namespace WalletService.Domain.Entities.Enum
{
    public enum WalletTransactionDirection
    {
        Inbound = 1, //deposits, payouts, rewards
        Outbound = 2, //withdrawal, stakes, fees
        InternalTransfer = 3 //between two wallets
    }
}
