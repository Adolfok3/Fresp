using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fresp;

internal class FakeResponseHandler(FakeResponseOptions options, string clientName, IHostEnvironment hostEnvironment, ILoggerFactory loggerFactory) : DelegatingHandler
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(nameof(FakeResponseHandler));
    private readonly bool _isProduction = hostEnvironment.IsProduction();

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!UseFakeResponse())
            return base.Send(request, cancellationToken);

        foreach (var func in options.Fakes)
        {
            try
            {
                var response = func(request);
                if (response is null)
                    continue;
                
                LogDebug("Sync fake response found for client {ClientName}. Returning fake response...", clientName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to get a sync fake response for client {ClientName}.", clientName);
            }
        }

        LogDebug("No sync fake response found for client {ClientName}. Forwarding request to the next handler...", clientName);
        return base.Send(request, cancellationToken);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!UseFakeResponse())
            return await base.SendAsync(request, cancellationToken);

        foreach (var func in options.FakesAsync)
        {
            try
            {
                var response = await func(request);
                if (response is null)
                    continue;
                
                LogDebug("Async fake response found for client {ClientName}. Returning fake response...", clientName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to get a async fake response for client {ClientName}.", clientName);
            }
        }

        LogDebug("No async fake response found for client {ClientName}. Forwarding request to the next handler...", clientName);
        return await base.SendAsync(request, cancellationToken);
    }

    private bool UseFakeResponse()
    {
        var isEnabled = options.Enabled && !_isProduction;
        if (!isEnabled)
            LogDebug("Fake response is disabled for client {ClientName}. Enabled: {Enabled} | Production: {Production}. Forwarding request to the next handler...", clientName, options.Enabled, _isProduction);

        return isEnabled;
    }

    private void LogDebug(string message, params object?[] args)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
            return;

        _logger.LogDebug(message, args);
    }
}
