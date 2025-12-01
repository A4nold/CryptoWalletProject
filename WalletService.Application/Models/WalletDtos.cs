namespace WalletService.Application.Models;

public record WalletAssetDto(
    Guid Id,
    string Symbol,
    string Network,
    decimal AvailableBalance,
    decimal PendingBalance);

public record ExternalWalletDto(
    Guid Id,
    string Network,
    string PublicKey,
    string? Label,
    bool IsPrimary,
    DateTimeOffset LinkedAt,
    DateTimeOffset? LastVerifiedAt);


public record WalletDto(
    Guid Id,
    Guid UserId,
    string WalletName,
    bool IsDefault,
    IReadOnlyList<WalletAssetDto> Assets,
    IReadOnlyList<ExternalWalletDto> ExternalWallets // NEW
);
public record WithdrawResultDto(
    WalletDto Wallet,
    string? BlockchainTransactionHash);