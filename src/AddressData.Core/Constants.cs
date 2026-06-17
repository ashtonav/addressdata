namespace AddressData.Core;

public static class Constants
{
    public const string OverpassTurboUrl = "https://maps.mail.ru/osm/tools/overpass/api/interpreter";
    public const string OverpassTurboGetAllCitiesQuery =
        "[out:csv(::id,\"name\",\"name:en\";true;\",\")];area[place=\"city\"];out;";
    public static string OverpassTurboGetCityQuery(long areaId) =>
        $"[out:csv(::id,\"name\",\"name:en\";true;\",\")];area({areaId});out;";
    public static string OverpassTurboGetLatitudeLongitudeQuery(long areaId) =>
        $"[out:csv(::lat, ::lon;true;\",\")];area({areaId})->.a;node(area.a);out 1;";
    public static string OverpassTurboGetAddressesQuery(long areaId) =>
        $"[out:csv(::lat, ::lon, \"addr:housenumber\", \"addr:street\", \"addr:postcode\";true;\",\")];area({areaId});nwr(area)[\"addr:housenumber\"][\"addr:street\"][\"addr:postcode\"];out center;";
    public static string OverpassTurboAreaInfoQuery(string latitude, string longitude) =>
        $"[out:json];is_in({latitude},{longitude})->.a;area.a[name][boundary=administrative][admin_level=2];out tags;area.a[name][boundary=administrative][admin_level=4];out tags;";

    public const string ErrorControllerRoute = "error";
    public const int MinimumNumberOfAddresses = 10;
}
