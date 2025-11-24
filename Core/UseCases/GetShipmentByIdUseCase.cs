using AppServices.Contracts.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using TransferaShipments.Domain.Entities;

namespace AppServices.UseCases
{
    public record GetShipmentByIdRequest(int Id) : IRequest<GetShipmentByIdResponse>;

    public record GetShipmentByIdResponse(Shipment? Shipment);

    public class GetShipmentByIdUseCase : IRequestHandler<GetShipmentByIdRequest, GetShipmentByIdResponse>
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly ILogger<GetShipmentByIdUseCase> _logger;

        public GetShipmentByIdUseCase(IShipmentRepository shipmentRepository, ILogger<GetShipmentByIdUseCase> logger)
        {
            _shipmentRepository = shipmentRepository;
            _logger = logger;
        }

        public async Task<GetShipmentByIdResponse> Handle(GetShipmentByIdRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Get Shipment by ID : {ShipmentId} - Start", request.Id);

            var shipment = await _shipmentRepository.GetShipmentByIdAsync(request.Id, cancellationToken);

            _logger.LogInformation("Get Shipment by ID : {ShipmentId} - End", request.Id);

            return new GetShipmentByIdResponse(shipment);
        }
    }
}
