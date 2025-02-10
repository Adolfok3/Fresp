![Fresp Icon](./resources/icon.png)

[![GithubActions](https://github.com/Adolfok3/fresp/actions/workflows/main.yml/badge.svg)](https://github.com/Adolfok3/fresp/actions)
[![License](https://img.shields.io/badge/license-MIT-green)](./LICENSE)
[![Coverage Status](https://coveralls.io/repos/github/Adolfok3/Fresp/badge.svg?branch=main)](https://coveralls.io/github/Adolfok3/Fresp?branch=main)
[![NuGet Version](https://img.shields.io/nuget/vpre/fresp)](https://www.nuget.org/packages/fresp)

# Fresp

Fresp (shorthand for `fake response`) is a .NET package that provides a way to mock API responses through your `HttpClient` during application execution. It allows you to configure both synchronous and asynchronous fake responses based on the incoming `HttpRequestMessage`.

## Problem

In many development or UAT environments, external APIs may be unreliable, slow, or even unavailable. This can cause significant delays and issues when trying to test and develop features that depend on these APIs. For example, if an external API is down, it can block the entire development process, making it difficult to proceed with testing and development.

To address this issue, the team needs a way to bypass the call to the external API and provide a fake response instead. This allows the development and testing to continue smoothly without being dependent on the availability or reliability of the external API.

The Fresp package helps to solve this problem by allowing developers to configure fake responses for their `HttpClient` requests, ensuring that development and testing can proceed without interruption.

> [!NOTE]
> Fresp is not intended for unit testing; it is recommended for use in UAT, QA, and development environments during execution.

> [!WARNING]
> By default, Fresp is disabled in the production environment, so the chance of getting a fake response in production is zero!

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

To make `Fresp` mock and return fake responses from your `HttpClient`, use the `AddFakeResponseHandler` extension method:

```csharp
services.AddHttpClient("MyClient")
        .AddFakeResponseHandler(options =>
        {
            options.Enabled = true;
        });
```

### Configuring Fake Responses

Use the method `AddFakeResponse` for synchronous request calls or `AddFakeResponseAsync` for asynchronous request calls:

- Synchronous:
```csharp
services.AddHttpClient("MyClient")
        .AddFakeResponseHandler(options =>
        {
            options.Enabled = true;
            options.AddFakeResponse(request =>
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
        .AddFakeResponseHandler(options =>
        {
            options.Enabled = true;
            options.AddFakeResponseAsync(async request =>
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

If the request predicate is matched, the following configured response will be returned. It's simple and lightweight!

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.