namespace AddressData.Core.Services.Interfaces;

using Models.Domain;

public interface IOverpassTurboService
{
    public Task<IEnumerable<CityInfoDomainModel>?> GetCities();
    public Task<CityInfoDomainModel?> GetCity(long areaId);
    public Task<LocationDomainModel?> GetLocation(long areaId);
    public Task<IEnumerable<AddressesDomainModel>?> GetAddresses(long areaId);
}
