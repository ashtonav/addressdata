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
        string city = null;

        if (response is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(response.CityEnglish))
        {
            city = response.CityEnglish;
        }
        else if (!string.IsNullOrWhiteSpace(response.City))
        {
            city = response.City;
        }

        if (response.AreaId == null || response.AreaId < 0 || response.AreaId == 0
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

    public static IEnumerable<AddressesDomainModel>? Map(IEnumerable<OverpassTurboAddressesResponse?>? addresses)
    {
        if (addresses is null)
        {
            return null;
        }

        var response = new List<AddressesDomainModel>();

        foreach (var address in addresses)
        {
            var mapped = Map(address);
            if (mapped != null)
            {
                response.Add(mapped);
            }
        }

        if (response.Count != 0)
        {
            return response;
        }

        return null;
    }

    public static IEnumerable<CityInfoDomainModel>? Map(IList<OverpassTurboCityInfoResponse>? cityInfos)
    {
        var response = new List<CityInfoDomainModel>();

        foreach (var cityInfo in cityInfos)
        {
            var mapped = Map(cityInfo);
            if (mapped != null)
            {
                response.Add(mapped);
            }
        }

        if (response.Count != 0)
        {
            return response;
        }

        return null;
    }
}
