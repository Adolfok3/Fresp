using System.Net;

namespace Fresp.Tests.Mocks;

internal class MockFakeResponseFromResponse : IFakeResponseFromResponse
{
    public Func<IServiceProvider, HttpResponseMessage, HttpResponseMessage?> GetFakeResponseFromResponse()
    {
        return (sp, response) =>
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
