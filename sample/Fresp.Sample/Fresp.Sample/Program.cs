using System.Net;
using System.Text;
using System.Text.Json;
using Fresp;

var builder = WebApplication.CreateBuilder(args);

// Register an HttpClient that talks to an external "Users" API.
// Note the BaseAddress points to a domain that does not exist on purpose: while Fresp is enabled,
// no real network call is ever made — the requests are intercepted and answered with fake responses.
builder.Services
    .AddHttpClient("ExternalUsersApi", client => client.BaseAddress = new Uri("https://external-users-api.example"))
    .AddFakeHandler(options =>
    {
        // Toggle the fakes. In a real app you would bind this to configuration (e.g. appsettings.json)
        // so you can enable it in DEV/UAT/QA and disable it elsewhere. Fresp also refuses to fake in
        // the Production environment unless ForceUseInProduction is set.
        options.Enabled = true;

        // Fake "GET /users/{id}": return a canned user instead of calling the real API.
        options.AddFakeResponseFromRequestAsync((serviceProvider, request) =>
        {
            if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath.StartsWith("/users/"))
            {
                var id = request.RequestUri.Segments[^1];
                var payload = JsonSerializer.Serialize(new { id, name = $"Fake User {id}", source = "fresp" });

                return Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });
            }

            // Returning null means "I don't handle this request" — Fresp moves on to the next fake
            // (and ultimately to the real API, if none matches).
            return Task.FromResult<HttpResponseMessage?>(null);
        });

        // Fake "POST /users": read the incoming body and echo it back as a created resource.
        options.AddFakeResponseFromRequestAsync(async (serviceProvider, request) =>
        {
            if (request.Method == HttpMethod.Post && request.RequestUri!.AbsolutePath == "/users")
            {
                var body = request.Content is null ? "{}" : await request.Content.ReadAsStringAsync();

                return new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
            }

            return null;
        });
    });

var app = builder.Build();

// These endpoints call the "external" API through the HttpClient. Because Fresp is enabled,
// the calls never leave the process — they are answered by the fakes configured above.

app.MapGet("/users/{id:int}", async (int id, IHttpClientFactory factory) =>
{
    var http = factory.CreateClient("ExternalUsersApi");
    var response = await http.GetAsync($"/users/{id}");
    var content = await response.Content.ReadAsStringAsync();

    return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
});

app.MapPost("/users", async (CreateUserRequest request, IHttpClientFactory factory) =>
{
    var http = factory.CreateClient("ExternalUsersApi");
    var response = await http.PostAsJsonAsync("/users", request);
    var content = await response.Content.ReadAsStringAsync();

    return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
});

app.Run();

internal record CreateUserRequest(string Name);
