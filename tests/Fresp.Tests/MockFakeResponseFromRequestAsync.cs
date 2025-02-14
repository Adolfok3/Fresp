
using System.Net;

namespace Fresp.Tests;

internal class MockFakeResponseFromRequestAsync : IFakeResponseFromRequestAsync
{
    public Func<HttpRequestMessage, Task<HttpResponseMessage?>> GetFakeResponseFromRequestAsync()
    {
        return async request =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake-2") && request.Method == HttpMethod.Get)
            {
                return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage
                {
                    Content = new StringContent("Faked!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked"
                });
            }

            return await Task.FromResult((HttpResponseMessage?)null);
        };
    }
}
