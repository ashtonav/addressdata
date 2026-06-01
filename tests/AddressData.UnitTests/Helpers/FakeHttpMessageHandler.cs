namespace AddressData.UnitTests.Helpers;

using System.Net;

public class FakeHttpMessageHandler(Queue<HttpResponseMessage> responses) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;

        if (responses.Count > 0)
        {
            return Task.FromResult(responses.Dequeue());
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
