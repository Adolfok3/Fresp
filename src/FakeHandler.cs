using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fresp;

internal class FakeHandler(FakeOptions options, string clientName, IHostEnvironment hostEnvironment, ILoggerFactory loggerFactory) : DelegatingHandler
{
    private readonly ILogger _logger = loggerFactory.CreateLogger(nameof(FakeHandler));
    private readonly bool _isProduction = hostEnvironment.IsProduction();

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        => !UseFakeHandler()
            ? base.Send(request, cancellationToken)
            : SendWithFakeResponseFromRequest(request, cancellationToken);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => !UseFakeHandler()
            ? await base.SendAsync(request, cancellationToken)
            : await SendWithFakeResponseFromRequestAsync(request, cancellationToken);

    private HttpResponseMessage SendWithFakeResponseFromRequest(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        foreach (var func in options.FakeResponseFromRequests)
        {
            try
            {
                var response = func(request);
                if (response is null)
                    continue;

                LogDebug("Sync fake request found for client {ClientName}. Returning fake response...", clientName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to get a sync fake request for client {ClientName}.", clientName);
            }
        }

        LogDebug("No sync fake request found for client {ClientName}. Forwarding request to the next handler...", clientName);
        return SendWithFakeResponseFromResponse(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendWithFakeResponseFromRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        foreach (var func in options.FakeResponsesFromRequestsAsync)
        {
            try
            {
                var response = await func(request);
                if (response is null)
                    continue;

                LogDebug("Async fake request found for client {ClientName}. Returning fake response...", clientName);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to get a async fake request for client {ClientName}.", clientName);
            }
        }

        LogDebug("No async fake request found for client {ClientName}. Forwarding request to the next handler...", clientName);
        return await SendWithFakeResponseFromResponseAsync(request, cancellationToken);
    }

    private HttpResponseMessage SendWithFakeResponseFromResponse(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = base.Send(request, cancellationToken);

        foreach (var func in options.FakeResponseFromResponses)
        {
            try
            {
                var newResponse = func(response);
                if (newResponse is null)
                    continue;

                LogDebug("Sync fake response found for client {ClientName}. Returning fake response...", clientName);
                return newResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to get a sync fake response for client {ClientName}.", clientName);
            }
        }

        LogDebug("No sync fake response found for client {ClientName}. Returning original response...", clientName);
        return response;
    }

    private async Task<HttpResponseMessage> SendWithFakeResponseFromResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        foreach (var func in options.FakeResponseFromResponsesAsync)
        {
            try
            {
                var newResponse = await func(response);
                if (newResponse is null)
                    continue;

                LogDebug("Async fake response found for client {ClientName}. Returning fake response...", clientName);
                return newResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while trying to get a async fake response for client {ClientName}.", clientName);
            }
        }

        LogDebug("No async fake response found for client {ClientName}. Returning original response...", clientName);
        return response;
    }

    private bool UseFakeHandler()
    {
        var isEnabled = options.Enabled && !_isProduction;
        if (!isEnabled)
            LogDebug("Fake handler is disabled for client {ClientName}. Enabled: {Enabled} | Production: {Production}. Forwarding request to the next handler...", clientName, options.Enabled, _isProduction);

        return isEnabled;
    }

    private void LogDebug(string message, params object?[] args)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
            return;

        _logger.LogDebug(message, args);
    }
}
