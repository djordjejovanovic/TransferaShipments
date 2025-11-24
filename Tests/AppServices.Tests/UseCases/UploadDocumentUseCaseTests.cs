using AppServices.Contracts.Messaging;
using AppServices.Contracts.Repositories;
using AppServices.Contracts.Storage;
using AppServices.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;

namespace AppServices.Tests.UseCases;

public class UploadDocumentUseCaseTests
{
    private readonly Mock<IShipmentRepository> _shipmentRepositoryMock;
    private readonly Mock<IBlobService> _blobServiceMock;
    private readonly Mock<IServiceBusPublisher> _serviceBusPublisherMock;
    private readonly Mock<ILogger<UploadDocumentUseCase>> _loggerMock;
    private readonly UploadDocumentUseCase _useCase;

    public UploadDocumentUseCaseTests()
    {
        _shipmentRepositoryMock = new Mock<IShipmentRepository>();
        _blobServiceMock = new Mock<IBlobService>();
        _serviceBusPublisherMock = new Mock<IServiceBusPublisher>();
        _loggerMock = new Mock<ILogger<UploadDocumentUseCase>>();
        _useCase = new UploadDocumentUseCase(
            _shipmentRepositoryMock.Object,
            _blobServiceMock.Object,
            _serviceBusPublisherMock.Object,
            _loggerMock.Object,
            configuration: null!);
    }

    public async Task ShouldUploadDocument()
    {
        var shipment = new Shipment
        {
            Id = 1,
            ReferenceNumber = "REF001",
            Sender = "Sender A",
            Recipient = "Recipient A",
            Status = ShipmentStatus.Created
        };

        using var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var request = new UploadDocumentRequest(
            ShipmentId: 1,
            FileStream: fileStream,
            FileName: "document.pdf",
            ContentType: "application/pdf",
            ContainerName: "shipments");
        var cancellationToken = CancellationToken.None;

        var expectedBlobUrl = "https://storage.blob.core.windows.net/shipments/1/guid_document.pdf";

        _shipmentRepositoryMock
            .Setup(x => x.GetShipmentByIdAsync(1, cancellationToken))
            .ReturnsAsync(shipment);

        _blobServiceMock
            .Setup(x => x.UploadAsync(
                "shipments",
                It.Is<string>(s => s.StartsWith("1/") && s.EndsWith("_document.pdf")),
                fileStream,
                "application/pdf",
                cancellationToken))
            .ReturnsAsync(expectedBlobUrl);

        _shipmentRepositoryMock
            .Setup(x => x.UpdateShipmentStatusAsync(It.IsAny<Shipment>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _serviceBusPublisherMock
            .Setup(x => x.PublishDocumentToProcessAsync(1, It.IsAny<string>(), cancellationToken))
            .Returns(Task.CompletedTask);

        var result = await _useCase.Handle(request, cancellationToken);

        result.Success.Should().BeTrue();
        result.BlobUrl.Should().Be(expectedBlobUrl);
        result.BlobName.Should().NotBeNullOrEmpty();
        result.BlobName.Should().StartWith("1/");
        result.BlobName.Should().EndWith("_document.pdf");
        result.ErrorMessage.Should().BeNull();
    }
}
