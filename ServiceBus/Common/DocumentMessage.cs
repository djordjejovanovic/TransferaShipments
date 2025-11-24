namespace TransferaShipments.ServiceBus.Common;

public record DocumentMessage(int ShipmentId, string BlobName);
