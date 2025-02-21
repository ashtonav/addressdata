namespace AddressData.UnitTests.Mappers;

using System;
using System.Collections.Generic;
using AddressData.Core.Mappers;
using AddressData.Core.Models.Domain;
using NUnit.Framework;

[TestFixture]
public class DomainToApiResponseMapperTests
{
    [Test]
    public void MapMongoDocumentDomainModelReturnsNullWhenDomainModelIsNull()
    {
        AddressDocumentDomainModel domainModel = null!;
        var result = DomainToApiResponseMapper.Map(domainModel);
        Assert.That(result, Is.Null);
    }

    [Test]
    public void MapMongoDocumentDomainModelReturnsExpectedResponseWhenDomainModelIsValid()
    {
        var domainModel = new AddressDocumentDomainModel
        {
            AreaId = 100,
            City = "Test City",
            State = "Test State",
            Country = "Test Country",
            Size = 5
        };

        var result = DomainToApiResponseMapper.Map(domainModel);

        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.AreaId, Is.EqualTo(100));
            Assert.That(result.City, Is.EqualTo("Test City"));
            Assert.That(result.State, Is.EqualTo("Test State"));
            Assert.That(result.Country, Is.EqualTo("Test Country"));
            Assert.That(result.Size, Is.EqualTo(5));
        }
    }

    [Test]
    public void MapMongoDocumentsDomainModelReturnsEmptyListWhenDomainModelListIsEmpty()
    {
        var result = DomainToApiResponseMapper.Map([]);
        Assert.That(result.Documents, Is.Empty);
    }

    [Test]
    public void MapMongoDocumentsDomainModelReturnsMappedDocumentsWhenMultipleValidItems()
    {
        var domainModels = new List<AddressDocumentDomainModel>
        {
            new()
            {
                AreaId = 1,
                City = "City1",
                State = "State1",
                Country = "Country1",
                Size = 11
            },
            new()
            {
                AreaId = 2,
                City = "City2",
                State = "State2",
                Country = "Country2",
                Size = 22
            }
        };

        var result = DomainToApiResponseMapper.Map(domainModels);

        Assert.That(result.Documents, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Documents[0].AreaId, Is.EqualTo(1));
            Assert.That(result.Documents[0].City, Is.EqualTo("City1"));
            Assert.That(result.Documents[0].State, Is.EqualTo("State1"));
            Assert.That(result.Documents[0].Country, Is.EqualTo("Country1"));
            Assert.That(result.Documents[0].Size, Is.EqualTo(11));

            Assert.That(result.Documents[1].AreaId, Is.EqualTo(2));
            Assert.That(result.Documents[1].City, Is.EqualTo("City2"));
            Assert.That(result.Documents[1].State, Is.EqualTo("State2"));
            Assert.That(result.Documents[1].Country, Is.EqualTo("Country2"));
            Assert.That(result.Documents[1].Size, Is.EqualTo(22));
        }
    }

    [Test]
    public void MapMongoDocumentsDomainModelIgnoresNullItems()
    {
        var domainModels = new List<AddressDocumentDomainModel?>
        {
            new()
            {
                AreaId = 1,
                City = "City1",
                State = "State1",
                Country = "Country1",
                Size = 11
            },
            null,
            new()
            {
                AreaId = 3,
                City = "City3",
                State = "State3",
                Country = "Country3",
                Size = 33
            }
        };

        var result = DomainToApiResponseMapper.Map(domainModels);

        Assert.That(result.Documents, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Documents[0].AreaId, Is.EqualTo(1));
            Assert.That(result.Documents[1].AreaId, Is.EqualTo(3));
        }
    }

    [Test]
    public void MapMongoDocumentsDomainModelThrowsExceptionWhenDomainModelListIsNull()
    {
        IEnumerable<AddressDocumentDomainModel> domainModels = null!;
        Assert.Throws<NullReferenceException>(() => DomainToApiResponseMapper.Map(domainModels));
    }
}
