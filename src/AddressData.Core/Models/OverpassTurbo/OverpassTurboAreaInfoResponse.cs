namespace AddressData.Core.Models.OverpassTurbo;

using System.Text.Json.Serialization;

public record OverpassTurboAreaInfoResponse
{
    [JsonPropertyName("elements")]
    public required List<OverpassTurboAreaElement> Elements { get; init; }
}

public record OverpassTurboAreaElement
{
    [JsonPropertyName("tags")]
    public OverpassTurboAreaTags? Tags { get; init; }
}

public record OverpassTurboAreaTags
{
    [JsonPropertyName("admin_level")]
    public string? AdminLevel { get; init; }

    [JsonPropertyName("name:en")]
    public string? NameEnglish { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
