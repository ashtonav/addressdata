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

    public static AddressDocumentsApiResponse Map(IEnumerable<AddressDocumentDomainModel?> domainModelList)
    {
        var response = new AddressDocumentsApiResponse { Documents = [] };

        foreach (var domainModel in domainModelList)
        {
            var mapped = Map(domainModel);
            if (mapped != null)
            {
                response.Documents.Add(mapped);
            }
        }

        return response;
    }

}
