namespace AddressData.Core.Models.OverpassTurbo;

using CsvHelper.Configuration.Attributes;

/// <summary>
/// We assume that Overpass Turbo may return nothing.
/// That is why every property is nullable
/// </summary>
public record OverpassTurboAddressesResponse
{
    [Name("addr:housenumber")] public string? HouseNumber { get; init; }
    [Name("addr:street")] public string? Street { get; init; }
    [Name("addr:postcode")] public string? Postcode { get; init; }
    [Name("@lat")] public string? Latitude { get; init; }
    [Name("@lon")] public string? Longitude { get; init; }

}
