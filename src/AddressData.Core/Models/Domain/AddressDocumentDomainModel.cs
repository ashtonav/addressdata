namespace AddressData.Core.Models.Domain;

public record AddressDocumentDomainModel
{
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Country { get; init; }
    public long? AreaId { get; init; }
    public required long Size { get; init; }
}
