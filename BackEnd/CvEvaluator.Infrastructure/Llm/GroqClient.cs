using CvEvaluator.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CvEvaluator.Infrastructure.Llm;

public class GroqClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public GroqClient(HttpClient httpClient, IConfiguration configuration)
    {
        var apiKey = configuration["Groq:ApiKey"]
            ?? throw new InvalidOperationException("Groq:ApiKey not configured");

        _model = configuration["Groq:Model"] ?? "llama-3.1-8b-instant";

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.groq.com");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<string> EvaluateCvAsync(string prompt, CancellationToken ct)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/openai/v1/chat/completions",
            new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            },
            ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<GroqResponse>(cancellationToken: ct);

        return json?.choices?[0]?.message?.content ?? string.Empty;
    }

    private class GroqResponse
    {
        public GroqChoice[]? choices { get; set; }
    }

    private class GroqChoice
    {
        public GroqMessage? message { get; set; }
    }

    private class GroqMessage
    {
        public string content { get; set; } = string.Empty;
    }
}
