using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using XRPService.Features.Payments;
using XRPService.Services;

namespace XRPService.Features.Wallets;

/// <summary>
/// XRP wallet management endpoints
/// </summary>
public static class WalletEndpoints
{
    private static readonly ActivitySource ActivitySource = new("XRPService.Wallets");

    /// <summary>
    /// Maps the wallet endpoints to the application
    /// </summary>
    /// <param name="app">Web application to configure</param>
    /// <returns>The configured web application</returns>
    public static WebApplication MapWalletEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/wallets")
            .WithTags("Wallets")
            .WithOpenApi();
        
        // Create a new wallet
        group.MapPost("/", async (
            [FromServices] IWalletService walletService,
            HttpContext context) =>
        {
            using var activity = ActivitySource.StartActivity("API: Create Wallet");
            
            try
            {
                var wallet = await walletService.CreateWalletAsync();
                
                activity?.SetStatus(ActivityStatusCode.Ok);
                return Results.Ok(new CreateWalletResponse
                {
                    Address = wallet.Address,
                    Seed = wallet.Seed,
                    Balance = wallet.Balance
                });
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                return Results.BadRequest(new { error = ex.Message });
            }
        });
        
        // Get wallet information
        group.MapGet("/{address}", async (
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
        
        // Fund test wallet (testnet/devnet only)
        group.MapPost("/{address}/fund", async (
            string address,
            [FromServices] IWalletService walletService,
            HttpContext context) =>
        {
            using var activity = ActivitySource.StartActivity("API: Fund Test Wallet");
            activity?.SetTag("wallet_address", address);
            
            try
            {
                var result = await walletService.FundTestWalletAsync(address);
                
                activity?.SetStatus(ActivityStatusCode.Ok);
                return Results.Ok(result);
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

/// <summary>
/// Response for creating a new wallet
/// </summary>
public class CreateWalletResponse
{
    /// <summary>
    /// Wallet address
    /// </summary>
    public required string Address { get; set; }
    
    /// <summary>
    /// Wallet seed (private, should be kept secure)
    /// </summary>
    public string? Seed { get; set; }
    
    /// <summary>
    /// Initial balance in XRP
    /// </summary>
    public decimal Balance { get; set; }
}