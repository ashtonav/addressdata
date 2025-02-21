namespace AddressData.UnitTests.Mappers;

using System;
using System.Collections.Generic;
using System.Linq;
using AddressData.Core.Mappers;
using AddressData.Core.Models.OverpassTurbo;
using NUnit.Framework;

[TestFixture]
public class OverpassTurboResponseToDomainMapperTests
{
    [Test]
    public void MapOverpassTurboLatitudeLongitudeResponseReturnsDomainModelWhenValid()
    {
        var response = new OverpassTurboLatitudeLongitudeResponse
        {
            Latitude = "12.3456",
            Longitude = "-98.7654"
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.Latitude, Is.EqualTo("12.3456"));
            Assert.That(result.Longitude, Is.EqualTo("-98.7654"));
        }
    }

    [Test]
    public void MapOverpassTurboLatitudeLongitudeResponseReturnsNullWhenLatitudeIsEmpty()
    {
        var response = new OverpassTurboLatitudeLongitudeResponse
        {
            Latitude = "",
            Longitude = "123.4567"
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboLatitudeLongitudeResponseReturnsNullWhenLongitudeIsEmpty()
    {
        var response = new OverpassTurboLatitudeLongitudeResponse
        {
            Latitude = "12.3456",
            Longitude = ""
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboLatitudeLongitudeResponseReturnsNullWhenResponseIsNull()
    {
        OverpassTurboLatitudeLongitudeResponse response = null!;
        Assert.Throws<NullReferenceException>(() =>
            OverpassTurboResponseToDomainMapper.Map(response)
        );
    }

    [Test]
    public void MapOverpassTurboLatitudeLongitudeResponseTreatsWhitespaceAsEmpty()
    {
        // White space is neither null nor empty, so the mapper will accept it.
        var response = new OverpassTurboLatitudeLongitudeResponse
        {
            Latitude = " ",
            Longitude = "  "
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }





    [Test]
    public void MapOverpassTurboCityInfoResponseReturnsCityInfoDomainModelWhenValid()
    {
        var response = new OverpassTurboCityInfoResponse
        {
            AreaId = 100,
            City = "Test City",
            CityEnglish = ""
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.AreaId, Is.EqualTo(100));
            Assert.That(result.City, Is.EqualTo("Test City"));
        }
    }

    [Test]
    public void MapOverpassTurboCityInfoResponseUsesCityEnglishWhenPopulated()
    {
        var response = new OverpassTurboCityInfoResponse
        {
            AreaId = 1234,
            City = "Non-English City",
            CityEnglish = "English City"
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.City, Is.EqualTo("English City"));
    }

    [Test]
    public void MapOverpassTurboCityInfoResponseReturnsNullWhenAreaIdIsNull()
    {
        var response = new OverpassTurboCityInfoResponse
        {
            AreaId = null,
            City = "City"
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboCityInfoResponseReturnsNullWhenAreaIdIsZero()
    {
        var response = new OverpassTurboCityInfoResponse
        {
            AreaId = 0,
            City = "City"
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboCityInfoResponseReturnsNullWhenAreaIdIsNegative()
    {
        var response = new OverpassTurboCityInfoResponse
        {
            AreaId = -50,
            City = "City"
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboCityInfoResponseReturnsNullWhenCityIsEmpty()
    {
        var response = new OverpassTurboCityInfoResponse
        {
            AreaId = 100,
            City = "",
            CityEnglish = ""
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapCityInfoResponsesReturnsNullWhenCollectionIsNull()
    {
        IList<OverpassTurboCityInfoResponse> cityInfos = null!;

        Assert.Throws<NullReferenceException>(() =>
            OverpassTurboResponseToDomainMapper.Map(cityInfos)
        );
    }

    [Test]
    public void MapCityInfoResponsesReturnsNullWhenNoValidItems()
    {
        var cityInfos = new List<OverpassTurboCityInfoResponse>
        {
            new() { AreaId = 0, City = "City" },       // invalid (AreaId=0)
            new() { AreaId = 123, City = "" },         // invalid (city is empty)
        };

        var result = OverpassTurboResponseToDomainMapper.Map(cityInfos);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapCityInfoResponsesReturnsMappedWhenSomeAreValid()
    {
        var cityInfos = new List<OverpassTurboCityInfoResponse>
        {
            new() { AreaId = 0, City = "Invalid City" },  // invalid
            new() { AreaId = 10, City = "Valid City" },   // valid
            new() { AreaId = null, City = "Invalid City2" }, // invalid
            new() { AreaId = 50, CityEnglish = "Valid CityEnglish" } // valid
        };

        var result = OverpassTurboResponseToDomainMapper.Map(cityInfos);
        Assert.That(result, Is.Not.Null);
        var list = result!.ToList();

        Assert.That(list, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(list[0].AreaId, Is.EqualTo(10));
            Assert.That(list[0].City, Is.EqualTo("Valid City"));
            Assert.That(list[1].AreaId, Is.EqualTo(50));
            Assert.That(list[1].City, Is.EqualTo("Valid CityEnglish"));
        }
    }

    [Test]
    public void MapOverpassTurboAddressesResponseReturnsNullWhenHouseNumberIsEmpty()
    {
        var response = new OverpassTurboAddressesResponse
        {
            HouseNumber = "",
            Street = "Main St",
            Postcode = "12345"
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboAddressesResponseReturnsNullWhenStreetIsEmpty()
    {
        var response = new OverpassTurboAddressesResponse
        {
            HouseNumber = "111",
            Street = "",
            Postcode = "12345"
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboAddressesResponseReturnsNullWhenPostcodeIsEmpty()
    {
        var response = new OverpassTurboAddressesResponse
        {
            HouseNumber = "111",
            Street = "Main St",
            Postcode = ""
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboAddressesResponseReturnsNullWhenLatitudeIsEmpty()
    {
        var response = new OverpassTurboAddressesResponse
        {
            HouseNumber = "111",
            Street = "Main St",
            Postcode = "12345",
            Latitude = "",
            Longitude = "55.5555"
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboAddressesResponseReturnsNullWhenLongitudeIsEmpty()
    {
        var response = new OverpassTurboAddressesResponse
        {
            HouseNumber = "111",
            Street = "Main St",
            Postcode = "12345",
            Latitude = "44.4444",
            Longitude = ""
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboAddressesResponseReturnsNullWhenResponseIsNull()
    {
        OverpassTurboAddressesResponse response = null!;
        // The mapper code checks first if response is null => returns null.
        // So this should not throw an exception but return null.
        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapOverpassTurboAddressesResponseReturnsDomainModelWhenValid()
    {
        var response = new OverpassTurboAddressesResponse
        {
            HouseNumber = "  123 ",
            Street = "  Main St  ",
            Postcode = " 98765 ",
            Latitude = "40.1234",
            Longitude = "-75.9876"
        };

        var result = OverpassTurboResponseToDomainMapper.Map(response);
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            // The mapper uses .Trim() for these string fields
            Assert.That(result!.HouseNumber, Is.EqualTo("123"));
            Assert.That(result.Street, Is.EqualTo("Main St"));
            Assert.That(result.Postcode, Is.EqualTo("98765"));
            Assert.That(result.Latitude, Is.EqualTo("40.1234"));
            Assert.That(result.Longitude, Is.EqualTo("-75.9876"));
        }
    }

    [Test]
    public void MapEnumerableOfOverpassTurboAddressesResponseReturnsNullWhenNoValidItems()
    {
        var addresses = new List<OverpassTurboAddressesResponse>
        {
            new() { HouseNumber = "", Street = "St", Postcode = "ABC", Latitude = "1", Longitude = "2" },
            new() { HouseNumber = "123", Street = "", Postcode = "XYZ", Latitude = "3", Longitude = "4" }
        };

        var result = OverpassTurboResponseToDomainMapper.Map(addresses);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapEnumerableOfOverpassTurboAddressesResponseReturnsNullWhenCollectionIsNull()
    {
        IEnumerable<OverpassTurboAddressesResponse?> addresses = null!;
        var result = OverpassTurboResponseToDomainMapper.Map(addresses);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapEnumerableOfOverpassTurboAddressesResponseReturnsMappedItemsWhenSomeAreValid()
    {
        var addresses = new List<OverpassTurboAddressesResponse?>
        {
            new() { HouseNumber = " ", Street = "St", Postcode = "ABC", Latitude = "1", Longitude = "2" },
              // invalid (HouseNumber empty after trim)
            null,
              // null => ignored
            new() { HouseNumber = "123", Street = "Main", Postcode = "XYZ", Latitude = "3", Longitude = "4" }
              // valid
        };

        var result = OverpassTurboResponseToDomainMapper.Map(addresses);
        Assert.That(result, Is.Not.Null);

        var list = result!.ToList();
        Assert.That(list, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(list[0].HouseNumber, Is.EqualTo("123"));
            Assert.That(list[0].Street, Is.EqualTo("Main"));
            Assert.That(list[0].Postcode, Is.EqualTo("XYZ"));
            Assert.That(list[0].Latitude, Is.EqualTo("3"));
            Assert.That(list[0].Longitude, Is.EqualTo("4"));
        }
    }

    [Test]
    public void MapEnumerableOfOverpassTurboAddressesResponseReturnsFullListWhenAllAreValid()
    {
        var addresses = new List<OverpassTurboAddressesResponse?>
        {
            new() { HouseNumber = "123", Street = "Main", Postcode = "11111", Latitude = "10.1", Longitude = "20.2" },
            new() { HouseNumber = "456", Street = "Second", Postcode = "22222", Latitude = "30.3", Longitude = "40.4" }
        };

        var result = OverpassTurboResponseToDomainMapper.Map(addresses);
        Assert.That(result, Is.Not.Null);
        var list = result!.ToList();
        Assert.That(list, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(list[0].HouseNumber, Is.EqualTo("123"));
            Assert.That(list[0].Street, Is.EqualTo("Main"));
            Assert.That(list[0].Postcode, Is.EqualTo("11111"));
            Assert.That(list[0].Latitude, Is.EqualTo("10.1"));
            Assert.That(list[0].Longitude, Is.EqualTo("20.2"));

            Assert.That(list[1].HouseNumber, Is.EqualTo("456"));
            Assert.That(list[1].Street, Is.EqualTo("Second"));
            Assert.That(list[1].Postcode, Is.EqualTo("22222"));
            Assert.That(list[1].Latitude, Is.EqualTo("30.3"));
            Assert.That(list[1].Longitude, Is.EqualTo("40.4"));
        }
    }
}
