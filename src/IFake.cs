using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fresp;

public interface IFakeResponseFromRequest
{
    Func<IServiceProvider, HttpRequestMessage, HttpResponseMessage?> GetFakeResponseFromRequest();
}

public interface IFakeResponseFromRequestAsync
{
    Func<IServiceProvider, HttpRequestMessage, Task<HttpResponseMessage?>> GetFakeResponseFromRequestAsync();
}

public interface IFakeResponseFromResponse
{
    Func<IServiceProvider, HttpResponseMessage, HttpResponseMessage?> GetFakeResponseFromResponse();
}

public interface IFakeResponseFromResponseAsync
{
    Func<IServiceProvider, HttpResponseMessage, Task<HttpResponseMessage?>> GetFakeResponseFromResponseAsync();
}
