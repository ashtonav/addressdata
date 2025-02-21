namespace AddressData.Core.Services;
using Interfaces;
using Mappers;
using Microsoft.Extensions.Logging;
using Models.Domain;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

public class SeedingService(
    IOverpassTurboService overpassTurboService,
    IDocumentService documentService,
    ILogger<SeedingService> logger)
    : ISeedingService
{
    public async Task<AddressDocumentDomainModel> AddCity(long areaId)
    {
        await ValidateRequest(areaId);

        var addresses = await overpassTurboService.GetAddresses(areaId);

        if (addresses is null || addresses.Count() < Constants.MinimumNumberOfAddresses)
        {
            throw new ValidationException($"The number of addresses in {areaId} is small.");
        }

        var writeModel = DomainToDataMapper.Map(addresses);

        var location = await overpassTurboService.GetLocation(areaId) ?? throw new ValidationException($"The city description in {areaId} is invalid.");

        var result = await documentService.InsertAsync(writeModel, location);
        return result;
    }

    private async Task ValidateRequest(long areaId)
    {
        // Validate that areaId number provided is valid
        if (areaId is < 0 or 0)
        {
            throw new ValidationException(
                $"Please provide a valid AreaId. It must be a positive number. AreaId provided: {areaId}.");
        }

        // Validate that city for areaId exists
        var city = await overpassTurboService.GetCity(areaId);

        if (city?.City == null)
        {
            throw new ValidationException($"{city?.City} not found in Overpass Turbo. Please try another city");
        }
    }

    public async Task<IEnumerable<AddressDocumentDomainModel>> RunSeeding(long? limit)
    {
        limit ??= long.MaxValue;
        var result = new List<AddressDocumentDomainModel>();
        var allCities = await overpassTurboService.GetCities();

        logger.LogInformation("Running seeding. All cities count: {Count}", allCities.Count());

        foreach (var city in allCities)
        {
            try
            {
                await Task.Delay(Constants.SeedingDelayMs); // Adding delay so not to overwhelm their APIs

                if (result.Count >= limit)
                {
                    break;
                }

                logger.LogInformation("Trying to add {City}. Area id: {AreaId}", city.City, city.AreaId);
                var toAdd = await AddCity(city.AreaId);
                result.Add(toAdd);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to add city: {City} with area id: {AreaId}", city.City, city.AreaId);
            }
        }

        return result;
    }
}
