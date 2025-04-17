namespace XRPService.Services;

/// <summary>
/// Interface for XRP Ledger interactions
/// </summary>
public interface IXRPLService
{
    /// <summary>
    /// Connects to the XRP Ledger
    /// </summary>
    /// <param name="network">Network to connect to (e.g., "mainnet", "testnet")</param>
    /// <returns>True if connection successful</returns>
    Task<bool> ConnectAsync(string network = "testnet");

    /// <summary>
    /// Submits a payment transaction to the XRP Ledger
    /// </summary>
    /// <param name="sourceWalletSeed">Sender wallet seed</param>
    /// <param name="destinationAddress">Recipient XRP address</param>
    /// <param name="amountInXrp">Amount to send in XRP</param>
    /// <param name="memo">Optional transaction memo</param>
    /// <returns>Transaction hash if successful</returns>
    Task<string> SubmitPaymentAsync(string sourceWalletSeed, string destinationAddress, decimal amountInXrp, string? memo = null);

    /// <summary>
    /// Gets transaction details by hash
    /// </summary>
    /// <param name="transactionHash">Transaction hash to lookup</param>
    /// <returns>Transaction details</returns>
    Task<object> GetTransactionAsync(string transactionHash);

    /// <summary>
    /// Gets the account information including XRP balance
    /// </summary>
    /// <param name="address">XRP address</param>
    /// <returns>Account information</returns>
    Task<object> GetAccountInfoAsync(string address);
}