namespace AddressData.Core.Services;

using System.Collections;
using System.Globalization;
using CsvHelper;
using Interfaces;
using Models.Domain;

public class DocumentService : IDocumentService
{
    public async Task<AddressDocumentDomainModel> InsertAsync(IEnumerable writeModel, LocationDomainModel location)
    {
        var fileName = GetFileName(location);
        Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);

        await WriteAsync(fileName, writeModel);

        var result = await GetAsync(location)
                     ?? throw new InvalidOperationException("Something went wrong. Address document could not be inserted.");

        return result;
    }

    public async Task<AddressDocumentDomainModel?> GetAsync(LocationDomainModel location)
    {
        var fileName = GetFileName(location);

        if (!File.Exists(fileName))
        {
            return null;
        }

        var result = await File.ReadAllLinesAsync(fileName);

        return new AddressDocumentDomainModel
        {
            City = location.City,
            State = location.State,
            Country = location.Country,
            AreaId = location.AreaId,
            Size = result.Length - 1
        };
    }

    public async Task<IEnumerable<AddressDocumentDomainModel>> GetAllAsync()
    {
        if (!Directory.Exists("output"))
        {
            return [];
        }

        var csvFiles = Directory.EnumerateFiles("output", "*.csv", SearchOption.AllDirectories);
        return await Task.WhenAll(csvFiles.Select(ReadDocumentAsync));
    }

    private static string GetFileName(LocationDomainModel location) =>
        $"output/{location.Country}/{location.State}/{location.City}.csv";

    private static async Task<AddressDocumentDomainModel> ReadDocumentAsync(string csvFile)
    {
        var dirParts = Path.GetDirectoryName(csvFile)!.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var country = dirParts[^2];
        var state = dirParts[^1];
        var city = Path.GetFileNameWithoutExtension(csvFile);
        var lines = await File.ReadAllLinesAsync(csvFile);

        return new AddressDocumentDomainModel
        {
            City = city,
            State = state,
            Country = country,
            Size = lines.Length - 1
        };
    }

    private static async Task WriteAsync(string fileName, IEnumerable writeModel)
    {
        await using var streamWriter = new StreamWriter(fileName);
        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        await csvWriter.WriteRecordsAsync(writeModel);
    }
}
