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
        var act = () => builder.AddFakeResponseHandler();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddFakeResponseHandler_WithOptions_ShouldAddSuccessfully()
    {
        // Arrange
        var builder = Substitute.For<IHttpClientBuilder>();

        var optionsInvoked = false;
        Action<FakeResponseOptions> configureOptions = options =>
        {
            optionsInvoked = true;
            options.ClientName = "TestClient";
            options.Enabled = true;
            options.AddFakeResponse(_ => new HttpResponseMessage());
            options.AddFakeResponseAsync(_ => Task.FromResult<HttpResponseMessage?>(new HttpResponseMessage()));
        };

        // Act
        var act = () => builder.AddFakeResponseHandler(configureOptions);

        // Assert
        act.Should().NotThrow();
        optionsInvoked.Should().BeTrue();
    }
}
