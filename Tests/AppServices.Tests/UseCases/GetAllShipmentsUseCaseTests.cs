using AppServices.Contracts.Repositories;
using AppServices.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;

namespace AppServices.Tests.UseCases;

public class GetAllShipmentsUseCaseTests
{
    private readonly Mock<IShipmentRepository> _shipmentRepositoryMock;
    private readonly GetAllShipmentsUseCase _useCase;
    private readonly Mock<ILogger<GetAllShipmentsUseCase>> _loggerMock;

    public GetAllShipmentsUseCaseTests()
    {
        _shipmentRepositoryMock = new Mock<IShipmentRepository>();
        _loggerMock = new Mock<ILogger<GetAllShipmentsUseCase>>();
        _useCase = new GetAllShipmentsUseCase(_shipmentRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldReturnShipments()
    {
        var request = new GetAllShipmentsRequest(Page: 1, PageSize: 10);
        var cancellationToken = CancellationToken.None;

        var shipments = new List<Shipment>
        {
            new Shipment { Id = 1, ReferenceNumber = "REF001", Sender = "Sender A", Recipient = "Recipient A", Status = ShipmentStatus.Created },
            new Shipment { Id = 2, ReferenceNumber = "REF002", Sender = "Sender B", Recipient = "Recipient B", Status = ShipmentStatus.Created }
        };

        _shipmentRepositoryMock
            .Setup(x => x.GetAllShipmentsAsync(1, 10, cancellationToken))
            .ReturnsAsync(shipments);

        _shipmentRepositoryMock
            .Setup(x => x.GetShipmentsCountAsync(cancellationToken))
            .ReturnsAsync(25);

        var result = await _useCase.Handle(request, cancellationToken);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }
}
