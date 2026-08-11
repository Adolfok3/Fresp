using Refit;
using System.Net;
using System.Text;

namespace Fresp.Tests;

/// <summary>
/// Proves that <see cref="FakeHttpClient"/> works with Refit-generated clients. Refit only wraps an
/// <see cref="HttpClient"/>, so the requests it issues flow through the Fresp <c>FakeHandler</c> and get faked.
/// Refit always calls <c>SendAsync</c>, therefore the asynchronous fakes must be used.
/// </summary>
public class RefitIntegrationTests
{
    public interface IUsersApi
    {
        [Get("/users/{id}")]
        Task<UserDto> GetUserAsync(int id);

        [Post("/users")]
        Task<ApiResponse<UserDto>> CreateUserAsync([Body] UserDto user);
    }

    public record UserDto(int Id, string Name);

    private static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");

    [Fact]
    public async Task RefitClient_ShouldDeserializeFakedResponse()
    {
        // Arrange
        var http = FakeHttpClient.Create(options =>
        {
            options.AddFakeResponseFromRequestAsync((sp, request) =>
            {
                if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath == "/users/1")
                    return Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = Json("{\"id\":1,\"name\":\"Alice\"}")
                    });

                return Task.FromResult<HttpResponseMessage?>(null);
            });
        });
        http.BaseAddress = new Uri("https://external-api.com");
        var api = RestService.For<IUsersApi>(http);

        // Act
        var user = await api.GetUserAsync(1);

        // Assert
        user.Id.Should().Be(1);
        user.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task RefitClient_ShouldReadFakedRequestBody()
    {
        // Arrange
        var http = FakeHttpClient.Create(options =>
        {
            options.AddFakeResponseFromRequestAsync(async (sp, request) =>
            {
                var body = await request.Content!.ReadAsStringAsync();
                if (request.Method == HttpMethod.Post && body.Contains("Alice"))
                    return new HttpResponseMessage(HttpStatusCode.Created) { Content = Json("{\"id\":10,\"name\":\"Alice\"}") };

                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            });
        });
        http.BaseAddress = new Uri("https://external-api.com");
        var api = RestService.For<IUsersApi>(http);

        // Act
        var response = await api.CreateUserAsync(new UserDto(0, "Alice"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Content!.Id.Should().Be(10);
    }
}
