using AppServices.Contracts.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;

namespace AppServices.UseCases
{
    public record CreateShipmentRequest(string ReferenceNumber, string Sender, string Recipient) : IRequest<CreateShipmentResponse>;

    public record CreateShipmentResponse(bool Success, int? Id = null, string? ErrorMessage = null);

    public class CreateShipmentUseCase : IRequestHandler<CreateShipmentRequest, CreateShipmentResponse>
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly ILogger<CreateShipmentUseCase> _logger;

        public CreateShipmentUseCase(IShipmentRepository shipmentRepository, ILogger<CreateShipmentUseCase> logger)
        {
            _shipmentRepository = shipmentRepository;
            _logger = logger;
        }

        public async Task<CreateShipmentResponse> Handle(CreateShipmentRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Create Shipment for ReferenceNumber: {ReferenceNumber} - Start", request.ReferenceNumber);
            var existingShipment = await _shipmentRepository.GetShipmentByReferenceNumberAsync(request.ReferenceNumber, cancellationToken);
            
            if (existingShipment != null)
            {
                return new CreateShipmentResponse(
                    Success: false,
                    Id: null,
                    ErrorMessage: $"Shipment with ReferenceNumber '{request.ReferenceNumber}' already exists."
                );
            }

            var shipment = new Shipment
            {
                ReferenceNumber = request.ReferenceNumber,
                Sender = request.Sender,
                Recipient = request.Recipient,
                CreatedAt = DateTime.UtcNow,
                Status = ShipmentStatus.Created
            };

            var result = await _shipmentRepository.CreateShipmentAsync(shipment, cancellationToken);

            _logger.LogInformation("Successfully created Shipment for ReferenceNumber: {ReferenceNumber} - End", result.ReferenceNumber);

            return new CreateShipmentResponse(Success: true, Id: result.Id);
        }
    }
}