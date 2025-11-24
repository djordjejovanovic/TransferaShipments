namespace TransferaShipments.Messages;
    public class DocumentMessage
    {
        public int ShipmentId { get; set; }
        public string BlobName { get; set; }

        public DocumentMessage(int shipmentId, string blobName)
        {
            ShipmentId = shipmentId;
            BlobName = blobName;
        }
    }