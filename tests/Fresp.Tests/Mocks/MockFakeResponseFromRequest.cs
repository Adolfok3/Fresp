using System.Net;

namespace Fresp.Tests.Mocks;

internal class MockFakeResponseFromRequest : IFakeResponseFromRequest
{
    public Func<HttpRequestMessage, HttpResponseMessage?> GetFakeResponseFromRequest()
    {
        return request =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake") && request.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent("Faked!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked"
                };
            }

            return null;
        };
    }
}
