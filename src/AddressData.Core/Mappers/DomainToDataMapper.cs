namespace AddressData.Core.Mappers;

using Models.Data;
using Models.Domain;

public static class DomainToDataMapper
{
    public static CsvAddressesWriteModel? Map(AddressesDomainModel? domainModel)
        => domainModel is null
            ? null
            : new CsvAddressesWriteModel
            {
                HouseNumber = domainModel.HouseNumber,
                Street = domainModel.Street,
                Postcode = domainModel.Postcode,
                Latitude = domainModel.Latitude,
                Longitude = domainModel.Longitude
            };

    public static IEnumerable<CsvAddressesWriteModel?>? Map(
        IEnumerable<AddressesDomainModel?>? domainModels)
        => domainModels?
            .Where(item => item is not null)
            .Select(Map);
}
