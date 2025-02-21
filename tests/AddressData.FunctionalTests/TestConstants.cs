namespace AddressData.FunctionalTests;

internal static class TestConstants
{
    // --------------------
    // Endpoints
    // --------------------
    public static string InsertDocumentEndpoint(string areaId) => $"documents/{areaId}";
    public static string SeedDocumentsEndpointWithLimit(string limit) => $"documents/seed?limit={limit}";

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
    public const string AreaIdField = "areaId";
    public const string DocumentsField = "documents";
}
