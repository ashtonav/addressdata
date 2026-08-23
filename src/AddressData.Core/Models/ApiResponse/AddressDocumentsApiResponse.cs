namespace AddressData.Core.Models.ApiResponse;

public record AddressDocumentsApiResponse
{
    public required IEnumerable<AddressDocumentApiResponse?> Documents { get; init; }
}
