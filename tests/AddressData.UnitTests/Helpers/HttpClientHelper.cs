namespace AddressData.UnitTests.Helpers;

using System.Net;
using System.Text;

public static class HttpClientHelper
{
    public static HttpClient CreateHttpClient(string content, string mediaType)
    {
        var response = CreateHttpResponse(content, mediaType);
        var handler = new FakeHttpMessageHandler(new Queue<HttpResponseMessage>([response]));
        return new HttpClient(handler);
    }

    public static HttpClient CreateHttpClient(Queue<HttpResponseMessage> responses)
    {
        var handler = new FakeHttpMessageHandler(responses);
        return new HttpClient(handler);
    }

    public static HttpResponseMessage CreateHttpResponse(string content, string mediaType) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(content, Encoding.UTF8, mediaType)
    };
}
