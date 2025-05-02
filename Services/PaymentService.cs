using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XRPService.Events;

namespace XRPService.Services;

public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly IWalletService _walletService;
    private readonly IXRPLService _xrplService;
    private readonly IBus _bus;
    private static readonly ActivitySource _activitySource = new("XRPService.Payments");
    // TODO: Inject database context/repository for storing sessions/transactions
    // TODO: Inject mechanism to resolve CPO destination address (e.g., IConfiguration, dedicated service)
    // TODO: Inject encryption service for wallet seeds

    // Temporary in-memory store for sessions - replace with proper persistence
    private static readonly Dictionary<string, PaymentSession> _paymentSessions = new();
    // Temporary in-memory store for transactions - replace with proper persistence
    private static readonly List<PaymentTransaction> _paymentTransactions = new();

    public PaymentService(
        ILogger<PaymentService> logger,
        IWalletService walletService,
        IXRPLService xrplService,
        IBus bus)
    {
        _logger = logger;
        _walletService = walletService;
        _xrplService = xrplService;
        _bus = bus;
    }

    public async Task<PaymentSession> InitializePaymentSessionAsync(string chargingSessionId, string userId, string stationId)
    {
        _logger.LogInformation("Initializing payment session for charging session {ChargingSessionId}, Station {StationId}", chargingSessionId, stationId);

        // 1. Generate a new temporary wallet for this session
        var walletInfo = await _walletService.CreateWalletAsync();
        if (walletInfo?.Seed == null)
        {
            _logger.LogError("Failed to create wallet for session {ChargingSessionId}", chargingSessionId);
            throw new InvalidOperationException("Could not create wallet for payment session.");
        }
        _logger.LogInformation("Created temporary wallet {WalletAddress} for session {ChargingSessionId}", walletInfo.Address, chargingSessionId);

        // 2. TODO: Resolve CPO destination address based on stationId
        string destinationAddress = "r..."; // Placeholder CPO address
        _logger.LogInformation("Resolved destination address {DestinationAddress} for station {StationId}", destinationAddress, stationId);


        // 3. TODO: Encrypt the wallet seed before storing
        string encryptedSeed = walletInfo.Seed; // Placeholder - store raw seed temporarily
        _logger.LogWarning("Storing wallet seed without encryption (placeholder) for session {ChargingSessionId}", chargingSessionId);


        // 4. Create and store the payment session details
        var paymentSession = new PaymentSession
        {
            Id = Guid.NewGuid().ToString(), // Generate unique ID for this payment session
            ChargingSessionId = chargingSessionId,
            UserId = userId,
            StationId = stationId,
            WalletAddress = walletInfo.Address, // This is the source wallet
            EncryptedSourceWalletSeed = encryptedSeed,
            DestinationAddress = destinationAddress,
            StartTime = DateTime.UtcNow,
            Status = PaymentSessionStatus.Initialized
        };

        // Store session (replace with DB)
        _paymentSessions[paymentSession.Id] = paymentSession;

        _logger.LogInformation("Payment session {PaymentSessionId} initialized successfully for charging session {ChargingSessionId}", paymentSession.Id, chargingSessionId);

        // Return only non-sensitive info
        return new PaymentSession
        {
            Id = paymentSession.Id,
            ChargingSessionId = paymentSession.ChargingSessionId,
            UserId = paymentSession.UserId,
            StationId = paymentSession.StationId,
            WalletAddress = paymentSession.WalletAddress, // Return source address
            DestinationAddress = paymentSession.DestinationAddress,
            StartTime = paymentSession.StartTime,
            Status = paymentSession.Status
            // DO NOT return the seed
        };
    }

    public async Task<PaymentTransaction> ProcessMicropaymentAsync(string paymentSessionId, decimal energyUsed, decimal amountInXrp)
    {
        _logger.LogInformation("Processing micropayment for session {PaymentSessionId}, Amount: {Amount} XRP", paymentSessionId, amountInXrp);

        // 1. Retrieve the payment session (replace with DB lookup)
        if (!_paymentSessions.TryGetValue(paymentSessionId, out var session))
        {
            _logger.LogError("Payment session {PaymentSessionId} not found for micropayment.", paymentSessionId);
            throw new KeyNotFoundException($"Payment session {paymentSessionId} not found.");
        }

        if (session.Status != PaymentSessionStatus.Initialized && session.Status != PaymentSessionStatus.Active)
        {
             _logger.LogWarning("Micropayment attempted on session {PaymentSessionId} with status {Status}", paymentSessionId, session.Status);
             throw new InvalidOperationException($"Payment session {paymentSessionId} is not in an active state ({session.Status}).");
        }

        // 2. TODO: Decrypt the source wallet seed
        string sourceWalletSeed = session.EncryptedSourceWalletSeed; // Placeholder
        if (string.IsNullOrEmpty(sourceWalletSeed))
        {
             _logger.LogError("Source wallet seed is missing for session {PaymentSessionId}", paymentSessionId);
             throw new InvalidOperationException("Cannot process payment: source wallet seed is missing.");
        }

        // 3. Submit payment to XRPL
        string transactionHash;
        try
        {
            var memo = $"Phevnix Charge: Session {session.ChargingSessionId}, Energy: {energyUsed}kWh";
            transactionHash = await _xrplService.SubmitPaymentAsync(
                sourceWalletSeed,
                session.DestinationAddress,
                amountInXrp,
                memo);

             _logger.LogInformation("Submitted micropayment transaction {TransactionHash} for session {PaymentSessionId}", transactionHash, paymentSessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit micropayment transaction for session {PaymentSessionId}", paymentSessionId);
            // TODO: Handle specific XRPL exceptions, update session status?
            throw; // Re-throw for now
        }

        // 4. Create and store transaction record (replace with DB)
        var transaction = new PaymentTransaction
        {
            Id = Guid.NewGuid().ToString(),
            PaymentSessionId = paymentSessionId,
            TransactionHash = transactionHash,
            SenderAddress = session.WalletAddress,
            ReceiverAddress = session.DestinationAddress,
            AmountInXrp = amountInXrp,
            EnergyAmount = energyUsed, // Store energy associated with this specific payment
            Timestamp = DateTime.UtcNow,
            Status = PaymentTransactionStatus.Confirmed, // Assuming transaction is immediately confirmed for now
            Type = PaymentTransactionType.Micropayment,
            Memo = $"Energy: {energyUsed}kWh"
        };
        
        // Store transaction in memory (replace with DB)
        _paymentTransactions.Add(transaction);

        // 5. Update session state (replace with DB update)
        session.Status = PaymentSessionStatus.Active;
        session.TotalAmountPaid += amountInXrp;
        session.TotalEnergyUsed += energyUsed; // Accumulate energy for the session
        session.TransactionHashes.Add(transactionHash);
        _paymentSessions[paymentSessionId] = session; // Update in-memory store

        _logger.LogInformation("Micropayment processed for session {PaymentSessionId}. Transaction: {TransactionHash}", paymentSessionId, transactionHash);

        // Publish PaymentConfirmedEvent
        try
        {
            var paymentConfirmedEvent = new PaymentConfirmedEvent
            {
                PaymentSessionId = paymentSessionId,
                ChargingSessionId = session.ChargingSessionId,
                UserId = session.UserId,
                StationId = session.StationId,
                TransactionId = transaction.Id,
                TransactionHash = transaction.TransactionHash,
                AmountInXrp = transaction.AmountInXrp,
                EnergyAmount = transaction.EnergyAmount,
                TotalEnergyUsed = session.TotalEnergyUsed,
                TotalAmountPaid = session.TotalAmountPaid,
                TransactionType = transaction.Type,
                Timestamp = transaction.Timestamp
            };

            _logger.LogInformation("Publishing PaymentConfirmedEvent for transaction {TransactionId}", transaction.Id);
            await _bus.Publish(paymentConfirmedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish PaymentConfirmedEvent for transaction {TransactionId}", transaction.Id);
            // Continue execution - don't let the event publishing failure affect the API response
        }

        return transaction; // Return details of this specific transaction
    }

    public async Task<PaymentSession> FinalizePaymentSessionAsync(string paymentSessionId, decimal totalEnergyUsed, decimal totalAmountInXrp)
    {
        _logger.LogInformation("Finalizing payment session {PaymentSessionId}", paymentSessionId);

        // 1. Retrieve the payment session (replace with DB lookup)
         if (!_paymentSessions.TryGetValue(paymentSessionId, out var session))
        {
            _logger.LogError("Payment session {PaymentSessionId} not found for finalization.", paymentSessionId);
            throw new KeyNotFoundException($"Payment session {paymentSessionId} not found.");
        }

        if (session.Status == PaymentSessionStatus.Completed || session.Status == PaymentSessionStatus.Failed || session.Status == PaymentSessionStatus.Cancelled)
        {
             _logger.LogWarning("Finalization attempted on already finalized session {PaymentSessionId} with status {Status}", paymentSessionId, session.Status);
             return session; // Or throw error? Return current state for now.
        }

        // 2. TODO: Perform final checks/calculations if needed (e.g., compare totalAmountInXrp with session.TotalAmountPaid)
        _logger.LogInformation("Session {PaymentSessionId} finalization check. Reported Total XRP: {ReportedTotal}, Calculated Total XRP: {CalculatedTotal}",
            paymentSessionId, totalAmountInXrp, session.TotalAmountPaid);

        // 3. Update session state (replace with DB update)
        session.EndTime = DateTime.UtcNow;
        session.Status = PaymentSessionStatus.Completed; // Assuming successful finalization
        session.TotalEnergyUsed = totalEnergyUsed; // Update with final reported energy
        // Potentially adjust session.TotalAmountPaid based on final calculation or reconciliation
        _paymentSessions[paymentSessionId] = session; // Update in-memory store

        // 4. TODO: Consider archiving or securely deleting the temporary wallet seed now that the session is complete.

        _logger.LogInformation("Payment session {PaymentSessionId} finalized successfully.", paymentSessionId);

        // Publish SessionFinalizedEvent
        try
        {
            var sessionFinalizedEvent = new SessionFinalizedEvent
            {
                PaymentSessionId = session.Id,
                ChargingSessionId = session.ChargingSessionId,
                UserId = session.UserId,
                StationId = session.StationId,
                TotalEnergyUsed = session.TotalEnergyUsed,
                TotalAmountPaid = session.TotalAmountPaid,
                StartTime = session.StartTime,
                EndTime = session.EndTime.Value,
                Status = session.Status,
                TransactionHashes = session.TransactionHashes,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Publishing SessionFinalizedEvent for session {SessionId}", session.Id);
            await _bus.Publish(sessionFinalizedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish SessionFinalizedEvent for session {SessionId}", session.Id);
            // Continue execution - don't let the event publishing failure affect the API response
        }

        // Return the final state of the session (without sensitive info)
        return new PaymentSession
        {
            Id = session.Id,
            ChargingSessionId = session.ChargingSessionId,
            UserId = session.UserId,
            StationId = session.StationId,
            WalletAddress = session.WalletAddress,
            DestinationAddress = session.DestinationAddress,
            StartTime = session.StartTime,
            EndTime = session.EndTime,
            Status = session.Status,
            TotalAmountPaid = session.TotalAmountPaid,
            TotalEnergyUsed = session.TotalEnergyUsed,
            TransactionHashes = session.TransactionHashes
            // DO NOT return the seed
        };
    }

    public Task<IEnumerable<PaymentTransaction>> GetPaymentHistoryAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null, int limit = 50)
    {
        using var activity = _activitySource.StartActivity("GetPaymentHistory");
        activity?.SetTag("user_id", userId);
        activity?.SetTag("limit", limit);
        if (fromDate.HasValue) activity?.SetTag("from_date", fromDate.Value.ToString("o"));
        if (toDate.HasValue) activity?.SetTag("to_date", toDate.Value.ToString("o"));

        _logger.LogInformation("Getting payment history for user {UserId}", userId);
        
        // Get session IDs for this user
        var userSessionIds = _paymentSessions.Values
            .Where(s => s.UserId == userId)
            .Select(s => s.Id)
            .ToList();
        
        // Filter transactions by user's sessions, date range, and limit
        var query = _paymentTransactions
            .Where(t => userSessionIds.Contains(t.PaymentSessionId));
            
        if (fromDate.HasValue)
            query = query.Where(t => t.Timestamp >= fromDate.Value);
            
        if (toDate.HasValue)
            query = query.Where(t => t.Timestamp <= toDate.Value);
            
        var history = query
            .OrderByDescending(t => t.Timestamp)
            .Take(limit)
            .ToList();
            
        _logger.LogInformation("Found {Count} transactions for user {UserId}", history.Count, userId);
        activity?.SetStatus(ActivityStatusCode.Ok);
        
        return Task.FromResult(history.AsEnumerable());
    }
}
