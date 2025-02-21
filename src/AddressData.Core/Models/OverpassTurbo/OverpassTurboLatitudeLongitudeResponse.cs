namespace AddressData.Core.Models.OverpassTurbo;

using CsvHelper.Configuration.Attributes;

/// <summary>
/// We assume that Overpass Turbo may return nothing.
/// That is why every property is nullable
/// </summary>
public record OverpassTurboLatitudeLongitudeResponse
{
    [Name("@lat")] public string? Latitude { get; init; }
    [Name("@lon")] public string? Longitude { get; init; }
}
