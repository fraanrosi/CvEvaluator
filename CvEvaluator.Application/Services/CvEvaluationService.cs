using System.Text.Json;
using CvEvaluator.Application.Interfaces;
using CvEvaluator.Application.Prompts;
using CvEvaluator.Domain.Enums;
using CvEvaluator.Domain.Models;

namespace CvEvaluator.Application.UseCases;

using CvEvaluator.Application.Interfaces;

public class CvEvaluationService : ICvEvaluationService
{
    private const int Threshold = 70;

    private readonly ILlmClient _llmClient;

    public CvEvaluationService(ILlmClient llmClient)
    {
        _llmClient = llmClient;
    }

    public async Task<(CvDecision Decision, CvEvaluationResult Result)> ExecuteAsync(string cvText)
    {
        if (string.IsNullOrWhiteSpace(cvText))
            throw new ArgumentException("CV text cannot be empty");

        var prompt = CvEvaluationPrompt.Build(cvText);

        var llmResponse = await _llmClient.EvaluateCvAsync(prompt);

        if (string.IsNullOrWhiteSpace(llmResponse))
            throw new Exception("LLM returned empty response");

        CvEvaluationResult? evaluationResult;

        try
        {
            evaluationResult = JsonSerializer.Deserialize<CvEvaluationResult>(
                llmResponse,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch (JsonException ex)
        {
            throw new Exception(
                $"Failed to parse LLM response as JSON. Response was: {llmResponse}",
                ex
            );
        }

        if (evaluationResult == null)
            throw new Exception("LLM response could not be deserialized");
        
        var decision = evaluationResult.Score >= Threshold
            ? CvDecision.Suitable
            : CvDecision.NotSuitable;

        return (decision, evaluationResult);
    }
}
