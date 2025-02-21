namespace AddressData.Core.Models.OverpassTurbo;

using CsvHelper.Configuration.Attributes;

/// <summary>
/// We assume that Overpass Turbo may return nothing.
/// That is why every property is nullable
/// </summary>
public record OverpassTurboCityInfoResponse
{
    [Name("@id")] public long? AreaId { get; init; }
    [Name("name")] public string? City { get; init; }
    [Name("name:en")] public string? CityEnglish { get; init; }
}
