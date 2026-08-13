using Fresp.Tests.Mocks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Fresp.Tests;

public class FakeHandlerTests
{
    [Fact]
    public async Task Send_InProduction_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        var handler = new SutFakeHandler(options, "clienttest", CreateProductionEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
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
    public async Task Send_InProduction_WithForceProperty_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true, ForceUseInProduction = true };
        options.AddFakeResponseFromRequest((sp, request) =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake") && request.Method == HttpMethod.Post)
                return new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK, ReasonPhrase = "Faked" };
            return null;
        });
        options.AddFakeResponseFromRequest((sp, request) =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake-2") && request.Method == HttpMethod.Get)
                return new HttpResponseMessage { Content = new StringContent("Faked2!"), StatusCode = HttpStatusCode.OK, ReasonPhrase = "Faked2" };
            return null;
        });
        var handler = new SutFakeHandler(options, "clienttest", CreateProductionEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/must-fake");
        var request2 = new HttpRequestMessage(HttpMethod.Get, "/must-fake-2");

        // Act
        var response = handler.Send(request, CancellationToken.None);
        var response2 = handler.Send(request2, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ReasonPhrase.Should().Be("Faked");
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");

        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.ReasonPhrase.Should().Be("Faked2");
        (await response2.Content.ReadAsStringAsync()).Should().Be("Faked2!");
    }

    [Fact]
    public async Task Send_WithDisabled_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeOptions { Enabled = false };
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithoutFakes_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromRequest_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromRequest((sp, request) =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake") && request.Method == HttpMethod.Post)
                return new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK, ReasonPhrase = "Faked" };
            return null;
        });
        options.AddFakeResponseFromRequest((sp, request) =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake-2") && request.Method == HttpMethod.Get)
                return new HttpResponseMessage { Content = new StringContent("Faked2!"), StatusCode = HttpStatusCode.OK, ReasonPhrase = "Faked2" };
            return null;
        });
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(HttpMethod.Post, "/must-fake"), CancellationToken.None);
        var response2 = handler.Send(new HttpRequestMessage(HttpMethod.Get, "/must-fake-2"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response2.Content.ReadAsStringAsync()).Should().Be("Faked2!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromResponse_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromResponse((sp, response) =>
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK, ReasonPhrase = "Faked" };
            return null;
        });
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateDebugLoggerFactory())
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(HttpMethod.Post, "/must-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromRequest_ShouldForwardWhenNoMatch()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromRequest((sp, request) =>
        {
            if (request.RequestUri?.AbsolutePath == "/must-fake" && request.Method == HttpMethod.Post)
                return new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK };
            return null;
        });
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(HttpMethod.Post, "/must-not-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromResponse_ShouldForwardWhenNoMatch()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromResponse((sp, response) =>
        {
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK };
            return null;
        });
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(HttpMethod.Post, "/must-not-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromRequest_ShouldThrowAndForward()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromRequest((sp, request) => throw new Exception("Fake exception"));
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromResponse_ShouldThrowAndForward()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromResponse((sp, response) => throw new Exception("Fake exception"));
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromRequest_ClassBased_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromRequest<MockFakeResponseFromRequest>();
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(HttpMethod.Post, "/must-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromResponse_ClassBased_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromResponse<MockFakeResponseFromResponse>();
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateDebugLoggerFactory())
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(HttpMethod.Post, "/must-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
    }

    // ============================================================
    // SendAsync - Async tests
    // ============================================================

    [Fact]
    public async Task SendAsync_InProduction_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        var handler = new SutFakeHandler(options, "clienttest", CreateProductionEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithDisabled_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeOptions { Enabled = false };
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithoutFakes_ShouldForwardRequest()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromRequest_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromRequestAsync(async (sp, request) =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake") && request.Method == HttpMethod.Post)
                return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK, ReasonPhrase = "Faked" });
            return await Task.FromResult<HttpResponseMessage?>(null);
        });
        options.AddFakeResponseFromRequestAsync(async (sp, request) =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake-2") && request.Method == HttpMethod.Get)
                return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage { Content = new StringContent("Faked2!"), StatusCode = HttpStatusCode.OK, ReasonPhrase = "Faked2" });
            return await Task.FromResult<HttpResponseMessage?>(null);
        });
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/must-fake"), CancellationToken.None);
        var response2 = await handler.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/must-fake-2"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response2.Content.ReadAsStringAsync()).Should().Be("Faked2!");
    }

    [Fact]
    public async Task SendAsync_InProduction_WithForceProperty_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true, ForceUseInProduction = true };
        options.AddFakeResponseFromRequestAsync(async (sp, request) =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake") && request.Method == HttpMethod.Post)
                return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK });
            return await Task.FromResult<HttpResponseMessage?>(null);
        });
        var handler = new SutFakeHandler(options, "clienttest", CreateProductionEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/must-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromResponse_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromResponseAsync(async (sp, response) =>
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK });
            return await Task.FromResult<HttpResponseMessage?>(null);
        });
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/must-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromRequest_ShouldForwardWhenNoMatch()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromRequestAsync(async (sp, request) =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake") && request.Method == HttpMethod.Post)
                return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK });
            return await Task.FromResult<HttpResponseMessage?>(null);
        });
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/must-not-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromResponse_ShouldForwardWhenNoMatch()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromResponseAsync(async (sp, response) =>
        {
            if (response.StatusCode == HttpStatusCode.BadRequest)
                return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK });
            return await Task.FromResult<HttpResponseMessage?>(null);
        });
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/must-not-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromRequest_ShouldThrowAndForward()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromRequestAsync((sp, request) => throw new Exception("Fake exception"));
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromResponse_ShouldThrowAndForward()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromResponseAsync((sp, response) => throw new Exception("Fake exception"));
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await response.Content.ReadAsStringAsync()).Should().Be("Mocked!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromRequest_ClassBased_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromRequestAsync<MockFakeResponseFromRequestAsync>();
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/must-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromResponse_ClassBased_ShouldReturnFromFake()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromResponseAsync<MockFakeResponseFromResponseAsync>();
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/must-fake"), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
    }

    // ============================================================
    // DI-specific tests (verifying IServiceProvider is passed)
    // ============================================================

    [Fact]
    public async Task Send_WithFakeResponseFromRequest_ShouldReceiveServiceProvider()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        IServiceProvider? receivedSp = null;
        options.AddFakeResponseFromRequest((sp, request) =>
        {
            receivedSp = sp;
            return new HttpResponseMessage { Content = new StringContent("DI!"), StatusCode = HttpStatusCode.OK };
        });
        var serviceProvider = CreateServiceProvider();
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), serviceProvider, CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        receivedSp.Should().BeSameAs(serviceProvider);
        (await response.Content.ReadAsStringAsync()).Should().Be("DI!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromRequestAsync_ShouldReceiveServiceProvider()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        IServiceProvider? receivedSp = null;
        options.AddFakeResponseFromRequestAsync(async (sp, request) =>
        {
            receivedSp = sp;
            return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage { Content = new StringContent("DIAsync!"), StatusCode = HttpStatusCode.OK });
        });
        var serviceProvider = CreateServiceProvider();
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), serviceProvider, CreateLoggerFactory())
        {
            InnerHandler = new MockDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        receivedSp.Should().BeSameAs(serviceProvider);
        (await response.Content.ReadAsStringAsync()).Should().Be("DIAsync!");
    }

    [Fact]
    public async Task Send_WithFakeResponseFromResponse_ShouldReceiveServiceProvider()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        IServiceProvider? receivedSp = null;
        options.AddFakeResponseFromResponse((sp, response) =>
        {
            receivedSp = sp;
            return new HttpResponseMessage { Content = new StringContent("DIResponse!"), StatusCode = HttpStatusCode.OK };
        });
        var serviceProvider = CreateServiceProvider();
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), serviceProvider, CreateDebugLoggerFactory())
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        receivedSp.Should().BeSameAs(serviceProvider);
        (await response.Content.ReadAsStringAsync()).Should().Be("DIResponse!");
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromResponseAsync_ShouldReceiveServiceProvider()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        IServiceProvider? receivedSp = null;
        options.AddFakeResponseFromResponseAsync(async (sp, response) =>
        {
            receivedSp = sp;
            return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage { Content = new StringContent("DIResponseAsync!"), StatusCode = HttpStatusCode.OK });
        });
        var serviceProvider = CreateServiceProvider();
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), serviceProvider, CreateLoggerFactory())
        {
            InnerHandler = new MockResponseDelegatingHandler()
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        receivedSp.Should().BeSameAs(serviceProvider);
        (await response.Content.ReadAsStringAsync()).Should().Be("DIResponseAsync!");
    }

    [Fact]
    public void Send_WithFakeResponseFromResponse_ShouldDisposeOriginalResponseWhenReplaced()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromResponse((sp, response) =>
            new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK });
        var inner = new MockTrackingResponseDelegatingHandler();
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = inner
        };

        // Act
        var response = handler.Send(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        inner.LastResponse.Should().NotBeSameAs(response);
        var readOriginal = () => inner.LastResponse!.Content.ReadAsStringAsync();
        readOriginal.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task SendAsync_WithFakeResponseFromResponse_ShouldDisposeOriginalResponseWhenReplaced()
    {
        // Arrange
        var options = new FakeOptions { Enabled = true };
        options.AddFakeResponseFromResponseAsync((sp, response) =>
            Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage { Content = new StringContent("Faked!"), StatusCode = HttpStatusCode.OK }));
        var inner = new MockTrackingResponseDelegatingHandler();
        var handler = new SutFakeHandler(options, "clienttest", CreateDevelopmentEnvironment(), CreateServiceProvider(), CreateLoggerFactory())
        {
            InnerHandler = inner
        };

        // Act
        var response = await handler.SendAsync(new HttpRequestMessage(), CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        inner.LastResponse.Should().NotBeSameAs(response);
        var readOriginal = () => inner.LastResponse!.Content.ReadAsStringAsync();
        await readOriginal.Should().ThrowAsync<ObjectDisposedException>();
    }

    private static IServiceProvider CreateServiceProvider() => Substitute.For<IServiceProvider>();

    private static IHostEnvironment CreateDevelopmentEnvironment()
    {
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(Environments.Development);
        return env;
    }

    private static IHostEnvironment CreateProductionEnvironment()
    {
        var env = Substitute.For<IHostEnvironment>();
        env.EnvironmentName.Returns(Environments.Production);
        return env;
    }

    private static ILoggerFactory CreateLoggerFactory()
    {
        var logger = Substitute.For<ILogger>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
        return loggerFactory;
    }

    private static ILoggerFactory CreateDebugLoggerFactory()
    {
        var logger = Substitute.For<ILogger>();
        logger.IsEnabled(LogLevel.Debug).Returns(true);
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
        return loggerFactory;
    }
}
