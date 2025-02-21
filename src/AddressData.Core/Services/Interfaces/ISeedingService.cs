namespace AddressData.Core.Services.Interfaces;

using Models.Domain;

public interface ISeedingService
{
    Task<IEnumerable<AddressDocumentDomainModel>> RunSeeding(long? limit);
    Task<AddressDocumentDomainModel> AddCity(long areaId);
}
