namespace AddressData.Core.Models.Data;

using CsvHelper.Configuration.Attributes;

/// <summary>
/// This model is used to write CSV files.
/// </summary>
public record CsvAddressesWriteModel
{
    [Name("House number")] public required string HouseNumber { get; init; }
    [Name("Street name")] public required string Street { get; init; }
    [Name("Postal code")] public required string Postcode { get; init; }
    [Name("Latitude")] public required string Latitude { get; init; }
    [Name("Longitude")] public required string Longitude { get; init; }
}
