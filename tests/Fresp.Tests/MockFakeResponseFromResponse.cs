using System.Net;

namespace Fresp.Tests;

internal class MockFakeResponseFromResponse : IFakeResponseFromResponse
{
    public Func<HttpResponseMessage, HttpResponseMessage?> GetFakeResponseFromResponse()
    {
        return response =>
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
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