using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fresp;

public class FakeResponseOptions
{
    internal readonly List<Func<HttpRequestMessage, HttpResponseMessage?>> Fakes = [];
    internal readonly List<Func<HttpRequestMessage, Task<HttpResponseMessage?>>> FakesAsync = [];

    /// <summary>
    /// Enabled or disable the handler to return fake responses. Default is false.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The name of the client that will be used to match the <see cref="HttpClient"/>. If null, the name from <see cref="Microsoft.Extensions.DependencyInjection.IHttpClientBuilder"/> will be used.
    /// </summary>
    public string? ClientName { get; set; } = null;

    /// <summary>
    /// Add a fake sync <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <param name="fake">A Func that takes an <see cref="HttpRequestMessage"/> and returns an <see cref="HttpResponseMessage"/> or null.</param>
    public void AddFakeResponse(Func<HttpRequestMessage, HttpResponseMessage?> fake) => Fakes.Add(fake);

    /// <summary>
    /// Add a fake async <see cref="HttpResponseMessage"/> response to the handler that match <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <param name="fake">A Func that takes an <see cref="HttpRequestMessage"/> and returns an <see cref="HttpResponseMessage"/> or null.</param>
    public void AddFakeResponseAsync(Func<HttpRequestMessage, Task<HttpResponseMessage?>> fake) => FakesAsync.Add(fake);
}
