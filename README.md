# XRPService

## Overview

XRPService is a microservice that enables XRP Ledger (XRPL) integration for EV charging payments. It provides functionality for:

- Processing real-time micropayments for EV charging sessions
- Managing XRP wallets for payment processing
- Interacting with the XRP Ledger
- Tracking payment transactions on the blockchain

## Features

- **Payment Processing**: Process micropayments in XRP for ongoing charging sessions
- **Wallet Management**: Create and manage XRP wallets for users and stations
- **Blockchain Integration**: Connect with the XRP Ledger for transaction processing
- **Transaction History**: Track and retrieve payment history for users
- **Distributed Tracing**: OpenTelemetry integration for monitoring payment flows

## Architecture

The service follows the same architectural patterns as other microservices in the system:

- **OpenTelemetry**: End-to-end tracing and metrics
- **MassTransit**: Event-based messaging
- **Minimal API**: Modern ASP.NET Core endpoints
- **Clean Architecture**: Separation of concerns into services, features, and models

## Workflow Diagram

This diagram illustrates the typical payment session lifecycle involving XRPService and other components:

```mermaid
sequenceDiagram
    participant CPO/EVSE as CPOService/EVSEService
    participant LoadBalancer
    participant XRPServiceAPI as XRPService (API)
    participant XRPServiceBus as XRPService (Bus)
    participant PaymentSvc as PaymentService
    participant WalletSvc as WalletService
    participant XRPLSvc as XRPLService
    participant XRPL as XRP Ledger
    participant OtherServices as Other Services (Ops, PayTerminal, CPO)

    Note over CPO/EVSE, XRPServiceAPI: Charging Session Starts
    CPO/EVSE->>+LoadBalancer: Request Start Charging (UserId, StationId)
    LoadBalancer->>+XRPServiceAPI: POST /api/payments/sessions (InitializePaymentRequest)
    XRPServiceAPI->>+PaymentSvc: InitializePaymentSessionAsync(sessionId, userId, stationId)
    PaymentSvc->>+WalletSvc: CreateWalletAsync() # Generate temporary session wallet
    WalletSvc-->>-PaymentSvc: WalletInfo (Address, Seed)
    PaymentSvc->>+OtherServices: Get CPO Destination Address(stationId) # Query CPOService or lookup config
    OtherServices-->>-PaymentSvc: CPO Destination Address
    PaymentSvc->>PaymentSvc: Store PaymentSession (Status: Initialized, EncryptedSeed, DestinationAddress)
    PaymentSvc-->>-XRPServiceAPI: PaymentSession (Id, SourceWalletAddress) # Return source address for user payment
    XRPServiceAPI-->>-LoadBalancer: 200 OK (InitializePaymentResponse)
    LoadBalancer-->>-CPO/EVSE: Session Info (PaymentSessionId, SourceWalletAddress)

    Note over CPO/EVSE, XRPServiceBus: During Charging (Micropayments)
    CPO/EVSE->>+XRPServiceBus: Publish EnergyUpdateEvent (SessionId, EnergyUsed)
    XRPServiceBus->>+PaymentSvc: Handle(EnergyUpdateEvent)
    PaymentSvc->>PaymentSvc: Calculate XRP Amount
    PaymentSvc->>PaymentSvc: Retrieve DecryptedSeed, DestinationAddress from PaymentSession
    PaymentSvc->>+XRPLSvc: SubmitPaymentAsync(DecryptedSeed, DestinationAddress, Amount, Memo) # Pay to CPO address
    XRPLSvc->>+XRPL: Submit Transaction
    XRPL-->>-XRPLSvc: Transaction Hash/Status
    XRPLSvc-->>-PaymentSvc: Transaction Result
    alt Transaction Successful
        PaymentSvc->>PaymentSvc: Store PaymentTransaction (Status: Confirmed)
        PaymentSvc->>PaymentSvc: Update PaymentSession (TotalPaid, Status: Active)
        PaymentSvc->>+XRPServiceBus: Publish PaymentConfirmedEvent
        XRPServiceBus-->>-OtherServices: Notify Payment
    else Transaction Failed
        PaymentSvc->>PaymentSvc: Store PaymentTransaction (Status: Failed)
        PaymentSvc->>+XRPServiceBus: Publish PaymentFailedEvent
        XRPServiceBus-->>-OtherServices: Notify Failure
    end
    PaymentSvc-->>-XRPServiceBus: Acknowledge Message

    Note over CPO/EVSE, XRPServiceAPI: Charging Session Ends
    CPO/EVSE->>+LoadBalancer: Request Stop Charging (SessionId, TotalEnergy)
    LoadBalancer->>+XRPServiceAPI: POST /api/payments/sessions/{sessionId}/finalize (FinalizePaymentRequest)
    XRPServiceAPI->>+PaymentSvc: FinalizePaymentSessionAsync(sessionId, totalEnergy, totalAmount)
    PaymentSvc->>PaymentSvc: Perform final calculation/check
    PaymentSvc->>PaymentSvc: Retrieve DecryptedSeed, DestinationAddress
    opt Final Payment Needed
        PaymentSvc->>+XRPLSvc: SubmitPaymentAsync(DecryptedSeed, DestinationAddress, ...)
        XRPLSvc-->>-PaymentSvc: Final Transaction Result
        PaymentSvc->>PaymentSvc: Store Final PaymentTransaction
    end
    PaymentSvc->>PaymentSvc: Update PaymentSession (Status: Completed/Failed) # Consider clearing/archiving seed
    PaymentSvc->>+XRPServiceBus: Publish SessionFinalizedEvent
    XRPServiceBus-->>-OtherServices: Notify Finalization
    PaymentSvc-->>-XRPServiceAPI: Finalized PaymentSession
    XRPServiceAPI-->>-LoadBalancer: 200 OK (FinalizePaymentResponse)
    LoadBalancer-->>-CPO/EVSE: Final Session Status

```

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Azure Service Bus (or local equivalent)
- XRP Ledger node access (testnet/devnet for development)

