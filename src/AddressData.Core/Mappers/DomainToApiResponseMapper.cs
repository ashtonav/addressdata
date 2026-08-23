namespace AddressData.Core.Mappers;

using Models.ApiResponse;
using Models.Domain;

public static class DomainToApiResponseMapper
{
    public static AddressDocumentApiResponse? Map(AddressDocumentDomainModel? domainModel)
    {
        if (domainModel == null)
        {
            return null;
        }

        return new AddressDocumentApiResponse
        {
            City = domainModel.City,
            State = domainModel.State,
            Country = domainModel.Country,
            AreaId = domainModel.AreaId,
            Size = domainModel.Size,
        };
    }

    public static AddressDocumentsApiResponse Map(IEnumerable<AddressDocumentDomainModel?>? domainModelList)
    {
        if (domainModelList is null)
        {
            return new AddressDocumentsApiResponse { Documents = [] };
        }

        return new AddressDocumentsApiResponse
        {
            Documents = domainModelList
                .Select(Map)
                .Where(mapped => mapped is not null)
        };
    }
}
