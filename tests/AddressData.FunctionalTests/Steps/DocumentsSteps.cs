namespace AddressData.FunctionalTests.Steps;

using System.Globalization;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Reqnroll;
using RestSharp;

[Binding]
public class DocumentsSteps(ScenarioContext context) : TestBase(context)
{
    private RestResponse _response;
    private string _areaId;
    private string _limit;

    [Given(@"I have an areaId (.*)")]
    public void GivenIHaveAnAreaId(string areaId) => _areaId = areaId;

    [When(@"I call the InsertDocument endpoint of the DocumentsController")]
    public void WhenICallTheInsertDocumentEndpointOfTheDocumentsController()
    {
        var endpoint = TestConstants.InsertDocumentEndpoint(_areaId);
        var request = new RestRequest(endpoint, Method.Post);

        _response = Client.Execute(request);
        Context[TestConstants.Response] = _response;
    }

    [Then(@"I expect to receive an error response with status code (.*)")]
    public void ThenIExpectToReceiveAnErrorResponseWithStatusCode(string expectedStatusCode)
    {
        var actualCode = (int)_response.StatusCode;
        Assert.That(
            actualCode.ToString(CultureInfo.InvariantCulture),
            Is.EqualTo(expectedStatusCode),
            $"Expected status code {expectedStatusCode}, but got {actualCode}."
        );
    }

    [Then(@"the response content should contain ""(.*)""")]
    public void ThenTheResponseContentShouldContain(string expectedErrorMessage) => Assert.That(
        _response.Content,
        Does.Contain(expectedErrorMessage),
        $"Expected response content to contain '{expectedErrorMessage}', but got '{_response.Content}'."
    );

    [When(@"I call the SeedDocuments endpoint with limit = (.*)")]
    public async Task WhenICallTheSeedDocumentsEndpointWithLimit(string limit)
    {
        _limit = limit;

        var resource = TestConstants.SeedDocumentsEndpointWithLimit(_limit);

        var request = new RestRequest(resource, Method.Post);
        _response = await Client.ExecuteAsync(request);
    }

    [Then(@"I expect a (.*) OK status code")]
    public void ThenIExpectAokStatusCode(int expectedCode)
    {
        var actualCode = (int)_response.StatusCode;
        Assert.That(
            actualCode,
            Is.EqualTo(expectedCode),
            $"Expected status code {expectedCode}, but got {actualCode}."
        );
    }

    [Then(@"I expect the response to contain the list of seeded cities matching the limit")]
    public void ThenIExpectTheResponseToContainTheListOfSeededCitiesMatchingTheLimit()
    {
        var parsedLimit = long.Parse(_limit, CultureInfo.InvariantCulture);
        Assert.That(_response.Content, Is.Not.Empty, "Expected a JSON list of seeded cities, but response was empty.");

        var json = JToken.Parse(_response.Content);
        var documents = json[TestConstants.DocumentsField] as JArray;

        Assert.That(documents, Is.Not.Null, "Expected a 'documents' array in the response, but it was not found.");
        Assert.That(documents.Count, Is.EqualTo(parsedLimit), $"Expected {parsedLimit} seeded cities, but got {documents.Count}.");
    }


    [Then(@"I expect a (.*) status code")]
    public void ThenIExpectAStatusCode(int expectedCode)
    {
        var actualCode = (int)_response.StatusCode;
        Assert.That(actualCode, Is.EqualTo(expectedCode),
            $"Expected status code {expectedCode}, but got {actualCode}.");
    }

    [Then(@"I expect the response to contain the newly created city's details")]
    public void ThenIExpectTheResponseToContainTheNewlyCreatedCitysDetails()
    {
        var json = JToken.Parse(_response.Content);
        var city = json[TestConstants.CityField]?.ToString();
        var areaIdInResponse = json[TestConstants.AreaIdField]?.ToString();

        Assert.That(city, Is.Not.Null.Or.Empty, "Expected 'city' to be populated in the response.");
        Assert.That(areaIdInResponse, Is.EqualTo(_areaId), "AreaId mismatch in the creation response.");
    }

    [When(@"I call the InsertDocument endpoint to add the city")]
    public void WhenICallTheInsertDocumentEndpointToAddTheCity()
    {
        var endpoint = TestConstants.InsertDocumentEndpoint(_areaId);
        var request = new RestRequest(endpoint, Method.Post);

        _response = Client.Execute(request);
        Context[TestConstants.InsertResponse] = _response;
    }

    [When(@"I immediately call GetDocument endpoint with the same areaId")]
    public void WhenIImmediatelyCallGetDocumentEndpointWithTheSameAreaId()
    {
        var endpoint = TestConstants.InsertDocumentEndpoint(_areaId);
        var request = new RestRequest(endpoint);

        _response = Client.Execute(request);
        Context[TestConstants.GetResponse] = _response;
    }

    [Then(@"I expect the city data to match what was inserted")]
    public void ThenIExpectTheCityDataToMatchWhatWasInserted()
    {
        var insertResponse = Context[TestConstants.InsertResponse] as RestResponse;
        var getResponse = Context[TestConstants.GetResponse] as RestResponse;

        var insertJson = JToken.Parse(insertResponse.Content);
        var getJson = JToken.Parse(getResponse.Content);

        Assert.That(
            getJson[TestConstants.CityField]?.ToString(),
            Is.EqualTo(insertJson[TestConstants.CityField]?.ToString()),
            "City name in GET does not match the inserted city."
        );

        Assert.That(
            getJson[TestConstants.AreaIdField]?.ToString(),
            Is.EqualTo(insertJson[TestConstants.AreaIdField]?.ToString()),
            "AreaId in GET does not match the inserted city."
        );
    }
}
