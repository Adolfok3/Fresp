using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Fresp;

/// <summary>
/// Provides extension methods for <see cref="IHttpClientBuilder"/> to add a <see cref="FakeResponseHandler"/>.
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="FakeResponseHandler"/> to the <see cref="IHttpClientBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to add the handler to.</param>
    /// <param name="options">An optional <see cref="Action{FakeResponseOptions}"/> to configure the <see cref="FakeResponseOptions"/>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> with the <see cref="FakeResponseHandler"/> added.</returns>
    public static IHttpClientBuilder AddFakeResponseHandler(this IHttpClientBuilder builder, Action<FakeResponseOptions>? options = null)
    {
        var handlerOptions = new FakeResponseOptions();
        options?.Invoke(handlerOptions);
        builder.AddHttpMessageHandler(services => new FakeResponseHandler(handlerOptions, handlerOptions.ClientName ?? builder.Name, services.GetRequiredService<IHostEnvironment>(), services.GetRequiredService<ILoggerFactory>()));

        return builder;
    }
}
