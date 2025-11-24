namespace AppServices.Contracts.Messaging;

public interface IServiceBusPublisher
{
    Task PublishDocumentToProcessAsync(int shipmentId, string blobName, CancellationToken cancellationToken = default);
}
