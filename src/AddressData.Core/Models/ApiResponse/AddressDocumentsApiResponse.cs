namespace AddressData.Core.Models.ApiResponse;

public record AddressDocumentsApiResponse
{
    public required IList<AddressDocumentApiResponse> Documents { get; init; }
}