### Configuration

The main configuration is in `appsettings.json`:

```json
{
  "XRPLedger": {
    "DefaultNetwork": "testnet",
    "Endpoints": {
      "Mainnet": "https://xrplcluster.com",
      "Testnet": "https://s.altnet.rippletest.net:51234",
      "Devnet": "https://s.devnet.rippletest.net:51234"
    }
  }
}
```

### Running the Service

```bash
dotnet run --project XRPService
```

### API Endpoints

- **POST /api/payments/sessions**: Initialize a payment session
- **POST /api/payments/micropayments**: Process a micropayment
- **POST /api/payments/sessions/{id}/finalize**: Finalize a payment session
- **GET /api/payments/history/{userId}**: Get payment history for a user
- **GET /api/payments/wallets/{address}**: Get wallet information

## Integration

This service integrates with:

1. **EVSEService**: For charging session management
2. **XRP Ledger**: For blockchain transactions
3. **Monitoring**: OpenTelemetry for tracing payments

## Development

### Key Components

- **IXRPLService**: Interface for XRP Ledger interactions
- **IWalletService**: Interface for wallet management
- **IPaymentService**: Interface for payment processing
- **PaymentsEndpoints**: API endpoints for payment operations

### Testing

Test projects are organized to mirror the service structure, focusing on:

- Unit tests for service implementations
- Integration tests for API endpoints
- Mock tests for blockchain interactions

## Deployment

The service is deployed as a Docker container in Kubernetes, similar to other services in the system.

## Learn More

- [XRP Ledger Documentation](https://xrpl.org/docs.html)
- [XRPL.NET Library](https://github.com/XRPLF/XRPL.NET)
- [OpenTelemetry Documentation](https://opentelemetry.io/docs/)
