using System.Net;

namespace Fresp.Tests.Mocks;

/// <summary>
/// A sample "system under test": a typed client that already receives an <see cref="HttpClient"/> through its
/// constructor and consumes an external API. In the tests it is given the fake <see cref="HttpClient"/> produced
/// by <see cref="FakeHttpClient"/> instead of one that hits the real network.
/// </summary>
internal class ExternalUsersApiClient(HttpClient http)
{
    public async Task<string> GetUserAsync(int id)
    {
        var response = await http.GetAsync($"/users/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<HttpStatusCode> CreateUserAsync(string body)
    {
        var response = await http.PostAsync("/users", new StringContent(body));
        return response.StatusCode;
    }

    public string GetUserSync(int id)
    {
        var response = http.Send(new HttpRequestMessage(HttpMethod.Get, $"/users/{id}"));
        response.EnsureSuccessStatusCode();
        return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    }
}
