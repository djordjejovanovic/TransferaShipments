using Microsoft.AspNetCore.Mvc;
using TransferaShipments.App.Models;
using MediatR;
using AppServices.UseCases;

namespace TransferaShipments.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public ShipmentsController(
        IMediator mediator,
        IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] ShipmentCreateDto shipmentCreateDto, CancellationToken cancellationToken)
    {
        if (shipmentCreateDto == null)
        {
            return BadRequest("Empty request");
        }

        var request = new CreateShipmentRequest(shipmentCreateDto.ReferenceNumber, shipmentCreateDto.Sender, shipmentCreateDto.Recipient);

        var response = await _mediator.Send(request, cancellationToken);

        if (!response.Success)
        {
            return Conflict(new { error = response.ErrorMessage });
        }

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var request = new GetAllShipmentsRequest(page, pageSize);

        var response = await _mediator.Send(request, cancellationToken);

        if (response?.Items == null)
        {
            return NotFound();
        }

        Response.Headers["X-Total-Count"] = response.TotalCount.ToString();

        return Ok(response);
    }

    [HttpGet("GetById/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        if(id <= 0)
        {
            return BadRequest("Invalid shipment ID");
        }

        var request = new GetShipmentByIdRequest(id);

        var response = await _mediator.Send(request, cancellationToken);

        if (response?.Shipment == null)
        {
            return NotFound();
        }

        return Ok(response.Shipment);
    }

    [HttpPost("{id:int}/Documents")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument(int id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is required");
        }

        var container = _configuration["Azure:BlobContainerName"] ?? "shipments-documents";
        using var stream = file.OpenReadStream();

        var request = new UploadDocumentRequest(id, stream, file.FileName, file.ContentType, container);
        var response = await _mediator.Send(request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(new { error = response.ErrorMessage });
        }

        return Accepted(new { blobName = response.BlobName, blobUrl = response.BlobUrl });
    }
}