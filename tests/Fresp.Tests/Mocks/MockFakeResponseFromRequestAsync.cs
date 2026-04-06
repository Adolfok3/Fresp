using System.Net;

namespace Fresp.Tests.Mocks;

internal class MockFakeResponseFromRequestAsync : IFakeResponseFromRequestAsync
{
    public Func<IServiceProvider, HttpRequestMessage, Task<HttpResponseMessage?>> GetFakeResponseFromRequestAsync()
    {
        return async (sp, request) =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake") && request.Method == HttpMethod.Post)
            {
                return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage
                {
                    Content = new StringContent("Faked!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked"
                });
            }

            return await Task.FromResult<HttpResponseMessage?>(null);
        };
    }
}
