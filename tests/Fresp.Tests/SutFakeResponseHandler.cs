using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fresp.Tests;

internal class SutFakeResponseHandler(FakeResponseOptions options, string clientName, IHostEnvironment hostEnvironment, ILoggerFactory loggerFactory) : FakeResponseHandler(options, clientName, hostEnvironment, loggerFactory)
{
    public new HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) => base.Send(request, cancellationToken);

    public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => base.SendAsync(request, cancellationToken);
}
