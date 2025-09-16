namespace AddressData.Core.Services.Interfaces;

using System.Collections;
using Models.Domain;

public interface IDocumentService
{
    public Task<AddressDocumentDomainModel> InsertAsync(IEnumerable writeModel, LocationDomainModel location);
    public Task<AddressDocumentDomainModel?> GetAsync(LocationDomainModel location);
    public Task<IEnumerable<AddressDocumentDomainModel>> GetAllAsync();
}
