using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using XRPService.Services;

namespace XRPService.Features.Payments;

/// <summary>
/// XRP payment endpoints for EV charging
/// </summary>
public static class PaymentsEndpoints
{
    private static readonly ActivitySource ActivitySource = new("XRPService.Payments");

    /// <summary>
    /// Maps the payment endpoints to the application
    /// </summary>
    /// <param name="app">Web application to configure</param>
    /// <returns>The configured web application</returns>
    public static WebApplication MapPaymentEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/payments")
            .WithTags("Payments")
            .WithOpenApi();
        
        // Initialize a payment session
        group.MapPost("/sessions", async (
            [FromBody] InitializePaymentRequest request,
            [FromServices] IPaymentService paymentService,
            HttpContext context) =>
        {
            using var activity = ActivitySource.StartActivity("API: Initialize Payment Session");
            activity?.SetTag("charging_session_id", request.ChargingSessionId);
            activity?.SetTag("user_id", request.UserId);
            
            try
            {
                var session = await paymentService.InitializePaymentSessionAsync(
                    request.ChargingSessionId,
                    request.UserId,
                    request.StationId);
                
                activity?.SetStatus(ActivityStatusCode.Ok);
                return Results.Ok(new InitializePaymentResponse
                {
                    PaymentSessionId = session.Id,
                    WalletAddress = session.WalletAddress,
                    StartTime = session.StartTime
                });
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return Results.BadRequest(new { error = ex.Message });
            }
        });
        
        // Process a micropayment
        group.MapPost("/micropayments", async (
            [FromBody] ProcessMicropaymentRequest request,
            [FromServices] IPaymentService paymentService,
            HttpContext context) =>
        {
            using var activity = ActivitySource.StartActivity("API: Process Micropayment");
            activity?.SetTag("payment_session_id", request.PaymentSessionId);
            activity?.SetTag("energy_used", request.EnergyUsed);
            
            try
            {
                var transaction = await paymentService.ProcessMicropaymentAsync(
                    request.PaymentSessionId,
                    request.EnergyUsed,
                    request.AmountInXrp);
                
                activity?.SetStatus(ActivityStatusCode.Ok);
                return Results.Ok(new ProcessMicropaymentResponse
                {
                    TransactionId = transaction.Id,
                    TransactionHash = transaction.TransactionHash,
                    Status = transaction.Status.ToString(),
                    Timestamp = transaction.Timestamp
                });
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return Results.BadRequest(new { error = ex.Message });
            }
        });
        
        // Finalize a payment session
        group.MapPost("/sessions/{sessionId}/finalize", async (
            string sessionId,
            [FromBody] FinalizePaymentRequest request,
            [FromServices] IPaymentService paymentService,
            HttpContext context) =>
        {
            using var activity = ActivitySource.StartActivity("API: Finalize Payment Session");
            activity?.SetTag("payment_session_id", sessionId);
            activity?.SetTag("total_energy", request.TotalEnergyUsed);
            
            try
            {
                var session = await paymentService.FinalizePaymentSessionAsync(
                    sessionId,
                    request.TotalEnergyUsed,
                    request.TotalAmountInXrp);
                
                activity?.SetStatus(ActivityStatusCode.Ok);
                return Results.Ok(new FinalizePaymentResponse
                {
                    PaymentSessionId = session.Id,
                    TotalEnergyUsed = session.TotalEnergyUsed,
                    TotalAmountPaid = session.TotalAmountPaid,
                    EndTime = session.EndTime,
                    Status = session.Status.ToString(),
                    TransactionHashes = session.TransactionHashes
                });
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return Results.BadRequest(new { error = ex.Message });
            }
        });
        
        // Get payment history
        group.MapGet("/history/{userId}", async (
            string userId,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int limit,
            [FromServices] IPaymentService paymentService,
            HttpContext context) =>
        {
            using var activity = ActivitySource.StartActivity("API: Get Payment History");
            activity?.SetTag("user_id", userId);
            
            try
            {
                var transactions = await paymentService.GetPaymentHistoryAsync(
                    userId,
                    fromDate,
                    toDate,
                    limit > 0 ? limit : 50);
                
                var response = transactions.Select(t => new PaymentTransactionResponse
                {
                    Id = t.Id,
                    TransactionHash = t.TransactionHash,
                    AmountInXrp = t.AmountInXrp,
                    EnergyAmount = t.EnergyAmount,
                    Timestamp = t.Timestamp,
                    Status = t.Status.ToString(),
                    Type = t.Type.ToString()
                });
                
                activity?.SetStatus(ActivityStatusCode.Ok);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return Results.BadRequest(new { error = ex.Message });
            }
        });
        
        // Get wallet information
        group.MapGet("/wallets/{address}", async (
            string address,
            [FromServices] IWalletService walletService,
            HttpContext context) =>
        {
            using var activity = ActivitySource.StartActivity("API: Get Wallet Info");
            activity?.SetTag("wallet_address", address);
            
            try
            {
                var walletInfo = await walletService.GetWalletInfoAsync(address);
                
                activity?.SetStatus(ActivityStatusCode.Ok);
                return Results.Ok(new WalletInfoResponse
                {
                    Address = walletInfo.Address,
                    Balance = walletInfo.Balance,
                    Sequence = walletInfo.Sequence
                });
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return Results.BadRequest(new { error = ex.Message });
            }
        });
        
        return app;
    }
}