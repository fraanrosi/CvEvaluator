using System.Net.Http.Json;
using CvEvaluator.Application.Interfaces;

namespace CvEvaluator.Infrastructure.Llm;

public class OllamaClient : ILlmClient
{
    private readonly HttpClient _httpClient;

    public OllamaClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> EvaluateCvAsync(string prompt)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "http://localhost:11434/api/generate",
            new
            {
                model = "llama3:8b",
                prompt = prompt,
                stream = false
            });

        var json = await response.Content.ReadFromJsonAsync<OllamaResponse>();
        return json?.response ?? string.Empty;
    }

    private class OllamaResponse
    {
        public string response { get; set; } = string.Empty;
    }
}