namespace AddressData.Core.Models.Domain;

public record AddressesDomainModel
{
    public required string HouseNumber { get; init; }
    public required string Street { get; init; }
    public required string Postcode { get; init; }
    public required string Latitude { get; init; }
    public required string Longitude { get; init; }
}
