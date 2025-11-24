using MediatR;
using AppServices.Contracts.Repositories;
using AppServices.Contracts.Storage;
using AppServices.Contracts.Messaging;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace AppServices.UseCases
{
    public record UploadDocumentRequest(
        int ShipmentId, 
        Stream FileStream,
        string FileName,
        string ContentType,
        string ContainerName) : IRequest<UploadDocumentResponse>;

    public record UploadDocumentResponse(bool Success, string? BlobName, string? BlobUrl, string? ErrorMessage);

    public class UploadDocumentUseCase : IRequestHandler<UploadDocumentRequest, UploadDocumentResponse>
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly IBlobService _blobService;
        private readonly IServiceBusPublisher _serviceBusPublisher;
        private readonly ILogger<UploadDocumentUseCase> _logger;
        private readonly ResiliencePipeline _retryPipeline;

        public UploadDocumentUseCase(
            IShipmentRepository shipmentRepository,
            IBlobService blobService,
            IServiceBusPublisher serviceBusPublisher,
            ILogger<UploadDocumentUseCase> logger)
        {
            _shipmentRepository = shipmentRepository;
            _blobService = blobService;
            _serviceBusPublisher = serviceBusPublisher;
            _logger = logger;
            _retryPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    OnRetry = args =>
                    {
                        _logger.LogWarning(
                            "Retry attempt {AttemptNumber} after {Delay}ms due to: {Exception}",
                            args.AttemptNumber,
                            args.RetryDelay.TotalMilliseconds,
                            args.Outcome.Exception?.Message);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        public async Task<UploadDocumentResponse> Handle(UploadDocumentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Upload document for ShipmentId: {ShipmentId} - Start", request.ShipmentId);
                if (request.FileStream == null)
                {
                    _logger.LogWarning("File stream is null for ShipmentId: {ShipmentId}", request.ShipmentId);
                    return new UploadDocumentResponse(false, null, null, "File is required");
                }

                var shipment = await _shipmentRepository.GetShipmentByIdAsync(request.ShipmentId, cancellationToken);

                if (shipment == null)
                {
                    _logger.LogWarning("Shipment not found with Id: {ShipmentId}", request.ShipmentId);
                    return new UploadDocumentResponse(false, null, null, "Shipment not found");
                }

                var fileName = Path.GetFileName(request.FileName);
                if (string.IsNullOrEmpty(fileName))
                {
                    _logger.LogWarning("Invalid file name for ShipmentId: {ShipmentId}", request.ShipmentId);
                    return new UploadDocumentResponse(false, null, null, "Invalid file name");
                }

                var blobName = $"{request.ShipmentId}/{Guid.NewGuid()}_{fileName}";
                
                var blobUrl = await _retryPipeline.ExecuteAsync(async token =>
                {
                    return await _blobService.UploadAsync(request.ContainerName, blobName, request.FileStream, request.ContentType, token);
                }, cancellationToken);

                shipment.LastDocumentBlobName = blobName;
                shipment.LastDocumentUrl = blobUrl;
                shipment.Status = TransferaShipments.Domain.Enums.ShipmentStatus.DocumentUploaded;

                await _shipmentRepository.UpdateShipmentStatusAsync(shipment, cancellationToken);

                await _retryPipeline.ExecuteAsync(async token =>
                {
                    await _serviceBusPublisher.PublishDocumentToProcessAsync(request.ShipmentId, blobName, token);
                }, cancellationToken);

                _logger.LogInformation("Upload document for ShipmentId: {ShipmentId} - End", request.ShipmentId);

                return new UploadDocumentResponse(true, blobName, blobUrl, null);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Document upload operation was cancelled for ShipmentId: {ShipmentId}", request.ShipmentId);
                return new UploadDocumentResponse(false, null, null, "Operation was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during document upload for ShipmentId: {ShipmentId}", request.ShipmentId);
                return new UploadDocumentResponse(false, null, null, "An error occurred while uploading the document. Please try again later.");
            }
        }
    }
}