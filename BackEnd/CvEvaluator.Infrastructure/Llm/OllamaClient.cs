using CvEvaluator.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace CvEvaluator.Infrastructure.Llm;

public class OllamaClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    public OllamaClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(
            configuration["Ollama:BaseUrl"]
            ?? throw new InvalidOperationException("OLLAMA_BASE_URL not configured")
        );
    }

    public async Task<string> EvaluateCvAsync(string prompt)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            new
            {
                model = "llama3:8b",
                prompt,
                stream = false
            });

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        return json?.response ?? string.Empty;
    }

    private class OllamaResponse
    {
        public string response { get; set; } = string.Empty;
    }
}