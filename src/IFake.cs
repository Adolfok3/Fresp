using System.Net.Http;
using System;
using System.Threading.Tasks;

namespace Fresp;

public interface IFakeResponseFromRequest
{
    Func<HttpRequestMessage, HttpResponseMessage?> GetFakeResponseFromRequest();
}

public interface IFakeResponseFromRequestAsync
{
    Func<HttpRequestMessage, Task<HttpResponseMessage?>> GetFakeResponseFromRequestAsync();
}

public interface IFakeResponseFromResponse
{
    Func<HttpResponseMessage, HttpResponseMessage?> GetFakeResponseFromResponse();
}

public interface IFakeResponseFromResponseAsync
{
    Func<HttpResponseMessage, Task<HttpResponseMessage?>> GetFakeResponseFromResponseAsync();
}
