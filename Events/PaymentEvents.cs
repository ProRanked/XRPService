using XRPService.Services;

namespace XRPService.Events;

/// <summary>
/// Base class for payment-related events
/// </summary>
public abstract class PaymentEventBase
{
    /// <summary>
    /// ID of the payment session
    /// </summary>
    public required string PaymentSessionId { get; set; }
    
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
    
    /// <summary>
    /// ID of the transaction
    /// </summary>
    public required string TransactionId { get; set; }
    
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
    /// Timestamp of the transaction
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event triggered when a payment is confirmed on the XRP Ledger
/// </summary>
public class PaymentConfirmedEvent : PaymentEventBase
{
    /// <summary>
    /// Total energy used in the session so far
    /// </summary>
    public decimal TotalEnergyUsed { get; set; }
    
    /// <summary>
    /// Total amount paid in the session so far
    /// </summary>
    public decimal TotalAmountPaid { get; set; }
    
    /// <summary>
    /// Type of the transaction
    /// </summary>
    public PaymentTransactionType TransactionType { get; set; }
}

/// <summary>
/// Event triggered when a payment fails on the XRP Ledger
/// </summary>
public class PaymentFailedEvent : PaymentEventBase
{
    /// <summary>
    /// Error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// Error code
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Retry count
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// Whether the payment should be retried
    /// </summary>
    public bool ShouldRetry { get; set; }
}