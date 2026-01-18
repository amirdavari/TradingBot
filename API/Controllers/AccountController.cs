using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// API Controller for managing the paper trading account.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        AccountService accountService,
        ILogger<AccountController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current account state.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
    public async Task<ActionResult<Account>> GetAccount()
    {
        try
        {
            var account = await _accountService.GetOrCreateAccountAsync();
            
            // Update equity with current market values
            account.Equity = await _accountService.CalculateCurrentEquityAsync();
            
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching account");
            return StatusCode(500, "Failed to fetch account data");
        }
    }

    /// <summary>
    /// Resets the account to initial balance.
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
    public async Task<ActionResult<Account>> ResetAccount()
    {
        try
        {
            await _accountService.ResetAccountAsync();
            var account = await _accountService.GetOrCreateAccountAsync();
            
            _logger.LogInformation("Account reset successfully");
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting account");
            return StatusCode(500, "Failed to reset account");
        }
    }
}
