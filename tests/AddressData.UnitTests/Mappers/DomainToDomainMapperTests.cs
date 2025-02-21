namespace AddressData.UnitTests.Mappers;

using System;
using AddressData.Core.Mappers;
using AddressData.Core.Models.Domain;
using NUnit.Framework;

[TestFixture]
public class DomainToDomainMapperTests
{
    [Test]
    public void MapCityInfoAndStateCountryReturnsLocationDomainModelWhenBothAreValid()
    {
        var cityInfo = new CityInfoDomainModel
        {
            AreaId = 123,
            City = "Test City"
        };

        var stateCountry = new StateCountryDomainModel
        {
            State = "Test State",
            Country = "Test Country"
        };

        var result = DomainToDomainMapper.Map(cityInfo, stateCountry);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.AreaId, Is.EqualTo(123));
            Assert.That(result.City, Is.EqualTo("Test City"));
            Assert.That(result.State, Is.EqualTo("Test State"));
            Assert.That(result.Country, Is.EqualTo("Test Country"));
        }
    }

    [Test]
    public void MapCityInfoAndStateCountryThrowsNullReferenceExceptionWhenCityInfoIsNull()
    {
        CityInfoDomainModel cityInfo = null!;
        var stateCountry = new StateCountryDomainModel
        {
            State = "State",
            Country = "Country"
        };

        Assert.Throws<NullReferenceException>(() => DomainToDomainMapper.Map(cityInfo, stateCountry));
    }

    [Test]
    public void MapCityInfoAndStateCountryThrowsNullReferenceExceptionWhenStateCountryIsNull()
    {
        var cityInfo = new CityInfoDomainModel
        {
            AreaId = 123,
            City = "City"
        };
        StateCountryDomainModel stateCountry = null!;

        Assert.Throws<NullReferenceException>(() => DomainToDomainMapper.Map(cityInfo, stateCountry));
    }
}
