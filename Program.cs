using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using XRPService.Features.Payments;
using XRPService.Services;

var builder = WebApplication.CreateBuilder(args);

// Generate unique instance ID for this service instance
var instanceId = Guid.NewGuid().ToString("N").Substring(0, 8);
Console.WriteLine($"Starting XRPService with instance ID: {instanceId}");

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService("XRPService")
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["service.instance.id"] = instanceId
        }))
    .WithTracing(tracing => tracing
        .AddSource("XRPService")
        .AddSource("XRPService.Payments")
        .AddSource("XRPService.MassTransit")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpExporter:Endpoint"] ?? "http://localhost:4317"))
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpExporter:Endpoint"] ?? "http://localhost:4317"))
        .AddConsoleExporter());

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// XRPL Services
builder.Services.AddSingleton<IXRPLService, XRPLService>();
builder.Services.AddSingleton<IWalletService, WalletService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Configure MassTransit
builder.Services.AddMassTransit(x =>
{
    // Add OpenTelemetry tracing
    x.AddOpenTelemetryTracing();
    
    // Register consumers
    x.AddConsumers(Assembly.GetExecutingAssembly());

    x.UsingAzureServiceBus((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("ServiceBus"));
        
        // Add middleware to propagate activity context
        cfg.ConfigureSend(sendCfg =>
        {
            sendCfg.UseExecuteAsync(context =>
            {
                // Propagate the current activity context
                if (context.TryGetPayload(out MessageSendContext? sendContext))
                {
                    // Logic to propagate trace context
                }
                return Task.CompletedTask;
            });
        });
        
        // Configure endpoint naming with instance ID
        cfg.ConfigureEndpoints(context, 
            new DefaultEndpointNameFormatter($"xrp-{instanceId}-", false));
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddCheck("xrpledger", () => HealthCheckResult.Healthy());

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapPaymentEndpoints();

// Health check endpoint
app.MapHealthChecks("/health");

app.Run();