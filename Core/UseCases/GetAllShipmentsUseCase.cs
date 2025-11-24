using AppServices.Common.Models;
using AppServices.Contracts.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using TransferaShipments.Domain.Entities;

namespace AppServices.UseCases
{
    public record GetAllShipmentsRequest(int Page, int PageSize) : IRequest<PaginatedResponse<Shipment>>;

    public class GetAllShipmentsUseCase : IRequestHandler<GetAllShipmentsRequest, PaginatedResponse<Shipment>>
    {
        private readonly IShipmentRepository _shipmentRepository;
        private readonly ILogger<GetAllShipmentsUseCase> _logger;

        public GetAllShipmentsUseCase(IShipmentRepository shipmentRepository, ILogger<GetAllShipmentsUseCase> logger)
        {
            _shipmentRepository = shipmentRepository;
            _logger = logger;
        }

        public async Task<PaginatedResponse<Shipment>> Handle(GetAllShipmentsRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Get all Shipments - Start");

            var page = request.Page;
            if(request.Page <= 0)
            {
                page = 1;
            }
            
            var pageSize = Math.Min(request.PageSize, 100);
            if(request.PageSize <= 0)
            {
                pageSize = 20;
            }   
            
            var shipments = await _shipmentRepository.GetAllShipmentsAsync(page, pageSize, cancellationToken);

            var total = await _shipmentRepository.GetShipmentsCountAsync(cancellationToken);

            _logger.LogInformation("Get all Shipments - End");

            return new PaginatedResponse<Shipment>(shipments, total, page, pageSize);
        }
    }
}
