using CvEvaluator.Infrastructure.Llm;
using CvEvaluator.Infrastructure.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace CvEvaluator.Infrastructure.Tests;

public class GroqClientTests
{
    private IConfiguration CreateConfig(string? apiKey = "test-api-key", string? model = null)
    {
        var dict = new Dictionary<string, string?>();
        if (apiKey != null) dict["Groq:ApiKey"] = apiKey;
        if (model != null) dict["Groq:Model"] = model;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    [Fact]
    public void Constructor_MissingApiKey_ThrowsInvalidOperationException()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var httpClient = new HttpClient();

        Assert.Throws<InvalidOperationException>(() => new GroqClient(httpClient, config));
    }

    [Fact]
    public async Task EvaluateCvAsync_SuccessfulResponse_ReturnsContent()
    {
        var json = """{"choices":[{"message":{"content":"evaluation result"}}]}""";
        var handler = MockHttpMessageHandler.WithJsonResponse(json);
        var httpClient = new HttpClient(handler);

        var client = new GroqClient(httpClient, CreateConfig());

        var result = await client.EvaluateCvAsync("test prompt", default);

        Assert.Equal("evaluation result", result);
    }

    [Fact]
    public async Task EvaluateCvAsync_NullChoices_ReturnsEmptyString()
    {
        var json = """{"choices":null}""";
        var handler = MockHttpMessageHandler.WithJsonResponse(json);
        var httpClient = new HttpClient(handler);

        var client = new GroqClient(httpClient, CreateConfig());

        var result = await client.EvaluateCvAsync("test prompt", default);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task EvaluateCvAsync_HttpError_ThrowsHttpRequestException()
    {
        var handler = MockHttpMessageHandler.WithStatusCode(HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler);

        var client = new GroqClient(httpClient, CreateConfig());

        await Assert.ThrowsAsync<HttpRequestException>(() => client.EvaluateCvAsync("test prompt", default));
    }

    [Fact]
    public async Task EvaluateCvAsync_SetsAuthorizationHeader()
    {
        string? capturedAuthHeader = null;
        var handler = new MockHttpMessageHandler((req, _) =>
        {
            capturedAuthHeader = req.Headers.Authorization?.ToString();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"choices":[{"message":{"content":"ok"}}]}""",
                    System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        });
        var httpClient = new HttpClient(handler);

        var client = new GroqClient(httpClient, CreateConfig(apiKey: "my-secret-key"));

        await client.EvaluateCvAsync("test", default);

        Assert.Equal("Bearer my-secret-key", capturedAuthHeader);
    }
}
