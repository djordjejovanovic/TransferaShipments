# TransferaShipments

Aplikacija za upravljanje pošiljkama sa asinhronim upload-om dokumenata. Sistem koristi Clean Architecture sa ASP.NET Core Web API, in-memory message queue za background processing, i Azurite emulator za lokalni development.

## Pregled Arhitekture

Aplikacija je organizovana u sledeće module:

- **App** - ASP.NET Core Web API sa Swagger dokumentacijom
- **Core (AppServices)** - Application services, Use Cases (MediatR pattern)
- **Domain** - Domain entiteti i enums (ShipmentStatus)
- **Persistence** - EF Core DbContext i repositories
- **BlobStorage** - Azure Blob Storage wrapper (podržava Azurite emulator)
- **ServiceBus** - Lokalni in-memory message queue za asinhronu obradu
  - `LocalServiceBusPublisher` - šalje poruke u Channel
  - `LocalDocumentProcessorHostedService` - Background worker koji procesira poruke

**Tehnologije:**
- .NET 8.0 / .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core
- MediatR (CQRS pattern)
- System.Threading.Channels (za in-memory queue)
- Azure.Storage.Blobs
- Azurite (Azure Storage Emulator)

---

## Kako Pokrenuti API

### Preduslovi
- .NET 8.0
- Node.js (za Azurite emulator)
- SQL Server (TransferaShipments.bak backup baze)

### Koraci za pokretanje

#### 1. Azurite Emulator (Blob Storage)

Azurite emulator omogućava lokalno testiranje Azure Blob Storage-a bez Azure naloga.

**Sa custom portovima:**
```bash
azurite --location ./azurite_workspace --silent --blobPort 10010 --queuePort 10011 --tablePort 10012
```
#### 2. Pokrenite App API projekat

#### 3. Worker/Background Servis

`LocalDocumentProcessorHostedService` je `BackgroundService` koji se registruje kao `HostedService` u ASP.NET Core aplikaciji i automatski se startuje prilikom pokretanja aplikacije.

## Konekcioni stringovi

Svi konekcioni stringovi se podešavaju u **`App/appsettings.json`** fajlu.

### Sadržaj appsettings.json

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=localhost;Database=TransferaShipments;User Id=appUser;Password=StrongP@ssw0rd!;TrustServerCertificate=True;MultipleActiveResultSets=True;",
    "AzureBlob": "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10010/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10011/devstoreaccount1;TableEndpoint=http://127.0.0.1:10012/devstoreaccount1;"
  },
  "Azure": {
    "BlobContainerName": "shipments-documents",
    "ServiceBusQueueName": "documents-toprocess"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

## Primeri Request-ova

`requests.http` fajl

## Arhitektura Sistema

<img width="1005" height="895" alt="ARH" src="https://github.com/user-attachments/assets/2dc1f225-f55b-4ffd-a47c-92703dc808d5" />

