namespace AddressData.Core.Models.Domain;

public record LocationDomainModel
{
    public required long AreaId { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Country { get; init; }
}
