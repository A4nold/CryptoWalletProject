using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WalletService.Application.Contracts;
using WalletService.Application.Models;

namespace WalletService.Infrastructure.Services;

public class BlockchainGatewayClient : IBlockchainGatewayClient
{
    private readonly HttpClient _httpClient;
    private readonly BlockchainGatewayOptions _options;
    private readonly ILogger<BlockchainGatewayClient> _logger;

    public BlockchainGatewayClient(
        HttpClient httpClient,
        IOptions<BlockchainGatewayOptions> options,
        ILogger<BlockchainGatewayClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<BlockchainWithdrawResponseDto>> WithdrawSolanaAsync(
        string networkCode,
        string fromAddress,
        string toAddress,
        decimal amount,
        string? correlationId)
    {
        var request = new
        {
            networkCode,
            fromAddress,
            toAddress,
            amount,
            correlationId
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync("api/solana/withdraw", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("BlockchainGateway withdraw failed: {Status} {Body}",
                    response.StatusCode, error);

                return Result<BlockchainWithdrawResponseDto>.Failure(
                    $"BlockchainGateway returned {response.StatusCode}");
            }

            var dto = await response.Content.ReadFromJsonAsync<BlockchainWithdrawResponseDto>();

            if (dto is null)
            {
                return Result<BlockchainWithdrawResponseDto>.Failure("Invalid response from BlockchainGateway.");
            }

            return Result<BlockchainWithdrawResponseDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling BlockchainGateway.");
            return Result<BlockchainWithdrawResponseDto>.Failure("Error calling BlockchainGateway.");
        }
    }
}

