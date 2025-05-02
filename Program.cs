using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using XRPService.Consumers;
using XRPService.Events;
using XRPService.Features.Payments;
using XRPService.Features.Wallets;
using XRPService.Services;

var builder = WebApplication.CreateBuilder(args);
var instanceId = Guid.NewGuid().ToString("N")[..8];
Console.WriteLine($"Starting XRPService with instance ID: {instanceId}");

// Configure services
ConfigureOpenTelemetry(builder.Services, builder.Environment.EnvironmentName, instanceId, builder.Configuration);
ConfigureServices(builder.Services);
ConfigureMassTransit(builder.Services, builder.Configuration, instanceId);

var app = builder.Build();

// Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.MapPaymentEndpoints();
app.MapWalletEndpoints();
app.MapHealthChecks("/health");

app.Run();

// Configuration methods
void ConfigureOpenTelemetry(IServiceCollection services, string environmentName, string serviceInstanceId, IConfiguration configuration)
{
    services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService("XRPService")
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environmentName,
                ["service.instance.id"] = serviceInstanceId
            }))
        .WithTracing(tracing => tracing
            .AddSource("XRPService")
            .AddSource("XRPService.Payments")
            .AddSource("XRPService.Wallets")
            .AddSource("XRPService.MassTransit")
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options => options.Endpoint = new Uri(
                configuration["OpenTelemetry:OtlpExporter:Endpoint"] ?? "http://localhost:4317"))
            .AddConsoleExporter())
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(options => options.Endpoint = new Uri(
                configuration["OpenTelemetry:OtlpExporter:Endpoint"] ?? "http://localhost:4317"))
            .AddConsoleExporter());
}

void ConfigureServices(IServiceCollection services)
{
    // API and documentation
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    
    // HttpClient for XRPL
    services.AddHttpClient("XRPL", client => 
        client.DefaultRequestHeaders.Add("Accept", "application/json"));
    
    // XRPL Services
    services.AddSingleton<IXRPLService, XRPLService>();
    services.AddScoped<IWalletService, WalletService>();
    services.AddScoped<IPaymentService, PaymentService>();
    
    // Health checks
    services.AddHealthChecks()
        .AddCheck("self", () => HealthCheckResult.Healthy())
        .AddCheck("xrpledger", () => HealthCheckResult.Healthy());
}

void ConfigureMassTransit(IServiceCollection services, IConfiguration configuration, string serviceInstanceId)
{
    services.AddMassTransit(x =>
    {
        // Register specific consumers
        x.AddConsumer<EnergyUpdateConsumer>();
        
        // Register all consumers in the assembly
        x.AddConsumers(Assembly.GetExecutingAssembly());

        x.UsingAzureServiceBus((context, cfg) =>
        {
            cfg.Host(configuration.GetConnectionString("ServiceBus"));
            
            // Add middleware to propagate activity context
            cfg.ConfigureSend(sendCfg =>
            {
                sendCfg.UseExecuteAsync(_ => Task.CompletedTask);
            });
            
            // Configure message types
            cfg.Message<EnergyUpdateEvent>(m => m.SetEntityName("energy-updates"));
            cfg.Message<PaymentConfirmedEvent>(m => m.SetEntityName("payment-confirmations"));
            cfg.Message<PaymentFailedEvent>(m => m.SetEntityName("payment-failures"));
            cfg.Message<SessionFinalizedEvent>(m => m.SetEntityName("session-finalizations"));
            
            // Configure endpoint naming with instance ID
            cfg.ConfigureEndpoints(context, 
                new DefaultEndpointNameFormatter($"xrp-{serviceInstanceId}-", false));
        });
    });
}