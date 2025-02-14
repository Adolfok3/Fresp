using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Fresp;

public class FakeOptions
{
    internal readonly List<Func<HttpResponseMessage, HttpResponseMessage?>> FakeResponseFromResponses = [];
    internal readonly List<Func<HttpResponseMessage, Task<HttpResponseMessage?>>> FakeResponseFromResponsesAsync = [];

    internal readonly List<Func<HttpRequestMessage, HttpResponseMessage?>> FakeResponseFromRequests = [];
    internal readonly List<Func<HttpRequestMessage, Task<HttpResponseMessage?>>> FakeResponsesFromRequestsAsync = [];

    /// <summary>
    /// Enabled or disable the handler to return fake responses. Default is false.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The name of the client that will be used to match the <see cref="HttpClient"/>. If not provided, the name from <see cref="IHttpClientBuilder"/> will be used.
    /// </summary>
    public string? ClientName { get; set; } = null;

    /// <summary>
    /// Add a fake sync <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <param name="fake">A Func that takes an <see cref="HttpRequestMessage"/> and returns an <see cref="HttpResponseMessage"/> or null.</param>
    public void AddFakeResponseFromRequest(Func<HttpRequestMessage, HttpResponseMessage?> fake)
        => FakeResponseFromRequests.Add(fake);

    /// <summary>
    /// Add a fake async <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <param name="fake">A Func that takes an <see cref="HttpRequestMessage"/> and returns an <see cref="HttpResponseMessage"/> or null.</param>
    public void AddFakeResponseFromRequestAsync(Func<HttpRequestMessage, Task<HttpResponseMessage?>> fake)
        => FakeResponsesFromRequestsAsync.Add(fake);

    /// <summary>
    /// Add a fake sync <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <typeparam name="T">The type of the fake response that implements <see cref="IFakeResponseFromRequest"/>.</typeparam>
    public void AddFakeResponseFromRequest<T>() where T : IFakeResponseFromRequest, new()
        => FakeResponseFromRequests.Add(new T().GetFakeResponseFromRequest());

    /// <summary>
    /// Add a fake async <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <typeparam name="T">The type of the fake response that implements <see cref="IFakeResponseFromRequestAsync"/>.</typeparam>
    public void AddFakeResponseFromRequestAsync<T>() where T : IFakeResponseFromRequestAsync, new()
        => FakeResponsesFromRequestsAsync.Add(new T().GetFakeResponseFromRequestAsync());

    /// <summary>
    /// Add a fake sync <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <typeparam name="T">The type of the fake response that implements <see cref="IFakeResponseFromResponse"/>.</typeparam>
    public void AddFakeResponseFromResponse<T>() where T : IFakeResponseFromResponse, new()
        => FakeResponseFromResponses.Add(new T().GetFakeResponseFromResponse());

    /// <summary>
    /// Add a fake async <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <typeparam name="T">The type of the fake response that implements <see cref="IFakeResponseFromResponseAsync"/>.</typeparam>
    public void AddFakeResponseFromResponseAsync<T>() where T : IFakeResponseFromResponseAsync, new()
        => FakeResponseFromResponsesAsync.Add(new T().GetFakeResponseFromResponseAsync());

    /// <summary>
    /// Add a fake sync <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="fake">A Func that takes an <see cref="HttpResponseMessage"/> and returns an <see cref="HttpResponseMessage"/> or null.</param>
    public void AddFakeResponseFromResponse(Func<HttpResponseMessage, HttpResponseMessage?> fake)
        => FakeResponseFromResponses.Add(fake);

    /// <summary>
    /// Add a fake async <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpResponseMessage"/>.
    /// </summary>
    /// <param name="fake">A Func that takes an <see cref="HttpResponseMessage"/> and returns an <see cref="HttpResponseMessage"/> or null.</param>
    public void AddFakeResponseFromResponseAsync(Func<HttpResponseMessage, Task<HttpResponseMessage?>> fake)
        => FakeResponseFromResponsesAsync.Add(fake);
}
