namespace XRPService.Events;

/// <summary>
/// Event triggered when energy consumption is updated during a charging session
/// </summary>
public class EnergyUpdateEvent
{
    /// <summary>
    /// ID of the charging session
    /// </summary>
    public required string ChargingSessionId { get; set; }
    
    /// <summary>
    /// ID of the payment session
    /// </summary>
    public required string PaymentSessionId { get; set; }
    
    /// <summary>
    /// ID of the user
    /// </summary>
    public required string UserId { get; set; }
    
    /// <summary>
    /// ID of the charging station
    /// </summary>
    public required string StationId { get; set; }
    
    /// <summary>
    /// Energy consumed so far in kWh
    /// </summary>
    public decimal EnergyUsed { get; set; }
    
    /// <summary>
    /// Amount to pay in XRP for this energy update
    /// </summary>
    public decimal AmountInXrp { get; set; }
    
    /// <summary>
    /// Timestamp of the energy update
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}