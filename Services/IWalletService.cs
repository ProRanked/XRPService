namespace XRPService.Services;

/// <summary>
/// Interface for XRPL wallet management operations
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Generates a new XRP wallet
    /// </summary>
    /// <returns>Wallet information including address and seed</returns>
    Task<WalletInfo> CreateWalletAsync();
    
    /// <summary>
    /// Funds a test wallet with test XRP (testnet/devnet only)
    /// </summary>
    /// <param name="address">Wallet address to fund</param>
    /// <returns>Funding transaction details</returns>
    Task<object> FundTestWalletAsync(string address);
    
    /// <summary>
    /// Gets wallet information by address
    /// </summary>
    /// <param name="address">XRP address</param>
    /// <returns>Wallet information</returns>
    Task<WalletInfo> GetWalletInfoAsync(string address);
    
    /// <summary>
    /// Signs a transaction with wallet credentials
    /// </summary>
    /// <param name="walletSeed">Wallet seed</param>
    /// <param name="transaction">Transaction to sign</param>
    /// <returns>Signed transaction blob</returns>
    Task<string> SignTransactionAsync(string walletSeed, object transaction);
}

/// <summary>
/// XRP wallet information
/// </summary>
public class WalletInfo
{
    /// <summary>
    /// XRP address (public)
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Wallet seed (private, should be kept secure)
    /// </summary>
    public string? Seed { get; set; }
    
    /// <summary>
    /// XRP balance
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Sequence number for the account
    /// </summary>
    public int Sequence { get; set; }
}