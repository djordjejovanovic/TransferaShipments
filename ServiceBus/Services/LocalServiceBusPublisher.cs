using Microsoft.Extensions.Logging;
using AppServices.Contracts.Messaging;
using System.Threading.Channels;
using System.Text.Json;

namespace TransferaShipments.ServiceBus.Services;

public class LocalServiceBusPublisher : IServiceBusPublisher
{
    private readonly ILogger<LocalServiceBusPublisher> _logger;
    private readonly Channel<string> _channel;

    public LocalServiceBusPublisher(ILogger<LocalServiceBusPublisher> logger, Channel<string> channel)
    {
        _logger = logger;
        _channel = channel;
    }

    public async Task PublishDocumentToProcessAsync(int shipmentId, string blobName, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            ShipmentId = shipmentId,
            BlobName = blobName
        };

        var msgBody = JsonSerializer.Serialize(payload);
        
        await _channel.Writer.WriteAsync(msgBody, cancellationToken);
        
        _logger.LogInformation("[LocalServiceBus] Published message to local queue: ShipmentId={ShipmentId}, BlobName={BlobName}", 
            shipmentId, blobName);
    }
}
