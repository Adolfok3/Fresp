using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Fresp.Tests;

public class FakeResponseHandlerTests
{
    [Fact]
    public async Task Send_InProduction_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeResponseOptions();
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Production);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithDisabled_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeResponseOptions
        {
            Enabled = false
        };
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_InProduction_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeResponseOptions();
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Production);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithDisabled_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeResponseOptions
        {
            Enabled = false
        };
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithoutFakes_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeResponseOptions { Enabled = true };
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);
        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithFakes_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeResponseOptions
        {
            Enabled = true
        };
        options.AddFakeResponse(request =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake") && request.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent("Faked!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked"
                };
            }

            return null;
        });
        options.AddFakeResponse(request =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake-2") && request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent("Faked2!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked2"
                };
            }

            return null;
        });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/must-fake");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/must-fake-2");

        // Act
        var response = handler.Send(request, CancellationToken.None);
        var response2 = handler.Send(request2, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReasonPhrase.Should().Be("Faked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Faked!");

        response2.Should().NotBeNull();
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.ReasonPhrase.Should().Be("Faked2");
        content = await response2.Content.ReadAsStringAsync();
        content.Should().Be("Faked2!");
    }

    [Fact]
    public async Task Send_WithFakes_ShouldReturnForward()
    {
        // Arrange
        var options = new FakeResponseOptions
        {
            Enabled = true
        };
        options.AddFakeResponse(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/must-fake" && request.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent("Faked!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked"
                };
            }

            return null;
        });
        options.AddFakeResponse(request =>
        {
            if (request.RequestUri?.AbsolutePath == "/must-fake-2" && request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage
                {
                    Content = new StringContent("Faked2!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked2"
                };
            }

            return null;
        });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/must-not-fake");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/must-not-fake-2");

        // Act
        var response = handler.Send(request, CancellationToken.None);
        var response2 = handler.Send(request2, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");

        response2.Should().NotBeNull();
        response2.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response2.ReasonPhrase.Should().Be("Mocked");
        content = await response2.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithoutFakes_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeResponseOptions { Enabled = true };
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);
        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithFakes_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeResponseOptions
        {
            Enabled = true
        };
        options.AddFakeResponseAsync(request =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake") && request.Method == HttpMethod.Post)
            {
                return Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage
                {
                    Content = new StringContent("Faked!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked"
                });
            }

            return Task.FromResult<HttpResponseMessage?>(null);
        });
        options.AddFakeResponseAsync(request =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake-2") && request.Method == HttpMethod.Get)
            {
                return Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage
                {
                    Content = new StringContent("Faked2!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked2"
                });
            }

            return Task.FromResult((HttpResponseMessage?)null);
        });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/must-fake");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/must-fake-2");

        // Act
        var response = await handler.SendAsync(request, CancellationToken.None);
        var response2 = await handler.SendAsync(request2, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReasonPhrase.Should().Be("Faked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Faked!");

        response2.Should().NotBeNull();
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.ReasonPhrase.Should().Be("Faked2");
        content = await response2.Content.ReadAsStringAsync();
        content.Should().Be("Faked2!");
    }

    [Fact]
    public async Task SendAsync_WithFakes_ShouldReturnForward()
    {
        // Arrange
        var options = new FakeResponseOptions
        {
            Enabled = true
        };
        options.AddFakeResponseAsync(request =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake") && request.Method == HttpMethod.Post)
            {
                return Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage
                {
                    Content = new StringContent("Faked!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked"
                });
            }

            return Task.FromResult((HttpResponseMessage?)null);
        });
        options.AddFakeResponseAsync(request =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake-2") && request.Method == HttpMethod.Get)
            {
                return Task.FromResult((HttpResponseMessage?)new HttpResponseMessage
                {
                    Content = new StringContent("Faked2!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked2"
                });
            }

            return Task.FromResult<HttpResponseMessage?>(null);
        });
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/must-not-fake");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/must-not-fake-2");

        // Act
        var response = await handler.SendAsync(request, CancellationToken.None);
        var response2 = await handler.SendAsync(request2, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");

        response2.Should().NotBeNull();
        response2.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response2.ReasonPhrase.Should().Be("Mocked");
        content = await response2.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithFakes_ShouldThrowsAndForwardRequest()
    {
        // Arrange
        var options = new FakeResponseOptions
        {
            Enabled = true
        };
        options.AddFakeResponse(_ => throw new Exception("Fake exception"));
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithFakes_ShouldThrowsAndForwardRequest()
    {
        // Arrange
        var options = new FakeResponseOptions
        {
            Enabled = true
        };
        options.AddFakeResponseAsync(_ => throw new Exception("Fake exception"));
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }
}
