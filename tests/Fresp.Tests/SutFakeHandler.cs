using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fresp.Tests;

internal class SutFakeHandler(FakeOptions options, string clientName, IHostEnvironment hostEnvironment, IServiceProvider serviceProvider, ILoggerFactory loggerFactory) : FakeHandler(options, clientName, hostEnvironment, serviceProvider, loggerFactory)
{
    public new HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => base.Send(request, cancellationToken);

    public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => base.SendAsync(request, cancellationToken);
}
