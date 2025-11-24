using TransferaShipments.Domain.Enums;

namespace TransferaShipments.Domain.Entities;

public class Shipment
{
    public int Id { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Sender { get; set; }
    public string? Recipient { get; set; }
    public ShipmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? LastDocumentBlobName { get; set; }
    public string? LastDocumentUrl { get; set; }
}