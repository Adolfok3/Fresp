using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Fresp;

public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds a <see cref="FakeHandler"/> to the <see cref="IHttpClientBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHttpClientBuilder"/> to add the handler to.</param>
    /// <param name="options">An optional <see cref="Action{FakeOptions}"/> to configure the <see cref="FakeOptions"/>.</param>
    /// <returns>The <see cref="IHttpClientBuilder"/> with the <see cref="FakeHandler"/> added.</returns>
    public static IHttpClientBuilder AddFakeHandler(this IHttpClientBuilder builder, Action<FakeOptions>? options = null)
    {
        var handlerOptions = new FakeOptions();
        options?.Invoke(handlerOptions);
        builder.AddHttpMessageHandler(services => new FakeHandler(handlerOptions, handlerOptions.ClientName ?? builder.Name, services.GetRequiredService<IHostEnvironment>(), services.GetRequiredService<ILoggerFactory>()));

        return builder;
    }
}
