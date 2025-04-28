namespace XRPService.Services;

/// <summary>
/// Interface for EV charging payments using XRP
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Initializes a payment session for an EV charging session
    /// </summary>
    /// <param name="sessionId">Charging session ID</param>
    /// <param name="userId">User ID</param>
    /// <param name="stationId">Charging station ID</param>
    /// <returns>Payment session details including wallet information</returns>
    Task<PaymentSession> InitializePaymentSessionAsync(string sessionId, string userId, string stationId);
    
    /// <summary>
    /// Processes a micropayment for ongoing charging
    /// </summary>
    /// <param name="sessionId">Charging session ID</param>
    /// <param name="energyUsed">Energy consumed in kWh</param>
    /// <param name="amountInXrp">Payment amount in XRP</param>
    /// <returns>Payment transaction details</returns>
    Task<PaymentTransaction> ProcessMicropaymentAsync(string sessionId, decimal energyUsed, decimal amountInXrp);
    
    /// <summary>
    /// Finalizes a payment session after charging completes
    /// </summary>
    /// <param name="sessionId">Charging session ID</param>
    /// <param name="totalEnergyUsed">Total energy consumed in kWh</param>
    /// <param name="totalAmountInXrp">Total payment amount in XRP</param>
    /// <returns>Finalized payment details</returns>
    Task<PaymentSession> FinalizePaymentSessionAsync(string sessionId, decimal totalEnergyUsed, decimal totalAmountInXrp);
    
    /// <summary>
    /// Gets payment history for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <returns>List of payment transactions</returns>
    Task<IEnumerable<PaymentTransaction>> GetPaymentHistoryAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, int limit = 50);
}

/// <summary>
/// Payment session for a charging session
/// </summary>
public class PaymentSession
{
    /// <summary>
    /// Unique payment session ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Related charging session ID
    /// </summary>
    public string ChargingSessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Charging station ID
    /// </summary>
    public string StationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Payment wallet address (Source address for this session)
    /// </summary>
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// Encrypted seed for the source wallet (temporary session wallet)
    /// </summary>
    public string EncryptedSourceWalletSeed { get; set; } = string.Empty;

    /// <summary>
    /// Destination wallet address (CPO's address)
    /// </summary>
    public string DestinationAddress { get; set; } = string.Empty;

    /// <summary>
    /// Session start time
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Session end time (null if ongoing)
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Total energy consumed in kWh
    /// </summary>
    public decimal TotalEnergyUsed { get; set; }
    
    /// <summary>
    /// Total amount paid in XRP
    /// </summary>
    public decimal TotalAmountPaid { get; set; }
    
    /// <summary>
    /// List of transaction hashes for this session
    /// </summary>
    public List<string> TransactionHashes { get; set; } = new();
    
    /// <summary>
    /// Current session status
    /// </summary>
    public PaymentSessionStatus Status { get; set; } = PaymentSessionStatus.Initialized;
}

/// <summary>
/// Status of a payment session
/// </summary>
public enum PaymentSessionStatus
{
    Initialized,
    Active,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Payment transaction
/// </summary>
public class PaymentTransaction
{
    /// <summary>
    /// Unique transaction ID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Related payment session ID
    /// </summary>
    public string PaymentSessionId { get; set; } = string.Empty;
    
    /// <summary>
    /// XRP Ledger transaction hash
    /// </summary>
    public string TransactionHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Sender wallet address
    /// </summary>
    public string SenderAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Receiver wallet address
    /// </summary>
    public string ReceiverAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Amount in XRP
    /// </summary>
    public decimal AmountInXrp { get; set; }
    
    /// <summary>
    /// Energy amount in kWh
    /// </summary>
    public decimal EnergyAmount { get; set; }
    
    /// <summary>
    /// Transaction timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Transaction status
    /// </summary>
    public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;
    
    /// <summary>
    /// Transaction type
    /// </summary>
    public PaymentTransactionType Type { get; set; }
    
    /// <summary>
    /// Optional transaction memo
    /// </summary>
    public string? Memo { get; set; }
}

/// <summary>
/// Status of a payment transaction
/// </summary>
public enum PaymentTransactionStatus
{
    Pending,
    Confirmed,
    Failed,
    Rejected
}

/// <summary>
/// Type of payment transaction
/// </summary>
public enum PaymentTransactionType
{
    Initialize,
    Micropayment,
    Finalize,
    Refund
}
