using BlockchainGateway.Application.Contracts;
using BlockchainGateway.Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlockchainGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SolanaController : ControllerBase
{
    private readonly ISolanaTransactionService _solanaService;

    public SolanaController(ISolanaTransactionService solanaService)
    {
        _solanaService = solanaService;
    }

    /// <summary>
    /// Called by WalletService (or other internal services) to perform a withdraw
    /// from our hot wallet to a user's external Solana wallet.
    /// </summary>
    [HttpPost("withdraw")]
    // You can add [Authorize] + some internal auth later. For now leave open or secure with API key.
    public async Task<IActionResult> Withdraw([FromBody] SolanaWithdrawRequest request)
    {
        var result = await _solanaService.WithdrawAsync(request);

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }
}

