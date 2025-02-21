namespace AddressData.UnitTests.Mappers;

using System;
using System.Collections.Generic;
using System.Linq;
using AddressData.Core.Mappers;
using AddressData.Core.Models.Domain;
using NUnit.Framework;

[TestFixture]
public class DomainToDataMapperTests
{
    [Test]
    public void MapSingleAddressesDomainModelThrowsNullReferenceExceptionWhenNull()
    {
        AddressesDomainModel domainModel = null!;
        Assert.Throws<NullReferenceException>(() => DomainToDataMapper.Map(domainModel));
    }

    [Test]
    public void MapSingleAddressesDomainModelReturnsCsvAddressesWriteModelWhenValid()
    {
        // Arrange
        var domainModel = new AddressesDomainModel
        {
            HouseNumber = "123",
            Street = "Main St",
            Postcode = "XYZ987",
            Latitude = "51.5074",
            Longitude = "-0.1278"
        };

        // Act
        var result = DomainToDataMapper.Map(domainModel);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.HouseNumber, Is.EqualTo("123"));
            Assert.That(result.Street, Is.EqualTo("Main St"));
            Assert.That(result.Postcode, Is.EqualTo("XYZ987"));
            Assert.That(result.Latitude, Is.EqualTo("51.5074"));
            Assert.That(result.Longitude, Is.EqualTo("-0.1278"));
        }
    }

    [Test]
    public void MapEnumerableOfAddressesDomainModelThrowsNullReferenceExceptionWhenNull()
    {
        IEnumerable<AddressesDomainModel> domainModels = null!;
        Assert.Throws<NullReferenceException>(() => DomainToDataMapper.Map(domainModels));
    }

    [Test]
    public void MapEnumerableOfAddressesDomainModelReturnsEmptyWhenInputIsEmpty()
    {
        var result = DomainToDataMapper
            .Map([])
            .ToList();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void MapEnumerableOfAddressesDomainModelReturnsMappedCollectionWhenValid()
    {
        var domainModels = new List<AddressesDomainModel>
        {
            new()
            {
                HouseNumber = "10A",
                Street = "Street A",
                Postcode = "A1A1A1",
                Latitude = "10.00",
                Longitude = "20.00"
            },
            new()
            {
                HouseNumber = "11B",
                Street = "Street B",
                Postcode = "B2B2B2",
                Latitude = "-10.50",
                Longitude = "50.25"
            }
        };

        var result = DomainToDataMapper.Map(domainModels).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            // First
            Assert.That(result[0].HouseNumber, Is.EqualTo("10A"));
            Assert.That(result[0].Street, Is.EqualTo("Street A"));
            Assert.That(result[0].Postcode, Is.EqualTo("A1A1A1"));
            Assert.That(result[0].Latitude, Is.EqualTo("10.00"));
            Assert.That(result[0].Longitude, Is.EqualTo("20.00"));

            // Second
            Assert.That(result[1].HouseNumber, Is.EqualTo("11B"));
            Assert.That(result[1].Street, Is.EqualTo("Street B"));
            Assert.That(result[1].Postcode, Is.EqualTo("B2B2B2"));
            Assert.That(result[1].Latitude, Is.EqualTo("-10.50"));
            Assert.That(result[1].Longitude, Is.EqualTo("50.25"));
        }
    }

    [Test]
    public void MapEnumerableOfAddressesDomainModelIgnoresNullItems()
    {
        var domainModels = new List<AddressesDomainModel?>
        {
            new()
            {
                HouseNumber = "10A",
                Street = "Street A",
                Postcode = "A1A1A1",
                Latitude = "10.00",
                Longitude = "20.00"
            },
            null,
            new()
            {
                HouseNumber = "99Z",
                Street = "Street Z",
                Postcode = "Z9Z9Z9",
                Latitude = "90.00",
                Longitude = "-90.00"
            }
        };

        var result = DomainToDataMapper.Map(domainModels).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].HouseNumber, Is.EqualTo("10A"));
            Assert.That(result[1].HouseNumber, Is.EqualTo("99Z"));
        }
    }
}
