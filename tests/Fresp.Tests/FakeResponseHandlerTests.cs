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
        var options = new FakeOptions();
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
        var options = new FakeOptions
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
        var options = new FakeOptions();
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
        var options = new FakeOptions
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
        var options = new FakeOptions { Enabled = true };
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
    public async Task Send_WithFakeResponseFromRequest_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromRequest(request =>
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
        options.AddFakeResponseFromRequest(request =>
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
    public async Task Send_WithFakeResponseFromResponse_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromResponse(response =>
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
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
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/must-fake");

        // Act
        var response = handler.Send(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReasonPhrase.Should().Be("Faked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Faked!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromRequest_ShouldReturnForward()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromRequest(request =>
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
        options.AddFakeResponseFromRequest(request =>
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
    public async Task Send_WithFakeResponseFromResponse_ShouldReturnForward()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromResponse(response =>
        {
        if (response.StatusCode == HttpStatusCode.InternalServerError)
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
        options.AddFakeResponseFromResponse(response =>
        {
            if (response.StatusCode == HttpStatusCode.GatewayTimeout)
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
            InnerHandler = new MockResponseDelegatingHandler()
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/must-not-fake");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/must-not-fake-2");

        // Act
        var response = handler.Send(request, CancellationToken.None);
        var response2 = handler.Send(request2, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");

        response2.Should().NotBeNull();
        response2.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response2.ReasonPhrase.Should().Be("Mocked");
        content = await response2.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithoutFakes_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
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
    public async Task SendAsync_WithFakeResponseFromRequest_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromRequestAsync(request =>
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
        options.AddFakeResponseFromRequestAsync(request =>
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
    public async Task SendAsync_WithFakeResponseFromResponse_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromResponseAsync(response =>
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
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
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/must-fake");

        // Act
        var response = await handler.SendAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReasonPhrase.Should().Be("Faked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Faked!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromRequest_ShouldReturnForward()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromRequestAsync(request =>
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
        options.AddFakeResponseFromRequestAsync(request =>
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
    public async Task SendAsync_WithFakeResponseFromResponse_ShouldReturnForward()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromResponseAsync(response =>
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
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
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/must-not-fake");

        // Act
        var response = await handler.SendAsync(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromRequest_ShouldThrowsAndForwardRequest()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromRequest(_ => throw new Exception("Fake exception"));
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
    public async Task Send_WithFakeResponseFromResponse_ShouldThrowsAndForwardResponse()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromResponse(_ => throw new Exception("Fake exception"));
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromRequest_ShouldThrowsAndForwardRequest()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromRequestAsync(_ => throw new Exception("Fake exception"));
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
    public async Task SendAsync_WithFakeResponseFromResponse_ShouldThrowsAndForwardResponse()
    {
        // Arrange
        var options = new FakeOptions
        {
            Enabled = true
        };
        options.AddFakeResponseFromResponseAsync(_ => throw new Exception("Fake exception"));
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        var logger = Substitute.For<ILoggerFactory>();
        logger.CreateLogger(Arg.Any<string>()).Returns(Substitute.For<ILogger>());
        var handler = new SutFakeResponseHandler(options, "clienttest", environment, logger)
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        response.ReasonPhrase.Should().Be("Mocked");
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Mocked!");
    }
}
