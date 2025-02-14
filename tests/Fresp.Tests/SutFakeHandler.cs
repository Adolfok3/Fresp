using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fresp.Tests;

internal class SutFakeHandler(FakeOptions options, string clientName, IHostEnvironment hostEnvironment, ILoggerFactory loggerFactory) : FakeHandler(options, clientName, hostEnvironment, loggerFactory)
{
    public new HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => base.Send(request, cancellationToken);

    public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => base.SendAsync(request, cancellationToken);
}
