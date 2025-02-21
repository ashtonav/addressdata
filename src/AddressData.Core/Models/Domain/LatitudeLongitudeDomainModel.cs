namespace AddressData.Core.Models.Domain;

public record LatitudeLongitudeDomainModel
{
    public required string Latitude { get; init; }
    public required string Longitude { get; init; }
}
