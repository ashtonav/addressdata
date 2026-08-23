namespace AddressData.Core.Mappers;

using Models.Domain;
using Models.OverpassTurbo;

public static class OverpassTurboResponseToDomainMapper
{
    public static LatitudeLongitudeDomainModel? Map(OverpassTurboLatitudeLongitudeResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Longitude) || string.IsNullOrWhiteSpace(response.Latitude))
        {
            return null;
        }

        return new LatitudeLongitudeDomainModel
        {
            Latitude = response.Latitude,
            Longitude = response.Longitude
        };
    }

    public static CityInfoDomainModel? Map(OverpassTurboCityInfoResponse? response)
    {
        if (response is null)
        {
            return null;
        }

        var city = !string.IsNullOrWhiteSpace(response.CityEnglish) ? response.CityEnglish : response.City;

        if (response.AreaId is null or <= 0
            || string.IsNullOrWhiteSpace(city))
        {
            return null;
        }

        return new CityInfoDomainModel { AreaId = (long)response.AreaId, City = city };
    }

    public static AddressesDomainModel? Map(OverpassTurboAddressesResponse? response)
    {
        if (response is null
            || string.IsNullOrWhiteSpace(response.HouseNumber)
            || string.IsNullOrWhiteSpace(response.Street)
            || string.IsNullOrWhiteSpace(response.Postcode)
            || string.IsNullOrWhiteSpace(response.Latitude)
            || string.IsNullOrWhiteSpace(response.Longitude))
        {
            return null;
        }

        return new AddressesDomainModel
        {
            HouseNumber = response.HouseNumber.Trim(),
            Street = response.Street.Trim(),
            Postcode = response.Postcode.Trim(),
            Latitude = response.Latitude,
            Longitude = response.Longitude,
        };
    }

    public static IEnumerable<AddressesDomainModel>? Map(IEnumerable<OverpassTurboAddressesResponse?>? addresses) =>
        MapCollection(addresses, Map);

    public static IEnumerable<CityInfoDomainModel>? Map(IEnumerable<OverpassTurboCityInfoResponse>? cityInfos) =>
        MapCollection(cityInfos, Map);

    private static List<TOut>? MapCollection<TIn, TOut>(IEnumerable<TIn?>? source, Func<TIn?, TOut?> map)
        where TOut : class
    {
        if (source is null)
        {
            return null;
        }

        var mapped = source
            .Select(map)
            .Where(item => item is not null)
            .ToList();

        return mapped.Count != 0 ? mapped : null;
    }
}
