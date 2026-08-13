namespace Fresp.Tests.Mocks;

internal class MockDelegatingHandler : DelegatingHandler
{
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => new HttpResponseMessage
    {
        Content = new StringContent("Mocked!"),
        StatusCode = System.Net.HttpStatusCode.Accepted,
        ReasonPhrase = "Mocked"
    };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage
    {
        Content = new StringContent("Mocked!"),
        StatusCode = System.Net.HttpStatusCode.Accepted,
        ReasonPhrase = "Mocked"
    });
}

internal class MockResponseDelegatingHandler : DelegatingHandler
{
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => new HttpResponseMessage
    {
        Content = new StringContent("Mocked!"),
        StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
        ReasonPhrase = "Mocked"
    };

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage
    {
        Content = new StringContent("Mocked!"),
        StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
        ReasonPhrase = "Mocked"
    });
}

internal class MockTrackingResponseDelegatingHandler : DelegatingHandler
{
    public HttpResponseMessage? LastResponse { get; private set; }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        => LastResponse = CreateResponse();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(LastResponse = CreateResponse());

    private static HttpResponseMessage CreateResponse() => new()
    {
        Content = new StringContent("Mocked!"),
        StatusCode = System.Net.HttpStatusCode.ServiceUnavailable,
        ReasonPhrase = "Mocked"
    };
}
