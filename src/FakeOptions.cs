using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Fresp;

public class FakeOptions
{
    internal readonly List<Func<IServiceProvider, HttpRequestMessage, HttpResponseMessage?>> FakeResponsesFromRequests = [];
    internal readonly List<Func<IServiceProvider, HttpRequestMessage, Task<HttpResponseMessage?>>> FakeResponsesFromRequestsAsync = [];
    internal readonly List<Func<IServiceProvider, HttpResponseMessage, HttpResponseMessage?>> FakeResponsesFromResponses = [];
    internal readonly List<Func<IServiceProvider, HttpResponseMessage, Task<HttpResponseMessage?>>> FakeResponsesFromResponsesAsync = [];

    /// <summary>
    /// Enabled or disable the handler to return fake responses. Default is false.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// WARNING: Force the use of fake responses in production environment. We strongly recommend leaving this option set to false. Default is false.
    /// </summary>
    public bool ForceUseInProduction { get; set; }

    /// <summary>
    /// The name of the client that will be used to match the <see cref="HttpClient"/>. If not provided, the name from <see cref="IHttpClientBuilder"/> will be used.
    /// </summary>
    public string? ClientName { get; set; } = null;

    /// <summary>
    /// Add a fake sync <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpRequestMessage"/> with access to <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="fake">A Func that takes an <see cref="IServiceProvider"/>, an <see cref="HttpRequestMessage"/> and returns an <see cref="HttpResponseMessage"/> or null.</param>
    public void AddFakeResponseFromRequest(Func<IServiceProvider, HttpRequestMessage, HttpResponseMessage?> fake)
        => FakeResponsesFromRequests.Add(fake);

    /// <summary>
    /// Add a fake async <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpRequestMessage"/> with access to <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="fake">A Func that takes an <see cref="IServiceProvider"/>, an <see cref="HttpRequestMessage"/> and returns a <see cref="Task{HttpResponseMessage?}"/> or null.</param>
    public void AddFakeResponseFromRequestAsync(Func<IServiceProvider, HttpRequestMessage, Task<HttpResponseMessage?>> fake)
        => FakeResponsesFromRequestsAsync.Add(fake);

    /// <summary>
    /// Add a fake sync <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpRequestMessage"/> with access to DI.
    /// </summary>
    /// <typeparam name="T">The type of the fake response that implements <see cref="IFakeResponseFromRequest"/>.</typeparam>
    public void AddFakeResponseFromRequest<T>() where T : IFakeResponseFromRequest, new()
        => FakeResponsesFromRequests.Add(new T().GetFakeResponseFromRequest());

    /// <summary>
    /// Add a fake async <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpRequestMessage"/> with access to DI.
    /// </summary>
    /// <typeparam name="T">The type of the fake response that implements <see cref="IFakeResponseFromRequestAsync"/>.</typeparam>
    public void AddFakeResponseFromRequestAsync<T>() where T : IFakeResponseFromRequestAsync, new()
        => FakeResponsesFromRequestsAsync.Add(new T().GetFakeResponseFromRequestAsync());

    /// <summary>
    /// Add a fake sync <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpResponseMessage"/> with access to <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="fake">A Func that takes an <see cref="IServiceProvider"/>, an <see cref="HttpResponseMessage"/> and returns an <see cref="HttpResponseMessage"/> or null.</param>
    public void AddFakeResponseFromResponse(Func<IServiceProvider, HttpResponseMessage, HttpResponseMessage?> fake)
        => FakeResponsesFromResponses.Add(fake);

    /// <summary>
    /// Add a fake async <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpResponseMessage"/> with access to <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="fake">A Func that takes an <see cref="IServiceProvider"/>, an <see cref="HttpResponseMessage"/> and returns a <see cref="Task{HttpResponseMessage?}"/> or null.</param>
    public void AddFakeResponseFromResponseAsync(Func<IServiceProvider, HttpResponseMessage, Task<HttpResponseMessage?>> fake)
        => FakeResponsesFromResponsesAsync.Add(fake);

    /// <summary>
    /// Add a fake sync <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpResponseMessage"/> with access to DI.
    /// </summary>
    /// <typeparam name="T">The type of the fake response that implements <see cref="IFakeResponseFromResponse"/>.</typeparam>
    public void AddFakeResponseFromResponse<T>() where T : IFakeResponseFromResponse, new()
        => FakeResponsesFromResponses.Add(new T().GetFakeResponseFromResponse());

    /// <summary>
    /// Add a fake async <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpResponseMessage"/> with access to DI.
    /// </summary>
    /// <typeparam name="T">The type of the fake response that implements <see cref="IFakeResponseFromResponseAsync"/>.</typeparam>
    public void AddFakeResponseFromResponseAsync<T>() where T : IFakeResponseFromResponseAsync, new()
        => FakeResponsesFromResponsesAsync.Add(new T().GetFakeResponseFromResponseAsync());
}
