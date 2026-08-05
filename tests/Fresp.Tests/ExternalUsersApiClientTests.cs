using Fresp.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Fresp.Tests;

/// <summary>
/// Tests for an existing typed client (<see cref="ExternalUsersApiClient"/>) that already receives an
/// <see cref="HttpClient"/>. Here it is given the fake <see cref="HttpClient"/> from <see cref="FakeHttpClient"/>
/// so the external API responses are faked instead of hitting the network.
/// </summary>
public class ExternalUsersApiClientTests
{
    [Fact]
    public async Task GetUserAsync_ShouldReturnFakedUser()
    {
        // Arrange
        var http = FakeHttpClient.Create(options =>
        {
            options.AddFakeResponseFromRequestAsync((sp, request) =>
            {
                if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath == "/users/1")
                    return Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"id\":1,\"name\":\"Alice\"}")
                    });

                return Task.FromResult<HttpResponseMessage?>(null);
            });
        });
        http.BaseAddress = new Uri("https://external-api.com");
        var sut = new ExternalUsersApiClient(http);

        // Act
        var result = await sut.GetUserAsync(1);

        // Assert
        result.Should().Be("{\"id\":1,\"name\":\"Alice\"}");
    }

    [Fact]
    public async Task GetUserAsync_WhenApiReturnsNotFound_ShouldThrow()
    {
        // Arrange
        var http = FakeHttpClient.Create(options =>
        {
            options.AddFakeResponseFromRequestAsync((sp, request) =>
                Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage(HttpStatusCode.NotFound)));
        });
        http.BaseAddress = new Uri("https://external-api.com");
        var sut = new ExternalUsersApiClient(http);

        // Act
        var act = () => sut.GetUserAsync(99);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnFakedStatusCode()
    {
        // Arrange
        var http = FakeHttpClient.Create(options =>
        {
            options.AddFakeResponseFromRequestAsync(async (sp, request) =>
            {
                var body = await request.Content!.ReadAsStringAsync();
                if (request.Method == HttpMethod.Post && body.Contains("Alice"))
                    return new HttpResponseMessage(HttpStatusCode.Created);

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });
        });
        http.BaseAddress = new Uri("https://external-api.com");
        var sut = new ExternalUsersApiClient(http);

        // Act
        var created = await sut.CreateUserAsync("{\"name\":\"Alice\"}");
        var badRequest = await sut.CreateUserAsync("{}");

        // Assert
        created.Should().Be(HttpStatusCode.Created);
        badRequest.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void GetUserSync_ShouldReturnFakedUser()
    {
        // Arrange (the SUT calls HttpClient.Send, so a synchronous fake is used)
        var http = FakeHttpClient.Create(options =>
        {
            options.AddFakeResponseFromRequest((sp, request) =>
            {
                if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath == "/users/7")
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\":7}") };

                return null;
            });
        });
        http.BaseAddress = new Uri("https://external-api.com");
        var sut = new ExternalUsersApiClient(http);

        // Act
        var result = sut.GetUserSync(7);

        // Assert
        result.Should().Be("{\"id\":7}");
    }

    [Fact]
    public async Task GetUserAsync_WithClassBasedFake_ShouldReturnFakedUser()
    {
        // Arrange
        var http = FakeHttpClient.Create(options => options.AddFakeResponseFromRequestAsync<MockFakeResponseFromRequestAsync>());
        http.BaseAddress = new Uri("https://external-api.com");
        var sut = new ExternalUsersApiClient(http);

        // Act
        var response = await http.PostAsync("/must-fake", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("Faked!");
    }

    [Fact]
    public async Task GetUserAsync_ResolvingDependencyFromServiceProvider_ShouldReturnFakedUser()
    {
        // Arrange: a fake that resolves a service registered in the test's service provider
        var services = new ServiceCollection();
        services.AddSingleton<IUserNameProvider, StubUserNameProvider>();
        var provider = services.BuildServiceProvider();

        var http = FakeHttpClient.Create(
            configure: options => options.AddFakeResponseFromRequestAsync((sp, request) =>
            {
                var name = sp.GetRequiredService<IUserNameProvider>().GetName();
                return Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent($"{{\"name\":\"{name}\"}}")
                });
            }),
            serviceProvider: provider);
        http.BaseAddress = new Uri("https://external-api.com");
        var sut = new ExternalUsersApiClient(http);

        // Act
        var result = await sut.GetUserAsync(1);

        // Assert
        result.Should().Be("{\"name\":\"Bob\"}");
    }

    private interface IUserNameProvider
    {
        string GetName();
    }

    private sealed class StubUserNameProvider : IUserNameProvider
    {
        public string GetName() => "Bob";
    }
}
