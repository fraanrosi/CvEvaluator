using CvEvaluator.Infrastructure.Llm;
using CvEvaluator.Infrastructure.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace CvEvaluator.Infrastructure.Tests;

public class OllamaClientTests
{
    private IConfiguration CreateConfig(string? baseUrl = "http://localhost:11434")
    {
        var dict = new Dictionary<string, string?>();
        if (baseUrl != null) dict["Ollama:BaseUrl"] = baseUrl;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public void Constructor_MissingBaseUrl_ThrowsInvalidOperationException()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var httpClient = new HttpClient();

        Assert.Throws<InvalidOperationException>(() => new OllamaClient(httpClient, config));
    }

    [Fact]
    public async Task EvaluateCvAsync_SuccessfulResponse_ReturnsContent()
    {
        var json = """{"response":"llama response text"}""";
        var handler = MockHttpMessageHandler.WithJsonResponse(json);
        var httpClient = new HttpClient(handler);

        var client = new OllamaClient(httpClient, CreateConfig());

        var result = await client.EvaluateCvAsync("test prompt", default);

        Assert.Equal("llama response text", result);
    }

    [Fact]
    public async Task EvaluateCvAsync_EmptyResponse_ReturnsEmptyString()
    {
        var json = """{"response":""}""";
        var handler = MockHttpMessageHandler.WithJsonResponse(json);
        var httpClient = new HttpClient(handler);

        var client = new OllamaClient(httpClient, CreateConfig());

        var result = await client.EvaluateCvAsync("test prompt", default);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task EvaluateCvAsync_HttpError_ThrowsHttpRequestException()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler);

        var client = new OllamaClient(httpClient, CreateConfig());

        await Assert.ThrowsAsync<HttpRequestException>(() => client.EvaluateCvAsync("test prompt", default));
    }
}
