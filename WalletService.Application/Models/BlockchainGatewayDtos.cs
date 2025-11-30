namespace WalletService.Application.Models;

public record BlockchainWithdrawResponseDto(
    Guid Id,
    string NetworkCode,
    string TxHash,
    string FromAddress,
    string ToAddress,
    decimal Amount,
    decimal Fee,
    string Status,
    string Direction,
    string? CorrelationId,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset? ConfirmedAt);

