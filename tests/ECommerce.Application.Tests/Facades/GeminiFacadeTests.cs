using System.Net;
using System.Net.Http.Json;
using ECommerce.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace ECommerce.Application.Tests.Facades;

public class GeminiFacadeTests
{
    private static GeminiFacade CreateSut(
        HttpMessageHandler handler,
        string? apiKey = "test-key",
        string? configuredModel = null)
    {
        var httpClient = new HttpClient(handler);

        var options = Options.Create(new GeminiOptionsModel { ApiKey = apiKey ?? string.Empty });

        var inMemoryConfig = new Dictionary<string, string?>();
        if (configuredModel is not null)
            inMemoryConfig["Gemini:Model"] = configuredModel;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfig)
            .Build();

        return new GeminiFacade(httpClient, options, config);
    }

    private static HttpMessageHandler CreateHttpHandler(HttpStatusCode statusCode, string responseJson)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseJson)
            });
        return handlerMock.Object;
    }

    private static string BuildGeminiSuccessJson(string text) => $$"""
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  { "text": "{{text}}" }
                ]
              }
            }
          ]
        }
        """;

    // ─── GenerateTextAsync – happy path ───────────────────────────────────────

    [Fact]
    public async Task GenerateTextAsync_SuccessResponse_ReturnsExtractedText()
    {
        // Arrange
        var json = BuildGeminiSuccessJson("A great product.");
        var handler = CreateHttpHandler(HttpStatusCode.OK, json);
        var sut = CreateSut(handler);

        // Act
        var result = await sut.GenerateTextAsync("Describe this product");

        // Assert
        Assert.Equal("A great product.", result);
    }

    // ─── GenerateTextAsync – missing API key ─────────────────────────────────

    [Fact]
    public async Task GenerateTextAsync_MissingApiKey_ThrowsInvalidOperationException()
    {
        // Arrange – pass empty key in options AND no env vars in config
        var handler = CreateHttpHandler(HttpStatusCode.OK, "{}");
        var sut = CreateSut(handler, apiKey: "");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GenerateTextAsync("Hello"));

        Assert.Contains("Gemini API key", ex.Message);
    }

    // ─── GenerateTextAsync – non-404 HTTP error ───────────────────────────────

    [Fact]
    public async Task GenerateTextAsync_ApiReturns400_ThrowsException()
    {
        // Arrange
        var handler = CreateHttpHandler(HttpStatusCode.BadRequest, "{\"error\":\"invalid\"}");
        var sut = CreateSut(handler);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.GenerateTextAsync("Prompt"));
        Assert.Contains("Gemini API error", ex.Message);
    }

    [Fact]
    public async Task GenerateTextAsync_ApiReturns403_ThrowsException()
    {
        // Arrange
        var handler = CreateHttpHandler(HttpStatusCode.Forbidden, "{\"error\":\"forbidden\"}");
        var sut = CreateSut(handler);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.GenerateTextAsync("Prompt"));
        Assert.Contains("Gemini API error", ex.Message);
    }

    // ─── GenerateTextAsync – empty candidates ────────────────────────────────

    [Fact]
    public async Task GenerateTextAsync_NoCandidates_ThrowsException()
    {
        // Arrange
        var handler = CreateHttpHandler(HttpStatusCode.OK, """{ "candidates": [] }""");
        var sut = CreateSut(handler);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.GenerateTextAsync("Prompt"));
        Assert.Contains("no content", ex.Message);
    }

    // ─── GenerateTextAsync – model fallback ──────────────────────────────────

    [Fact]
    public async Task GenerateTextAsync_AllModelsFail404_ThrowsException()
    {
        // Arrange – every call returns 404
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("{}")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new GeminiOptionsModel { ApiKey = "test-key" });
        var config = new ConfigurationBuilder().Build();
        var sut = new GeminiFacade(httpClient, options, config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => sut.GenerateTextAsync("Prompt"));
        Assert.Contains("model not found", ex.Message);
    }

    [Fact]
    public async Task GenerateTextAsync_ConfiguredModelFails404_FallsBackAndSucceeds()
    {
        // Arrange – first call (configured model) → 404; second call (fallback) → 200
        var callCount = 0;
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent("{}")
                    };
                }
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(BuildGeminiSuccessJson("Fallback worked!"))
                };
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var options = Options.Create(new GeminiOptionsModel { ApiKey = "test-key" });
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Gemini:Model"] = "my-custom-model" })
            .Build();

        var sut = new GeminiFacade(httpClient, options, config);

        // Act
        var result = await sut.GenerateTextAsync("Prompt");

        // Assert
        Assert.Equal("Fallback worked!", result);
        Assert.True(callCount >= 2, "Should have tried at least two models.");
    }

    // ─── ResolveApiKey – config sources ──────────────────────────────────────

    [Fact]
    public async Task GenerateTextAsync_ApiKeyFromConfiguration_UsesIt()
    {
        // Arrange – no key in options; supply via GEMINI_API_KEY config key
        var json = BuildGeminiSuccessJson("OK");
        var handler = CreateHttpHandler(HttpStatusCode.OK, json);

        var httpClient = new HttpClient(handler);
        var options = Options.Create(new GeminiOptionsModel { ApiKey = string.Empty });
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["GEMINI_API_KEY"] = "env-key" })
            .Build();

        var sut = new GeminiFacade(httpClient, options, config);

        // Act – should not throw (key is present)
        var result = await sut.GenerateTextAsync("Prompt");

        // Assert
        Assert.Equal("OK", result);
    }
}
