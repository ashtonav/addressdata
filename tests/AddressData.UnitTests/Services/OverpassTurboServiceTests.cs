namespace AddressData.UnitTests.Services;
using AddressData.Core.Services;
using AddressData.UnitTests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

[TestFixture]
public class OverpassTurboServiceTests
{
    private Mock<IHttpClientFactory> _httpClientFactoryMock;
    private Mock<ILogger<OverpassTurboService>> _loggerMock;

    [SetUp]
    public void SetUp()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<OverpassTurboService>>();
    }

    [Test]
    public async Task GetLocationReturnsNullIfJsonHasNoAdminLevelProperty()
    {
        var responses = new Queue<HttpResponseMessage>();

        // 1) Valid lat/long CSV
        var latLongCsv = "@lat,@lon\r\n12.3456,-98.7654\r\n";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(latLongCsv, "text/csv"));

        // 2) JSON missing admin_level => can't parse state/country => returns null
        var json = /*lang=json,strict*/ @"{
                ""elements"": [
                    {
                        ""tags"": {
                            ""name:en"": ""WeHaveNoAdminLevel""
                        }
                    }
                ]
            }";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(json, "application/json"));

        // 3) City info => valid
        var cityCsv = "@id,name,name:en\r\n123,LocalCity,\r\n";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(cityCsv, "text/csv"));

        var httpClient = HttpClientHelper.CreateHttpClient(responses);
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var service = new OverpassTurboService(_httpClientFactoryMock.Object, _loggerMock.Object);

        // Act
        var result = await service.GetLocation(123);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetLocationReturnsNullIfJsonHasPartialAdminLevelsButNoStateOrCountry()
    {
        var responses = new Queue<HttpResponseMessage>();

        // 1) Valid lat/long
        var latLongCsv = "@lat,@lon\r\n12.3456,-98.7654\r\n";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(latLongCsv, "text/csv"));

        // 2) JSON has only admin_level=4 => "Test State" but missing the admin_level=2 => country
        var json = /*lang=json,strict*/ @"{
                ""elements"": [
                    {
                        ""tags"": {
                            ""admin_level"": ""4"",
                            ""name:en"": ""Test State""
                        }
                    }
                ]
            }";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(json, "application/json"));

        // 3) city info => valid => areaId=123
        var cityCsv = "@id,name,name:en\r\n123,LocalCity,\r\n";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(cityCsv, "text/csv"));

        var httpClient = HttpClientHelper.CreateHttpClient(responses);
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var service = new OverpassTurboService(_httpClientFactoryMock.Object, _loggerMock.Object);
        var result = await service.GetLocation(123);

        // No country found => returns null
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetCitiesReturnsValidRowsAndIgnoresInvalidRows()
    {
        // Mix of valid & invalid rows:
        // Row1 => areaId=10 => city=TestCity => valid
        // Row2 => areaId=0 => invalid
        // Row3 => areaId=99 => name= => name:en=English99 => city => "English99" => valid
        var csvContent =
            "@id,name,name:en\r\n" +
            "10,TestCity,\r\n" +
            "0,IgnoreCity,\r\n" +
            "99,,English99\r\n";

        var httpClient = HttpClientHelper.CreateHttpClient(csvContent, "text/csv");
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var service = new OverpassTurboService(_httpClientFactoryMock.Object, _loggerMock.Object);
        var result = await service.GetCities();

        Assert.That(result, Is.Not.Null);
        var list = result!.ToList();
        Assert.That(list, Has.Count.EqualTo(2)); // only two valid rows
        using (Assert.EnterMultipleScope())
        {
            Assert.That(list[0].AreaId, Is.EqualTo(10));
            Assert.That(list[0].City, Is.EqualTo("TestCity"));
            Assert.That(list[1].AreaId, Is.EqualTo(99));
            Assert.That(list[1].City, Is.EqualTo("English99"));
        }
    }





    [Test]
    public async Task GetCityLogsErrorAndReturnsNullWhenCsvDataIsMalformed()
    {
        // We simulate a malformed CSV that can't be parsed by CsvHelper.
        // e.g., mismatching columns or an incomplete row, etc.
        // This might cause an exception in 'FetchDataSingle<T>'.
        var csvContent = "@id,name\r\n123"; // Missing row break or columns
        var httpClient = HttpClientHelper.CreateHttpClient(csvContent, "text/csv");
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var service = new OverpassTurboService(_httpClientFactoryMock.Object, _loggerMock.Object);

        var result = await service.GetCity(123);

        Assert.That(result, Is.Null);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("An error occured while fetching data from Overpass Turbo")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once
        );
    }

    [Test]
    public async Task GetLocationReturnsNullIfNoStateOrCountryParsed()
    {
        var responses = new Queue<HttpResponseMessage>();

        // 1) lat/long => valid
        var latLongCsv = "@lat,@lon\r\n45.0000,90.0000\r\n";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(latLongCsv, "text/csv"));

        // 2) empty JSON => can't find state/country => returns null
        var json = /*lang=json,strict*/ @"{ ""elements"": [] }";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(json, "application/json"));

        // 3) city => valid => areaId=100
        var cityCsv = "@id,name,name:en\r\n100,SomeCity,\r\n";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(cityCsv, "text/csv"));

        var httpClient = HttpClientHelper.CreateHttpClient(responses);
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var service = new OverpassTurboService(_httpClientFactoryMock.Object, _loggerMock.Object);
        var locationResult = await service.GetLocation(100);

        // Because state/country not found => null
        Assert.That(locationResult, Is.Null);
    }

    [Test]
    public async Task GetLocationReturnsNullIfCityIsNull()
    {
        var responses = new Queue<HttpResponseMessage>();

        // 1) lat/long => valid
        var latLongCsv = "@lat,@lon\r\n45.0000,90.0000\r\n";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(latLongCsv, "text/csv"));

        // 2) JSON => valid => returns state=TestState, country=TestCountry
        var json = /*lang=json,strict*/ @"{
                ""elements"": [
                    {
                        ""tags"": {
                            ""admin_level"": ""4"",
                            ""name:en"": ""TestState""
                        }
                    },
                    {
                        ""tags"": {
                            ""admin_level"": ""2"",
                            ""name:en"": ""TestCountry""
                        }
                    }
                ]
            }";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(json, "application/json"));

        // 3) city => invalid => areaId=0 => mapper => null
        var cityCsv = "@id,name,name:en\r\n0,NoCity,\r\n";
        responses.Enqueue(HttpClientHelper.CreateHttpResponse(cityCsv, "text/csv"));

        var httpClient = HttpClientHelper.CreateHttpClient(responses);
        _httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var service = new OverpassTurboService(_httpClientFactoryMock.Object, _loggerMock.Object);
        var locationResult = await service.GetLocation(0);

        Assert.That(locationResult, Is.Null);
    }
}
