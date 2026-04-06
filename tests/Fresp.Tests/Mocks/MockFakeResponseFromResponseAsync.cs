using System.Net;

namespace Fresp.Tests.Mocks;

internal class MockFakeResponseFromResponseAsync : IFakeResponseFromResponseAsync
{
    public Func<IServiceProvider, HttpResponseMessage, Task<HttpResponseMessage?>> GetFakeResponseFromResponseAsync()
    {
        return async (sp, response) =>
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

            return await Task.FromResult<HttpResponseMessage?>(null);
        };
    }
}
