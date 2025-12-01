namespace BlockchainGateway.Application.Models;

public record BlockchainNetworkDto(
    Guid Id,
    string Name,
    string Symbol,
    string NetworkCode,
    string RpcEndpoint,
    string? ExplorerBaseUrl,
    string NetworkType,
    bool IsEnabled);

public record BlockchainTransactionDto(
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
