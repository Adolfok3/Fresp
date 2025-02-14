![Fresp Icon](./resources/icon.png)

[![License](https://img.shields.io/badge/license-MIT-green)](./LICENSE)
[![GithubActions](https://github.com/Adolfok3/fresp/actions/workflows/main.yml/badge.svg)](https://github.com/Adolfok3/fresp/actions)
[![Tests](https://rife2.com/tests-badge/badge/com.uwyn/tests-badge)](https://github.com/Adolfok3/Fresp/actions/workflows/main.yml)
[![Coverage Status](https://coveralls.io/repos/github/Adolfok3/Fresp/badge.svg?branch=main)](https://coveralls.io/github/Adolfok3/Fresp?branch=main)
[![NuGet Version](https://img.shields.io/nuget/vpre/fresp)](https://www.nuget.org/packages/fresp)

# Fresp - Fake Responses

Fresp (shorthand for `fake response`) is a .NET package based on `DelegatingHandler` that provides a way to mock API responses through your `HttpClient` during application execution. It allows you to configure both synchronous and asynchronous fake responses based on the incoming `HttpRequestMessage` or `HttpResponseMessage`.

## Problem

In many development or UAT environments, external APIs may be unreliable, slow, or even unavailable. This can cause significant delays and issues when trying to test and develop features that depend on these APIs. For example, if an external API is down, it can block the entire development process, making it difficult to proceed with testing and development.

To address this issue, the team needs a way to bypass the call to the external API and provide a fake response instead. This allows the development and testing to continue smoothly without being dependent on the availability or reliability of the external API.

The Fresp package helps to solve this problem by allowing developers to configure fake responses for their `HttpClient` requests, ensuring that development and testing can proceed without interruption.

> [!NOTE]
> Fresp is not intended for unit testing; it is recommended for use in UAT, QA, and development environments during execution.

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

#### Fake responses from request

To add a fake response from a <b>request</b>, use the method `AddFakeResponseFromRequest` for synchronous request calls or `AddFakeResponseFromRequestAsync` for asynchronous request calls:

- Synchronous:
```csharp
services.AddHttpClient("MyClient")
        .AddFakeHandler(options =>
        {
            options.Enabled = true;
            options.AddFakeResponseFromRequest(request =>
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
            options.AddFakeResponseFromRequestAsync(async request =>
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
            options.AddFakeResponseFromResponse(response =>
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
            options.AddFakeResponseFromResponse(async response =>
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

#### Multiple Fake Responses

Sometimes you can have a lot of `FromRequest` and `FromResponse` fakes configured in options. To make it cleaner, you can use classes that implement some of the interfaces: `IFakeResponseFromRequest`, `IFakeResponseFromRequestAsync`, `IFakeResponseFromResponse`, and `IFakeResponseFromResponseAsync`. E.g.:

Your fake response class:
```csharp
public class MyFakeResponseClass : IFakeResponseFromRequestAsync
{
  public Func<HttpRequestMessage, Task<HttpResponseMessage?>> GetFakeResponseFromRequestAsync()
    {
        return async request =>
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

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
