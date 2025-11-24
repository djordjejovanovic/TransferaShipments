using TransferaShipments.Domain.Entities;

namespace AppServices.Contracts.Repositories;

public interface IShipmentRepository
{
    Task<Shipment> CreateShipmentAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task<Shipment?> GetShipmentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Shipment?> GetShipmentByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Shipment>> GetAllShipmentsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetShipmentsCountAsync(CancellationToken cancellationToken = default);
    Task UpdateShipmentStatusAsync(Shipment shipment, CancellationToken cancellationToken = default);
}
