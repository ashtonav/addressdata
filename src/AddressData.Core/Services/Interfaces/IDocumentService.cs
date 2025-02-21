namespace AddressData.Core.Services.Interfaces;

using System.Collections;
using Models.Domain;

public interface IDocumentService
{
    Task<AddressDocumentDomainModel> InsertAsync(IEnumerable writeModel, LocationDomainModel location);
    Task<AddressDocumentDomainModel?> GetAsync(LocationDomainModel location);
    Task<IEnumerable<AddressDocumentDomainModel>> GetAllAsync();
}
