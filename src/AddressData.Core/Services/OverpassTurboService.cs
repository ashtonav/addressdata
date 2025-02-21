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

    private async Task<IList<T>?> FetchDataCollection<T>(string query)
    {
        try
        {
            var response = await httpClientFactory
                .CreateClient()
                .GetAsync($"{Constants.OverpassTurboUrl}{query}");

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

            return csvReader
                .GetRecords<T>()
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while fetching data from Overpass Turbo");
        }

        return null;
    }

    private async Task<T?> FetchDataSingle<T>(string query)
    {
        try
        {
            var response = await httpClientFactory
                .CreateClient()
                .GetAsync($"{Constants.OverpassTurboUrl}{query}");

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            await csvReader.ReadAsync();

            return csvReader
                .GetRecord<T>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while fetching data from Overpass Turbo");
        }

        return default; //null
    }


    private async Task<StateCountryDomainModel?> GetStateAndCountry(LatitudeLongitudeDomainModel location)
    {
        var httpResponse = await httpClientFactory
            .CreateClient()
            .GetAsync($"{Constants.OverpassTurboUrl}{Constants.OverpassTurboAreaInfoQuery(location.Latitude, location.Longitude)}");

        using var jsonDocument = JsonDocument.Parse(await httpResponse.Content.ReadAsStringAsync());

        return ParseJson(jsonDocument);
    }

    private static StateCountryDomainModel? ParseJson(JsonDocument jsonDocument)
    {
        string countryName = null;
        string stateName = null;

        // TODO: HACK: Manually parsing JSON here. There must be a better way...
        var elements = jsonDocument.RootElement.GetProperty(Constants.OverpassTurboResponseElements);

        foreach (var element in elements.EnumerateArray())
        {
            if (element.TryGetProperty(Constants.OverpassTurboResponseTags, out var tags))
            {
                if (tags.TryGetProperty(Constants.OverpassTurboResponseAdministrativeLevel, out var adminLevel))
                {
                    if (adminLevel.GetString() == Constants.OverpassTurboResponseStateAdministrationLevel)
                    {
                        if (tags.TryGetProperty(Constants.OverpassTurboResponseEnglishName, out var stateEnglish))
                        {
                            stateName = stateEnglish.GetString();
                        }
                        else if (tags.TryGetProperty(Constants.OverpassTurboResponseName, out var state))
                        {
                            stateName = state.GetString();
                        }
                    }
                    else if (adminLevel.GetString() == Constants.OverpassTurboResponseCountryAdministrationLevel)
                    {
                        if (tags.TryGetProperty(Constants.OverpassTurboResponseEnglishName, out var countryEnglish))
                        {
                            countryName = countryEnglish.GetString();
                        }
                        else if (tags.TryGetProperty(Constants.OverpassTurboResponseName, out var country))
                        {
                            countryName = country.GetString();
                        }
                    }
                }
            }
        }

        if (countryName is not null && stateName is not null)
        {
            return new StateCountryDomainModel { Country = countryName, State = stateName };
        }

        return null;
    }
}
