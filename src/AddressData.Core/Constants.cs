namespace AddressData.Core;

public static class Constants
{
    // Overpass Turbo Queries
    public const string OverpassTurboUrl = "https://maps.mail.ru/osm/tools/overpass/api/interpreter";
    public const string OverpassTurboGetAllCitiesQuery =
        "?data=[out:csv(::id,\"name\",\"name:en\";true;\",\")];area[place=\"city\"];out;";
    public static string OverpassTurboGetCityQuery(long areaId) =>
        $"?data=[out:csv(::id,\"name\",\"name:en\";true;\",\")];area({areaId});out;";
    public static string OverpassTurboGetLatitudeLongitudeQuery(long areaId) =>
        $"?data=[out:csv(::lat, ::lon;true;\",\")];area({areaId})->.a;node(area.a);out 1;";
    public static string OverpassTurboGetAddressesQuery(long areaId) =>
        $"?data=[out:csv(::lat, ::lon, \"addr:housenumber\", \"addr:street\", \"addr:postcode\";true;\",\")];area({areaId});nwr(area)[\"addr:housenumber\"][\"addr:street\"][\"addr:postcode\"];out center;";
    public static string OverpassTurboAreaInfoQuery(string latitude, string longitude) =>
        $"?data=[out:json];is_in({latitude},{longitude})->.a;area.a[name][boundary=administrative][admin_level=2];out tags;area.a[name][boundary=administrative][admin_level=4];out tags;";

    // Overpass Turbo Response
    public const string OverpassTurboResponseElements = "elements";
    public const string OverpassTurboResponseEnglishName = "name:en";
    public const string OverpassTurboResponseName = "name";
    public const string OverpassTurboResponseTags = "tags";
    public const string OverpassTurboResponseAdministrativeLevel = "admin_level";

    // https://wiki.openstreetmap.org/wiki/Tag:boundary%3Dadministrative#:~:text=to%20that%20talk).-,Table%C2%A0%3A%20Admin_level%20for%20all%20countries,-(Edit%20this
    public const string OverpassTurboResponseCountryAdministrationLevel = "2";
    public const string OverpassTurboResponseStateAdministrationLevel = "4";

    // Controllers
    public const string ErrorControllerRoute = "error";

    // Other settings
    public const int MinimumNumberOfAddresses = 50;
    public const int SeedingDelayMs = 1000;
}
