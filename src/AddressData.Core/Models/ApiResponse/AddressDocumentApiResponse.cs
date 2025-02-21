namespace AddressData.Core.Models.ApiResponse;

public record AddressDocumentApiResponse
{
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Country { get; init; }
    public long? AreaId { get; init; }
    public required long Size { get; init; }
}
