namespace WalletService.Application.Models;

public record DepositRequest(
    string Symbol,
    string Network,
    decimal Amount,
    string? Note);

public record WithdrawRequest(
    string Symbol,
    string Network,
    decimal Amount,
    string ToAddress,
    string? Note);

public record TransactionsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Symbol = null,
    string? Network = null);

