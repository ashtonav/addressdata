namespace AddressData.UnitTests.Services;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using AddressData.Core.Models.Domain;
using AddressData.Core.Services;
using AddressData.Core.Services.Interfaces;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

[TestFixture]
public class SeedingServiceTests
{
    private Mock<IOverpassTurboService> _overpassTurboServiceMock;
    private Mock<IDocumentService> _documentServiceMock;
    private Mock<ILogger<SeedingService>> _loggerMock;
    private SeedingService _seedingService;

    [SetUp]
    public void SetUp()
    {
        _overpassTurboServiceMock = new Mock<IOverpassTurboService>(MockBehavior.Strict);
        _documentServiceMock = new Mock<IDocumentService>(MockBehavior.Strict);
        _loggerMock = new Mock<ILogger<SeedingService>>();

        _seedingService = new SeedingService(
            _overpassTurboServiceMock.Object,
            _documentServiceMock.Object,
            _loggerMock.Object
        );
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-50)]
    public void AddCityThrowsValidationExceptionWhenAreaIdIsNonPositive(long invalidAreaId)
    {
        // Arrange
        // The code checks: if (areaId < 0 or 0) => throw ValidationException

        // Act + Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _seedingService.AddCity(invalidAreaId);
        });

        Assert.That(ex!.Message, Does.Contain("valid AreaId"));
    }

    [Test]
    public void AddCityThrowsValidationExceptionWhenCityIsNotFound()
    {
        // Arrange
        long areaId = 123;

        // If overpassTurboService.GetCity(areaId) returns null => throw ValidationException
        _overpassTurboServiceMock
            .Setup(s => s.GetCity(areaId))
            .ReturnsAsync((CityInfoDomainModel)null);

        // Act + Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _seedingService.AddCity(areaId);
        });
        Assert.That(ex!.Message, Does.Contain("not found"));

        _overpassTurboServiceMock.VerifyAll();
    }

    [Test]
    public void AddCityThrowsValidationExceptionWhenCityHasNullName()
    {
        // Arrange
        long areaId = 123;

        // City is returned but city.City = null
        var cityInfo = new CityInfoDomainModel
        {
            AreaId = areaId,
            City = null
        };

        _overpassTurboServiceMock
            .Setup(s => s.GetCity(areaId))
            .ReturnsAsync(cityInfo);

        // Act + Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _seedingService.AddCity(areaId);
        });
        Assert.That(ex!.Message, Does.Contain("not found"));

        _overpassTurboServiceMock.VerifyAll();
    }

    [Test]
    public void AddCityThrowsValidationExceptionWhenAddressesAreNull()
    {
        // Arrange
        long areaId = 456;

        var cityInfo = new CityInfoDomainModel
        {
            AreaId = areaId,
            City = "ValidCity"
        };

        _overpassTurboServiceMock
            .Setup(s => s.GetCity(areaId))
            .ReturnsAsync(cityInfo);

        // GetAddresses returns null => "The number of addresses in X is small."
        _overpassTurboServiceMock
            .Setup(s => s.GetAddresses(areaId))
            .ReturnsAsync((IEnumerable<AddressesDomainModel>)null);

        // Act + Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _seedingService.AddCity(areaId);
        });
        Assert.That(ex!.Message, Does.Contain("small"));

        _overpassTurboServiceMock.VerifyAll();
    }

    [Test]
    public void AddCityThrowsValidationExceptionWhenAddressesCountBelowMinimum()
    {
        // Arrange
        long areaId = 789;

        // Valid city
        var cityInfo = new CityInfoDomainModel { AreaId = areaId, City = "CityName" };
        _overpassTurboServiceMock
            .Setup(s => s.GetCity(areaId))
            .ReturnsAsync(cityInfo);

        // only 1 address => below the required minimum
        var fewAddresses = new List<AddressesDomainModel>
        {
            new()
            {
                HouseNumber = "1",
                Street = "ShortStreet",
                Postcode = "00001",
                Latitude = "12.3456",
                Longitude = "65.4321"
            }
        };

        _overpassTurboServiceMock
            .Setup(s => s.GetAddresses(areaId))
            .ReturnsAsync(fewAddresses);

        // Act + Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _seedingService.AddCity(areaId);
        });
        Assert.That(ex!.Message, Does.Contain("small"));

        _overpassTurboServiceMock.VerifyAll();
    }

    [Test]
    public void AddCityThrowsValidationExceptionWhenLocationIsNull()
    {
        // Arrange
        long areaId = 234;

        // Valid city
        var cityInfo = new CityInfoDomainModel { AreaId = areaId, City = "CityX" };
        _overpassTurboServiceMock
            .Setup(s => s.GetCity(areaId))
            .ReturnsAsync(cityInfo);

        // Enough addresses => passes addresses check
        var addresses = CreateManyAddresses();
        _overpassTurboServiceMock
            .Setup(s => s.GetAddresses(areaId))
            .ReturnsAsync(addresses);

        // getLocation => returns null => "The city description in X is invalid."
        _overpassTurboServiceMock
            .Setup(s => s.GetLocation(areaId))
            .ReturnsAsync((LocationDomainModel)null);

        // Act + Assert
        var ex = Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _seedingService.AddCity(areaId);
        });
        Assert.That(ex!.Message, Does.Contain("invalid"));

        _overpassTurboServiceMock.VerifyAll();
    }

    [Test]
    public async Task AddCityReturnsDocumentFromDocumentServiceWhenAllValid()
    {
        // Arrange
        long areaId = 111;

        var cityInfo = new CityInfoDomainModel
        {
            AreaId = areaId,
            City = "TestCity"
        };
        _overpassTurboServiceMock
            .Setup(s => s.GetCity(areaId))
            .ReturnsAsync(cityInfo);

        // Must have >= MINIMUM_NUMBER_OF_ADDRESSES
        var addresses = CreateManyAddresses();

        _overpassTurboServiceMock
            .Setup(s => s.GetAddresses(areaId))
            .ReturnsAsync(addresses);

        // location => must not be null
        var location = new LocationDomainModel
        {
            City = "TestCity",
            State = "TestState",
            Country = "TestCountry",
            AreaId = areaId
        };
        _overpassTurboServiceMock
            .Setup(s => s.GetLocation(areaId))
            .ReturnsAsync(location);

        // InsertAsync => returns an AddressDocumentDomainModel
        var insertedDoc = new AddressDocumentDomainModel
        {
            City = "TestCity",
            State = "TestState",
            Country = "TestCountry",
            AreaId = areaId,
            Size = 999
        };
        _documentServiceMock
            .Setup(s => s.InsertAsync(
                It.IsAny<IEnumerable>(),
                It.Is<LocationDomainModel>(l => l.City == "TestCity")))
            .ReturnsAsync(insertedDoc);

        // Act
        var result = await _seedingService.AddCity(areaId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Size, Is.EqualTo(999));

        _overpassTurboServiceMock.VerifyAll();
        _documentServiceMock.VerifyAll();
    }

    [Test]
    public async Task RunSeedingLogsInformationAndReturnsEmptyIfNoCitiesReturned()
    {
        // Arrange
        _overpassTurboServiceMock
            .Setup(s => s.GetCities())
            .ReturnsAsync([]);

        // Act
        var result = await _seedingService.RunSeeding(null);

        // Assert => we expect an empty result
        Assert.That(result, Is.Empty);

        // Optionally verify logger call
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Running seeding. All cities count: 0")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _overpassTurboServiceMock.VerifyAll();
    }

    [Test]
    public async Task RunSeedingStopsWhenLimitReached()
    {
        // Arrange
        var cityList = new List<CityInfoDomainModel>
        {
            new() { AreaId = 1, City = "CityOne" },
            new() { AreaId = 2, City = "CityTwo" },
            new() { AreaId = 3, City = "CityThree" }
        };
        _overpassTurboServiceMock
            .Setup(s => s.GetCities())
            .ReturnsAsync(cityList);

        // We'll set limit=1 => it should process only the first city
        // We'll mock success for the first city
        SetupMockForAddCitySuccess(1, "CityOne", "CityOneDoc");
        // No need to set up for CityTwo or CityThree => won't be called

        // Act
        var result = await _seedingService.RunSeeding(1);

        using (Assert.EnterMultipleScope())
        {
            // Assert => only one city was added
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().City, Is.EqualTo("CityOneDoc"));
        }

        _overpassTurboServiceMock.VerifyAll();
        _documentServiceMock.VerifyAll();
    }

    [Test]
    public async Task RunSeedingCallsAddCityForEachAndLogsErrorsOnFailures()
    {
        // Arrange
        var cityList = new List<CityInfoDomainModel>
        {
            new() { AreaId = 11, City = "GoodCity" },
            new() { AreaId = 22, City = "BadCity" },
        };
        _overpassTurboServiceMock
            .Setup(s => s.GetCities())
            .ReturnsAsync(cityList);

        // We'll mock so that GoodCity and AnotherGoodCity succeed, but BadCity fails
        SetupMockForAddCitySuccess(11, "GoodCity", "GoodCityDoc");
        SetupMockForAddCityFailure(22, "BadCity");

        // Act
        var result = await _seedingService.RunSeeding(null);

        // Assert => 1 success, 1 fail => does not stop on fail
        Assert.That(result.Count(), Is.EqualTo(1));

        var sorted = result.OrderBy(r => r.City).ToList();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(sorted[0].City, Is.EqualTo("GoodCityDoc"));
        }

        // Verify error log for failing city
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to add city: BadCity with area id: 22")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _overpassTurboServiceMock.VerifyAll();
        _documentServiceMock.VerifyAll();
    }

    [Test]
    public void AddCityThrowsExceptionIfInsertAsyncFails()
    {
        // Arrange
        long areaId = 9999;

        var cityInfo = new CityInfoDomainModel
        {
            AreaId = areaId,
            City = "ExceptionalCity"
        };
        _overpassTurboServiceMock
            .Setup(s => s.GetCity(areaId))
            .ReturnsAsync(cityInfo);

        // Addresses >= minimum => passes
        var addresses = CreateManyAddresses();
        _overpassTurboServiceMock
            .Setup(s => s.GetAddresses(areaId))
            .ReturnsAsync(addresses);

        // Location => not null => passes
        var location = new LocationDomainModel
        {
            City = "ExceptionalCity",
            State = "ExceptionalState",
            Country = "ExceptionalCountry",
            AreaId = areaId
        };
        _overpassTurboServiceMock
            .Setup(s => s.GetLocation(areaId))
            .ReturnsAsync(location);

        // InsertAsync => throws e.g. InvalidOperationException
        _documentServiceMock
            .Setup(s => s.InsertAsync(It.IsAny<IEnumerable>(), location))
            .ThrowsAsync(new InvalidOperationException("Disk error"));

        // Act + Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _seedingService.AddCity(areaId);
        });
        Assert.That(ex!.Message, Does.Contain("Disk error"));

        _overpassTurboServiceMock.VerifyAll();
        _documentServiceMock.VerifyAll();
    }

    [Test]
    public async Task AddCityCalledTwiceForSameAreaIdProducesSameResultIfDocumentServiceAlwaysSucceeds()
    {
        // Arrange
        long areaId = 2222;

        var cityInfo = new CityInfoDomainModel { AreaId = areaId, City = "RepeatedCity" };
        _overpassTurboServiceMock
            .SetupSequence(s => s.GetCity(areaId))
            .ReturnsAsync(cityInfo)     // 1st call
            .ReturnsAsync(cityInfo);    // 2nd call

        var addresses = CreateManyAddresses();

        _overpassTurboServiceMock
            .SetupSequence(s => s.GetAddresses(areaId))
            .ReturnsAsync(addresses)     // 1st call
            .ReturnsAsync(addresses);    // 2nd call

        var location = new LocationDomainModel
        {
            City = "RepeatedCity",
            State = "RepeatedState",
            Country = "RepeatedCountry",
            AreaId = areaId
        };
        _overpassTurboServiceMock
            .SetupSequence(s => s.GetLocation(areaId))
            .ReturnsAsync(location)      // 1st call
            .ReturnsAsync(location);     // 2nd call

        var insertedDoc = new AddressDocumentDomainModel
        {
            City = "RepeatedCity",
            State = "RepeatedState",
            Country = "RepeatedCountry",
            AreaId = areaId,
            Size = 123
        };
        _documentServiceMock
            .SetupSequence(s => s.InsertAsync(It.IsAny<IEnumerable>(), location))
            .ReturnsAsync(insertedDoc)   // 1st call
            .ReturnsAsync(insertedDoc);  // 2nd call

        // Act
        var first = await _seedingService.AddCity(areaId);
        var second = await _seedingService.AddCity(areaId);

        using (Assert.EnterMultipleScope())
        {
            // Assert => Both calls produce the same result in this scenario
            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first!.Size, Is.EqualTo(second!.Size));
            Assert.That(first.City, Is.EqualTo(second.City));
        }

        _overpassTurboServiceMock.VerifyAll();
        _documentServiceMock.VerifyAll();
    }

    [Test]
    public async Task RunSeedingReturnsEmptyWhenLimitIsZero()
    {
        // Arrange
        // If limit=0 => we never add any city => result is empty
        var cityList = new List<CityInfoDomainModel>
        {
            new() { AreaId = 10, City = "CityOne" },
            new() { AreaId = 20, City = "CityTwo" }
        };
        _overpassTurboServiceMock
            .Setup(s => s.GetCities())
            .ReturnsAsync(cityList);

        // Act
        var result = await _seedingService.RunSeeding(0);

        // Assert
        Assert.That(result, Is.Empty);

        _overpassTurboServiceMock.VerifyAll();
        _documentServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public void RunSeedingThrowsExceptionIfGetCitiesFails()
    {
        // Arrange
        _overpassTurboServiceMock
            .Setup(s => s.GetCities())
            .ThrowsAsync(new InvalidOperationException("OverpassTurboService error"));

        // Act + Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _seedingService.RunSeeding(null);
        });
        Assert.That(ex!.Message, Does.Contain("OverpassTurboService error"));

        _overpassTurboServiceMock.VerifyAll();
    }

    /// <summary>
    /// Helper that sets up the mocks for a successful AddCity scenario for the given areaId and cityName.
    /// The returned AddressDocumentDomainModel will have .City set to 'returnCityDoc'.
    /// </summary>
    private void SetupMockForAddCitySuccess(long areaId, string cityName, string returnCityDoc)
    {
        // 1) GetCity => non-null city => cityName
        _overpassTurboServiceMock
            .Setup(s => s.GetCity(areaId))
            .ReturnsAsync(new CityInfoDomainModel { AreaId = areaId, City = cityName });

        // 2) GetAddresses => Must have >= MINIMUM_NUMBER_OF_ADDRESSES
        var addresses = CreateManyAddresses();

        _overpassTurboServiceMock
            .Setup(s => s.GetAddresses(areaId))
            .ReturnsAsync(addresses);

        // 3) GetLocation => non-null
        _overpassTurboServiceMock
            .Setup(s => s.GetLocation(areaId))
            .ReturnsAsync(new LocationDomainModel
            {
                City = cityName,
                State = "AnyState",
                Country = "AnyCountry",
                AreaId = areaId
            });

        // 4) DocumentService.InsertAsync => returns doc with .City = returnCityDoc
        var doc = new AddressDocumentDomainModel
        {
            City = returnCityDoc,
            State = "AnyState",
            Country = "AnyCountry",
            AreaId = areaId,
            Size = 123
        };

        _documentServiceMock
            .Setup(s => s.InsertAsync(
                It.IsAny<IEnumerable>(),
                It.IsAny<LocationDomainModel>()))
            .ReturnsAsync(doc);
    }

    /// <summary>
    /// Helper that sets up mocks so that AddCity will fail for the given areaId, cityName by throwing a ValidationException.
    /// We'll specifically fail at the addresses step: returning fewer than MINIMUM_NUMBER_OF_ADDRESSES.
    /// </summary>
    private void SetupMockForAddCityFailure(long areaId, string cityName)
    {
        // City => valid
        _overpassTurboServiceMock
            .Setup(s => s.GetCity(areaId))
            .ReturnsAsync(new CityInfoDomainModel { AreaId = areaId, City = cityName });

        // Addresses => only 1 => triggers "number of addresses is small" exception
        var insufficientAddresses = new List<AddressesDomainModel>
        {
            new()
            {
                HouseNumber = "OnlyOne",
                Street = "FailStreet",
                Postcode = "FailPost",
                Latitude = "FailLat",
                Longitude = "FailLon"
            }
        };
        _overpassTurboServiceMock
            .Setup(s => s.GetAddresses(areaId))
            .ReturnsAsync(insufficientAddresses);
    }

    private static IEnumerable<AddressesDomainModel> CreateManyAddresses()
    {
        var fixture = new Fixture();
        return fixture.CreateMany<AddressesDomainModel>(100);
    }
}
