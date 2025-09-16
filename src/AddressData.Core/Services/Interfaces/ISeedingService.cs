namespace AddressData.Core.Services.Interfaces;

using Models.Domain;

public interface ISeedingService
{
    public Task<IEnumerable<AddressDocumentDomainModel>> RunSeeding(long? limit);
    public Task<AddressDocumentDomainModel> AddCity(long areaId);
}
