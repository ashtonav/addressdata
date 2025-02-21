namespace AddressData.Core.Models.Domain;

public record StateCountryDomainModel
{
    public required string State { get; init; }
    public required string Country { get; init; }
}
