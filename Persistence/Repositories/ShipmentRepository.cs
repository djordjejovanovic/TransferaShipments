using Microsoft.EntityFrameworkCore;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Persistence.Data;
using AppServices.Contracts.Repositories;

namespace TransferaShipments.Persistence.Repositories;

public class ShipmentRepository : IShipmentRepository
{
    private readonly AppDbContext _db;
    public ShipmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Shipment> CreateShipmentAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        var result = (await _db.Shipments.AddAsync(shipment, cancellationToken)).Entity;

        await _db.SaveChangesAsync(cancellationToken);
  
        return result;
    }

    public async Task<Shipment?> GetShipmentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _db.Shipments.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Shipment?> GetShipmentByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken = default)
    {
        return await _db.Shipments.FirstOrDefaultAsync(s => s.ReferenceNumber != null && s.ReferenceNumber.ToUpper() == referenceNumber.ToUpper(), cancellationToken);
    }

    public async Task<IEnumerable<Shipment>> GetAllShipmentsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _db.Shipments
            .AsNoTracking()
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetShipmentsCountAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Shipments.CountAsync(cancellationToken);
    }

    public async Task UpdateShipmentStatusAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        _db.Shipments.Update(shipment);
        
        await _db.SaveChangesAsync(cancellationToken);
    }
}