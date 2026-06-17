namespace AddressData.Core.Models.OverpassTurbo;

using CsvHelper.Configuration.Attributes;

public record OverpassTurboCityInfoResponse
{
    [Name("@id")] public long? AreaId { get; init; }
    [Name("name")] public string? City { get; init; }
    [Name("name:en")] public string? CityEnglish { get; init; }
}
