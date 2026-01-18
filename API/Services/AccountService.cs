using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// Service for managing the paper trading account.
/// Handles account initialization, balance updates, and equity calculations.
/// </summary>
public class AccountService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AccountService> _logger;
    private const int ACCOUNT_ID = 1; // Single account for MVP
    private const decimal DEFAULT_INITIAL_BALANCE = 1000m; // 1,000 EUR

    public AccountService(
        ApplicationDbContext context,
        ILogger<AccountService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates the account (Singleton pattern).
    /// </summary>
    public async Task<Account> GetOrCreateAccountAsync()
    {
        var account = await _context.Accounts.FindAsync(ACCOUNT_ID);

        if (account == null)
        {
            _logger.LogInformation("Account not found, creating new account with initial balance: {Balance} EUR", DEFAULT_INITIAL_BALANCE);
            
            account = new Account
            {
                Id = ACCOUNT_ID,
                InitialBalance = DEFAULT_INITIAL_BALANCE,
                Balance = DEFAULT_INITIAL_BALANCE,
                Equity = DEFAULT_INITIAL_BALANCE,
                AvailableCash = DEFAULT_INITIAL_BALANCE,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Account created successfully with ID: {Id}", account.Id);
        }

        return account;
    }

    /// <summary>
    /// Updates the account balance and equity.
    /// </summary>
    public async Task UpdateAccountAsync(decimal balance, decimal equity, decimal availableCash)
    {
        var account = await GetOrCreateAccountAsync();

        account.Balance = balance;
        account.Equity = equity;
        account.AvailableCash = availableCash;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Account updated - Balance: {Balance}, Equity: {Equity}, Available: {Available}", 
            balance, equity, availableCash);
    }

    /// <summary>
    /// Resets the account to initial state.
    /// </summary>
    public async Task ResetAccountAsync()
    {
        var account = await GetOrCreateAccountAsync();

        account.InitialBalance = DEFAULT_INITIAL_BALANCE;
        account.Balance = DEFAULT_INITIAL_BALANCE;
        account.Equity = DEFAULT_INITIAL_BALANCE;
        account.AvailableCash = DEFAULT_INITIAL_BALANCE;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Account reset to initial balance: {Balance} EUR", DEFAULT_INITIAL_BALANCE);
    }

    /// <summary>
    /// Calculates current equity including unrealized PnL from open trades.
    /// </summary>
    public async Task<decimal> CalculateCurrentEquityAsync()
    {
        var account = await GetOrCreateAccountAsync();
        
        // TODO: Add unrealized PnL from open trades when PaperTrade management is implemented
        // For now, equity equals balance
        var equity = account.Balance;

        return equity;
    }

    /// <summary>
    /// Allocates capital for a new trade (reduces available cash).
    /// </summary>
    public async Task AllocateCapitalAsync(decimal amount)
    {
        var account = await GetOrCreateAccountAsync();

        if (account.AvailableCash < amount)
        {
            throw new InvalidOperationException($"Insufficient available cash. Required: {amount}, Available: {account.AvailableCash}");
        }

        account.AvailableCash -= amount;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Capital allocated: {Amount} EUR. Available cash: {Available} EUR", amount, account.AvailableCash);
    }

    /// <summary>
    /// Releases capital when a trade is closed (increases available cash).
    /// </summary>
    public async Task ReleaseCapitalAsync(decimal amount, decimal pnl)
    {
        var account = await GetOrCreateAccountAsync();

        account.AvailableCash += amount; // Return invested capital
        account.Balance += pnl;           // Add/subtract profit or loss
        account.Equity = await CalculateCurrentEquityAsync();
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Capital released: {Amount} EUR, PnL: {PnL} EUR. New balance: {Balance} EUR", 
            amount, pnl, account.Balance);
    }
}
