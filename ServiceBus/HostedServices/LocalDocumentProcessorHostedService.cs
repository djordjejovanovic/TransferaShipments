using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Channels;
using AppServices.Contracts.Storage;
using AppServices.Contracts.Repositories;
using TransferaShipments.Domain.Enums;
using TransferaShipments.ServiceBus.Common;

namespace TransferaShipments.ServiceBus.HostedServices;

public class LocalDocumentProcessorHostedService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IBlobService _blobService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LocalDocumentProcessorHostedService> _logger;
    private readonly Channel<string> _channel;

    public LocalDocumentProcessorHostedService(
        IConfiguration configuration,
        IBlobService blobService,
        IServiceProvider serviceProvider,
        ILogger<LocalDocumentProcessorHostedService> logger,
        Channel<string> channel)
    {
        _configuration = configuration;
        _blobService = blobService;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[LocalServiceBus] Starting local document processor (in-memory queue)");

        await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessMessageAsync(message, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LocalServiceBus] Error processing message: {Message}", message);
            }
        }

        _logger.LogInformation("[LocalServiceBus] Local document processor stopped");
    }

    private async Task ProcessMessageAsync(string messageBody, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(messageBody))
            {
                _logger.LogWarning("[LocalServiceBus] Received empty message");
                return;
            }

            DocumentMessage? document = JsonSerializer.Deserialize<DocumentMessage>(messageBody);
            if (document == null)
            {
                _logger.LogWarning("[LocalServiceBus] Invalid message payload: {Message}", messageBody);
                return;
            }

            _logger.LogInformation("[LocalServiceBus] Processing message: ShipmentId={ShipmentId}, BlobName={BlobName}", 
                document.ShipmentId, document.BlobName);

            var container = _configuration["Azure:BlobContainerName"] ?? "shipments-documents";

            using var stream = await _blobService.DownloadAsync(container, document.BlobName, cancellationToken);

            using var scope = _serviceProvider.CreateScope();
            var shipmentRepository = scope.ServiceProvider.GetRequiredService<IShipmentRepository>();

            var shipment = await shipmentRepository.GetShipmentByIdAsync(document.ShipmentId, cancellationToken);
            if (shipment != null)
            {
                shipment.Status = ShipmentStatus.Processed;
                await shipmentRepository.UpdateShipmentStatusAsync(shipment, cancellationToken);
                
                _logger.LogInformation("[LocalServiceBus] Successfully processed document for ShipmentId={ShipmentId}", 
                    document.ShipmentId);
            }
            else
            {
                _logger.LogWarning("[LocalServiceBus] Shipment not found: ShipmentId={ShipmentId}", document.ShipmentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[LocalServiceBus] Error processing message: {Message}", messageBody);
        }
    }
}
