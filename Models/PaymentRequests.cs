namespace XRPService.Features.Payments;

/// <summary>
/// Request to initialize a payment session
/// </summary>
public class InitializePaymentRequest
{
    /// <summary>
    /// ID of the charging session
    /// </summary>
    public required string ChargingSessionId { get; set; }
    
    /// <summary>
    /// ID of the user
    /// </summary>
    public required string UserId { get; set; }
    
    /// <summary>
    /// ID of the charging station
    /// </summary>
    public required string StationId { get; set; }
}

/// <summary>
/// Response for initializing a payment session
/// </summary>
public class InitializePaymentResponse
{
    /// <summary>
    /// ID of the payment session
    /// </summary>
    public required string PaymentSessionId { get; set; }
    
    /// <summary>
    /// Wallet address for the payment
    /// </summary>
    public required string WalletAddress { get; set; }
    
    /// <summary>
    /// Time the session was started
    /// </summary>
    public DateTime StartTime { get; set; }
}

/// <summary>
/// Request to process a micropayment
/// </summary>
public class ProcessMicropaymentRequest
{
    /// <summary>
    /// ID of the payment session
    /// </summary>
    public required string PaymentSessionId { get; set; }
    
    /// <summary>
    /// Energy used in kWh
    /// </summary>
    public decimal EnergyUsed { get; set; }
    
    /// <summary>
    /// Amount to pay in XRP
    /// </summary>
    public decimal AmountInXrp { get; set; }
}

/// <summary>
/// Response for processing a micropayment
/// </summary>
public class ProcessMicropaymentResponse
{
    /// <summary>
    /// ID of the transaction
    /// </summary>
    public required string TransactionId { get; set; }
    
    /// <summary>
    /// Hash of the transaction on the XRP Ledger
    /// </summary>
    public required string TransactionHash { get; set; }
    
    /// <summary>
    /// Status of the transaction
    /// </summary>
    public required string Status { get; set; }
    
    /// <summary>
    /// Time the transaction was processed
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Request to finalize a payment session
/// </summary>
public class FinalizePaymentRequest
{
    /// <summary>
    /// Total energy used in kWh
    /// </summary>
    public decimal TotalEnergyUsed { get; set; }
    
    /// <summary>
    /// Total amount to pay in XRP
    /// </summary>
    public decimal TotalAmountInXrp { get; set; }
}

/// <summary>
/// Response for finalizing a payment session
/// </summary>
public class FinalizePaymentResponse
{
    /// <summary>
    /// ID of the payment session
    /// </summary>
    public required string PaymentSessionId { get; set; }
    
    /// <summary>
    /// Total energy used in kWh
    /// </summary>
    public decimal TotalEnergyUsed { get; set; }
    
    /// <summary>
    /// Total amount paid in XRP
    /// </summary>
    public decimal TotalAmountPaid { get; set; }
    
    /// <summary>
    /// Time the session was finalized
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// Status of the payment session
    /// </summary>
    public required string Status { get; set; }
    
    /// <summary>
    /// List of transaction hashes
    /// </summary>
    public required List<string> TransactionHashes { get; set; }
}

/// <summary>
/// Response for a payment transaction
/// </summary>
public class PaymentTransactionResponse
{
    /// <summary>
    /// ID of the transaction
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// Hash of the transaction on the XRP Ledger
    /// </summary>
    public required string TransactionHash { get; set; }
    
    /// <summary>
    /// Amount paid in XRP
    /// </summary>
    public decimal AmountInXrp { get; set; }
    
    /// <summary>
    /// Energy amount in kWh
    /// </summary>
    public decimal EnergyAmount { get; set; }
    
    /// <summary>
    /// Time the transaction was processed
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Status of the transaction
    /// </summary>
    public required string Status { get; set; }
    
    /// <summary>
    /// Type of the transaction
    /// </summary>
    public required string Type { get; set; }
}

/// <summary>
/// Response for wallet information
/// </summary>
public class WalletInfoResponse
{
    /// <summary>
    /// Wallet address
    /// </summary>
    public required string Address { get; set; }
    
    /// <summary>
    /// Balance in XRP
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Account sequence number
    /// </summary>
    public int Sequence { get; set; }
}