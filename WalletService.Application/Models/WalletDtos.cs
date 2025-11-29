namespace WalletService.Application.Models;

public record WalletAssetDto(
    Guid Id,
    string Symbol,
    string Network,
    decimal AvailableBalance,
    decimal PendingBalance);

public record WalletDto(
    Guid Id,
    Guid UserId,
    string WalletName,
    bool IsDefault,
    IEnumerable<WalletAssetDto> Assets);
