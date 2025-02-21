namespace AddressData.Core.Mappers;

using Models.Data;
using Models.Domain;

public static class DomainToDataMapper
{
    public static CsvAddressesWriteModel Map(AddressesDomainModel domainModel)
        => new()
        {
            HouseNumber = domainModel.HouseNumber,
            Street = domainModel.Street,
            Postcode = domainModel.Postcode,
            Latitude = domainModel.Latitude,
            Longitude = domainModel.Longitude
        };

    public static IEnumerable<CsvAddressesWriteModel> Map(
        IEnumerable<AddressesDomainModel?> domainModels)
    {
        var data = new List<CsvAddressesWriteModel>();

        foreach (var item in domainModels)
        {
            if (item is not null)
            {
                data.Add(Map(item));
            }
        }

        return data;
    }
}
