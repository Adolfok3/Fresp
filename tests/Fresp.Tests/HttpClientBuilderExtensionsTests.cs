using Fresp.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;

namespace Fresp.Tests;

public class HttpClientBuilderExtensionsTests
{
    [Fact]
    public void AddFakeResponseHandler_WithoutOptions_ShouldAddSuccessfully()
    {
        // Arrange
        var builder = Substitute.For<IHttpClientBuilder>();
        builder.Name.Returns("TestClient");

        // Act
        var act = () => builder.AddFakeHandler();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddFakeResponseHandler_WithOptions_ShouldAddSuccessfully()
    {
        // Arrange
        var builder = Substitute.For<IHttpClientBuilder>();

        var optionsInvoked = false;
        Action<FakeOptions> configureOptions = options =>
        {
            optionsInvoked = true;
            options.ClientName = "TestClient";
            options.Enabled = true;
            options.AddFakeResponseFromRequest((sp, req) => new HttpResponseMessage());
            options.AddFakeResponseFromRequestAsync(async (sp, req) => await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage()));
            options.AddFakeResponseFromResponse((sp, res) => new HttpResponseMessage());
            options.AddFakeResponseFromResponseAsync(async (sp, res) => await Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage()));
            options.AddFakeResponseFromRequest<MockFakeResponseFromRequest>();
            options.AddFakeResponseFromRequestAsync<MockFakeResponseFromRequestAsync>();
            options.AddFakeResponseFromResponse<MockFakeResponseFromResponse>();
            options.AddFakeResponseFromResponseAsync<MockFakeResponseFromResponseAsync>();
        };

        // Act
        var act = () => builder.AddFakeHandler(configureOptions);

        // Assert
        act.Should().NotThrow();
        optionsInvoked.Should().BeTrue();
    }
}
