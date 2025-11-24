using AppServices.Contracts.Repositories;
using AppServices.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TransferaShipments.Domain.Entities;
using TransferaShipments.Domain.Enums;

namespace AppServices.Tests.UseCases;

public class CreateShipmentUseCaseTests
{
    private readonly Mock<IShipmentRepository> _shipmentRepositoryMock;
    private readonly CreateShipmentUseCase _useCase;
    private readonly Mock<ILogger<CreateShipmentUseCase>> _loggerMock;

    public CreateShipmentUseCaseTests()
    {
        _shipmentRepositoryMock = new Mock<IShipmentRepository>();
        _loggerMock = new Mock<ILogger<CreateShipmentUseCase>>();
        _useCase = new CreateShipmentUseCase(_shipmentRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldCreateShipment()
    {
        var request = new CreateShipmentRequest("REF001", "Sender A", "Recipient B");
        var cancellationToken = CancellationToken.None;

        _shipmentRepositoryMock
            .Setup(x => x.GetShipmentByReferenceNumberAsync(request.ReferenceNumber, cancellationToken))
            .ReturnsAsync((Shipment?)null);

        var createdShipment = new Shipment
        {
            Id = 1,
            ReferenceNumber = request.ReferenceNumber,
            Sender = request.Sender,
            Recipient = request.Recipient,
            Status = ShipmentStatus.Created
        };

        _shipmentRepositoryMock
            .Setup(x => x.CreateShipmentAsync(It.IsAny<Shipment>(), cancellationToken))
            .ReturnsAsync(createdShipment);

        var result = await _useCase.Handle(request, cancellationToken);

        result.Success.Should().BeTrue();
        result.Id.Should().Be(1);
        result.ErrorMessage.Should().BeNull();
    }
}
