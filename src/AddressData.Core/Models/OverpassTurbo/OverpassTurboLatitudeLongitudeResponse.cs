namespace AddressData.Core.Models.OverpassTurbo;

using CsvHelper.Configuration.Attributes;

public record OverpassTurboLatitudeLongitudeResponse
{
    [Name("@lat")] public string? Latitude { get; init; }
    [Name("@lon")] public string? Longitude { get; init; }
}
