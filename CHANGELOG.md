# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.1.0]

### Added

- **`FakeHttpClient` factory for unit testing.** New public static class `FakeHttpClient` that creates an `HttpClient` (`FakeHttpClient.Create`) or its underlying `HttpMessageHandler` (`FakeHttpClient.CreateHandler`) already wired with fake responses — with **no need for dependency injection, `IHostEnvironment`, or `IHttpClientBuilder`**. This makes it possible to fake external APIs consumed through an `HttpClient` directly in unit tests, instead of mocking the API interface.
  - Fakes are **enabled by default** in the factory.
  - Fakes are configured exactly like during application execution: all `FromRequest`/`FromResponse` methods (sync and async) and the class-based `IFakeResponseFrom*` interfaces are supported.
  - Optional `serviceProvider` parameter, passed through to the fake handlers so fakes can resolve services via DI. When omitted, an empty provider (that always returns `null`) is used.
  - Optional `defaultResponse` parameter that simulates the "real" external API response when no `FromRequest` fake matches — enabling `FromResponse` fakes and status-code assertions.
  - Requests that don't match any fake throw a descriptive `InvalidOperationException` by default, so unexpected calls fail tests loudly.
  - Verified to work with typed clients, a mocked `IHttpClientFactory`, and [Refit](https://github.com/reactiveui/refit)-generated clients.

### Documentation

- Added a **"Using Fresp in unit tests"** section to the README covering the `FakeHttpClient` factory, dependency injection, unmatched-request behavior, usage with a mocked `IHttpClientFactory`, and Refit integration.

### Changed

- Updated dependencies:
  - **.NET 10** target: `Microsoft.Extensions.Http` and `Microsoft.Extensions.Hosting.Abstractions` `10.0.2` → `10.0.11`.
  - **.NET 9** target: `Microsoft.Extensions.Http` and `Microsoft.Extensions.Hosting.Abstractions` `9.0.11` → `9.0.19`.
  - **.NET 8** target: unchanged (`8.0.1`).

### Notes

- No breaking changes. Existing application-execution usage (`AddFakeHandler` on `IHttpClientBuilder`) is unchanged.

[3.1.0]: https://github.com/Adolfok3/Fresp/releases/tag/3.1.0
