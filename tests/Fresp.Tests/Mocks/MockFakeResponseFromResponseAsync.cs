using System.Net;

namespace Fresp.Tests.Mocks;

internal class MockFakeResponseFromResponseAsync : IFakeResponseFromResponseAsync
{
    public Func<HttpResponseMessage, Task<HttpResponseMessage?>> GetFakeResponseFromResponseAsync()
    {
        return async response =>
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
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
