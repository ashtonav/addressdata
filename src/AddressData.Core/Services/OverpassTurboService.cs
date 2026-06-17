namespace AddressData.Core.Services;

using System.Globalization;
using System.Text.Json;
using CsvHelper;
using Interfaces;
using Mappers;
using Microsoft.Extensions.Logging;
using Models.Domain;
using Models.OverpassTurbo;

public class OverpassTurboService
    (IHttpClientFactory httpClientFactory,
    ILogger<OverpassTurboService> logger)
    : IOverpassTurboService
{
    public async Task<CityInfoDomainModel?> GetCity(long areaId)
    {
        var city = await FetchDataSingle<OverpassTurboCityInfoResponse?>
            (Constants.OverpassTurboGetCityQuery(areaId));

        return OverpassTurboResponseToDomainMapper.Map(city);
    }

    public async Task<IEnumerable<CityInfoDomainModel>?> GetCities()
    {
        var cities = await FetchDataCollection<OverpassTurboCityInfoResponse>
            (Constants.OverpassTurboGetAllCitiesQuery);

        return OverpassTurboResponseToDomainMapper.Map(cities);
    }

    public async Task<IEnumerable<AddressesDomainModel>?> GetAddresses(long areaId)
    {
        var addresses = await FetchDataCollection<OverpassTurboAddressesResponse>
            (Constants.OverpassTurboGetAddressesQuery(areaId));

        return OverpassTurboResponseToDomainMapper.Map(addresses);
    }

    public async Task<LocationDomainModel?> GetLocation(long areaId)
    {
        var latitudeLongitude = await FetchDataSingle<OverpassTurboLatitudeLongitudeResponse?>
            (Constants.OverpassTurboGetLatitudeLongitudeQuery(areaId));

        if (latitudeLongitude == null)
        {
            return null;
        }

        var latitudeLongitudeDomainModel = OverpassTurboResponseToDomainMapper.Map(latitudeLongitude);

        if (latitudeLongitudeDomainModel is null)
        {
            return null;
        }

        var stateCountry = await GetStateAndCountry(latitudeLongitudeDomainModel);
        var city = await GetCity(areaId);

        if (stateCountry is null || city is null)
        {
            return null;
        }

        return DomainToDomainMapper.Map(city, stateCountry);
    }

    private async Task<Stream> PostAndGetStream(string query)
    {
        var response = await httpClientFactory
            .CreateClient()
            .PostAsync(Constants.OverpassTurboUrl,
                new FormUrlEncodedContent([new("data", query)]));
        return await response.Content.ReadAsStreamAsync();
    }

    private async Task<IList<T>?> FetchDataCollection<T>(string query)
    {
        try
        {
            await using var stream = await PostAndGetStream(query);
            using var streamReader = new StreamReader(stream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            return [.. csvReader.GetRecords<T>()];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while fetching data from Overpass Turbo");
            return null;
        }
    }

    private async Task<T?> FetchDataSingle<T>(string query)
    {
        try
        {
            await using var stream = await PostAndGetStream(query);
            using var streamReader = new StreamReader(stream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();
            return csvReader.GetRecord<T>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while fetching data from Overpass Turbo");
            return default;
        }
    }


    private async Task<StateCountryDomainModel?> GetStateAndCountry(LatitudeLongitudeDomainModel location)
    {
        var httpResponse = await httpClientFactory
            .CreateClient()
            .PostAsync(Constants.OverpassTurboUrl,
                new FormUrlEncodedContent([new("data", Constants.OverpassTurboAreaInfoQuery(location.Latitude, location.Longitude))]));

        var json = await httpResponse.Content.ReadAsStringAsync();
        var response = JsonSerializer.Deserialize<OverpassTurboAreaInfoResponse>(json);
        if (response?.Elements is null)
        {
            return null;
        }

        string? countryName = null;
        string? stateName = null;

        foreach (var element in response.Elements)
        {
            if (element.Tags is null)
            {
                continue;
            }

            if (element.Tags.AdminLevel == "4")
            {
                stateName = element.Tags.NameEnglish ?? element.Tags.Name;
            }
            else if (element.Tags.AdminLevel == "2")
            {
                countryName = element.Tags.NameEnglish ?? element.Tags.Name;
            }
        }

        return countryName is not null && stateName is not null
            ? new StateCountryDomainModel { Country = countryName, State = stateName }
            : null;
    }
}
