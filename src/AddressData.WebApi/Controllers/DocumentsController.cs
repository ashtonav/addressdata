namespace AddressData.WebApi.Controllers;

using Core.Mappers;
using Core.Models.ApiResponse;
using Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class DocumentsController(ISeedingService seedingService,
    IDocumentService documentService,
    IOverpassTurboService overpassTurboService)
    : Controller
{
    [HttpPost("seed")]
    [ProducesResponseType<AddressDocumentsApiResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AddressDocumentsApiResponse>> SeedDocuments([FromQuery] long? limit)
    {
        var seedingResult = await seedingService.RunSeeding(limit);

        var mapped = DomainToApiResponseMapper.Map(seedingResult);
        return Ok(mapped);
    }

    [HttpPost("{areaId}")]
    [ProducesResponseType<AddressDocumentApiResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddressDocumentApiResponse>> InsertDocument(long areaId)
    {
        var cityCreated = await seedingService.AddCity(areaId);

        var response = DomainToApiResponseMapper.Map(cityCreated);

        return response is null
            ? Conflict($"Something went wrong when trying to create a city {areaId}")
            : Created($"documents/{areaId}", response);
    }

    [HttpGet("{areaId}")]
    [ProducesResponseType<AddressDocumentApiResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AddressDocumentApiResponse>> GetDocument(long areaId)
    {
        var location = await overpassTurboService.GetLocation(areaId);
        if (location is null)
        {
            return NotFound("Location not found");
        }
        var document = await documentService.GetAsync(location);

        if (document is null)
        {
            return NotFound("Document not found");
        }

        var mapped = DomainToApiResponseMapper.Map(document);

        return mapped is null
            ? NotFound($"Document with area id {areaId} not found")
            : Ok(mapped);
    }

    [HttpGet]
    [ProducesResponseType<AddressDocumentsApiResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AddressDocumentsApiResponse>> GetAllDocuments()
    {
        var documents = await documentService.GetAllAsync();
        var mapped = DomainToApiResponseMapper.Map(documents);
        return Ok(mapped);
    }
}
