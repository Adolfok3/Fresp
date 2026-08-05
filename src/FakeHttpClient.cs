using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fresp;

/// <summary>
/// Provides factory methods to create <see cref="HttpClient"/> instances (or their underlying
/// <see cref="HttpMessageHandler"/>) wired with Fresp fake responses for <b>unit testing</b>, without
/// requiring any dependency injection container, <see cref="IHostEnvironment"/> or
/// <c>IHttpClientBuilder</c> setup.
/// </summary>
/// <remarks>
/// Use this when you have a class that consumes an external API through an <see cref="HttpClient"/> and
/// you want to fake the API responses in a unit test instead of mocking the API interface.
/// </remarks>
public static class FakeHttpClient
{
    /// <summary>
    /// Creates an <see cref="HttpClient"/> wired with the configured fake responses, ready to be injected
    /// into the class under test.
    /// </summary>
    /// <param name="configure">An <see cref="Action{FakeOptions}"/> used to add the fake responses. Fakes are enabled by default.</param>
    /// <param name="serviceProvider">
    /// An optional <see cref="IServiceProvider"/> passed to the fake response handlers. Provide one when your
    /// fakes resolve dependencies through DI. When omitted, an empty provider (that always returns <c>null</c>) is used.
    /// </param>
    /// <param name="defaultResponse">
    /// An optional factory that produces the response returned when no <c>FromRequest</c> fake matches. This
    /// simulates the "real" external API response and enables <c>FromResponse</c> fakes to run against it. When
    /// omitted, an <see cref="InvalidOperationException"/> is thrown for any request that is not faked, so that
    /// unexpected calls fail the test loudly.
    /// </param>
    /// <returns>An <see cref="HttpClient"/> that returns fake responses.</returns>
    public static HttpClient Create(Action<FakeOptions> configure, IServiceProvider? serviceProvider = null, Func<HttpRequestMessage, HttpResponseMessage>? defaultResponse = null)
        => new(CreateHandler(configure, serviceProvider, defaultResponse));

    /// <summary>
    /// Creates the <see cref="HttpMessageHandler"/> wired with the configured fake responses. Use this overload
    /// when you need to build the <see cref="HttpClient"/> yourself or plug the handler into a mocked
    /// <c>IHttpClientFactory</c>.
    /// </summary>
    /// <param name="configure">An <see cref="Action{FakeOptions}"/> used to add the fake responses. Fakes are enabled by default.</param>
    /// <param name="serviceProvider">
    /// An optional <see cref="IServiceProvider"/> passed to the fake response handlers. Provide one when your
    /// fakes resolve dependencies through DI. When omitted, an empty provider (that always returns <c>null</c>) is used.
    /// </param>
    /// <param name="defaultResponse">
    /// An optional factory that produces the response returned when no <c>FromRequest</c> fake matches. This
    /// simulates the "real" external API response and enables <c>FromResponse</c> fakes to run against it. When
    /// omitted, an <see cref="InvalidOperationException"/> is thrown for any request that is not faked, so that
    /// unexpected calls fail the test loudly.
    /// </param>
    /// <returns>An <see cref="HttpMessageHandler"/> that returns fake responses.</returns>
    public static HttpMessageHandler CreateHandler(Action<FakeOptions> configure, IServiceProvider? serviceProvider = null, Func<HttpRequestMessage, HttpResponseMessage>? defaultResponse = null)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var options = new FakeOptions { Enabled = true };
        configure(options);

        return new FakeHandler(options, options.ClientName ?? "FrespTestClient", TestHostEnvironment.Instance, serviceProvider ?? EmptyServiceProvider.Instance, NullLoggerFactory.Instance)
        {
            InnerHandler = new UnmatchedRequestHandler(options.ClientName, defaultResponse)
        };
    }
}

/// <summary>
/// Terminal handler used by <see cref="FakeHttpClient"/>. It is reached only when no <c>FromRequest</c> fake
/// matched the request. It returns the configured default response (so <c>FromResponse</c> fakes can run), or
/// throws a descriptive exception so that unexpected requests fail the test loudly.
/// </summary>
internal sealed class UnmatchedRequestHandler(string? clientName, Func<HttpRequestMessage, HttpResponseMessage>? defaultResponse) : HttpMessageHandler
{
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        => defaultResponse is not null ? defaultResponse(request) : throw BuildException(request);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => defaultResponse is not null ? Task.FromResult(defaultResponse(request)) : throw BuildException(request);

    private InvalidOperationException BuildException(HttpRequestMessage request)
    {
        var client = clientName is null ? string.Empty : $" for client '{clientName}'";
        return new InvalidOperationException(
            $"Fresp: no fake response matched the request '{request.Method} {request.RequestUri}'{client}. " +
            "Configure a matching fake with AddFakeResponseFromRequest/AddFakeResponseFromRequestAsync, " +
            "or supply a 'defaultResponse' when creating the fake HttpClient.");
    }
}

/// <summary>
/// A no-op <see cref="IServiceProvider"/> used by <see cref="FakeHttpClient"/> when the caller does not supply one.
/// </summary>
internal sealed class EmptyServiceProvider : IServiceProvider
{
    public static readonly EmptyServiceProvider Instance = new();

    public object? GetService(Type serviceType) => null;
}

/// <summary>
/// A minimal non-production <see cref="IHostEnvironment"/> used by <see cref="FakeHttpClient"/> so the
/// <see cref="FakeHandler"/> production guard never blocks fakes during unit testing.
/// </summary>
internal sealed class TestHostEnvironment : IHostEnvironment
{
    public static readonly TestHostEnvironment Instance = new();

    public string EnvironmentName { get; set; } = "Test";
    public string ApplicationName { get; set; } = "Fresp.Tests";
    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
    public IFileProvider ContentRootFileProvider { get; set; } = null!;
}
