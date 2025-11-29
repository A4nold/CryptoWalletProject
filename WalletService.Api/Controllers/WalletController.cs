using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletService.Application.Contracts;
using WalletService.Application.Models;

namespace WalletService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // All endpoints require a valid JWT (from Identity service)
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    /// <summary>
    /// Helper: extract the user id (Guid) from the JWT "sub" claim.
    /// This corresponds to the Identity AppUser Id.
    /// </summary>
    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (sub is null)
            throw new InvalidOperationException("User id (sub) claim is missing from token.");

        return Guid.Parse(sub);
    }

    /// <summary>
    /// Get or create the default wallet for the current user.
    /// Includes all assets and balances.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWallet()
    {
        var userId = GetUserId();

        var result = await _walletService.GetOrCreateDefaultWalletAsync(userId);

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    /// <summary>
    /// Deposit off-chain credits into a specific asset (Symbol + Network).
    /// For MVP this is a trusted operation (e.g. admin credit, test funds).
    /// Later it will be tied to on-chain or fiat confirmations.
    /// </summary>
    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
    {
        var userId = GetUserId();

        var result = await _walletService.DepositAsync(userId, request);

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    /// <summary>
    /// Withdraw off-chain credits from a specific asset.
    /// For MVP this only adjusts balances; later it will trigger a blockchain transaction
    /// through the BlockchainGateway and mark the transaction as Pending until confirmed.
    /// </summary>
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
    {
        var userId = GetUserId();

        var result = await _walletService.WithdrawAsync(userId, request);

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }

    /// <summary>
    /// Get recent wallet transactions with optional Symbol/Network filters.
    /// </summary>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? symbol = null,
        [FromQuery] string? network = null)
    {
        var userId = GetUserId();

        var query = new TransactionsQuery(page, pageSize, symbol, network);
        var result = await _walletService.GetTransactionsAsync(userId, query);

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors });

        return Ok(result.Value);
    }
}
