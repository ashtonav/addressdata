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
        var path = $"output/{location.Country}/{location.State}";
        Directory.CreateDirectory(path);
        var fileName = $"{path}/{location.City}.csv";

        await WriteAsync(fileName, writeModel);

        var result = await GetAsync(location)
                     ?? throw new InvalidOperationException("Something went wrong. Address document could not be inserted.");

        return result;
    }

    public async Task<AddressDocumentDomainModel?> GetAsync(LocationDomainModel location)
    {
        var path = $"output/{location.Country}/{location.State}";
        var fileName = $"{path}/{location.City}.csv";

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
            Size = result.Length - 1 // The first line is the header
        };
    }

    public async Task<IEnumerable<AddressDocumentDomainModel>> GetAllAsync()
    {
        var results = new List<AddressDocumentDomainModel>();

        if (!Directory.Exists("output"))
        {
            return results;
        }
        var countryDirectories = Directory.GetDirectories("output");

        foreach (var countryDir in countryDirectories)
        {
            var country = Path.GetFileName(countryDir);
            var stateDirectories = Directory.GetDirectories(countryDir);

            foreach (var stateDir in stateDirectories)
            {
                var state = Path.GetFileName(stateDir);
                var csvFiles = Directory.GetFiles(stateDir, "*.csv", SearchOption.TopDirectoryOnly);

                foreach (var csvFile in csvFiles)
                {
                    var city = Path.GetFileNameWithoutExtension(csvFile);
                    var lines = await File.ReadAllLinesAsync(csvFile);

                    results.Add(new AddressDocumentDomainModel
                    {
                        City = city,
                        State = state,
                        Country = country,
                        Size = lines.Length - 1
                    });
                }
            }
        }

        return results;
    }

    private static async Task WriteAsync(string fileName, IEnumerable writeModel)
    {
        await using var streamWriter = new StreamWriter(fileName);
        await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        await csvWriter.WriteRecordsAsync(writeModel);
    }
}
