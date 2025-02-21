namespace AddressData.Core.Services.Interfaces;

using Models.Domain;

public interface IOverpassTurboService
{
    Task<IEnumerable<CityInfoDomainModel>?> GetCities();
    Task<CityInfoDomainModel?> GetCity(long areaId);
    Task<LocationDomainModel?> GetLocation(long areaId);
    Task<IEnumerable<AddressesDomainModel>?> GetAddresses(long areaId);
}
