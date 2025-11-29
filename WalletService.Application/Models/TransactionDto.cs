using WalletService.Domain.Entities.Enum;

namespace WalletService.Application.Models;

public record TransactionDto(
    Guid Id,
    Guid WalletId,
    Guid WalletAssetId,
    WalletTransactionDirection Direction,
    WalletTransactionType Type,
    WalletTransactionStatus Status,
    decimal Amount,
    decimal FeeAmount,
    string? FeeAssetSymbol,
    string? ExternalTransactionId,
    DateTimeOffset RequestedAt,
    DateTimeOffset? CompletedAt,
    string? Note);
