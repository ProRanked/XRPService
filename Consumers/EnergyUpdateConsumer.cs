using MassTransit;
using System.Diagnostics;
using XRPService.Events;
using XRPService.Services;

namespace XRPService.Consumers;

/// <summary>
/// Consumer for handling energy update events and processing micropayments
/// </summary>
public class EnergyUpdateConsumer : IConsumer<EnergyUpdateEvent>
{
    private readonly ILogger<EnergyUpdateConsumer> _logger;
    private readonly IPaymentService _paymentService;
    private readonly IBus _bus;
    private static readonly ActivitySource _activitySource = new("XRPService.MassTransit");

    public EnergyUpdateConsumer(
        ILogger<EnergyUpdateConsumer> logger,
        IPaymentService paymentService,
        IBus bus)
    {
        _logger = logger;
        _paymentService = paymentService;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<EnergyUpdateEvent> context)
    {
        var energyUpdate = context.Message;
        using var activity = _activitySource.StartActivity("Consume EnergyUpdateEvent");
        activity?.SetTag("charging_session_id", energyUpdate.ChargingSessionId);
        activity?.SetTag("payment_session_id", energyUpdate.PaymentSessionId);
        activity?.SetTag("energy_used", energyUpdate.EnergyUsed);
        activity?.SetTag("amount_in_xrp", energyUpdate.AmountInXrp);

        _logger.LogInformation("Processing energy update for session {SessionId}. Energy: {Energy} kWh, Amount: {Amount} XRP",
            energyUpdate.PaymentSessionId, energyUpdate.EnergyUsed, energyUpdate.AmountInXrp);

        try
        {
            // Process the micropayment
            var transaction = await _paymentService.ProcessMicropaymentAsync(
                energyUpdate.PaymentSessionId,
                energyUpdate.EnergyUsed,
                energyUpdate.AmountInXrp);

            // Publish a payment confirmed event
            if (transaction.Status == PaymentTransactionStatus.Confirmed)
            {
                var paymentConfirmedEvent = new PaymentConfirmedEvent
                {
                    PaymentSessionId = energyUpdate.PaymentSessionId,
                    ChargingSessionId = energyUpdate.ChargingSessionId,
                    UserId = energyUpdate.UserId,
                    StationId = energyUpdate.StationId,
                    TransactionId = transaction.Id,
                    TransactionHash = transaction.TransactionHash,
                    AmountInXrp = transaction.AmountInXrp,
                    EnergyAmount = transaction.EnergyAmount,
                    TotalEnergyUsed = energyUpdate.EnergyUsed, // This might need to be updated from the session
                    TotalAmountPaid = transaction.AmountInXrp, // This might need to be updated from the session
                    TransactionType = transaction.Type,
                    Timestamp = transaction.Timestamp
                };

                _logger.LogInformation("Publishing PaymentConfirmedEvent for transaction {TransactionId}", transaction.Id);
                await _bus.Publish(paymentConfirmedEvent);
            }
            else
            {
                // Handle failed payment
                var paymentFailedEvent = new PaymentFailedEvent
                {
                    PaymentSessionId = energyUpdate.PaymentSessionId,
                    ChargingSessionId = energyUpdate.ChargingSessionId,
                    UserId = energyUpdate.UserId,
                    StationId = energyUpdate.StationId,
                    TransactionId = transaction.Id,
                    TransactionHash = transaction.TransactionHash,
                    AmountInXrp = transaction.AmountInXrp,
                    EnergyAmount = transaction.EnergyAmount,
                    ErrorMessage = "Transaction failed or was rejected",
                    ErrorCode = transaction.Status.ToString(),
                    RetryCount = 0,
                    ShouldRetry = true,
                    Timestamp = transaction.Timestamp
                };

                _logger.LogWarning("Publishing PaymentFailedEvent for transaction {TransactionId}", transaction.Id);
                await _bus.Publish(paymentFailedEvent);
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing energy update for session {SessionId}", energyUpdate.PaymentSessionId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            // Publish a payment failed event
            var paymentFailedEvent = new PaymentFailedEvent
            {
                PaymentSessionId = energyUpdate.PaymentSessionId,
                ChargingSessionId = energyUpdate.ChargingSessionId,
                UserId = energyUpdate.UserId,
                StationId = energyUpdate.StationId,
                TransactionId = Guid.NewGuid().ToString(), // Generate a placeholder ID
                TransactionHash = string.Empty,
                AmountInXrp = energyUpdate.AmountInXrp,
                EnergyAmount = energyUpdate.EnergyUsed,
                ErrorMessage = ex.Message,
                ErrorCode = "PROCESSING_ERROR",
                RetryCount = 0,
                ShouldRetry = true,
                Timestamp = DateTime.UtcNow
            };

            await _bus.Publish(paymentFailedEvent);
        }
    }
}