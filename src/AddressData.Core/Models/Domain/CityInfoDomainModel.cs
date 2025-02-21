namespace AddressData.Core.Models.Domain;

public record CityInfoDomainModel
{
    public required long AreaId { get; init; }
    public required string City { get; init; }
}
