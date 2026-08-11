![Fresp Icon](./resources/icon.png)

[![GithubActions](https://github.com/Adolfok3/fresp/actions/workflows/main.yml/badge.svg)](https://github.com/Adolfok3/fresp/actions)
[![License](https://img.shields.io/badge/license-MIT-green)](./LICENSE)
[![Coverage Status](https://coveralls.io/repos/github/Adolfok3/Fresp/badge.svg?branch=main)](https://coveralls.io/github/Adolfok3/Fresp?branch=main)
[![NuGet Version](https://img.shields.io/nuget/vpre/fresp)](https://www.nuget.org/packages/fresp)

# Fresp - Fake Responses

Fresp (shorthand for `fake response`) is a .NET package based on `DelegatingHandler` that provides a way to mock API responses through your `HttpClient` during application execution. It allows you to configure both synchronous and asynchronous fake responses based on the incoming `HttpRequestMessage` or `HttpResponseMessage`, with full access to `IServiceProvider` for dependency injection.

It can also be used in **unit tests** to fake an external API through the `HttpClient` your code consumes, without mocking the API interface. See [Using Fresp in unit tests](#using-fresp-in-unit-tests).

## Problem

In many development or UAT environments, external APIs may be unreliable, slow, or even unavailable. This can cause significant delays and issues when trying to test and develop features that depend on these APIs. For example, if an external API is down, it can block the entire development process, making it difficult to proceed with testing and development.

To address this issue, the team needs a way to bypass the call to the external API and provide a fake response instead. This allows the development and testing to continue smoothly without being dependent on the availability or reliability of the external API.

The Fresp package helps to solve this problem by allowing developers to configure fake responses for their `HttpClient` requests, ensuring that development and testing can proceed without interruption.

> [!NOTE]
> During application execution, Fresp is recommended for use in UAT, QA, and development environments. It can additionally be used in unit tests through the `FakeHttpClient` factory — see [Using Fresp in unit tests](#using-fresp-in-unit-tests).

> [!WARNING]
> Fresp has a guard to avoid execution in the production environment, so the chance of getting a fake response in production is zero! Unless your `ASPNETCORE_ENVIRONMENT` variable is incorrectly set on the production server...

## Installation

To install Fresp, use one of the following methods:

### NuGet Package Manager Console

```powershell
Install-Package Fresp
```

### .NET CLI

```bash
dotnet add package Fresp
```

## Usage

### Adding Fake Response to your HttpClient

To make `Fresp` mock and return fake responses from your `HttpClient`, use the `AddFakeHandler` extension method:

```csharp
services.AddHttpClient("MyClient")
        .AddFakeHandler(options =>
        {
            options.Enabled = true; // Toggle fake responses for this client. It is recommended to use this in conjunction with configuration settings from appsettings.json to enable/disable easily
        });
```

### Configuring Fake Responses

There are two ways to return fake responses, `FromRequest` and `FromResponse`:

- **FromRequest**: will return a fake response <b>before</b> the request is sent to the target API, if the request predicate is matched.

- **FromResponse**: will return a fake response <b>after</b> the request was sent to the target API, if the response predicate is matched.

> [!TIP]
> All fake response handlers receive `IServiceProvider` as their first parameter, allowing you to resolve any registered service (databases, caches, other HttpClients, etc.).

#### Fake responses from request

To add a fake response from a <b>request</b>, use the method `AddFakeResponseFromRequest` for synchronous request calls or `AddFakeResponseFromRequestAsync` for asynchronous request calls:

- Synchronous:
```csharp
services.AddHttpClient("MyClient")
        .AddFakeHandler(options =>
        {
            options.Enabled = true;
            options.AddFakeResponseFromRequest((serviceProvider, request) =>
            {
              if (request.RequestUri?.AbsolutePath == "/endpoint")
              {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                  Content = new StringContent("Sync fake response")
                };
              }
              return null;
          });
        });
```
- Asynchronous:
```csharp
services.AddHttpClient("MyClient")
        .AddFakeHandler(options =>
        {
            options.Enabled = true;
            options.AddFakeResponseFromRequestAsync(async (serviceProvider, request) =>
            {
              var body = await request.Content.ReadAsStringAsync();
              if (body.Contains("something"))
              {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                  Content = new StringContent("Async fake response")
                };
              }

              return await Task.FromResult<HttpResponseMessage?>(null);
          });
        });
```

#### Fake responses from response

If you need to add a fake response from a <b>response</b>, use the method `AddFakeResponseFromResponse` for synchronous request calls or `AddFakeResponseFromResponseAsync` for asynchronous request calls:

- Synchronous:
```csharp
services.AddHttpClient("MyClient")
        .AddFakeHandler(options =>
        {
            options.Enabled = true;
            options.AddFakeResponseFromResponse((serviceProvider, response) =>
            {
              if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
              {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                  Content = new StringContent("Sync fake response")
                };
              }
              return null;
          });
        });
```
- Asynchronous:
```csharp
services.AddHttpClient("MyClient")
        .AddFakeHandler(options =>
        {
            options.Enabled = true;
            options.AddFakeResponseFromResponseAsync(async (serviceProvider, response) =>
            {
              var body = await response.Content.ReadAsStringAsync();
              if (body.Contains("something"))
              {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                  Content = new StringContent("Async fake response")
                };
              }

              return await Task.FromResult<HttpResponseMessage?>(null);
          });
        });
```

### Tips

#### Mock API
Fresp is a nice way to create mock APIs to test API calls during execution (similar to [WireMock-Net](https://github.com/WireMock-Net/WireMock.Net)). Just create a random `HttpClient` and configure the fake responses:

```csharp
services.AddHttpClient("FakeHttpClient")
        .AddFakeHandler(options =>
        {
          // Configure your fake responses...
        })
        .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://this-api-does-not-exist.com"));
```

#### Accessing Dependencies (DI)

All fake response handlers have access to the `IServiceProvider`, allowing you to resolve any registered service (databases, caches, other HttpClients, configuration, etc.):

```csharp
services.AddHttpClient("MyClient")
        .AddFakeHandler(options =>
        {
            options.Enabled = true;
            options.AddFakeResponseFromRequest((serviceProvider, request) =>
            {
                var db = serviceProvider.GetRequiredService<IMyDbContext>();
                var data = db.MyEntities.FirstOrDefault();

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(data?.ToJson() ?? "no data")
                };
            });
        });
```

**Async with DI:**
```csharp
options.AddFakeResponseFromRequestAsync(async (serviceProvider, request) =>
{
    var cache = serviceProvider.GetRequiredService<IDistributedCache>();
    var cached = await cache.GetStringAsync("my-key");
    if (cached != null)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(cached)
        };
    }
    return null;
});
```

**From Response with DI:**
```csharp
options.AddFakeResponseFromResponse((serviceProvider, response) =>
{
    if (response.StatusCode == HttpStatusCode.InternalServerError)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<MyService>>();
        logger.LogWarning("API returned 500, returning fallback response");

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"fallback\": true}")
        };
    }
    return null;
});
```

#### Multiple Fake Responses

Sometimes you can have a lot of `FromRequest` and `FromResponse` fakes configured in options. To make it cleaner, you can use classes that implement some of the interfaces: `IFakeResponseFromRequest`, `IFakeResponseFromRequestAsync`, `IFakeResponseFromResponse`, and `IFakeResponseFromResponseAsync`. E.g.:

Your fake response class:
```csharp
public class MyFakeResponseClass : IFakeResponseFromRequestAsync
{
    public Func<IServiceProvider, HttpRequestMessage, Task<HttpResponseMessage?>> GetFakeResponseFromRequestAsync()
    {
        return async (sp, request) =>
        {
            if (request.RequestUri != null && request.RequestUri.ToString().EndsWith("/must-fake-2") && request.Method == HttpMethod.Get)
            {
                return await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage
                {
                    Content = new StringContent("Faked!"),
                    StatusCode = HttpStatusCode.OK,
                    ReasonPhrase = "Faked"
                });
            }

            return await Task.FromResult((HttpResponseMessage?)null);
        };
    }
}
```

In the options configuration:
```csharp
services.AddHttpClient("MyClient")
        .AddFakeHandler(options =>
        {
            options.Enabled = true;
            options.AddFakeResponseFromRequestAsync<MyFakeResponseClass>();
        });
```

## Using Fresp in unit tests

Suppose you have a class that consumes an external API through an `HttpClient`:

```csharp
public class UsersApiClient(HttpClient http)
{
    public async Task<string> GetUserAsync(int id)
    {
        var response = await http.GetAsync($"/users/{id}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

Instead of mocking the API interface (or mocking `HttpMessageHandler` by hand), use the `FakeHttpClient` factory to build an `HttpClient` wired with fake responses and inject it into the class under test. There is **no need for dependency injection, `IHostEnvironment`, or `IHttpClientBuilder`** — fakes are enabled by default:

```csharp
[Fact]
public async Task GetUserAsync_ReturnsUser()
{
    // Arrange
    var http = FakeHttpClient.Create(options =>
    {
        options.AddFakeResponseFromRequestAsync(async (sp, request) =>
        {
            if (request.RequestUri!.AbsolutePath == "/users/1" && request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"id\":1,\"name\":\"Alice\"}")
                };
            }

            return null;
        });
    });
    http.BaseAddress = new Uri("https://external-api.com");
    var sut = new UsersApiClient(http);

    // Act
    var result = await sut.GetUserAsync(1);

    // Assert
    result.Should().Contain("Alice");
}
```

You configure fakes exactly like in application execution — all the methods from [Configuring Fake Responses](#configuring-fake-responses) (`FromRequest`/`FromResponse`, sync/async, and class-based via `IFakeResponseFrom*`) work the same way.

> [!IMPORTANT]
> Fresp keeps synchronous and asynchronous fakes in separate pipelines: `HttpClient.Send(...)` matches the fakes added with `AddFakeResponseFromRequest`/`AddFakeResponseFromResponse`, while the async calls (`GetAsync`, `PostAsync`, `SendAsync`, ...) match the fakes added with `AddFakeResponseFromRequestAsync`/`AddFakeResponseFromResponseAsync`. Register the fakes that match how your code under test calls the API (async code is the most common case).

### Passing dependencies (DI)

If your fakes resolve services through `IServiceProvider`, pass one to the factory:

```csharp
var http = FakeHttpClient.Create(
    configure: options =>
    {
        options.AddFakeResponseFromRequest((serviceProvider, request) =>
        {
            var db = serviceProvider.GetRequiredService<IMyDbContext>();
            // ...
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
    },
    serviceProvider: myTestServiceProvider);
```

When omitted, an empty provider (that always returns `null`) is used.

### Requests that are not faked

By default, any request that does not match a fake throws an `InvalidOperationException`, so unexpected calls fail the test loudly:

```
Fresp: no fake response matched the request 'GET https://external-api.com/orders'...
```

If you want to simulate the "real" external API response instead (for example to exercise a `FromResponse` fake, or to assert your code's behavior on a specific status code), supply a `defaultResponse`:

```csharp
var http = FakeHttpClient.Create(
    configure: options =>
    {
        options.AddFakeResponseFromResponse((sp, response) =>
        {
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("fallback") };
            return null;
        });
    },
    defaultResponse: request => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
```

### Using with a mocked `IHttpClientFactory`

If your code under test resolves clients through `IHttpClientFactory`, use `CreateHandler` and build the `HttpClient` yourself (example using NSubstitute):

```csharp
var handler = FakeHttpClient.CreateHandler(options =>
{
    options.AddFakeResponseFromRequestAsync(async (sp, request) => /* ... */);
});

var factory = Substitute.For<IHttpClientFactory>();
factory.CreateClient("MyClient").Returns(new HttpClient(handler) { BaseAddress = new Uri("https://external-api.com") });
```

### Working with Refit

Fresp works with [Refit](https://github.com/reactiveui/refit) clients out of the box, because Refit simply wraps an `HttpClient`. Build the fake client and pass it to `RestService.For<T>` — the requests generated by your Refit interface flow through the fake handler:

```csharp
public interface IUsersApi
{
    [Get("/users/{id}")]
    Task<UserDto> GetUserAsync(int id);
}

public record UserDto(int Id, string Name);

[Fact]
public async Task GetUserAsync_ReturnsFakedUser()
{
    // Arrange
    var http = FakeHttpClient.Create(options =>
    {
        options.AddFakeResponseFromRequestAsync((sp, request) =>
        {
            if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath == "/users/1")
                return Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"id\":1,\"name\":\"Alice\"}", Encoding.UTF8, "application/json")
                });

            return Task.FromResult<HttpResponseMessage?>(null);
        });
    });
    http.BaseAddress = new Uri("https://external-api.com");
    var api = RestService.For<IUsersApi>(http);

    // Act
    var user = await api.GetUserAsync(1);

    // Assert
    user.Name.Should().Be("Alice");
}
```

> [!IMPORTANT]
> Refit always issues its requests through `SendAsync`, so configure the **asynchronous** fakes (`AddFakeResponseFromRequestAsync` / `AddFakeResponseFromResponseAsync`). Remember to set the `application/json` `Content-Type` on the fake response content so Refit can deserialize it into your model.

> [!TIP]
> If you fake an error status code (4xx/5xx) that Refit turns into an `ApiException`, set `RequestMessage` on the fake response (`new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request }`) so Refit has the originating request available when building the exception.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
