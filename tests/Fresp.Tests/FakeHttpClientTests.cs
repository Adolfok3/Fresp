using Fresp.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Fresp.Tests;

public class FakeHttpClientTests
{
    [Fact]
    public async Task Create_WithFakeResponseFromRequest_ShouldReturnFake()
    {
        // Arrange (sync fakes serve the synchronous HttpClient.Send path)
        var client = FakeHttpClient.Create(options =>
        {
            options.AddFakeResponseFromRequest((sp, request) =>
            {
                if (request.RequestUri!.AbsolutePath == "/users/1")
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\":1}") };
                return null;
            });
        });

        // Act
        var response = client.Send(new HttpRequestMessage(HttpMethod.Get, "https://external-api.com/users/1"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("{\"id\":1}");
    }

    [Fact]
    public async Task Create_WithFakeResponseFromRequestAsync_ShouldReturnFake()
    {
        // Arrange
        var client = FakeHttpClient.Create(options =>
        {
            options.AddFakeResponseFromRequestAsync(async (sp, request) =>
            {
                if (request.Method == HttpMethod.Post)
                    return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("created") });
                return null;
            });
        });

        // Act
        var response = await client.PostAsync("https://external-api.com/users", new StringContent("payload"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await response.Content.ReadAsStringAsync()).Should().Be("created");
    }

    [Fact]
    public async Task Create_WithClassBasedFake_ShouldReturnFake()
    {
        // Arrange
        var client = FakeHttpClient.Create(options => options.AddFakeResponseFromRequest<MockFakeResponseFromRequest>());

        // Act
        var response = client.Send(new HttpRequestMessage(HttpMethod.Post, "https://external-api.com/must-fake"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
    }

    [Fact]
    public async Task Create_WithoutMatchingFake_AndNoDefaultResponse_ShouldThrow()
    {
        // Arrange
        var client = FakeHttpClient.Create(options =>
        {
            options.AddFakeResponseFromRequest((sp, request) => request.RequestUri!.AbsolutePath == "/match" ? new HttpResponseMessage() : null);
        });

        // Act
        var act = () => client.GetAsync("https://external-api.com/does-not-match");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*no fake response matched*");
    }

    [Fact]
    public async Task Create_WithoutMatchingFake_AndDefaultResponse_ShouldReturnDefault()
    {
        // Arrange
        var client = FakeHttpClient.Create(
            configure: options => options.AddFakeResponseFromRequest((sp, request) => null),
            defaultResponse: request => new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("fallback") });

        // Act
        var response = await client.GetAsync("https://external-api.com/anything");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadAsStringAsync()).Should().Be("fallback");
    }

    [Fact]
    public async Task Create_WithFakeResponseFromResponse_ShouldTransformDefaultResponse()
    {
        // Arrange
        var client = FakeHttpClient.Create(
            configure: options => options.AddFakeResponseFromResponse((sp, response) =>
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("recovered") };
                return null;
            }),
            defaultResponse: request => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        // Act
        var response = client.Send(new HttpRequestMessage(HttpMethod.Get, "https://external-api.com/flaky"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("recovered");
    }

    [Fact]
    public async Task Create_ShouldPassProvidedServiceProviderToFakes()
    {
        // Arrange
        var expectedProvider = new ServiceCollection().BuildServiceProvider();
        IServiceProvider? received = null;
        var client = FakeHttpClient.Create(
            configure: options => options.AddFakeResponseFromRequestAsync((sp, request) =>
            {
                received = sp;
                return Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage(HttpStatusCode.OK));
            }),
            serviceProvider: expectedProvider);

        // Act
        await client.GetAsync("https://external-api.com/anything");

        // Assert
        received.Should().BeSameAs(expectedProvider);
    }

    [Fact]
    public void CreateHandler_ShouldBeUsableWithCustomHttpClient()
    {
        // Arrange
        var handler = FakeHttpClient.CreateHandler(options =>
            options.AddFakeResponseFromRequest((sp, request) => new HttpResponseMessage(HttpStatusCode.OK)));

        // Act
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://external-api.com") };

        // Assert
        handler.Should().BeAssignableTo<HttpMessageHandler>();
        client.BaseAddress.Should().Be(new Uri("https://external-api.com"));
    }

    [Fact]
    public void CreateHandler_WithNullConfigure_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => FakeHttpClient.CreateHandler(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Create_WithDisabledFakes_ShouldReachTerminalHandler()
    {
        // Arrange
        var client = FakeHttpClient.Create(options =>
        {
            options.Enabled = false;
            options.AddFakeResponseFromRequest((sp, request) => new HttpResponseMessage(HttpStatusCode.OK));
        });

        // Act
        var act = () => client.GetAsync("https://external-api.com/anything");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*no fake response matched*");
    }
}
