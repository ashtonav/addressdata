namespace AddressData.FunctionalTests;

internal static class TestConstants
{
    // --------------------
    // Endpoints
    // --------------------
    public static string InsertDocumentEndpoint(string areaId) => $"documents/{areaId}";
    public static string SeedDocumentsEndpointWithLimit(string limit) => $"documents/seed?limit={limit}";
    public const string DocumentsEndpoint = "documents";

    // --------------------
    // ScenarioContext Keys
    // --------------------
    public const string Response = "Response";
    public const string InsertResponse = "InsertResponse";
    public const string GetResponse = "GetResponse";

    // --------------------
    // JSON Fields
    // --------------------
    public const string CityField = "city";
    public const string CountryField = "country";
    public const string AreaIdField = "areaId";
    public const string SizeField = "size";
    public const string DocumentsField = "documents";
}
