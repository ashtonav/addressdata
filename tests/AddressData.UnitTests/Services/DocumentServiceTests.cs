namespace AddressData.UnitTests.Services;
using AddressData.Core.Models.Domain;
using AddressData.Core.Services;
using NUnit.Framework;

[TestFixture]
public class DocumentServiceTests
{
    private string _testRoot;
    private DocumentService _documentService;

    [SetUp]
    public void Setup()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "DocumentServiceTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testRoot);

        // The DocumentService uses hardcoded paths: "output/{country}/{state}..."
        // We'll override the current directory so "output" is created under our test root.
        Directory.SetCurrentDirectory(_testRoot);

        _documentService = new DocumentService();
    }

    [TearDown]
    public void Teardown()
    {
        // Move one level up to avoid "Directory not empty" errors
        Directory.SetCurrentDirectory(Path.GetTempPath());

        // Clean up the test directory
        if (Directory.Exists(_testRoot))
        {
            Directory.Delete(_testRoot, recursive: true);
        }
    }

    [Test]
    public async Task InsertAsyncCreatesCsvFileAndReturnsInsertedDocument()
    {
        // Arrange
        var location = new LocationDomainModel
        {
            City = "TestCity",
            State = "TestState",
            Country = "TestCountry",
            AreaId = 999
        };

        var writeModel = new List<TestCsvRecord>
        {
            new() { SomeField = "Record1" },
            new() { SomeField = "Record2" }
        };

        // Act
        var insertedDoc = await _documentService.InsertAsync(writeModel, location);

        // Assert
        Assert.That(insertedDoc, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(insertedDoc.City, Is.EqualTo("TestCity"));
            Assert.That(insertedDoc.State, Is.EqualTo("TestState"));
            Assert.That(insertedDoc.Country, Is.EqualTo("TestCountry"));
            Assert.That(insertedDoc.AreaId, Is.EqualTo(999));
            Assert.That(insertedDoc.Size, Is.EqualTo(2));
        }

        // 3) Verify file on disk
        var path = Path.Combine("output", location.Country, location.State);
        var fileName = Path.Combine(path, location.City + ".csv");
        Assert.That(File.Exists(fileName), Is.True);

        // Should have 3 lines: 1 header + 2 data rows.
        var lines = await File.ReadAllLinesAsync(fileName);
        Assert.That(lines, Has.Length.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(lines[0], Does.Contain("SomeField"));
            Assert.That(lines[1], Does.Contain("Record1"));
            Assert.That(lines[2], Does.Contain("Record2"));
        }
    }

    [Test]
    public async Task GetAsyncReturnsNullWhenFileDoesNotExist()
    {
        // Arrange
        var location = new LocationDomainModel
        {
            City = "UnknownCity",
            State = "UnknownState",
            Country = "UnknownCountry",
            AreaId = 888
        };

        // Act
        var result = await _documentService.GetAsync(location);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAsyncReturnsAddressDocumentWhenFileExists()
    {
        // Arrange
        var location = new LocationDomainModel
        {
            City = "KnownCity",
            State = "KnownState",
            Country = "KnownCountry",
            AreaId = 555
        };

        // Create the target directory + CSV file
        var dir = Path.Combine("output", location.Country, location.State);
        Directory.CreateDirectory(dir);

        var fileName = Path.Combine(dir, location.City + ".csv");
        // We'll write 1 header + 3 data lines => size=3
        var lines = new[]
        {
            "HeaderLine",
            "DataLine1",
            "DataLine2",
            "DataLine3"
        };
        await File.WriteAllLinesAsync(fileName, lines);

        // Act
        var result = await _documentService.GetAsync(location);

        // Assert
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result!.City, Is.EqualTo("KnownCity"));
            Assert.That(result.State, Is.EqualTo("KnownState"));
            Assert.That(result.Country, Is.EqualTo("KnownCountry"));
            Assert.That(result.AreaId, Is.EqualTo(555));
            // size = totalLines - 1 => 4 - 1 = 3
            Assert.That(result.Size, Is.EqualTo(3));
        }
    }

    [Test]
    public async Task GetAllAsyncReturnsAllDocumentsFromNestedFolders()
    {
        // Suppose we have:
        // output/
        //   CountryA/
        //     StateA/
        //       CityOne.csv
        //       CityTwo.csv
        //   CountryB/
        //     StateX/
        //       CityZ.csv
        // Each .csv will have 1 header + N data lines => size = N

        Directory.CreateDirectory(Path.Combine("output", "CountryA", "StateA"));
        Directory.CreateDirectory(Path.Combine("output", "CountryB", "StateX"));

        // CityOne.csv => 1 header + 2 data => size=2
        var cityOneFile = Path.Combine("output", "CountryA", "StateA", "CityOne.csv");
        await File.WriteAllLinesAsync(cityOneFile, ["Header", "Data1", "Data2"]);

        // CityTwo.csv => 1 header + 1 data => size=1
        var cityTwoFile = Path.Combine("output", "CountryA", "StateA", "CityTwo.csv");
        await File.WriteAllLinesAsync(cityTwoFile, ["Header", "Data3"]);

        // CityZ.csv => 1 header + 3 data => size=3
        var cityZFile = Path.Combine("output", "CountryB", "StateX", "CityZ.csv");
        await File.WriteAllLinesAsync(cityZFile, ["Header", "Row1", "Row2", "Row3"]);

        // Act
        var results = (await _documentService.GetAllAsync()).ToList();

        // Assert
        Assert.That(results, Has.Count.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            // 1) CityOne
            var cityOne = results.FirstOrDefault(r => r.City == "CityOne");
            Assert.That(cityOne, Is.Not.Null);
            Assert.That(cityOne!.State, Is.EqualTo("StateA"));
            Assert.That(cityOne.Country, Is.EqualTo("CountryA"));
            Assert.That(cityOne.Size, Is.EqualTo(2));

            // 2) CityTwo
            var cityTwo = results.FirstOrDefault(r => r.City == "CityTwo");
            Assert.That(cityTwo, Is.Not.Null);
            Assert.That(cityTwo!.State, Is.EqualTo("StateA"));
            Assert.That(cityTwo.Country, Is.EqualTo("CountryA"));
            Assert.That(cityTwo.Size, Is.EqualTo(1));

            // 3) CityZ
            var cityZ = results.FirstOrDefault(r => r.City == "CityZ");
            Assert.That(cityZ, Is.Not.Null);
            Assert.That(cityZ!.State, Is.EqualTo("StateX"));
            Assert.That(cityZ.Country, Is.EqualTo("CountryB"));
            Assert.That(cityZ.Size, Is.EqualTo(3));
        }
    }

    [Test]
    public async Task InsertAsyncOverwritesExistingFileWithSameName()
    {
        // Arrange
        var location = new LocationDomainModel
        {
            City = "OverlapCity",
            State = "OverlapState",
            Country = "OverlapCountry",
            AreaId = 777
        };
        var dir = Path.Combine("output", location.Country, location.State);
        Directory.CreateDirectory(dir);

        var fileName = Path.Combine(dir, location.City + ".csv");

        // Let's pre-create a file with 1 header + 5 data lines
        var existingLines = new string[]
        {
            "HeaderLine",
            "OldData1",
            "OldData2",
            "OldData3",
            "OldData4",
            "OldData5"
        };
        await File.WriteAllLinesAsync(fileName, existingLines);

        // Now we do InsertAsync with 3 new lines. It should overwrite the entire file.
        var newWriteModel = new List<TestCsvRecord>
        {
            new() { SomeField = "NewData1" },
            new() { SomeField = "NewData2" },
            new() { SomeField = "NewData3" }
        };

        // Act
        var doc = await _documentService.InsertAsync(newWriteModel, location);

        // Assert: The new file should have 1 header + 3 lines => size=3
        Assert.That(doc, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(doc.City, Is.EqualTo("OverlapCity"));
            Assert.That(doc.Size, Is.EqualTo(3));
        }
        var newLines = await File.ReadAllLinesAsync(fileName);
        Assert.That(newLines, Has.Length.EqualTo(4)); // 1 header + 3 data
        using (Assert.EnterMultipleScope())
        {
            Assert.That(newLines[0], Does.Contain("SomeField"));
            Assert.That(newLines[1], Does.Contain("NewData1"));
            Assert.That(newLines[2], Does.Contain("NewData2"));
            Assert.That(newLines[3], Does.Contain("NewData3"));
        }
    }
    
    [Test]
    public async Task GetAsyncReturnsZeroSizeWhenCsvHasOnlyHeader()
    {
        // Arrange
        var location = new LocationDomainModel
        {
            City = "HeaderOnlyCity",
            State = "HeaderOnlyState",
            Country = "HeaderOnlyCountry",
            AreaId = 222
        };
        var dir = Path.Combine("output", location.Country, location.State);
        Directory.CreateDirectory(dir);

        var fileName = Path.Combine(dir, $"{location.City}.csv");
        // Only 1 line => the header
        var lines = new[] { "HeaderLine" };
        await File.WriteAllLinesAsync(fileName, lines);

        // Act
        var doc = await _documentService.GetAsync(location);

        // Assert
        Assert.That(doc, Is.Not.Null);
        Assert.That(doc!.Size, Is.EqualTo(0));
    }

    [Test]
    public async Task GetAsyncIgnoresNonCsvFilesInDirectory()
    {
        var location = new LocationDomainModel
        {
            City = "TextFileCity",
            State = "IgnoreTextFiles",
            Country = "DummyCountry",
            AreaId = 333
        };
        var dir = Path.Combine("output", location.Country, location.State);
        Directory.CreateDirectory(dir);

        // Create "TextFileCity.txt" but not "TextFileCity.csv"
        var txtFile = Path.Combine(dir, location.City + ".txt");
        await File.WriteAllTextAsync(txtFile, "This is a text file.");

        // Act
        var doc = await _documentService.GetAsync(location);

        // Assert => Because the .csv does not exist, it returns null
        Assert.That(doc, Is.Null);
    }

    [Test]
    public async Task GetAllAsyncIgnoresFilesWithoutCsvExtension()
    {
        // Arrange
        Directory.CreateDirectory("output");
        var countryDir = Path.Combine("output", "NoCsvCountry");
        var stateDir = Path.Combine(countryDir, "NoCsvState");
        Directory.CreateDirectory(stateDir);

        // Create a .txt file
        var txtFile = Path.Combine(stateDir, "SomeCity.txt");
        await File.WriteAllTextAsync(txtFile, "This is just some text content.");

        // Create a valid CSV
        var csvFile = Path.Combine(stateDir, "RealCity.csv");
        await File.WriteAllLinesAsync(csvFile, ["Header", "DataLine"]);

        // Act
        var results = await _documentService.GetAllAsync();
        var list = new List<AddressDocumentDomainModel>(results);

        // Assert => Only the CSV file is recognized, so we get exactly one result
        Assert.That(list, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(list[0].City, Is.EqualTo("RealCity"));
            Assert.That(list[0].State, Is.EqualTo("NoCsvState"));
            Assert.That(list[0].Country, Is.EqualTo("NoCsvCountry"));
            Assert.That(list[0].Size, Is.EqualTo(1)); // 1 data line, ignoring header
        }
    }

    [Test]
    public async Task GetAllAsyncSkipsEmptyDirectories()
    {
        Directory.CreateDirectory(Path.Combine("output", "SomeCountry", "SomeState"));
        Directory.CreateDirectory(Path.Combine("output", "AnotherCountry"));

        var results = (await _documentService.GetAllAsync()).ToList();
        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task InsertAsyncAllowsCustomIEnumerableTypes()
    {
        // The InsertAsync signature accepts `IEnumerable writeModel`,
        // so it can be any type that implements `IEnumerable`.
        // For instance, an array or a custom collection.

        var location = new LocationDomainModel
        {
            City = "ArrayCity",
            State = "ArrayState",
            Country = "ArrayCountry",
            AreaId = 444
        };

        // Instead of a List<TestCsvRecord>, let's pass a plain array
        TestCsvRecord[] arrayWriteModel =
        [
            new() { SomeField = "ArrayRow1" },
            new() { SomeField = "ArrayRow2" }
        ];

        var doc = await _documentService.InsertAsync(arrayWriteModel, location);

        Assert.That(doc, Is.Not.Null);
        Assert.That(doc!.Size, Is.EqualTo(2));

        var fileName = Path.Combine("output", location.Country, location.State, $"{location.City}.csv");
        var lines = await File.ReadAllLinesAsync(fileName);
        // lines => 1 header + 2 data lines => total 3
        Assert.That(lines, Has.Length.EqualTo(3));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(lines[1], Does.Contain("ArrayRow1"));
            Assert.That(lines[2], Does.Contain("ArrayRow2"));
        }
    }

    /// <summary>
    /// A trivial record used to demonstrate writing CSV rows.
    /// The first row will be the CSV header containing the property name "SomeField".
    /// </summary>
    private sealed record TestCsvRecord
    {
        public string SomeField { get; init; }
    }
}
