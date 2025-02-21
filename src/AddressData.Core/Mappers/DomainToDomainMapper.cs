namespace AddressData.Core.Mappers;

using Models.Domain;

public static class DomainToDomainMapper
{
    public static LocationDomainModel Map(CityInfoDomainModel cityInfo, StateCountryDomainModel stateCountry) => new()
    {
        AreaId = cityInfo.AreaId,
        City = cityInfo.City,
        State = stateCountry.State,
        Country = stateCountry.Country
    };
}
