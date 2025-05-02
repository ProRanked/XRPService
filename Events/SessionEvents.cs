using XRPService.Services;

namespace XRPService.Events;

/// <summary>
/// Event triggered when a charging session is finalized
/// </summary>
public class SessionFinalizedEvent
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
    /// Total energy used in kWh
    /// </summary>
    public decimal TotalEnergyUsed { get; set; }
    
    /// <summary>
    /// Total amount paid in XRP
    /// </summary>
    public decimal TotalAmountPaid { get; set; }
    
    /// <summary>
    /// Session start time
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Session end time
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Session status
    /// </summary>
    public PaymentSessionStatus Status { get; set; }
    
    /// <summary>
    /// List of transaction hashes
    /// </summary>
    public List<string> TransactionHashes { get; set; } = new();
    
    /// <summary>
    /// Timestamp of the finalization
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}