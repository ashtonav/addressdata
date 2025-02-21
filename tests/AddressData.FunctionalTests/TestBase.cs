namespace AddressData.FunctionalTests;

using Microsoft.AspNetCore.Mvc.Testing;
using Reqnroll;
using RestSharp;

public class TestBase
{
    protected RestClient Client { get; set; }
    protected ScenarioContext Context { get; set; }

    protected TestBase(ScenarioContext context)
    {
        var timeout = TimeSpan.FromMinutes(5);
        var app = new WebApplicationFactory<WebApi.Program>();
        var client = app.CreateClient();
        client.Timeout = timeout;

        Client = new RestClient(client, new RestClientOptions { Timeout = timeout });
        Context = context;
    }
}
